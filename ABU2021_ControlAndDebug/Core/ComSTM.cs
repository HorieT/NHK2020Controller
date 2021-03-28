using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using JoypadControl;

namespace ABU2021_ControlAndDebug.Core
{
    class ComStm : ComDevice
    {
        private static readonly int MaxQueueSize = 256;
        private SerialPort _port;
        private ConcurrentQueue<ReceiveDataMsg> _readMsgQueue = new ConcurrentQueue<ReceiveDataMsg>();
        private List<byte> _readDataBuff = new List<byte>();
        private bool _isConencted = false;
        private Task _readTask;


        #region Property
        public bool IsConnected { get => _isConencted; }
        public ControlType.UsbBoardPid BoardType{ get; private set; }
        #endregion



        public ComStm()
        {
        }
        ~ComStm()
        {
            if (IsConnected) Disconnect();
        }



        #region Method
        #region public
        public Task Connect()
        {
            if (IsConnected) throw new InvalidOperationException("Already connected");

            return Task.Run(() =>
            {
                var ports = GetComports();
                bool isFound = false;

                foreach (var board in Enum.GetValues(typeof(Core.ControlType.UsbBoardPid)))
                {
                    try
                    {
                        DestinationSelection(ControlType.STM_VID, (int)board, ports);
                    }
                    catch (ArgumentException e)
                    {
                        if (e.ParamName == "vid")
                        {
                            throw new InvalidOperationException("STM device not found");
                        }
                        else continue;
                    }
                    catch
                    {
                        throw;
                    }
                    isFound = true;
                    BoardType = (Core.ControlType.UsbBoardPid)board;
                    break;
                }
                if (!isFound)
                {
                    throw new InvalidOperationException("No valid STM device found");
                }


                _port.DtrEnable = true;
                _port.RtsEnable = true;
                try { _port.Open(); }
                catch { throw; }
                _isConencted = true;
                _readTask = ReadData();
            });
        }
        public void Disconnect()
        {
            _readDataBuff.Clear();
            _readMsgQueue = new ConcurrentQueue<ReceiveDataMsg>();
            if (!IsConnected) return;//throw new InvalidOperationException("Not connected");
            try
            {
                _port.Close();
            } 
            catch
            {
                throw;
            }
            finally
            {
                _isConencted = false;
            }
        }

        /// <summary>
        /// 送信
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="header"></param>
        /// <param name="data"></param>
        public async Task SendMsgAsync(SendDataMsg msg)
        {
            await Task.Run(() =>
            {
                var sendData = COBS_Encode(msg.ConvByte());
                try
                {
                    _port.Write(sendData, 0, sendData.Length);
                }
                catch
                {
                    Disconnect();
                    throw new InvalidOperationException("Connecton error");
                }
            });

        }
        /// <summary>
        /// 受信メッセージの取り出し
        /// </summary>
        /// <returns></returns>
        public async Task<ReceiveDataMsg> ReadMsgAsync()
        {
            return await Task.Run(async () =>
            {
                ReceiveDataMsg msg;
                while (true)
                {
                    if (!_readMsgQueue.IsEmpty)
                    {
                        if (_readMsgQueue.TryDequeue(out msg))
                            return msg;
                    }
                    await Task.Delay(50);
                }
            });
        }
        #endregion

        #region private
        /// <summary>
        /// 接続のあるCOMポートの取得
        /// </summary>
        /// <returns></returns>
        private static IReadOnlyList<Comport> GetComports()
        {
            var list = new List<Comport>();
            var management = new ManagementClass("Win32_PnPEntity");

            foreach (var m in management.GetInstances()) using (m)
                {
                    list.Add(new Comport()
                    {
                        DeviceID = (string)m.GetPropertyValue("DeviceID"),
                        Description = (string)m.GetPropertyValue("Caption"),
                        PNPDeviceID = (string)m.GetPropertyValue("PNPDeviceID")
                    });
                }

            return list.AsReadOnly();
        }
        /// <summary>
        /// COMポートの選択
        /// </summary>
        /// <param name="vid"></param>
        /// <param name="pid"></param>
        private void DestinationSelection(int vid, int pid)
        {
            try { DestinationSelection(vid, pid, GetComports()); }
            catch { throw; }
        }
        /// <summary>
         /// COMポートの選択
         /// </summary>
         /// <param name="vid"></param>
         /// <param name="pid"></param>
        private void DestinationSelection(int vid, int pid, IReadOnlyList<Comport> ports)
        {
            if (IsConnected) throw new InvalidOperationException("Already connected to USB port");
            if (ports.Count(i => i.VID == vid) == 0) throw new ArgumentException("No matching port was found", nameof(vid));
            var pnpPort = ports.FirstOrDefault(i => (i.VID == vid && i.PID == pid));
            if (pnpPort == null) throw new ArgumentException("No matching port was found", nameof(pid));

            _port = new SerialPort(pnpPort.PortName, 115200, Parity.None, 8, StopBits.One);
        }

        /// <summary>
        /// 受信バイト読み
        /// </summary>
        /// <returns></returns>
        private Task ReadData()
        {
            return Task.Run(() =>
            {
                int byteData;
                while (_isConencted)
                {
                    try
                    {
                        byteData = _port.ReadByte();
                    }
                    catch (TimeoutException)
                    {
                        byteData = -1;
                    }

                    if (byteData != -1)
                    {
                        _readDataBuff.Add((byte)byteData);
                        if(byteData == 0)//終端
                        {
                            if (_readDataBuff.Count() > 2)
                            {
                                try
                                {
                                    _readMsgQueue.Enqueue(new ReceiveDataMsg(COBS_Decode(_readDataBuff)));
                                }
                                catch
                                {
                                    //デコード失敗
                                }
                            }
                            _readDataBuff.Clear();
                        }
                    }
                }
            });
        }
        private byte[] COBS_Encode(IReadOnlyList<byte> inData)
        {
            if (inData.Count() > 255 || inData.Count() < 1) throw new ArgumentException("The data size must be 1 to 255 bytes", nameof(inData));
            var encodeData = new List<byte>();
            int zeroIndex = 0;

            encodeData.Add(0);
            encodeData.AddRange(inData);
            for (int i = 1; i < encodeData.Count; ++i)
            {
                if (encodeData[i] == 0)
                {
                    encodeData[zeroIndex] = (byte)(i - zeroIndex);
                    zeroIndex = i;
                }
            }
            encodeData[zeroIndex] = (byte)(encodeData.Count - zeroIndex);
            encodeData.Add(0);

            return encodeData.ToArray();
        }
        private byte[] COBS_Decode(IReadOnlyList<byte> inData)
        {
            if (inData.Last() != 0x00) throw new ArgumentException("The end of the data column must be 0", nameof(inData));
            if (inData.Count() > 257 || inData.Count() < 3) throw new ArgumentException("The data size must be 3 to 257 bytes", nameof(inData));
            var decodeData = inData.ToList();
            int zeroIndex = 0;

            while (zeroIndex < decodeData.Count - 1)
            {
                if (decodeData[zeroIndex] == 0) throw new ArgumentException("Data decoding failed", nameof(inData));
                var tmpIndex = zeroIndex;
                zeroIndex += decodeData[zeroIndex];
                decodeData[tmpIndex] = 0;
            }
            decodeData.RemoveAt(0);
            decodeData.RemoveAt(decodeData.Count - 1);

            return decodeData.ToArray();
        }
        #endregion
        #endregion
    }
}
