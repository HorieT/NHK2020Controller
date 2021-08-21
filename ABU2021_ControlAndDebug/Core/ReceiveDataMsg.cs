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
            //byte
            //float
            I_ANGLE = 0x40,
            //sp
            M_POS = 0x80,
            POT_POS,
            INJECT_Q,
            M_STATE,
            M_SEQUENCE,
            OFFSET_RAD,
            //debug
            DEBUG_POS = 0xF0
        }
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
                switch (Header)
                {
                    case HeaderType.M_SEQUENCE:
                    case HeaderType.M_STATE:
                        //string,string[]
                        Data = msgData;
                        break;
                    case HeaderType.E_STOP:
                        //bool
                        Data = bool.Parse(msgData);
                        break;
                    case HeaderType.I_ANGLE:
                        Data = float.Parse(msgData);
                        break;
                    case HeaderType.OFFSET_RAD:
                        Data = double.Parse(msgData);
                        break;
                    case HeaderType.INJECT_Q:
                        //int[]
                        Data = ToIntArray(msgData);
                        break;
                    case HeaderType.POT_POS:
                        //double[]
                        Data = ToDoubleArray(msgData);
                        break;
                    case HeaderType.M_POS:
                        Data = ToPosition(msgData);
                        break;
                    case HeaderType.DEBUG_POS:
                        Data = ToDebugPos(msgData);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch
            {
                throw new ArgumentException("Data not found : Header " + Header.ToString());
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
                switch (Header)
                {
                    case HeaderType.E_STOP:
                        //bool
                        Data = BitConverter.ToBoolean(msg.ToArray(), 1);
                        break;
                    case HeaderType.I_ANGLE:
                        Data = BitConverter.ToSingle(msg.ToArray(), 1);
                        break;
                    case HeaderType.M_POS:
                        Data = ToPosition(msg, 1);
                        break;
                    case HeaderType.DEBUG_POS:
                        Data = ToDebugPos(msg, 1);
                        break;
                    case HeaderType.M_SEQUENCE:
                    case HeaderType.M_STATE:
                    case HeaderType.INJECT_Q:
                    case HeaderType.POT_POS:
                    default:
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
        private static (Vector, double, int) ToPosition(String msgData)
        {
            var split = Regex.Split(msgData, ",");
            try
            {
                Vector vec = new Vector(double.Parse(split[0]), double.Parse(split[1]));
                double rad = double.Parse(split[2]);
                int num = split.Count() > 3 ? int.Parse(split[3]) : 0;
                return (vec, rad, num);
            }
            catch
            {
                throw new ArgumentException();
            }
        }
        private static (Vector, double, int) ToPosition(IReadOnlyList<byte> msg, int offset)
        {
            if (msg.Count != 12 + offset) throw new ArgumentException();
            var array = msg.ToArray();
            Vector vec = new Vector(BitConverter.ToSingle(array, offset), BitConverter.ToSingle(array, offset+4));
            double rad = BitConverter.ToSingle(array, offset+8);
            return (vec, rad, 0);
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

        #region ROS only
        private static int[] ToIntArray(String msgData)
        {
            var split = Regex.Split(msgData, ",");
            try
            {
                int test;
                if (split.Count() == 1 &&  !int.TryParse(split[0], out test)) return new int[0];
                return split.Select(sp => int.Parse(sp)).ToArray(); ;
            }
            catch
            {
                throw new ArgumentException();
            }
        }
        private static double[] ToDoubleArray(String msgData)
        {
            var split = Regex.Split(msgData, ",");
            try
            {
                double test;
                if (split.Count() == 1 && !double.TryParse(split[0], out test)) return new double[0];
                return split.Select(sp => double.Parse(sp)).ToArray(); ;
            }
            catch
            {
                throw new ArgumentException();
            }
        }
        #endregion
        #endregion
    }
}
