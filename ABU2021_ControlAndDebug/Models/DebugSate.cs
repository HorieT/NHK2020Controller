using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVVMLib;

namespace ABU2021_ControlAndDebug.Models
{
    /// <summary>
    /// この"アプリ"のデバックステータス
    /// </summary>
    class DebugSate : NotifyPropertyChanged
    {
        #region Singleton instance
        private static DebugSate _instance;
        public static DebugSate GetInstance
        {
            get
            {
                return _instance ?? (_instance = new DebugSate());
            }
        }
        private DebugSate()
        {
        }
        #endregion




        #region Property
        private bool _isOutputMsg;
        private bool _isCommunicatSafe;
        private bool _isUnlockUI;

        public bool IsOutputMsg
        {
            get => _isOutputMsg;
            set { SetProperty(ref _isOutputMsg, value); }
        }
        public bool IsCommunicatSafe
        {
            get => _isCommunicatSafe;
            set { SetProperty(ref _isCommunicatSafe, value); }
        }
        public bool IsUnlockUI
        {
            get => _isUnlockUI;
            set { SetProperty(ref _isUnlockUI, value); }
        }
        #endregion
    }
}
