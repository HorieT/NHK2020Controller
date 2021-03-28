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
        public enum Machine : int
        {
            TR = 0,
            DR,
            Etc
        }

        public static readonly int STM_VID = 1155;
        public static readonly int STM_VCOM_PID = 22336;


        /// <summary>
        /// ホントはPIDを自己定義すべきじゃないけど個人の範疇なので許して
        /// 0x5740~0x574Fまで許容(一応空き番号)
        /// </summary>
        public enum USBBoardPID : int
        {
            TestDevice = 22336,     //0x5740
            TR,
            DR,
            CANDebugger,
            InjectMD_Master,
            InjectMD_Sleave,
            Etc = 22351,            //0x574F
        }


        public static readonly string TCP_IP_ADDRESS = "10.42.0.1";
        //public static readonly string TCP_IP_ADDRESS = "127.0.0.1";//確認用

        public enum TcpPort : int
        {
            TR = 8080,
            DR = 8011
        }
    }
}
