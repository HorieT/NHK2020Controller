using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using MVVMLib;
using System.Windows;

namespace ABU2021_ControlAndDebug.ViewModels
{
    class MenuBar : ViewModel 
    {
        #region Model
        public Models.OutputLog Log { get; private set; }
        public Models.Communicator Communicator{ get; private set; }
        public Models.JoypadHandl Joypad { get; private set; }
        public Models.DebugSate DebugSate { get; private set; }
        #endregion


        #region Property
        private bool _isEnableDissconnect = false;
        private bool _isEnableConnect = true;
        private bool _isCheckedStmUsb = false;
        private bool _isCheckedRosWifi = false;
        private bool _isCheckedJoypad = false;
        private string _connectedText = "接続デバイス無し";
        private string _connectedJoypadText = "ジョイパッド無し";
        private string _wifiSSID = "Wifi : None";

        public bool IsEnableDissconnect
        {
            get => _isEnableDissconnect;
            private set { SetProperty(ref _isEnableDissconnect, value); }
        }
        public bool IsEnableConnect
        {
            get => _isEnableConnect;
            private set { SetProperty(ref _isEnableConnect, value); }
        }
        public bool IsCheckedStmUsb
        {
            get => _isCheckedStmUsb;
            set { SetProperty(ref _isCheckedStmUsb, value); }
        }
        public bool IsCheckedRosWifi
        {
            get => _isCheckedRosWifi;
            set { SetProperty(ref _isCheckedRosWifi, value); }
        }
        public bool IsCheckedJoypad
        {
            get => _isCheckedJoypad;
            set { SetProperty(ref _isCheckedJoypad, value); }
        }
        public string ConnectedText
        {
            get => _connectedText;
            set { SetProperty(ref _connectedText, value); }
        }
        public string ConnectedJoypadText
        {
            get => _connectedJoypadText;
            set { SetProperty(ref _connectedJoypadText, value); }
        }
        public string WifiSSID
        {
            get => _wifiSSID;
            set { SetProperty(ref _wifiSSID, value); }
        }
        #endregion


        #region Command
        private ICommand _file_Click;
        private ICommand _shutdown_Click;
        private ICommand _minimize_Click;
        private ICommand _connectTrRosWifi_Click;
        private ICommand _connectDrRosWifi_Click;
        private ICommand _connectStmUsb_Click;
        private ICommand _disconnect_Click;
        private ICommand _activeJoypad_Click;
        private ICommand _viewJoypad_Click;

