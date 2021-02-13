using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// </summary>
    class JoypadHandl : NotifyPropertyChanged
    {
        private static readonly uint PAD_NUM = 0;
        private OutputLog _log;
        private Xbox360_JoyPad _pad;


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
        }
        #endregion


        #region Property
        private bool _isExisted;
        private bool _isEnabled;

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
    }
}
