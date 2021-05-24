using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using MVVMLib;

namespace ABU2021_ControlAndDebug.Models
{
    class OutputLog : NotifyPropertyChanged
    {
        private DebugSate _debugSate;


        #region Singleton instance
        private static OutputLog _instance;
        public static OutputLog GetInstance
        {
            get
            {
                return _instance ?? (_instance = new OutputLog());
            }
        }
        private OutputLog() 
        {
            _debugSate = DebugSate.GetInstance;
        }
        #endregion


        #region Property
        private string _text;

        public string Text
        {
            get => _text;
            set { SetProperty(ref _text, value); }
        }
        #endregion


        public void WiteLine(string s)
        {
            Text += s + '\n';
        }
        public void Write(string s)
        {
            Text += s;
        }
        public void WiteDebugMsg(string s)
        {
            if (!_debugSate.IsOutputMsg) return;
            Text += "[Debug] : " + DateTime.Now.ToString("HH:mm:ss:fff") + " " + s + '\n';
            //Text += "[Debug] : " + s + '\n';
        }
        public void WiteErrorMsg(string s)
        {
            Text += "[Error] : " + DateTime.Now.ToString("HH:mm:ss:fff") + " " + s + '\n';
        }
    }
}
