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

namespace ABU2021_ControlAndDebug.ViewModels
{
    class ControlTabTR : ViewModel
    {
        #region Model
        public Models.ControlTR TR { get; private set; }
        private Models.OutputLog _log;
        #endregion

        #region Property
        private string _injectSpeed = "1.00";
        private string _injectAngle = "27.00";
        private string _injectAngleNow = "27.00";
        private string _fallPrediction;

        public string InjectSpeed
        {
            get => _injectSpeed;
            set { SetProperty(ref _injectSpeed, value); }
        }
        public string InjectAngle
        {
            get => _injectAngle;
            set { SetProperty(ref _injectAngle, value); }
        }
        public string InjectAngleNow
        {
            get => _injectAngleNow;
            set { SetProperty(ref _injectAngleNow, value); }
        }
        public string FallPrediction
        {
            get => _fallPrediction;
            set { SetProperty(ref _fallPrediction, value); }
        }
        #endregion



        #region Command
        private ICommand _noPasteTextBox_PreviewExecuted;
        private ICommand _injectTextBox_PreviewTextInput;
        private ICommand _injectTextBox_LostFocus;
        private ICommand _angleTextBox_PreviewTextInput;
        private ICommand _angleTextBox_LostFocus;
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
                            float speed;
                            try
                            {
                                speed = float.Parse(InjectSpeed);
                            }
                            catch
                            {
                                _log.WiteLine("入力が数値ではありません");
                                InjectSpeed = TR.InjectSpeedTag.ToString("F2");
                                return;
                            }
                            if(speed > 0.0 && speed < Models.ControlTR.INJECT_SPEED_MAX)
                            {
                                TR.InjectSpeedTag = speed;
                                FallPrediction = TR.GetArrowFallPos(0).ToString("F2");
                            }
                            else
                            {
                                _log.WiteLine("入力が範囲外です");
                                InjectSpeed = TR.InjectSpeedTag.ToString("F2");
                            }
                        }));
            }
        }
        public ICommand AngleTextBox_PreviewTextInput
        {
            get
            {
                return _angleTextBox_PreviewTextInput ??
                    (_angleTextBox_PreviewTextInput = CreateCommand(
                        (TextCompositionEventArgs e) => {
                            var box = e.Source as TextBox;
                            if (box == null) return;
                            e.Handled = new Regex(@"[^0-9]").IsMatch(e.Text) ?
                            (new Regex(@"\D").IsMatch(box.Text) || new Regex(@"[^.]").IsMatch(e.Text)) : false;
                        }));
            }
        }
        public ICommand AngleTextBox_LostFocus
        {
            get
            {
                return _angleTextBox_LostFocus ??
                    (_angleTextBox_LostFocus = CreateCommand(
                        (RoutedEventArgs e) => {
                            float angle;
                            try
                            {
                                angle = float.Parse(InjectAngle);
                            }
                            catch
                            {
                                _log.WiteLine("入力が数値ではありません");
                                InjectAngle = TR.InjectAngleTag.ToString("F2");
                                return;
                            }
                            if (angle > Models.ControlTR.INJECT_ANGLE_MIN && angle < Models.ControlTR.INJECT_ANGLE_MAX)
                            {
                                TR.InjectAngleTag = angle;
                                FallPrediction = TR.GetArrowFallPos(0).ToString("F2");
                            }
                            else
                            {
                                _log.WiteLine("入力が範囲外です");
                                InjectAngle = TR.InjectAngleTag.ToString("F2");
                            }
                        }));
            }
        }
        #endregion


        public ControlTabTR()
        {
            TR = Models.ControlTR.GetInstance;
            _log = Models.OutputLog.GetInstance;


            TR.InjectSpeedTag = float.Parse(InjectSpeed);
            TR.InjectAngleTag = float.Parse(InjectAngle);
            FallPrediction = TR.GetArrowFallPos(0).ToString("F2");
            TR.PropertyChanged += TR_PropertyChanged;
        }


        #region Method
        private void TR_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(TR.InjectAngleNow)){
                InjectAngleNow = TR.InjectAngleNow.ToString("F2");
            }
        }
        #endregion
    }
}
