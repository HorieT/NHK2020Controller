using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;
using MVVMLib;

namespace ABU2021_ControlAndDebug.ViewModels
{
    class MenuBar : ViewModel 
    {
        #region Model
        public Models.OutputLog Log { get; private set; }
        public Models.Communicator Communicator{ get; private set; }
        public Models.JoypadHandl Joypad { get; private set; }
        #endregion


        #region Property
        private bool _isEnableDissconnect = false;
        private bool _isEnableConnect = true;
        private bool _isCheckedSTM_USB = false;
        private bool _isCheckedROS_Wifi = false;
        private bool _isCheckedJoypad = false;
        private string _connectedText = "接続デバイス無し";
        private string _connectedJoypadText = "ジョイパッド無し";

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
        public bool IsCheckedSTM_USB
        {
            get => _isCheckedSTM_USB;
            set { SetProperty(ref _isCheckedSTM_USB, value); }
        }
        public bool IsCheckedROS_Wifi
        {
            get => _isCheckedROS_Wifi;
            set { SetProperty(ref _isCheckedROS_Wifi, value); }
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
        #endregion


        #region Command
        private ICommand _fileClick;
        private ICommand _connectTR_ROS_WifiClick;
        private ICommand _connectDR_ROS_WifiClick;
        private ICommand _connectSTM_USBClick;
        private ICommand _discinnectClick;
        private ICommand _activeJoypadClick;

        public ICommand FileClick
        {
            get
            {
                return _fileClick ??
                    (_fileClick = CreateCommand(
                        (object sender) =>
                        {

                        }));
            }
        }
        public ICommand ConnectTR_ROS_WifiClick
        {
            get
            {
                return _connectTR_ROS_WifiClick ??
                    (_connectTR_ROS_WifiClick = CreateCommand(
                        (object sender) =>
                        {
                            Log.WiteLine("ROS(Wifi)接続開始...");
                            IsEnableConnect = false;
                            Task.Run(async () =>
                            {
                                try
                                {
                                    await Communicator.ConnectROS(Core.ControlType.TcpPort.TR);
                                }
                                catch
                                {
                                    Log.WiteLine("接続失敗");
                                    IsEnableConnect = true;
                                    IsCheckedROS_Wifi = false;
                                    return;
                                }
                                Log.WiteLine("接続成功");
                                IsCheckedROS_Wifi = true;
                            });
                        }));
            }
        }
        public ICommand ConnectDR_ROS_WifiClick
        {
            get
            {
                return _connectDR_ROS_WifiClick ??
                    (_connectDR_ROS_WifiClick = CreateCommand(
                        (object sender) =>
                        {
                            Log.WiteLine("ROS(Wifi)接続開始...");
                            IsEnableConnect = false;
                            Task.Run(async () =>
                            {
                                try
                                {
                                    await Communicator.ConnectROS(Core.ControlType.TcpPort.DR);
                                }
                                catch
                                {
                                    Log.WiteLine("接続失敗");
                                    IsEnableConnect = true;
                                    IsCheckedROS_Wifi = false;
                                    return;
                                }
                                Log.WiteLine("接続成功");
                                IsCheckedROS_Wifi = true;
                            });
                        }));
            }
        }
        public ICommand ConnectSTM_USBClick
        {
            get
            {
                return _connectSTM_USBClick ??
                    (_connectSTM_USBClick = CreateCommand(
                        (object sender) =>
                        {
                            Log.WiteLine("STM(USB)接続開始...");
                            try
                            {
                                Communicator.ConnectSTM();
                            }
                            catch
                            {
                                Log.WiteLine("接続失敗");
                                IsCheckedSTM_USB = false;
                                return;
                            }
                            Log.WiteLine("接続成功");
                            IsCheckedSTM_USB = true;
                        }));
            }
        }
        public ICommand DiscinnectClick
        {
            get
            {
                return _discinnectClick ??
                    (_discinnectClick = CreateCommand(
                        (object sender) =>
                        {
                            if (!IsEnableConnect)
                            {
                                Communicator.Disconnect();
                                IsCheckedSTM_USB = false;
                                IsCheckedROS_Wifi = false;
                                Log.WiteLine("切断");
                            }
                        }));
            }
        }
        public ICommand ActiveJoypadClick
        {
            get
            {
                //現状ジョイパッドには切断続の概念が無い
                return _activeJoypadClick ??
                    (_activeJoypadClick = CreateCommand(
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
        #endregion


        public MenuBar()
        {
            Log = Models.OutputLog.GetInstance;
            Communicator = Models.Communicator.GetInstance;
            Joypad = Models.JoypadHandl.GetInstance;

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
                    IsCheckedSTM_USB = false;
                    IsCheckedROS_Wifi = false;
                }
                ConnectedText = "接続:" + Communicator.ConnectedDviseName;
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
