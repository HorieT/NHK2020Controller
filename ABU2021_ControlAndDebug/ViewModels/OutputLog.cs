using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using MVVMLib;

namespace ABU2021_ControlAndDebug.ViewModels
{
    class OutputLog : ViewModel
    {
        #region Models
        public Models.OutputLog Log { get; private set; }
        #endregion


        #region Command
        private ICommand _logChanged;

        public ICommand LogChanged
        {
            get
            {
                return _logChanged ??
                    (_logChanged = CreateCommand(
                        (ExecutedRoutedEventArgs args) =>
                        {
                            Log.WiteDebugMsg(args.Command.ToString());
                        }));
            }
        }
        #endregion

        #region Method
        public OutputLog()
        {
            Log = Models.OutputLog.GetInstance;
        }
        #endregion
    }
}
