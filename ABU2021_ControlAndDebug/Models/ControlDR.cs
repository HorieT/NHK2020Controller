using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MVVMLib;

namespace ABU2021_ControlAndDebug.Models
{
    class ControlDR : NotifyPropertyChanged
    {
        private OutputLog _log;
        private Communicator _communicator;
        private DebugSate _debugSate;

        #region Singleton instance
        private static ControlDR _instance;
        public static ControlDR GetInstance
        {
            get
            {
                return _instance ?? (_instance = new ControlDR());
            }
        }
        private ControlDR()
        {
            _log = OutputLog.GetInstance;
            _communicator = Communicator.GetInstance;
            _debugSate = DebugSate.GetInstance;

            _communicator.PropertyChanged += _communicator_PropertyChanged;
        }

        #endregion




        #region Property
        #region private
        private bool _isEnabled;
        // OneWay (input from device only / output to VM) : ReceiveDataType
        private bool _isEmergencyStopped;
        private Vector _position = new Vector(2000, 2000);
        private double _positionRot;


        #endregion
        #region public
        public bool IsEnabaled
        {
            get => _isEnabled;
            set { SetProperty(ref _isEnabled, value); }
        }

        #region Get only
        public bool IsEmergencyStopped
        {
            get => _isEmergencyStopped;
            private set { SetProperty(ref _isEmergencyStopped, value); }
        }
        public Vector Positon
        {
            get => _position;
            private set { SetProperty(ref _position, value); }
        }
        public double PositonRot
        {
            get => _positionRot;
            private set { SetProperty(ref _positionRot, value); }
        }
        #endregion
        #endregion
        #endregion


        #region Method
        private async Task ReadMsg()
        {
            while (IsEnabaled)
            {
                var msg = await _communicator.ReadMsgAsync();
                try
                {
                    switch (msg.Header)
                    {
                        case Core.ReceiveDataMsg.HeaderType.POSITION:
                            Positon = ((ValueTuple<Vector, double>)msg.Data).Item1;
                            PositonRot = ((ValueTuple<Vector, double>)msg.Data).Item2;
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    _log.WiteDebugMsg(e.Message);
                    throw;
                }
            }
        }
        private void _communicator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_communicator.IsConnected))
            {
                IsEnabaled = _communicator.Device == Core.ControlType.Device.DR && _communicator.IsConnected;
                Task.Run(async () => { await ReadMsg(); });
            }
        }
        #endregion
    }
}
