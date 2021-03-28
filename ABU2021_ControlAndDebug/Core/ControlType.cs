using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABU2021_ControlAndDebug.Core
{
    /// <summary>
    /// 定数定義
    /// </summary>
    static class ControlType
    {
        public enum Device : int
        {
            TR0 = 0,
            DR0,
            TR1,
            DR1,
            TR2,
            DR2,
            CANDebugger,
            InjectMD_Master,
            InjectMD_Sleave,
            Etc
        }

        public static readonly int STM_VID = 1155;
        public static readonly int STM_VCOM_PID = 22336;


        /// <summary>
        /// ホントはPIDを自己定義すべきじゃないけど個人の範疇なので許して
        /// 0x5740~0x574Fまで許容(一応空き番号)
        /// </summary>
        public enum UsbBoardPid : int
        {
            TestDevice  = 22336,     //0x5740
            TR0         = 22337,
            DR0         = 22338,
            TR1         = 22339,
            DR1         = 22340,
            TR2         = 22341,
            DR2         = 22342,

            CANDebugger     = 22345,
            InjectMD_Master = 22346,
            InjectMD_Sleave = 22347,
            Etc = 22351,            //0x574F
        }


        public static readonly string TCP_IP_ADDRESS = "10.42.0.1";
        //public static readonly string TCP_IP_ADDRESS = "192.168.179.6";//確認用

        public enum TcpPort : int
        {
            Etc = 0,
            TR0 = 8080,
            DR0 = 8011,
            TR1 = 8012,
            DR1 = 8013,
            TR2 = 8014,
            DR2 = 8015,
        }


        public static Device ToDevice(UsbBoardPid pid)
        {
            switch (pid)
            {
                case UsbBoardPid.TR0:
                    return Device.TR0;
                case UsbBoardPid.DR0:
                    return Device.DR0;
                case UsbBoardPid.TR1:
                    return Device.TR1;
                case UsbBoardPid.DR1:
                    return Device.DR1;
                case UsbBoardPid.TR2:
                    return Device.TR2;
                case UsbBoardPid.DR2:
                    return Device.DR2;
                case UsbBoardPid.CANDebugger:
                    return Device.CANDebugger;
                case UsbBoardPid.InjectMD_Master:
                    return Device.InjectMD_Master;
                case UsbBoardPid.InjectMD_Sleave:
                    return Device.InjectMD_Sleave;
                default:
                    return Device.Etc;
            }
        }
        public static Device ToDevice(TcpPort port)
        {
            switch (port)
            {
                case TcpPort.TR0:
                    return Device.TR0;
                case TcpPort.DR0:
                    return Device.DR0;
                case TcpPort.TR1:
                    return Device.TR1;
                case TcpPort.DR1:
                    return Device.DR1;
                case TcpPort.TR2:
                    return Device.TR2;
                case TcpPort.DR2:
                    return Device.DR2;
                default:
                    return Device.Etc;
            }
        }
    }
}
