using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JoypadControl;
using MVVMLib;

namespace ABU2021_ControlAndDebug.Models
{
    /// <summary>
    /// joypadの操作モデル
    /// padナンバーは0選択固定
    /// GPD(Xboxコン)とDualshockのキー配置は全く違う　うんち！！！！！！！！！！
    /// 当面はGPDのみで考える
    /// 
    ///  JoypadControlがY回転を読んでくれないorz
    /// </summary>
    class JoypadHandl : NotifyPropertyChanged
    {
        private static readonly uint PAD_NUM = 0;
        private OutputLog _log;
        private Xbox360_JoyPad _pad;
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
            _pad = new Xbox360_JoyPad();
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
                uint[] pads = _pad.GetJoypads();

                IsExisted = (pads.Count(i => i == PAD_NUM) == 1);
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
                    SetProperty(ref _isEnabled, value); 
            }
        }
        public bool IsDebug
        {
            get => _isDebug;
            set { SetProperty(ref _isDebug, value); }
        }
        #endregion


        public Xbox360_JoyPad GetPad()
        {
            if(IsExisted == false)
            {
                if (IsExisted == false) _log.WiteErrorMsg("ジョイパッドがありません");
                throw new InvalidOperationException("Joypad not found");
            }

            Joypad.JOYERR error = _pad.GetPosEx(PAD_NUM);
            Joypad.JOYINFOEX ex = _pad.JoyInfoEx;

            if(error != Joypad.JOYERR.NOERROR)
            {
                throw new Exception("Joypad connection error : " + error.ToString());
            }

            return _pad;
        }


        private void CheckPad(object sender)
        {
            if (_isExisted && IsDebug)
            {
                _log.WiteDebugMsg(" Button:" + GetPad().JoyInfoEx.dwButtons.ToString("X8"));
                _log.WiteDebugMsg(" X pos:" + GetPad().JoyInfoEx.dwXpos.ToString());
                _log.WiteDebugMsg(" Y pos:" + GetPad().JoyInfoEx.dwYpos.ToString());
                _log.WiteDebugMsg(" Z pos:" + GetPad().JoyInfoEx.dwZpos.ToString());
                _log.WiteDebugMsg(" X rot:" + GetPad().JoyInfoEx.dwXrot.ToString());
                _log.WiteDebugMsg(" Y rot:" + GetPad().JoyInfoEx.dwYrot.ToString());
                _log.WiteDebugMsg(" Z rot:" + GetPad().JoyInfoEx.dwZrot.ToString());
                _log.WiteDebugMsg(" Flag:" + GetPad().JoyInfoEx.dwFlags.ToString());
                _log.WiteDebugMsg(" Reserrved1:" + GetPad().JoyInfoEx.dwReserved1.ToString());
                _log.WiteDebugMsg(" Reserrved2:" + GetPad().JoyInfoEx.dwReserved2.ToString());
                _log.WiteLine("");
            }
        }
    }
}
