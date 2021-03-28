using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JoypadControl;

namespace ABU2021_ControlAndDebug.Core
{
    static class RosJoyConverter
    {
        public static uint ButtonConv(Joypad.JOYINFOEX joy)
        {
            return (
                (uint)(joy.dwPOV == 0 ? 0x0010 : 0x0000) |
                (uint)(joy.dwPOV == 4500 ? 0x0030 : 0x0000) |
                (uint)(joy.dwPOV == 9000 ? 0x0020 : 0x0000) |
                (uint)(joy.dwPOV == 13500 ? 0x0060 : 0x0000) |
                (uint)(joy.dwPOV == 18000 ? 0x0040 : 0x0000) |
                (uint)(joy.dwPOV == 22500 ? 0x00C0 : 0x0000) |
                (uint)(joy.dwPOV == 27000 ? 0x0080 : 0x0000) |
                (uint)(joy.dwPOV == 31500 ? 0x0090 : 0x0000) |
                ((joy.dwButtons & 0x0080) >> 7) |   //select
                ((joy.dwButtons & 0x0300) >> 7) |   //L3,R3
                ((joy.dwButtons & 0x0040) >> 3) |   //start
                ((joy.dwButtons & 0x0C00) >> 2) |   //L2,R2
                ((joy.dwButtons & 0x0030) << 6) |   //L1,R1
                ((joy.dwButtons & 0x0008) << 9) |   //Y(三角)
                ((joy.dwButtons & 0x0002) << 12) |  //B(丸)
                ((joy.dwButtons & 0x0001) << 14) |  //A(バツ)
                ((joy.dwButtons & 0x0004) << 13) |  //X(四角)
                ((joy.dwButtons & 0x1000) << 4)   //HOME
                );
        }

        public static float AnalogToFloat(uint analogData)
        {
            return ((float)analogData - ushort.MaxValue / 2 - 1) / ushort.MaxValue * 2;
        }
        public static byte AnalogToByte(uint analogData)
        {
            return (byte)(((int)analogData - ushort.MaxValue / 2 - 1) / 256);
        }
    }
}
