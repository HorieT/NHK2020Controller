using JoypadControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABU2021_ControlAndDebug.Core
{
    class ComROS
    {
        private InterfaceWifi _wifi;
        //private InterfaceBT _bluetooth;


        #region Property
        public bool IsConnected { get; private set; }
        public ControlType.TcpPort Port { get; private set; }
        #endregion



        public ComROS()
        {
            _wifi = new InterfaceWifi();
        }
        ~ComROS()
        {

        }






        #region Method
        public Task Connect(ControlType.TcpPort port)
        {
            Port = port;
            _wifi.Port = (int)Port;
            return Task.Run(async () =>
            {
                try
                {
                    await _wifi.Connect();
                }
                catch (Exception e)
                {
                    throw;
                }
                IsConnected = true;
            });
        }

        public void Disconnect()
        {
            try
            {
                _wifi.Disconnect();
            }
            catch
            {
                throw;
            }
            finally
            {
                IsConnected = false;
            }
        }


        /// <summary>
        /// 送信
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="header"></param>
        /// <param name="data"></param>
        public void Send(string header, string data)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected");
            try
            {
                _wifi.WriteLine(header + ":" + data);
            }
            catch
            {
                Disconnect();
                throw new InvalidOperationException("Connecton error");
            }
        }
        /// <summary>
        /// 送信のジョイパッド特殊化
        /// </summary>
        /// <param name="header">ヘッダは外部定義</param>
        /// <param name="joy"></param>
        public void Send(string header, Joypad.JOYINFOEX joy)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected");

            string data = "";
            uint button = 0;
            button |=
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
                ((joy.dwButtons & 0x1000) << 4) ;   //HOME

            data += button.ToString("X8");
            data += "," + (((float)joy.dwXpos - ushort.MaxValue / 2 - 1) / ushort.MaxValue * 2).ToString("F3");
            data += "," + (((float)joy.dwYpos - ushort.MaxValue / 2 - 1) / ushort.MaxValue * 2).ToString("F3");
            data += "," + (((float)joy.dwVpos - ushort.MaxValue / 2 - 1) / ushort.MaxValue * 2).ToString("F3");
            data += "," + (((float)joy.dwRpos - ushort.MaxValue / 2 - 1) / ushort.MaxValue * 2).ToString("F3");

            try
            {
                _wifi.WriteLine(header + ":" + data);
            }
            catch
            {
                Disconnect();
                throw new InvalidOperationException("Connecton error");
            }
        }


        async Task<string> ReadSentenceAsync()
        {
            return  await _wifi.ReadLineAsync();
        }
        #endregion
    }
}