        public ICommand File_Click
        {
            get
            {
                return _file_Click ??
                    (_file_Click = CreateCommand(
                        (object sender) =>
                        {

                        }));
            }
        }
        public ICommand Shutdown_Click
        {
            get
            {
                return _shutdown_Click ??
                    (_shutdown_Click = CreateCommand(
                        (object sender) =>
                        {
                            Application.Current.Shutdown();
                        }));
            }
        }
        public ICommand Minimize_Click
        {
            get
            {
                return _minimize_Click ??
                    (_minimize_Click = CreateCommand(
                        (object sender) =>
                        {
                            Application.Current.MainWindow.WindowState = WindowState.Minimized;
                        }));
            }
        }
        public ICommand ConnectTrRosWifi_Click
        {
            get
            {
                return _connectTrRosWifi_Click ??
                    (_connectTrRosWifi_Click = CreateCommand(
                        (object sender) =>
                        {
                            Log.WiteLine("ROS(Wifi)接続開始...");
                            IsEnableConnect = false;
                            Task.Run(async () =>
                            {
                                try
                                {
                                    await Communicator.ConnectRosAsync(Core.ControlType.TcpPort.TR);
                                }
                                catch
                                {
                                    Log.WiteLine("接続失敗");
                                    IsEnableConnect = true;
                                    IsCheckedRosWifi = false;
                                    return;
                                }
                                Log.WiteLine("接続成功");
                                IsCheckedRosWifi = true;
                            });
                        }));
            }
        }
        public ICommand ConnectDrRosWifi_Click
        {
            get
            {
                return _connectDrRosWifi_Click ??
                    (_connectDrRosWifi_Click = CreateCommand(
                        (object sender) =>
                        {
                            Log.WiteLine("ROS(Wifi)接続開始...");
                            IsEnableConnect = false;
                            Task.Run(async () =>
                            {
                                try
                                {
                                    await Communicator.ConnectRosAsync(Core.ControlType.TcpPort.DR);
                                }
                                catch
                                {
                                    Log.WiteLine("接続失敗");
                                    IsEnableConnect = true;
                                    IsCheckedRosWifi = false;
                                    return;
                                }
                                Log.WiteLine("接続成功");
                                IsCheckedRosWifi = true;
                            });
                        }));
            }
        }
        public ICommand ConnectStmUsb_Click
        {
            get
            {
                return _connectStmUsb_Click ??
                    (_connectStmUsb_Click = CreateCommand(
                        (object sender) =>
                        {
                            Log.WiteLine("STM(USB)接続開始...");
                            Task.Run(async () =>
                            {
                                try
                                {
                                    await Communicator.ConnectStmAsync();
                                }
                                catch
                                {
                                    Log.WiteLine("接続失敗");
                                    IsEnableConnect = true;
                                    IsCheckedStmUsb = false;
                                    return;
                                }
                                Log.WiteLine("接続成功");
                                IsCheckedStmUsb = true;
                            });
                        }));
            }
        }
        public ICommand Disconnect_Click
        {
            get
            {
                return _disconnect_Click ??
                    (_disconnect_Click = CreateCommand(
                        (object sender) =>
                        {
                            if (!IsEnableConnect)
                            {
                                Communicator.Disconnect();
                                IsCheckedStmUsb = false;
                                IsCheckedRosWifi = false;
                                Log.WiteLine("切断");
                            }
                        }));
            }
        }
        public ICommand ActiveJoypad_Click
        {
            get
            {
                //現状ジョイパッドには切断続の概念が無い
                return _activeJoypad_Click ??
                    (_activeJoypad_Click = CreateCommand(
                        (object sender) =>
                        {
                            if (Joypad.IsExisted)
                            {
                                Joypad.IsEnabled = IsCheckedJoypad;
                            }
                            else
                            {
                                IsCheckedJoypad = false;
                            }
                        }));
            }
        }
        public ICommand ViewJoypad_Click
        {
            get
            {

                return _viewJoypad_Click ??
                  (_viewJoypad_Click = CreateCommand(
                      (object sender) =>
                      {
                          var main = sender as Window;
                          if (main == null) return;
                          var win = new SubWindows.JoypadTestWindow();
                          win.Owner = main;
                          win.ShowDialog();
                      }));
            }
        }
        #endregion


        public MenuBar()
        {
            Log = Models.OutputLog.GetInstance;
            Communicator = Models.Communicator.GetInstance;
            Joypad = Models.JoypadHandl.GetInstance;
            DebugSate = Models.DebugSate.GetInstance;

            Communicator.PropertyChanged += Communicator_PropertyChanged;
            Joypad.PropertyChanged += Joypad_PropertyChanged;
            ConnectedJoypadText = !Joypad.IsExisted ?
                "ジョイパッド無し" :
                "ジョイパッドあり" + (Joypad.IsEnabled ? "(有効)" : "(無効)");
            IsCheckedJoypad = Joypad.IsEnabled;
        }




        #region Method
        private void Communicator_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Communicator.IsConnected))
            {
                IsEnableDissconnect = Communicator.IsConnected;
                IsEnableConnect = !IsEnableDissconnect;

                if (!Communicator.IsConnected)
                {
                    IsCheckedStmUsb = false;
                    IsCheckedRosWifi = false;
                }
                ConnectedText = "接続:" + Communicator.ConnectedDviseName;
            }
            else if(e.PropertyName == nameof(Communicator.NetworkName))
            {
                WifiSSID = "Wifi : " + Communicator.NetworkName;
            }
        }
        private void Joypad_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Joypad.IsExisted) || e.PropertyName == nameof(Joypad.IsEnabled))//切断続or有効無効
            {
                if(e.PropertyName == nameof(Joypad.IsEnabled) || !Joypad.IsExisted) IsCheckedJoypad = Joypad.IsEnabled;
                ConnectedJoypadText = !Joypad.IsExisted ? 
                    "ジョイパッド無し" : 
                    "ジョイパッドあり" + (Joypad.IsEnabled ? "(有効)" : "(無効)");
            }
        }
        #endregion
    }
}
