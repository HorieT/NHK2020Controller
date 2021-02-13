using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVVMLib;

namespace ABU2021_ControlAndDebug.Models
{
    class ControlDR : NotifyPropertyChanged
    {
        private OutputLog _log;

        #region Singleton instance
        private static ControlDR _instance;
        public static ControlDR GetInstance
        {
            get
            {
                return _instance ?? (_instance = new ControlDR());
            }
        }
        private ControlDR()
        {
            _log = OutputLog.GetInstance;
        }
        #endregion
    }
}
