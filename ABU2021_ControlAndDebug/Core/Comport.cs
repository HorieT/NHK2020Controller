using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ABU2021_ControlAndDebug.Core
{
    class Comport
    {
        private static readonly Regex RegexUSB = new Regex(@"^USB");
        private static readonly Regex RegexVID = new Regex(@"(?<=VID_)\d+");
        private static readonly Regex RegexPID = new Regex(@"(?<=PID_)\d+");
        private static readonly Regex RegexComPort = new Regex(@"(?<=\()COM\d+(?=\))");
        private string _pnpDeviceID;

        public string DeviceID { get; set; }
        public string Description { get; set; }
        public string PNPDeviceID { 
            get => _pnpDeviceID; 
            set 
            {
                if (_pnpDeviceID == value) return;
                _pnpDeviceID = value;
                IsUSB = RegexUSB.IsMatch(_pnpDeviceID);

                if (IsUSB)
                {
                    try
                    {
                        VID = Convert.ToInt32(RegexVID.Match(_pnpDeviceID).Value, 16);
                        PID = Convert.ToInt32(RegexPID.Match(_pnpDeviceID).Value, 16);
                    }
                    catch
                    {
                        VID = 0;
                        PID = 0;
                    }
                    PortName = RegexComPort.Match(Description).Value;
                }
                else
                {
                    VID = 0;
                    PID = 0;
                }
            } 
        }

        public bool IsUSB { get; private set; }
        public int VID { get; private set; }
        public int PID { get; private set; }


        public string PortName{ get; private set; }
    }
}
