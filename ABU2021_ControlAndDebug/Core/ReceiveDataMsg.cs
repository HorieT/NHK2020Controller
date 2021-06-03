using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ABU2021_ControlAndDebug.Core
{
    class ReceiveDataMsg
    {
        #region Type
        /// <summary>
        /// 受信ヘッダ型
        /// 定義文字列はそのままROS通信に、数値はSTM通信に
        /// TR,DR共用
        /// </summary>
        public enum HeaderType : byte
        {
            //bool
            E_STOP = 0x01,
            COLLECT,
            I_LOAD_END,
            INJECT_END,
            //byte
            //float
            I_ANGLE = 0x80,
            //sp
            POSITION = 0xE0,
            //debug
            DEBUG_POS = 0xF0
        }

        public static readonly Dictionary<HeaderType, Type> DataType = new Dictionary<HeaderType, Type>
        {
            {HeaderType.E_STOP, typeof(bool)},
            {HeaderType.COLLECT, typeof(bool)},
            {HeaderType.I_LOAD_END,typeof(bool)},
            {HeaderType.INJECT_END, typeof(bool)},
            {HeaderType.I_ANGLE, typeof(float)},
            {HeaderType.POSITION, typeof((Vector, double))},
            {HeaderType.DEBUG_POS, typeof((Vector, double, Vector, double))},
        };
        #endregion

        public ReceiveDataMsg(string msg)
        {
            var split = Regex.Split(msg, @":");
            try
            {
                Header = (HeaderType)Enum.Parse(typeof(HeaderType), split[0]);
            }
            catch
            {
                throw new ArgumentException("Header not found");
            }


            try
            {
                string msgData = split[1];
                //型変換 switchステートは使えない
                if (DataType[Header] == typeof(bool))
                {
                    Data = bool.Parse(msgData);
                }
                else if (DataType[Header] == typeof(byte))
                {
                    Data = byte.Parse(msgData);
                }
                else if (DataType[Header] == typeof(char))
                {
                    Data = char.Parse(msgData);
                }
                else if (DataType[Header] == typeof(ushort))
                {
                    Data = ushort.Parse(msgData);
                }
                else if (DataType[Header] == typeof(short))
                {
                    Data = short.Parse(msgData);
                }
                else if (DataType[Header] == typeof(uint))
                {
                    Data = uint.Parse(msgData);
                }
                else if (DataType[Header] == typeof(int))
                {
                    Data = int.Parse(msgData);
                }
                else if (DataType[Header] == typeof(float))
                {
                    Data = float.Parse(msgData);
                }
                else if (DataType[Header] == typeof((Vector, double)))
                {
                    Data = ToPosition(msgData);
                }
                else if (DataType[Header] == typeof((Vector, double, Vector, double)))
                {
                    Data = ToDebugPos(msgData);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch
            {
                throw new ArgumentException("Data not found");
            }
        }
        public ReceiveDataMsg(IReadOnlyList<byte> msg)
        {
            if(!Enum.IsDefined(typeof(HeaderType), msg[0]))
                throw new ArgumentException("Header not found");

            Header = (HeaderType)msg[0];


            try
            {
                //型変換 switchステートは使えない
                if (DataType[Header] == typeof(bool))
                {
                    Data = BitConverter.ToBoolean(msg.ToArray(), 1);
                }
                else if (DataType[Header] == typeof(byte))
                {
                    Data = msg[1];
                }
                else if (DataType[Header] == typeof(char))
                {
                    Data = BitConverter.ToChar(msg.ToArray(), 1);
                }
                else if (DataType[Header] == typeof(ushort))
                {
                    Data = BitConverter.ToUInt16(msg.ToArray(), 1);
                }
                else if (DataType[Header] == typeof(short))
                {
                    Data = BitConverter.ToInt16(msg.ToArray(), 1);
                }
                else if (DataType[Header] == typeof(uint))
                {
                    Data = BitConverter.ToUInt32(msg.ToArray(), 1);
                }
                else if (DataType[Header] == typeof(int))
                {
                    Data = BitConverter.ToInt32(msg.ToArray(), 1);
                }
                else if (DataType[Header] == typeof(float))
                {
                    Data = BitConverter.ToSingle(msg.ToArray(), 1);
                }
                else if (DataType[Header] == typeof((Vector, double)))
                {
                    Data = ToPosition(msg, 1);
                }
                else if (DataType[Header] == typeof((Vector, double, Vector, double)))
                {
                    Data = ToDebugPos(msg, 1);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch
            {
                throw new ArgumentException("Data not found");
            }
        }


        #region Property
        public HeaderType Header { get; private set; }
        public object Data { get; private set; }
        #endregion



        #region Method
        private static (Vector, double) ToPosition(String msgData)
        {
            var split = Regex.Split(msgData, ",");
            try
            {
                Vector vec = new Vector(double.Parse(split[0]), double.Parse(split[1]));
                double rad = double.Parse(split[2]);
                return (vec, rad);
            }
            catch
            {
                throw new ArgumentException();
            }
        }
        private static (Vector, double) ToPosition(IReadOnlyList<byte> msg, int offset)
        {
            if (msg.Count != 12 + offset) throw new ArgumentException();
            var array = msg.ToArray();
            Vector vec = new Vector(BitConverter.ToSingle(array, offset), BitConverter.ToSingle(array, offset+4));
            double rad = BitConverter.ToSingle(array, offset+8);
            return (vec, rad);
        }
        private static (Vector, double, Vector, double) ToDebugPos(String msgData)
        {
            var split = Regex.Split(msgData, ",");
            try
            {
                Vector vec1 = new Vector(double.Parse(split[0]), double.Parse(split[1]));
                double rad1 = double.Parse(split[2]);
                Vector vec2 = new Vector(double.Parse(split[3]), double.Parse(split[4]));
                double rad2 = double.Parse(split[5]);
                return (vec1, rad1, vec2, rad2);
            }
            catch
            {
                throw new ArgumentException();
            }
        }
        private static (Vector, double, Vector, double) ToDebugPos(IReadOnlyList<byte> msg, int offset)
        {
            if (msg.Count != 24 + offset) throw new ArgumentException();
            var array = msg.ToArray();
            Vector vec1 = new Vector(BitConverter.ToSingle(array, offset), BitConverter.ToSingle(array, offset + 4));
            double rad1 = BitConverter.ToSingle(array, offset + 8);
            Vector vec2 = new Vector(BitConverter.ToSingle(array, offset + 12), BitConverter.ToSingle(array, offset + 16));
            double rad2 = BitConverter.ToSingle(array, offset + 20);
            return (vec1, rad1, vec2, rad2);
        }
        #endregion
    }
}
