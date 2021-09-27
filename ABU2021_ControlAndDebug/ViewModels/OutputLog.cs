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
        public Models.DebugSate DebugSate { get; private set; }
        #endregion


        #region Command
        private ICommand _log_TextChanged;
        private ICommand _test_KeyDown;

        public ICommand Log_TextChanged
        {
            get
            {
                return _log_TextChanged ??
                    (_log_TextChanged = CreateCommand(
                        (TextChangedEventArgs e) =>
                        {
                            var box = e.Source as TextBox;
                            if (box == null) return;
                            box.ScrollToEnd();
                        }));
            }
        }
        public ICommand Test_KeyDown
        {
            get => _test_KeyDown ??
                (_test_KeyDown = CreateCommand(
                    (KeyEventArgs e) =>
                    {
                        if (DebugSate.IsUnlockUI)
                        {
                            Log.WiteLine(e.Key.ToString());
                        }
                    }));
        }
        #endregion

        #region Method
        public OutputLog()
        {
            Log = Models.OutputLog.GetInstance;
            DebugSate = Models.DebugSate.GetInstance;
        }
        #endregion
    }
}
