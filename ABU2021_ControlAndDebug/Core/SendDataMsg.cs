using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JoypadControl;

namespace ABU2021_ControlAndDebug.Core
{
    class SendDataMsg
    {
        #region Types
        /// <summary>
        /// 送信ヘッダ型
        /// 定義文字列はそのままROS通信に、数値はSTM通信に
        /// TR,DR共用
        /// </summary>
        public enum HeaderType : byte
        {
            //bool
            E_STOP = 0x01,
            COLLECT,
            //byte
            I_LOAD = 0x20,
            //float
            INJECT = 0x80,
            I_ANGLE,
            //sp
            JOY = 0xF0
        }
        public static readonly Dictionary<HeaderType, Type> DataType = new Dictionary<HeaderType, Type>
        {
            {HeaderType.E_STOP, typeof(bool)},
            {HeaderType.COLLECT, typeof(bool)},
            {HeaderType.I_LOAD,typeof(byte)},
            {HeaderType.INJECT, typeof(float)},
            {HeaderType.I_ANGLE, typeof(float)},
            {HeaderType.JOY, typeof(Joypad.JOYINFOEX)},
        };
        #endregion


        public SendDataMsg(HeaderType header, object data)
        {
            if (DataType[header] != data.GetType()) throw new ArgumentException("Header and type do not match");
            Header = header;
            Data = data;
        }


        #region Property
        public HeaderType Header { get; private set; }
        public object Data { get; private set; }
        #endregion


        #region Method
        public string ConvString()
        {
            string head = Header.ToString();
            string data;
            
            if(Data is float d)//浮動小数点型
            {
                data = d.ToString("F3");
            }
            else if(Data.GetType().IsPrimitive)//その他プリミティブ型
            {
                data = Data.ToString();
            }
            else//自己定義型(複数データ列)
            {
                switch (Data)
                {
                    case Joypad.JOYINFOEX joy:
                        data = JoyToString(joy);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            return head + ":" + data;
        }

        public List<byte> ConvByte()
        {
            List<byte> msg = new List<byte>();
            msg.Add((byte)Header);

            switch(Data)//コピペの嵐    Marshal.Copyの方がよかったか？
            {
                case bool d:
                    msg.AddRange(BitConverter.GetBytes(d));
                    break;
                case byte d:
                    msg.AddRange(BitConverter.GetBytes(d));
                    break;
                case char d:
                    msg.AddRange(BitConverter.GetBytes(d));
                    break;
                case ushort d:
                    msg.AddRange(BitConverter.GetBytes(d));
                    break;
                case short d:
                    msg.AddRange(BitConverter.GetBytes(d));
                    break;
                case uint d:
                    msg.AddRange(BitConverter.GetBytes(d));
                    break;
                case int d:
                    msg.AddRange(BitConverter.GetBytes(d));
                    break;
                case float d:
                    msg.AddRange(BitConverter.GetBytes(d));
                    break;
                /*以下自己定義型*/
                case Joypad.JOYINFOEX joy:
                    msg.AddRange(JoyToByte(joy));
                    break;
                default:
                    throw new NotImplementedException();
            }
            return msg;
        }


        private static string JoyToString(Joypad.JOYINFOEX joy)
        {
            return 
                RosJoyConverter.ButtonConv(joy).ToString("X8") + "," +
                RosJoyConverter.AnalogToFloat(joy.dwXpos).ToString("F3") + "," +
                RosJoyConverter.AnalogToFloat(joy.dwYpos).ToString("F3") + "," +
                RosJoyConverter.AnalogToFloat(joy.dwXrot).ToString("F3") + "," +
                RosJoyConverter.AnalogToFloat(joy.dwZrot).ToString("F3");
        }
        private static List<byte> JoyToByte(Joypad.JOYINFOEX joy)
        {
            List<byte> packet = new List<byte>();
            packet.AddRange(BitConverter.GetBytes(RosJoyConverter.ButtonConv(joy)));
            packet.Add(RosJoyConverter.AnalogToByte(joy.dwXpos));//X軸
            packet.Add(RosJoyConverter.AnalogToByte(joy.dwYpos));//Y軸
            packet.Add(RosJoyConverter.AnalogToByte(joy.dwXrot));//X回転
            packet.Add(RosJoyConverter.AnalogToByte(joy.dwYrot));//Y回転
            //checksum
            packet.Add(packet.Aggregate((sum, item) => (byte)(sum + item)));//byte列にはSum()が使えない
            return packet;
        }
        #endregion
    }
}
