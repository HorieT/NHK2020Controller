using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ABU2021_ControlAndDebug.Core
{
    /// <summary>
    /// 定数定義
    /// ここはデフォルトの定義にして呼び出しはModel.Configに移したい
    /// </summary>
    static class ControlType
    {
        public static readonly string CONFIG_FILE_NAME = "config";

        public enum Device : int
        {
            TR = 0,
            DR,
            CANDebugger,
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
            TR         = 22337,
            DR         = 22338,

            CANDebugger     = 22345,
            Etc = 22351,            //0x574F
        }


        public static readonly string TCP_IP_ADDRESS = "10.42.0.50";//"192.168.43.240";//"10.42.0.1";
        //public static readonly string TCP_IP_ADDRESS = "192.168.179.6";//確認用

        public enum TcpPort : int
        {
            Etc = 0,
            TR = 8080,
            DR = 8011,
        }


        public enum Pot : int
        {
            _1Right = 0,
            _1Left = 1,
            _2Front = 2,
            _2Back  = 3,
            _3      = 4,
        }
        public static readonly Color[] PotsColor = {
            new Color{R = 255, G = 180, B = 127, A = 255},//25
            new Color{R = 255, G = 233, B = 127, A = 255},//50
            new Color{R = 191, G = 255, B = 127, A = 255},//90
            new Color{R = 127, G = 255, B = 127, A = 255},//120
            new Color{R = 127, G = 255, B = 255, A = 191},//150
        };
        public static readonly string[] PotsName = {
            "1型右 [0]",
            "1型左 [1]",
            "2型前 [2]",
            "2型奥 [3]",
            "3型   [4]",
        };


        public static Device ToDevice(UsbBoardPid pid)
        {
            switch (pid)
            {
                case UsbBoardPid.TR:
                    return Device.TR;
                case UsbBoardPid.DR:
                    return Device.DR;
                case UsbBoardPid.CANDebugger:
                    return Device.CANDebugger;
                default:
                    return Device.Etc;
            }
        }
        public static Device ToDevice(TcpPort port)
        {
            switch (port)
            {
                case TcpPort.TR:
                    return Device.TR;
                case TcpPort.DR:
                    return Device.DR;
                default:
                    return Device.Etc;
            }
        }
    }
}
