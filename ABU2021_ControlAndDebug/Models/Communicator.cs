using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MVVMLib;
using ManagedNativeWifi;
using SharpDX.DirectInput;

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
        private Config _config;
        private Timer _sendMsgTimer;
        private object _sendMsgLock = new object();
        private System.Threading.SemaphoreSlim _SendMsgSemaphore = new System.Threading.SemaphoreSlim(1, 1);
        private Core.ComDevice _comDevice;
        private Core.SendDataMsg _sendMsgJoy = new Core.SendDataMsg(Core.SendDataMsg.HeaderType.JOY, new JoystickState());
        private SynchronizationContext _mainContext;
        private Timer _checkNetworkTimer;
        private static readonly int checkNetworkPeriod = 2000;

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
            _config = Config.GetInstance;
            _mainContext = SynchronizationContext.Current;
            _checkNetworkTimer = new Timer(
                _ => {
                    try
                        {
                        
                        var active = ManagedNativeWifi.NativeWifi.EnumerateConnectedNetworkSsids();
                        if (active.Count() != 0)
                        {
                            NetworkName =
                                active.Aggregate("", (name, net) => name + net.ToString() + ",");
                        }
                        else
                        {
                            NetworkName = "None";
                        }
                    }
                    catch
                    {
                        NetworkName = "None";
                    }
                }, 
                null, 0, checkNetworkPeriod);
        }
        ~Communicator()
        {
            if (IsConnected) Disconnect();
        }
        #endregion



        #region Property
        private Core.ControlType.Device _machine = Core.ControlType.Device.Etc;
        private int _sendMsgPeriodMs = 50;
        private string _networkName;

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
        public string NetworkName
        {
            get => _networkName;
            private set { SetProperty(ref _networkName, value); }
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
            ros.IP = _config.IP;
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

            Trace.WriteLine("Try disconnect Communicator.");
            try
            {
                _sendMsgTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                _sendMsgTimer.Dispose();
            }
            catch(ObjectDisposedException)
            {
                Trace.WriteLine("Already diposed \"_sendMsgTimer\".");
            }

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
                _log.WiteErrorMsg("切断されました(sm)");
                _mainContext.Post(_ => Disconnect(), null);
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
                _log.WiteErrorMsg("切断されました(rm)");
                _mainContext.Post(_ => Disconnect(), null);
                throw;
            }
        }


        /// <summary>
        /// 接続デバイス、方式に関わらず周期的に行う送信
        /// </summary>
        /// <param name="sender"></param>
        private void PeriodicSendMsg(object sender)
        {
            lock (_sendMsgLock)
            {
                if (!(_comDevice?.IsConnected ?? false))
                {
                    _sendMsgTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    _mainContext.Post(_ => Disconnect(), null);
                    _log.WiteErrorMsg("切断されました(pm)");
                    return;
                }

                if (_joypad.IsEnabled)
                {
                    Task.Run(async () => {
                        var data = new Core.SendDataMsg(Core.SendDataMsg.HeaderType.JOY, _joypad.GetPad());
                        try
                        {
                            await _comDevice?.SendMsgAsync(data); 
                            _log.WiteDebugMsg(data.ConvString());
                        }
                        catch
                        {
                            _log.WiteDebugMsg("送信失敗");
                            _mainContext.Post(_ => Disconnect(), null);
                            _log.WiteErrorMsg("切断されました");
                        }
                    });
                }
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
