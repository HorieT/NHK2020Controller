using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        }

        public static readonly Dictionary<HeaderType, Type> DataType = new Dictionary<HeaderType, Type>
        {
            {HeaderType.E_STOP, typeof(bool)},
            {HeaderType.COLLECT, typeof(bool)},
            {HeaderType.I_LOAD_END,typeof(bool)},
            {HeaderType.INJECT_END, typeof(bool)},
            {HeaderType.I_ANGLE, typeof(float)},
        };
        #endregion

        public ReceiveDataMsg(string msg)
        {
            var matchHead = Regex.Match(msg, @"^\S+(?=:)");
            var matchData = Regex.Match(msg, @"(?<=:)\S+");

            try
            {
                Header = (HeaderType)Enum.Parse(typeof(HeaderType), matchHead.Value);
            }
            catch
            {
                throw new ArgumentException("Header not found");
            }


            try
            {
                //型変換 switchステートは使えない
                if (DataType[Header] == typeof(bool))
                {
                    Data = bool.Parse(matchData.Value);
                }
                else if (DataType[Header] == typeof(byte))
                {
                    Data = byte.Parse(matchData.Value);
                }
                else if (DataType[Header] == typeof(char))
                {
                    Data = char.Parse(matchData.Value);
                }
                else if (DataType[Header] == typeof(ushort))
                {
                    Data = ushort.Parse(matchData.Value);
                }
                else if (DataType[Header] == typeof(short))
                {
                    Data = short.Parse(matchData.Value);
                }
                else if (DataType[Header] == typeof(uint))
                {
                    Data = uint.Parse(matchData.Value);
                }
                else if (DataType[Header] == typeof(int))
                {
                    Data = int.Parse(matchData.Value);
                }
                else if (DataType[Header] == typeof(float))
                {
                    Data = float.Parse(matchData.Value);
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

        #endregion
    }
}
