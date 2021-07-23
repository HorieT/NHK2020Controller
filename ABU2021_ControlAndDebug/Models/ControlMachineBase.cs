using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MVVMLib;

namespace ABU2021_ControlAndDebug.Models
{
    abstract class ControlMachineBase : NotifyPropertyChanged
    {
        #region Property
        #region private

        private bool _isEnabled;
        private bool _isConnected;
        private bool _isEmergencyStopped;

        private Vector _centerPosition = new Vector();
        private double _centerPositionRot;
        #endregion

        #region public
        public bool IsEnabaled
        {
            get => _isEnabled;
            set { SetProperty(ref _isEnabled, value); }
        }
        public bool IsConnected
        {
            get => _isConnected;
            set { SetProperty(ref _isConnected, value); }
        }

        #region Get only
        public bool IsEmergencyStopped
        {
            get => _isEmergencyStopped;
            private set { SetProperty(ref _isEmergencyStopped, value); }
        }
        public Vector Positon
        {
            get => _centerPosition;
            protected set { SetProperty(ref _centerPosition, value); }
        }
        public double PositonRot
        {
            get => _centerPositionRot;
            protected set { SetProperty(ref _centerPositionRot, value); }
        }
        #endregion
        #endregion
        #endregion


        #region Method
        protected abstract void SaveDebugLog(string fileName);
        #endregion
    }
}