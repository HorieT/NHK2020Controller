using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;

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
            //byte
            I_LOAD = 0x20,
            //float
            INJECT = 0x40,
            //sp
            JOY = 0x80,
            INJECT_POT,
            INJECT_Q_INS,
            INJECT_Q_DEL,
        }
        public static readonly Dictionary<HeaderType, Type> DataType = new Dictionary<HeaderType, Type>
        {
            {HeaderType.E_STOP,         typeof(bool)},
            {HeaderType.I_LOAD,         typeof(byte)},
            {HeaderType.INJECT,         typeof(float)},
            {HeaderType.JOY,            typeof(JoystickState)},
            {HeaderType.INJECT_POT,     typeof(int)},
            {HeaderType.INJECT_Q_INS,   typeof(int[])},
            {HeaderType.INJECT_Q_DEL,   typeof(int)},
        };
        #endregion


        public SendDataMsg(HeaderType header, object data)
        {
            try
            {
                Reset(header, data);
            }
            catch
            {
                throw;
            }
        }


        #region Property
        public HeaderType Header { get; private set; }
        public object Data { get; private set; }
        #endregion


        #region Method
        public void Reset(HeaderType header, object data)
        {
            if (DataType[header] != data.GetType()) throw new ArgumentException("Header and type do not match");
            Header = header;
            Data = data;
        }
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
            else if (Data.GetType().IsArray)
            {
                data = string.Join(", ", Data);
            }
            else//自己定義型(複数データ列)
            {
                switch (Data)
                {
                    case JoystickState joy:
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
                case JoystickState joy:
                    msg.AddRange(JoyToByte(joy));
                    break;
                default:
                    throw new NotImplementedException();
            }
            return msg;
        }


        private static string JoyToString(JoystickState joy)
        {
            return 
                JoyPad.ButtonConv(joy).ToString("X8") + "," +
                JoyPad.AnalogToFloat(joy.X).ToString("F3") + "," +
                JoyPad.AnalogToFloat(joy.Y).ToString("F3") + "," +
                JoyPad.AnalogToFloat(joy.RotationX).ToString("F3") + "," +
                JoyPad.AnalogToFloat(joy.RotationY).ToString("F3");
        }
        private static List<byte> JoyToByte(JoystickState joy)
        {
            List<byte> packet = new List<byte>();
            packet.AddRange(BitConverter.GetBytes(JoyPad.ButtonConv(joy)));
            packet.Add(JoyPad.AnalogToByte(joy.X));//X軸
            packet.Add(JoyPad.AnalogToByte(joy.Y));//Y軸
            packet.Add(JoyPad.AnalogToByte(joy.RotationX));//X回転
            packet.Add(JoyPad.AnalogToByte(joy.RotationY));//Y回転
            //checksum
            packet.Add(packet.Aggregate((sum, item) => (byte)(sum + item)));//byte列にはSum()が使えない
            return packet;
        }
        #endregion
    }
}
