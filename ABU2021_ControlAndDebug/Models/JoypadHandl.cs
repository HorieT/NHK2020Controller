using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MVVMLib;
using SharpDX.DirectInput;

namespace ABU2021_ControlAndDebug.Models
{
    /// <summary>
    /// joypadの操作モデル
    /// padナンバーは0選択固定
    /// GPD(Xboxコン)とDualshockのキー配置は全く違う　うんち！！！！！！！！！！
    /// 当面はGPDのみで考える
    /// </summary>
    class JoypadHandl : NotifyPropertyChanged
    {
        //private static readonly uint PAD_NUM = 0;
        private OutputLog _log;
        private Core.JoyPad _pad;
        private Timer _checkPad;


        #region Singleton instance
        private static JoypadHandl _instance;
        public static JoypadHandl GetInstance
        {
            get
            {
                return _instance ?? (_instance = new JoypadHandl());
            }
        }
        private JoypadHandl()
        {
            _log = OutputLog.GetInstance;
            _pad = new Core.JoyPad();
            if (IsExisted == false) _log.WiteErrorMsg("ジョイパッドがありません");
            else IsEnabled = true;
            _checkPad = new Timer(CheckPad, null, 0, 5000);//周期的にパッドの接続を確認
        }
        #endregion


        #region Property
        private bool _isExisted;
        private bool _isEnabled;
        private bool _isDebug;

        public bool IsExisted
        {
            get 
            {
                if (_pad.IsEnabled) IsExisted = true;
                else
                {
                    var pads = _pad.GetDevices();
                    IsExisted = (pads.Count() > 0);
                }
                return _isExisted;
            }
            private set 
            { 
               if(SetProperty(ref _isExisted, value))
                {
                    if (!_isExisted) IsEnabled = false;
                } 
            }
        }
        public bool IsEnabled
        {
            get => _isEnabled;
            set 
            { 
                if(IsExisted || !value)
                {
                    if (value) _pad.SetDevice(_pad.GetDevices()[0], IntPtr.Zero);
                    SetProperty(ref _isEnabled, value);
                }
            }
        }
        public bool IsDebug
        {
            get => _isDebug;
            set { SetProperty(ref _isDebug, value); }
        }
        #endregion


        public JoystickState GetPad()
        {
            if(!_pad.IsEnabled)
            {
                _log.WiteErrorMsg("ジョイパッドがありません");
                throw new InvalidOperationException("Joypad not found");
            }

            try
            {
                return _pad.GetJoy();
            }
            catch
            {
                IsEnabled = false;
                throw;
            }
        }


        private void CheckPad(object sender)
        {
            if (_isExisted && IsDebug)
            {
                try
                {
                    var pad = GetPad();
                    BitArray bit = new BitArray(pad.Buttons);
                    int[] button = new int[bit.Count / 32];
                    bit.CopyTo(button, 0);

                    _log.WiteDebugMsg(" Button:" + button[0].ToString("X8"));
                    _log.WiteDebugMsg(" X pos:" + pad.X.ToString());
                    _log.WiteDebugMsg(" Y pos:" + pad.Y.ToString());
                    _log.WiteDebugMsg(" Z pos:" + pad.Z.ToString());
                    _log.WiteDebugMsg(" X rot:" + pad.RotationX.ToString());
                    _log.WiteDebugMsg(" Y rot:" + pad.RotationY.ToString());
                    _log.WiteDebugMsg(" Z rot:" + pad.RotationZ.ToString());
                    _log.WiteDebugMsg(" POV:" + pad.PointOfViewControllers.ToString());
                    _log.WiteLine("");
                }
                catch(Exception ex)
                {
                    IsEnabled = false;
                    _log.WiteErrorMsg("ジョイパッドをロストしました");
                    Trace.WriteLine("Joypad lost. -> " + ex.ToString() + " : " + ex.Message);
                }
            }
        }
    }
}
