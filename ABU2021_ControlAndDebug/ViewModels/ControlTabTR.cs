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
        private double _injectSpeed = 1.00;
        private readonly double _injectAngle = 45.00;


        #region Model
        public Models.ControlTR TR { get; private set; }
        private Models.OutputLog _log;
        private Models.DebugSate _debugSate;
        #endregion

        #region Property
        private bool _isEnabled;
        private string _injectSpeedText = "1.00";
        private string _fallPredictionText;


        public bool IsEnabed
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
        #endregion



        #region Command
        private ICommand _noPasteTextBox_PreviewExecuted;
        private ICommand _inputDataTextBox_PreviewKeyDown;
        private ICommand _injectTextBox_PreviewTextInput;
        private ICommand _injectTextBox_LostFocus;
        private ICommand _injectButton_Click;
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
                                        if (e.Source == VisualTreeHelper.GetChild(ancestor, i))
                                        {
                                            while(i != count - 1)
                                            {
                                                // フォーカスできるか
                                                if (VisualTreeHelper.GetChild(ancestor, ++i) is UIElement element && element.Focusable)
                                                {
                                                    element.Focus(); // フォーカスを当てる
                                                    return;
                                                }
                                            }
                                            break;
                                        }
                                    }
                                    ancestor = VisualTreeHelper.GetParent(ancestor);
                                }
                            }
                        }));
            }
        }
        public ICommand InjectTextBox_PreviewTextInput
        {
            get
            {
                return _injectTextBox_PreviewTextInput ??
                    (_injectTextBox_PreviewTextInput = CreateCommand(
                        (TextCompositionEventArgs e) => {
                            var box = e.Source as TextBox;
                            if (box == null) return;
                            e.Handled = new Regex(@"[^0-9]").IsMatch(e.Text) ?
                            (new Regex(@"\D").IsMatch(box.Text) || new Regex(@"[^.]").IsMatch(e.Text)) : false;
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
                IsEnabed = _debugSate.IsUnlockUI || TR.IsEnabaled;
            }
        }


        #region Method
        private void TR_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TR.IsEnabaled))
            {
                IsEnabed = _debugSate.IsUnlockUI || TR.IsEnabaled;
            }
        }
        #endregion
    }
}
