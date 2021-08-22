using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Text.RegularExpressions;
using MVVMLib;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;

namespace ABU2021_ControlAndDebug.ViewModels
{
    class ControlTabTR : ViewModel
    {
        public static readonly double OffsetDegMax = 2.0;
        public static readonly double OffsetDegMin = -2.0;
        public static readonly int OffsetSliderDivLarge = 40;
        public static readonly double OffsetButtonDivSmall = 0.05;
        public static readonly double OffsetButtonDivLarge = 0.5;

        private static readonly double _injectAngle = 45.00;
        private double _injectSpeed = 1.00;
        private bool _isOffsetRadSending = false;


        #region Model
        public Models.ControlTR TR { get; private set; }
        private Models.OutputLog _log;
        private Models.DebugSate _debugSate;
        #endregion

        #region Property
        private bool _isEnabled;
        private string _injectSpeedText = "1.00";
        private string _fallPredictionText;
        private int _injectQueueSelectedIndex = -1;

        private bool _offsetRadIsEq = true;
        private double _offsetRad = 0.000;
        private string _offsetRadText = "0.0000";
        private string _offsetDegText = "0.000";

        public static string OffsetDegMaxText { get; } = OffsetDegMax.ToString() + "°";
        public static string OffsetDegMinText { get; } = OffsetDegMin.ToString() + "°";
        public static double OffsetRadMax { get; } = OffsetDegMax * Math.PI / 180.0;
        public static double OffsetRadMin { get; } = OffsetDegMin * Math.PI / 180.0;
        public static double OffsetRadSliderSmall { get; } = 0.0001;
        public static double OffsetRadSliderLarge { get; } = (OffsetRadMax - OffsetRadMin) / OffsetSliderDivLarge;

        public bool IsEnabled
        {
            get => _isEnabled;
            private set { SetProperty(ref _isEnabled, value); }
        }
        public string InjectSpeedText
        {
            get => _injectSpeedText;
            set { SetProperty(ref _injectSpeedText, value); }
        }
        public string FallPredictionText
        {
            get => _fallPredictionText;
            set { SetProperty(ref _fallPredictionText, value); }
        }
        public int InjectQueueSelectedIndex
        {
            get => _injectQueueSelectedIndex;
            set { SetProperty(ref _injectQueueSelectedIndex, value); }
        }


        public bool OffsetRadIsEq
        {
            get => _offsetRadIsEq;
            private set { SetProperty(ref _offsetRadIsEq, value); }
        }
        public double OffsetRad
        {
            get => _offsetRad;
            set
            {
                if(Math.Abs(value - _offsetRad) >= OffsetRadSliderSmall)
                {
                    _offsetRad = value;
                    _offsetRadText = value.ToString("F4");
                    _offsetDegText = (value != 0.0 ? value * 180.0 / Math.PI : 0.0).ToString("F3");
                    RaisePropertyChanged(nameof(OffsetRad));
                    RaisePropertyChanged(nameof(OffsetRadText));
                    RaisePropertyChanged(nameof(OffsetDegText));
                    OffsetRadIsEq = false;
                    _isOffsetRadSending = true;
                    TR.SetRadOffset(value);
                }
            }
        }
        public string OffsetDegText
        {
            get => _offsetDegText;
            set { SetProperty(ref _offsetDegText, value); }
        }
        public string OffsetRadText
        {
            get => _offsetRadText;
            set { SetProperty(ref _offsetRadText, value); }
        }
        #endregion



        #region Command
        private ICommand _noPasteTextBox_PreviewExecuted;
        private ICommand _inputDataTextBox_PreviewKeyDown;
        private ICommand _decimalTextBox_PreviewTextInput;
        private ICommand _injectTextBox_LostFocus;
        private ICommand _offsetRadBox_LostFocus;
        private ICommand _offsetDegBox_LostFocus;
        private ICommand _offsetDegIncLButton_Click;
        private ICommand _offsetDegIncSButton_Click;
        private ICommand _offsetDegDecLButton_Click;
        private ICommand _offsetDegDecSButton_Click;

