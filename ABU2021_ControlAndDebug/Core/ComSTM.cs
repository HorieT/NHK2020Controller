using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using JoypadControl;

namespace ABU2021_ControlAndDebug.Core
{
    class ComSTM
    {
        private InterfaceUSB _usb;


        #region Property
        public ControlType.USBBoardPID BoardType{ get; private set; }
        public bool IsConnected { get; private set; }
        #endregion



        public ComSTM()
        {
            _usb = new InterfaceUSB();
            _usb.DataReceived += _usb_DataReceived;
        }
        ~ComSTM()
        {
            if (IsConnected) Disconnect();
        }



        #region Method
        public void Connect()
        {
            var ports = Core.InterfaceUSB.GetComports();
            bool isFound = false;

#if DEBUG
            //_log.WiteDebugMsg("Serch VID : " + Core.ControlType.STM_VID.ToString("X4"));
            /*foreach (var p in ports)
            {
                if (Regex.IsMatch(p.PNPDeviceID, @"^USB"))
                    _log.WiteDebugMsg("Get Port PNP : " + p.PNPDeviceID);
            }*/
#endif

            foreach (var board in Enum.GetValues(typeof(Core.ControlType.USBBoardPID)))
            {
                try
                {
                    _usb.DestinationSelection(Core.ControlType.STM_VID, (int)board, ports);
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
                BoardType = (Core.ControlType.USBBoardPID)board;
                break;
            }
            if (!isFound)
            {
                throw new InvalidOperationException("No valid STM device found");
            }

            try
            {
                _usb.Connect();
            }
            catch (Exception e)
            {
                throw;
            }
            IsConnected = true;
        }

        public void Disconnect()
        {
            try
            {
                _usb.Disconnect();
            }
            catch 
            {
                throw;
            }
            finally
            {
                IsConnected = false;
            }
        }


        /// <summary>
        /// 送信
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="header"></param>
        /// <param name="data"></param>
        public void Send<T>(byte header, T data) where T : struct
        {
            int size = Marshal.SizeOf(data);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, ptr, false);
            byte[] bytes = new byte[size + 1];
            Marshal.Copy(ptr, bytes, 1, size);
            bytes[0] = header;

            var sendData = COBS_Encode(bytes.ToList());
            try
            {
                _usb.Write(sendData, sendData.Length);
            }
            catch
            {
                Disconnect();
                throw new InvalidOperationException("Connecton error");
            }
        }
        /// <summary>
        /// 送信
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="header"></param>
        /// <param name="data"></param>
        /// <param name="type"
        public void Send(byte header, object data, Type type)
        {
            if (!type.IsValueType) throw new ArgumentException("Type must be a value type ", nameof(type));
            int size = Marshal.SizeOf(type);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, ptr, false);
            byte[] bytes = new byte[size + 1];
            Marshal.Copy(ptr, bytes, 1, size);
            bytes[0] = header;

            var sendData = COBS_Encode(bytes.ToList());
            try
            {
                _usb.Write(sendData, sendData.Length);
            }
            catch
            {
                Disconnect();
                throw new InvalidOperationException("Connecton error");
            }
        }
        /// <summary>
        /// 送信のジョイパッド特殊化
        /// </summary>
        /// <param name="header">ヘッダは外部定義</param>
        /// <param name="joy"></param>
        public void Send(byte header, Joypad.JOYINFOEX joy)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected");
            List<byte> packet = new List<byte>();

            //header
            packet.Add(0x80);
            packet.AddRange(BitConverter.GetBytes(joy.dwButtons));
            packet.AddRange(BitConverter.GetBytes(joy.dwPOV));
            packet.Add((byte)(((int)joy.dwXpos - ushort.MaxValue / 2 - 1) / 256));//X軸
            packet.Add((byte)(((int)joy.dwYpos - ushort.MaxValue / 2 - 1) / 256));//Y軸
            packet.Add((byte)(((int)joy.dwVpos - ushort.MaxValue / 2 - 1) / 256));//X回転
            packet.Add((byte)(((int)joy.dwRpos - ushort.MaxValue / 2 - 1) / 256));//Y回転
            //checksum
            packet.Add(packet.Aggregate((sum, item) => (byte)(sum + item)));//byte列にはSum()が使えない

            var padData = COBS_Encode(packet);
            try
            {
                _usb.Write(padData, padData.Length);
            }
            catch
            {
                Disconnect();
                throw new InvalidOperationException("Connecton error");
            }
        }


        public async Task<string> ReadSentenceAsync()
        {
            var task = Task<string>.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        return _usb.ReadTo("\0");
                    }
                    catch (TimeoutException e)
                    {
                        continue;
                    }
                    catch
                    {
                        Disconnect();
                        throw new InvalidOperationException("Connecton error");
                    }
                }
            });

            return await task;
        }

        private void _usb_DataReceived(object sender)
        {
            throw new NotImplementedException();
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
    }
}
