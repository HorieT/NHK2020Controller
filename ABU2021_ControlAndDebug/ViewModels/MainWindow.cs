using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MVVMLib;

namespace ABU2021_ControlAndDebug.ViewModels
{
    class MainWindow : ViewModel
    {

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
        }

        #region Command
        private ICommand _ctrl0;
        private ICommand _ctrl1;
        private ICommand _ctrl2;
        private ICommand _ctrl3;
        private ICommand _ctrl4;

        public ICommand Ctrl0{
            get
            {
                return _ctrl0 ?? 
                    (_ctrl0 = CreateCommand(
                        (object sender) => {
                            if (TR.IsEnabaled) TR.AddInjectQueue(Core.ControlType.Pot._1Right, 0);
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
                            if (TR.IsEnabaled) TR.AddInjectQueue(Core.ControlType.Pot._1Left, 0);
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
                            if (TR.IsEnabaled) TR.AddInjectQueue(Core.ControlType.Pot._2Front, 0);
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
                            if (TR.IsEnabaled) TR.AddInjectQueue(Core.ControlType.Pot._2Back, 0);
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
                            if (TR.IsEnabaled) TR.AddInjectQueue(Core.ControlType.Pot._3, 0);
                        }));
            }
        }
        #endregion


        #region Method
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
