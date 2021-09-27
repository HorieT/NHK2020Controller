using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MVVMLib;

namespace ABU2021_ControlAndDebug.ViewModels
{

    /* Viewと癒着しまくりのｳﾝﾁ */
    class MainWindow : ViewModel
    {
        private static readonly TimeSpan InputTimeSpan = new TimeSpan((long)(1.5e7));//100ns単位
        private Stopwatch _keyInputStopwatch = new Stopwatch();
        private int _stopwatch_count = 0;
        private Timer _checkStopwatchTimer;


        #region Model
        public Models.OutputLog Log { get; private set; }
        public Models.MapProperty MapProperty { get; private set; }
        public Models.ControlTR TR { get; private set; }
        public Models.ControlDR DR { get; private set; }
        #endregion


        public MainWindow()
        {
            #region get instance
            Log = Models.OutputLog.GetInstance;
            MapProperty = Models.MapProperty.GetInstance;
            TR = Models.ControlTR.GetInstance;
            DR = Models.ControlDR.GetInstance;
            #endregion

            MapProperty.PropertyChanged += MapProperty_PropertyChanged;
            TR.PropertyChanged += TR_PropertyChanged;
            DR.PropertyChanged += DR_PropertyChanged;

            _checkStopwatchTimer = new Timer(_ => { StopwatchTime = _keyInputStopwatch.Elapsed.TotalMilliseconds; }, null, 0, 10);
            //StopwatchTime = InputTimeSpanMs;
        }

        #region Property
        private double _stopwatchTimeMs;

        public double StopwatchTime
        {
            get => _stopwatchTimeMs;
            set => SetProperty(ref _stopwatchTimeMs, value);
        }
        public double InputTimeSpanMs { get => InputTimeSpan.TotalMilliseconds; }
        #endregion

        #region Command
        private ICommand _ctrl0;
        private ICommand _ctrl1;
        private ICommand _ctrl2;
        private ICommand _ctrl3;
        private ICommand _ctrl4;
        private ICommand _previewKeyDown;

        public ICommand Ctrl0{
            get
            {
                return _ctrl0 ?? 
                    (_ctrl0 = CreateCommand(
                        (object sender) => {
                            AddInjectQueue(Core.ControlType.Pot._1Right, true);
                        }));
            }
        }
        public ICommand Ctrl1
        {
            get
            {
                return _ctrl1 ??
                    (_ctrl1 = CreateCommand(
                        (object sender) => {
                            AddInjectQueue(Core.ControlType.Pot._1Left, true);
                        }));
            }
        }
        public ICommand Ctrl2
        {
            get
            {
                return _ctrl2 ??
                    (_ctrl2 = CreateCommand(
                        (object sender) => {
                            AddInjectQueue(Core.ControlType.Pot._2Front, true);
                        }));
            }
        }
        public ICommand Ctrl3
        {
            get
            {
                return _ctrl3 ??
                    (_ctrl3 = CreateCommand(
                        (object sender) => {
                            AddInjectQueue(Core.ControlType.Pot._2Back, true);
                        }));
            }
        }
        public ICommand Ctrl4
        {
            get
            {
                return _ctrl4 ??
                    (_ctrl4 = CreateCommand(
                        (object sender) => {
                            AddInjectQueue(Core.ControlType.Pot._3, true);
                        }));
            }
        }
        public ICommand PreviewKeyDown
        {
            get
            {
                return _previewKeyDown ??
                    (_previewKeyDown = CreateCommand(
                        (KeyEventArgs e) => {
                            bool checkStopwatch = (Keyboard.GetKeyStates(Key.OemPeriod) & KeyStates.Down) == KeyStates.Down;
                            if (!checkStopwatch && ((Keyboard.GetKeyStates(Key.LeftCtrl) | Keyboard.GetKeyStates(Key.RightCtrl)) & KeyStates.Down) != KeyStates.Down) return;

                            Core.ControlType.Pot pot;
                            switch (e.Key)
                            {
                                case Key.D0:
                                case Key.Oem3:
                                case Key.DbeSbcsChar:
                                case Key.OemEnlw:
                                    pot = Core.ControlType.Pot._1Right;
                                    break;
                                case Key.D1:
                                    pot = Core.ControlType.Pot._1Left;
                                    break;
                                case Key.D2:
                                    pot = Core.ControlType.Pot._2Front;
                                    break;
                                case Key.D3:
                                    pot = Core.ControlType.Pot._2Back;
                                    break;
                                case Key.D4:
                                    pot = Core.ControlType.Pot._3;
                                    break;
                                default:
                                    return;
                            }

                            AddInjectQueue(pot, checkStopwatch);
                            e.Handled = true;
                        }));
            }
        }
        #endregion


        #region Method
        private bool AddInjectQueue(Core.ControlType.Pot pot, bool is_inc = false)
        {
            if (!TR.IsEnabaled) return false;
            if (is_inc)
            {
                if (!_keyInputStopwatch.IsRunning) _keyInputStopwatch.Start();
                else
                {
                    if (_keyInputStopwatch.ElapsedMilliseconds < InputTimeSpan.TotalMilliseconds) ++_stopwatch_count;
                    else _stopwatch_count = 0;
                    _keyInputStopwatch.Restart();
                }
            }
            else
            {
                _keyInputStopwatch.Stop();
                _stopwatch_count = 0;
            }

            TR.AddInjectQueue(pot, _stopwatch_count);
            return true;
        }

        private void MapProperty_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }
        private void TR_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }
        private void DR_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }
        #endregion
    }
}
