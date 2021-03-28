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
        private Core.ComROS _ros;
        private Core.ComSTM _stm;//こいつら抽象化するのアリ


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
            _ros = new Core.ComROS();
            _stm = new Core.ComSTM();

        }
        #endregion



        #region Property
        private Core.ControlType.Machine _machine = Core.ControlType.Machine.Etc;
        private bool _isConnected;
        private int _sendMsgPeriodMs = 50;

        public Core.ControlType.Machine Machine
        {
            get => _machine;
            private set { SetProperty(ref _machine, value); }
        }
        public bool IsConnected
        {
            get => _isConnected;
            private set { SetProperty(ref _isConnected, value); }
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

        public string ConnectedDviseName
        {
            get
            {
                if (_stm.IsConnected) return "STマイコン-USB(" + _stm.BoardType.ToString() + ")";
                if (_ros.IsConnected) return "ROS-Wifi(" + _ros.Port.ToString() + ")";
                return "無し";
            }
        }
        public bool IsConnectedSTM
        {
            get{ return _stm.IsConnected; }
        }
        public bool IsConnectedROS
        {
            get { return _ros.IsConnected; }
        }
        #endregion


        #region Method
        public Task ConnectROS(Core.ControlType.TcpPort machine, bool use_bt = false)
        {
            if (_isConnected) throw new InvalidOperationException("already connected");
            if (use_bt) throw new NotImplementedException("Don't use bluetooth");

            return Task.Run(async () => {
                try
                {
                    await _ros.Connect(machine);
                }
                catch
                {
                    throw;
                }
                _sendMsgTimer = new Timer(PeriodicSendMsg, null, 0, SendMsgPeriod);
                IsConnected = true;
            });
        }
        public void ConnectSTM()
        {
            if (_isConnected) throw new InvalidOperationException("already connected");


            try
            {
                _stm.Connect();
            }
            catch (Exception e)
            {
#if DEBUG
                _log.WiteDebugMsg(e.ToString());
#endif
                throw;
            }
            _sendMsgTimer = new Timer(PeriodicSendMsg, null, 0, SendMsgPeriod);
            IsConnected = true;
        }
        public void Disconnect()
        {
            if (!_isConnected) throw new InvalidOperationException("Non connected");
            _sendMsgTimer.Dispose();
            if (IsConnectedSTM)
            {
                try { _stm.Disconnect(); }
                catch { throw; }
                finally { IsConnected = false; }
                return;
            }
            else if (IsConnectedROS)
            {
                try { _ros.Disconnect(); }
                catch { throw; }
                finally { IsConnected = false; }
                return;
            }
            throw new Exception("`IsConnected` is true, but not connect device");
        }

        public void SendMsg(Core.SendDataMsg msg)
        {
            if (!IsConnected) throw new InvalidOperationException("not connected");

            try
            {
                if (_stm.IsConnected) _stm.Send(msg);
                else if (_ros.IsConnected)_ros.Send(msg);
            }
            catch
            {
                //Disconnect();
                _log.WiteErrorMsg("切断されました");
                _sendMsgTimer.Dispose();
                IsConnected = false;
            }
        }
        public async Task<Core.ReceiveDataMsg> ReadMsgAsync()
        {
            if (!IsConnected) throw new InvalidOperationException("not connected");

            try
            {
                if (_stm.IsConnected)
                {
                    return await _stm.ReadMsgAsync();
                }
                else //if (_ros.IsConnected)
                {
                    return await _ros.ReadMsgAsync();
                }

            }
            catch
            {
                _log.WiteErrorMsg("切断されました");
                _sendMsgTimer.Dispose();
                IsConnected = false;
                throw;
            }
        }


        private void PeriodicSendMsg(object sender)
        {
            if (_joypad.IsEnabled)
            {
                try
                {
                    if (_stm.IsConnected)
                    {
                        _stm.Send(new Core.SendDataMsg(Core.SendDataMsg.HeaderType.JOY, _joypad.GetPad().JoyInfoEx));
                    }
                    else if (_ros.IsConnected)
                    {
                        _ros.Send(new Core.SendDataMsg(Core.SendDataMsg.HeaderType.JOY, _joypad.GetPad().JoyInfoEx));
                    }
                }
                catch
                {
                    //Disconnect();
                    _log.WiteErrorMsg("切断されました");
                    _sendMsgTimer.Dispose();
                    IsConnected = false;
                }
            }
        }
        #endregion
    }
}
