using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MVVMLib;

/// <summary>
/// 通信インターフェースモデル
/// 接続インターフェースをラップする
/// 通信メッセージのエンコードもここ
/// </summary>
namespace ABU2021_ControlAndDebug.Models
{

    /// <summary>
    /// 通信を総括するモデル
    /// ジョイパッドと接続確認連絡は周期送信、その他は逐次送信
    /// </summary>
    class Communicator : NotifyPropertyChanged
    {
        private OutputLog _log;
        private JoypadHandl _joypad;
        private Timer _sendMsgTimer;
        private Core.ComDevice _comDevice;
        private Core.SendDataMsg _sendMsgJoy = new Core.SendDataMsg(Core.SendDataMsg.HeaderType.JOY, new JoypadControl.Joypad.JOYINFOEX());


        #region Singleton instance
        private static Communicator _instance;
        public static Communicator GetInstance
        {
            get
            {
                return _instance ?? (_instance = new Communicator());
            }
        }
        private Communicator()
        {
            _log = OutputLog.GetInstance;
            _joypad = JoypadHandl.GetInstance;
        }
        ~Communicator()
        {
            if (IsConnected) Disconnect();
        }
        #endregion



        #region Property
        private Core.ControlType.Device _machine = Core.ControlType.Device.Etc;
        private int _sendMsgPeriodMs = 50;

        public Core.ControlType.Device Device
        {
            get => _machine;
            private set { SetProperty(ref _machine, value); }
        }
        public bool IsConnected{ get => _comDevice != null; }
        public int SendMsgPeriod
        {
            get => _sendMsgPeriodMs;
            set
            {
                if (SetProperty(ref _sendMsgPeriodMs, value))
                    _sendMsgTimer.Change(0, _sendMsgPeriodMs);
            }
        }

        public string ConnectedDviseName
        {
            get
            {
                var stm = _comDevice as Core.ComStm;
                var ros = _comDevice as Core.ComRos;
                if (stm != null) return "STマイコン-USB(" + stm.BoardType.ToString() + ")";
                if (ros != null) return "ROS-Wifi(" + ros.Port.ToString() + ")";
                return "無し";
            }
        }
        public bool IsConnectedStm
        {
            get{ return _comDevice is Core.ComStm; }
        }
        public bool IsConnectedRos
        {
            get { return _comDevice is Core.ComRos; }
        }
        #endregion


        #region Method
        public async Task ConnectRosAsync(Core.ControlType.TcpPort machine, bool useBt = false)
        {
            if (IsConnected) throw new InvalidOperationException("already connected");
            if (useBt) throw new NotImplementedException("Don't use bluetooth");

            var ros = new Core.ComRos();
            ros.Port = machine;
            try
            {
                await ros.Connect();
            }
            catch
            {
                throw;
            }
            _comDevice = ros;
            _sendMsgTimer = new Timer(PeriodicSendMsg, null, 0, SendMsgPeriod);
            Device = Core.ControlType.ToDevice(machine);
            ConnectStatusChage();

        }
        public async Task ConnectStmAsync()
        {
            if (IsConnected) throw new InvalidOperationException("already connected");


            var stm = new Core.ComStm();
            try
            {
               await stm.Connect();
            }
            catch (Exception e)
            {
#if DEBUG
                _log.WiteDebugMsg(e.ToString());
#endif
                throw;
            }
            _comDevice = stm;
            _sendMsgTimer = new Timer(PeriodicSendMsg, null, 0, SendMsgPeriod);
            Device = Core.ControlType.ToDevice(stm.BoardType);
            ConnectStatusChage();
        }
        public void Disconnect()
        {
            if (!IsConnected) return;// throw new InvalidOperationException("Non connected");
            _sendMsgTimer.Dispose();

            try
            {
                _comDevice.Disconnect();
            }
            finally
            {
                _comDevice = null;
                ConnectStatusChage();
            }
        }

        public void SendMsg(Core.SendDataMsg msg)
        {
            if (!IsConnected) throw new InvalidOperationException("not connected");

            try
            {
                _comDevice.SendMsgAsync(msg);
            }
            catch
            {
                //Disconnect();
                _log.WiteErrorMsg("切断されました");
                Disconnect();
            }
        }
        public async Task<Core.ReceiveDataMsg> ReadMsgAsync()
        {
            if (!IsConnected) throw new InvalidOperationException("not connected");

            try
            {
                return await _comDevice.ReadMsgAsync();
            }
            catch
            {
                _log.WiteErrorMsg("切断されました");
                Disconnect();
                throw;
            }
        }


        private void PeriodicSendMsg(object sender)
        {
            if (_joypad.IsEnabled)
            {
                Task.Run(async () => {
                    try
                    {
                        //_sendMsgJoy.Reset(Core.SendDataMsg.HeaderType.JOY, _joypad.GetPad().JoyInfoEx);
                        var data = new Core.SendDataMsg(Core.SendDataMsg.HeaderType.JOY, _joypad.GetPad().JoyInfoEx);
                        await _comDevice?.SendMsgAsync(data);
                        _log.WiteDebugMsg(data.ConvString());
                    }
                    catch
                    {
                        //Disconnect();
                        _log.WiteErrorMsg("切断されました");
                        Disconnect();
                    }
                });
            }
        }
        private void ConnectStatusChage()
        {
            RaisePropertyChanged("IsConnected");
            RaisePropertyChanged("ConnectedDviseName");
            RaisePropertyChanged("IsConnectedStm");
            RaisePropertyChanged("IsConnectedRos");
        }
        #endregion
    }
}
