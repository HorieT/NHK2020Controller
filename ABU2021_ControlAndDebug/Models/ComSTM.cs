using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MVVMLib;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace ABU2021_ControlAndDebug.Models
{
    /// <summary>
    /// STM組み込み基板との通信モデル
    /// 通信プロトコル等の送受信処理はここで担う
    /// </summary>
    partial class ComSTM : NotifyPropertyChanged
    {
        private OutputLog _log;
        private Core.InterfaceUSB _usb;
        private JoypadHandl _joypad;
        private Timer _sendMsgTimer;
        private ControlTR _tr;
        private ControlDR _dr;
        private ConcurrentQueue<List<byte>> _sendMsgQueue = new ConcurrentQueue<List<byte>>();


        #region Singleton instance
        private static ComSTM _instance;
        public static ComSTM GetInstance
        {
            get
            {
                return _instance ?? (_instance = new ComSTM());
            }
        }
        private ComSTM()
        {
            _log = OutputLog.GetInstance;
            _usb = new Core.InterfaceUSB();
            _joypad = JoypadHandl.GetInstance;
            _tr = ControlTR.GetInstance;
            _dr = ControlDR.GetInstance;

            _usb.DataReceived += _usb_DataReceived;
            _tr.PropertyChanged += _tr_PropertyChanged;
            _dr.PropertyChanged += _dr_PropertyChanged;
        }
        #endregion






        #region Property
        private Core.ControlType.USBBoardPID _boardType;
        private bool _isConnected;
        private bool _isJoypadEnable;
        private int _sendMsgPeriodMs = 50;

        public Core.ControlType.USBBoardPID BoardType
        {
            get => _boardType;
            private set { SetProperty(ref _boardType, value); }
        }
        public bool IsConnected
        {
            get => _isConnected;
            private set { SetProperty(ref _isConnected, value); }
        }
        public bool IsJoypadEnable
        {
            get => _isJoypadEnable;
            set { SetProperty(ref _isJoypadEnable, value); }
        }
        public int SendMsgPeriod 
        {
            get => _sendMsgPeriodMs;
            set
            {
                if (SetProperty(ref _sendMsgPeriodMs, value)) 
                    _sendMsgTimer.Change(0, _sendMsgPeriodMs);
            }
        }
        #endregion


        #region Method
        public void Connect()
        {
            var ports = Core.InterfaceUSB.GetComports();
            bool isFound = false;

#if DEBUG
            _log.WiteDebugMsg("Serch VID : " + Core.ControlType.STM_VID.ToString("X4"));
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
                        _log.WiteLine("接続対象が見つかりませんでした。");
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
                _log.WiteLine("接続対象が見つかりませんでした。");
                throw new InvalidOperationException("No valid STM device found");
            }

            try
            {
                _usb.Connect();
            }
            catch(Exception e)
            {
                _log.WiteErrorMsg(e.Message);
                throw;
            }
            IsConnected = true;
            _sendMsgTimer = new Timer(SendMsg, null, 0, SendMsgPeriod);
        }

        public void Disconnect()
        {
            _sendMsgTimer.Dispose();
            try
            {
                _usb.Disconnect();
            }
            catch(Exception e)
            {
                _log.WiteErrorMsg(e.Message);
                //throw;
            }
            IsConnected = false;
        }
        /// <summary>
        /// 周期送信メソッド
        /// COBSを使用し送信
        /// </summary>
        /// <param name="sender"></param>
        private void SendMsg(object sender)
        {
            if (!IsConnected) return;

            //Jypad
            if (_joypad.IsEnabled)
            {
                var padData = COBS_Encode(MakeJoypadDataPacket());
                try
                {
                    _usb.Write(padData, padData.Length);
                }
                catch
                {
                    Disconnect();
                    _log.WiteLine("通信が切断されました");
                    return;
                }
            }

            //GUIからのコントロール
            List<byte> guiData;
            List<byte> sendData = new List<byte>();
            while (_sendMsgQueue.Count > 0)
            {
                //エンコードは_sendMsgQueueに入れる前に
                if (_sendMsgQueue.TryDequeue(out guiData))
                    sendData.AddRange(guiData);
            }
            if(sendData.Count > 0)
            {
                try
                {
                    _usb.Write(sendData.ToArray(), sendData.Count);
                }
                catch
                {
                    Disconnect();
                    _log.WiteLine("通信が切断されました");
                    return;
                }
            }
        }
        

        private void _usb_DataReceived(object sender)
        {
            throw new NotImplementedException();
        }


        private void _tr_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (BoardType != Core.ControlType.USBBoardPID.TR || !IsConnected) return;

            try
            {
                ControlTR.SendDataType header = (ControlTR.SendDataType)Enum.Parse(typeof(ControlTR.SendDataType), e.PropertyName);

                _sendMsgQueue.Enqueue(MakeGUIDataPacket((byte)header, _tr[e.PropertyName], e.GetType()));
            }
            catch//送信データではなかった
            {
            }
        }
        private void _dr_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (BoardType != Core.ControlType.USBBoardPID.DR || !IsConnected) return;
        }
        #endregion


    }
}