        private ICommand _injectButton_Click;
        private ICommand _injectQueue_SelectedCellChanged;
        private ICommand _arrowNumOnMachineButton_Click;
        private ICommand _arrowNumOnRackButton_Click;
        public ICommand NoPasteTextBox_PreviewExecuted
        {
            get
            {
                return _noPasteTextBox_PreviewExecuted ??
                    (_noPasteTextBox_PreviewExecuted = CreateCommand(
                        (ExecutedRoutedEventArgs e) => {
                            // 貼り付けを許可しない
                            if (e.Command == ApplicationCommands.Paste)
                                e.Handled = true;
                        }));
            }
        }
        public ICommand InputDataTextBox_PreviewKeyDown
        {
            get
            {
                return _inputDataTextBox_PreviewKeyDown ??
                    (_inputDataTextBox_PreviewKeyDown = CreateCommand(
                        (KeyEventArgs e) => {
                            // Enterキーでフォーカスを移動
                            if (e.Key == Key.Enter)
                            {
                                DependencyObject ancestor = ((TextBox)e.Source).Parent;


                                while (ancestor != null)
                                {
                                    int count = VisualTreeHelper.GetChildrenCount(ancestor);
                                    for (int i = 0; i < count; ++i)
                                    {
                                        // フォーカスできるか
                                        if (VisualTreeHelper.GetChild(ancestor, i) is UIElement element && element.Focusable && element != e.Source)
                                        {
                                            element.Focus(); // フォーカスを当てる
                                            return;
                                        }
                                    }
                                    ancestor = VisualTreeHelper.GetParent(ancestor);
                                }
                            }
                        }));
            }
        }
        public ICommand DecimalTextBox_PreviewTextInput
        {
            get
            {
                return _decimalTextBox_PreviewTextInput ??
                    (_decimalTextBox_PreviewTextInput = CreateCommand(
                        (TextCompositionEventArgs e) => {
                            var box = e.Source as TextBox;
                            if (box == null) return;
                            //入力文字が数値又は"-"又は"."ならfalse
                            e.Handled = new Regex(@"[^0-9.-]").IsMatch(e.Text);
                        }));
            }
        }
        public ICommand InjectTextBox_LostFocus
        {
            get
            {
                return _injectTextBox_LostFocus ??
                    (_injectTextBox_LostFocus = CreateCommand(
                        (RoutedEventArgs e) => {
                            double speed;
                            try
                            {
                                speed = double.Parse(InjectSpeedText);
                            }
                            catch
                            {
                                _log.WiteLine("入力が数値ではありません");
                                InjectSpeedText = _injectSpeed.ToString("F2");
                                return;
                            }
                            if(speed > 0.0 && speed < Models.ControlTR.INJECT_SPEED_MAX)
                            {
                                _injectSpeed = speed;
                                FallPredictionText = TR.GetArrowFallPos(0, _injectSpeed, _injectAngle).ToString("F2");
                            }
                            else
                            {
                                _log.WiteLine("入力が範囲外です");
                                InjectSpeedText = _injectSpeed.ToString("F2");
                            }
                        }));
            }
        }
        public ICommand OffsetRadBox_LostFocus
        {
            get
            {
                return _offsetRadBox_LostFocus ??
                    (_offsetRadBox_LostFocus = CreateCommand(
                        (RoutedEventArgs e) => {
                            double rad;
                            try
                            {
                                rad = double.Parse(OffsetRadText);
                            }
                            catch
                            {
                                _log.WiteLine("入力が数値ではありません");
                                OffsetRadText = (OffsetRad).ToString("F4");
                                return;
                            }

                            if (rad >= OffsetRadMin && rad <= OffsetRadMax)//範囲内
                            {
                                OffsetRad = rad;
                            }
                            else
                            {
                                _log.WiteLine("入力が範囲外です");
                                OffsetRadText = (OffsetRad).ToString("F4");
                            }
                        }));
            }
        }
        public ICommand OffsetDegBox_LostFocus
        {
            get
            {
                return _offsetDegBox_LostFocus ??
                    (_offsetDegBox_LostFocus = CreateCommand(
                        (RoutedEventArgs e) => {
                            double deg;
                            try
                            {
                                deg = double.Parse(OffsetDegText);
                            }
                            catch
                            {
                                _log.WiteLine("入力が数値ではありません");
                                OffsetDegText = (OffsetRad * 180.0 / Math.PI).ToString("F3");
                                return;
                            }

                            if (deg >= OffsetDegMin && deg <= OffsetDegMax)//範囲内
                            {
                                OffsetRad = (deg * Math.PI / 180.0);
                            }
                            else
                            {
                                _log.WiteLine("入力が範囲外です");
                                OffsetDegText = (OffsetRad * 180.0 / Math.PI).ToString("F4");
                            }
                        }));
            }
        }


        public ICommand OffsetDegIncLButton_Click
        {
            get
            {
                return _offsetDegIncLButton_Click ??
                    (_offsetDegIncLButton_Click = CreateCommand(
                        (object sender) =>
                        {
                            var rad = OffsetRad + OffsetButtonDivLarge * Math.PI / 180.0;
                            OffsetRad = (rad < OffsetRadMax) ? rad : OffsetRadMax;
                        }));
            }
        }
        public ICommand OffsetDegIncSButton_Click
        {
            get
            {
                return _offsetDegIncSButton_Click ??
                    (_offsetDegIncSButton_Click = CreateCommand(
                        (object sender) =>
                        {
                            var rad = OffsetRad + OffsetButtonDivSmall * Math.PI / 180.0;
                            OffsetRad = (rad < OffsetRadMax) ? rad : OffsetRadMax;
                        }));
            }
        }

