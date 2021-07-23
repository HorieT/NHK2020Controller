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
        private SynchronizationContext _mainContext;

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
            _mainContext = SynchronizationContext.Current;
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
        /// <summary>
        /// ROSとの非同期接続
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="useBt"></param>
        /// <returns></returns>
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
        /// <summary>
        /// USBでのマイコンとの非同期接続
        /// </summary>
        /// <returns></returns>
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
            _sendMsgTimer.Change(Timeout.Infinite, Timeout.Infinite);
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
                _log.WiteErrorMsg("切断されました");
                Disconnect();
                throw;
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


        /// <summary>
        /// 接続デバイス、方式に関わらず周期的に行う送信
        /// </summary>
        /// <param name="sender"></param>
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
                        _log.WiteDebugMsg("送信失敗");
                        _mainContext.Post(_ => Disconnect(), null);
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
