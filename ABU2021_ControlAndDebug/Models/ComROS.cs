using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABU2021_ControlAndDebug.Models
{
    class ComROS
    {
        private OutputLog _log;

        #region Singleton instance
        private static ComROS _instance;
        public static ComROS GetInstance
        {
            get
            {
                return _instance ?? (_instance = new ComROS());
            }
        }
        private ComROS()
        {
            _log = OutputLog.GetInstance;
        }
        #endregion
    }
}