        public ICommand OffsetDegDecLButton_Click
        {
            get
            {
                return _offsetDegDecLButton_Click ??
                    (_offsetDegDecLButton_Click = CreateCommand(
                        (object sender) =>
                        {
                            var rad = OffsetRad - OffsetButtonDivLarge * Math.PI / 180.0;
                            OffsetRad = (rad > OffsetRadMin) ? rad : OffsetRadMin;
                        }));
            }
        }
        public ICommand OffsetDegDecSButton_Click
        {
            get
            {
                return _offsetDegDecSButton_Click ??
                    (_offsetDegDecSButton_Click = CreateCommand(
                        (object sender) =>
                        {
                            var rad = OffsetRad - OffsetButtonDivSmall * Math.PI / 180.0;
                            OffsetRad = (rad > OffsetRadMin) ? rad : OffsetRadMin;
                        }));
            }
        }



        public ICommand InjectButton_Click
        {
            get
            {
                return _injectButton_Click ??
                    (_injectButton_Click = CreateCommand(
                        (object sender) =>
                        {
                            try
                            {
                                TR.Inject(double.Parse(InjectSpeedText));
                            }
                            catch(Exception ex)
                            {
                                _log.WiteErrorMsg("Inject error.");
                                Trace.WriteLine("Inject error. ->" + ex.Message);
                            }
                        }));
            }
        }
        
        public ICommand InjectQueue_SelectedCellChanged
        {
            get
            {
                return _injectQueue_SelectedCellChanged ??
                    (_injectQueue_SelectedCellChanged = CreateCommand(
                        (SelectedCellsChangedEventArgs e) => {
                            if(InjectQueueSelectedIndex > -1)
                            {
                                TR.DeleatInjectQueue(InjectQueueSelectedIndex);
                                _log.WiteLine("キューから削除 : Index " + InjectQueueSelectedIndex.ToString());
                            }
                            InjectQueueSelectedIndex = -1;
                        }));
            }
        }
        public ICommand ArrowNumOnMachineButton_Click
        {
            get
            {
                return _arrowNumOnMachineButton_Click ??
                    (_arrowNumOnMachineButton_Click = CreateCommand(
                        (object sender) =>
                        {
                            var main = sender as Window;
                            if (main == null) return;
                            var win = new SubWindows.SetArrowNumWindow();
                            win.IsMachine = true;
                            win.Owner = main;
                            win.ShowDialog();
                            if (win.ArrowNum >= 0 && win.ArrowNum <= 5) TR.SetArrowNumOnMachine(win.ArrowNum);
                        }));
            }
        }
        public ICommand ArrowNumOnRackButton_Click
        {
            get
            {
                return _arrowNumOnRackButton_Click ??
                    (_arrowNumOnRackButton_Click = CreateCommand(
                        (object sender) =>
                        {
                            var main = sender as Window;
                            if (main == null) return;
                            var win = new SubWindows.SetArrowNumWindow();
                            win.IsMachine = false;
                            win.Owner = main;
                            win.ShowDialog();
                            if (win.ArrowNum >= 0 && win.ArrowNum <= 5) TR.SetArrowNumOnRack(win.ArrowNum);
                        }));
            }
        }



        #endregion


        public ControlTabTR()
        {
            TR = Models.ControlTR.GetInstance;
            _log = Models.OutputLog.GetInstance;
            _debugSate = Models.DebugSate.GetInstance;

            FallPredictionText = TR.GetArrowFallPos(0, _injectSpeed, _injectAngle).ToString("F2");
            TR.PropertyChanged += TR_PropertyChanged;
            _debugSate.PropertyChanged += _debugSate_PropertyChanged;
        }

        private void _debugSate_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_debugSate.IsUnlockUI))
            {
                IsEnabled = _debugSate.IsUnlockUI || TR.IsEnabaled;
            }
        }


        #region Method
        private void TR_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TR.IsEnabaled))
            {
                IsEnabled = _debugSate.IsUnlockUI || TR.IsEnabaled;
                return;
            }

            if(e.PropertyName == nameof(TR.OffsetRad))
            {
                if(Math.Abs(TR.OffsetRad - _offsetRad) >= OffsetRadSliderSmall)//表示値と受信値が異なる
                {
                    if (_isOffsetRadSending)//送信中
                    {
                        OffsetRadIsEq = false;
                        return;
                    }
                    else
                    {
                        _offsetRad = TR.OffsetRad;
                        _offsetRadText = _offsetRad.ToString("F4");
                        _offsetDegText = (_offsetRad != 0.0 ? _offsetRad * 180.0 / Math.PI : 0.0).ToString("F3");
                        RaisePropertyChanged(nameof(OffsetRad));
                        RaisePropertyChanged(nameof(OffsetRadText));
                        RaisePropertyChanged(nameof(OffsetDegText));
                    }
                }
                //表示値有効化
                _isOffsetRadSending = false;
                OffsetRadIsEq = true;
            }
        }
        #endregion
    }
}
