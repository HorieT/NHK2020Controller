using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ABU2021_ControlAndDebug.Models
{
    partial class ComSTM
    {
        private List<byte> MakeJoypadDataPacket()
        {
            List<byte> packet = new List<byte>();
            
            //header
            packet.Add(0x80);
            var pad = _joypad.GetPad().JoyInfoEx;
            packet.AddRange(BitConverter.GetBytes(pad.dwButtons));
            packet.AddRange(BitConverter.GetBytes(pad.dwPOV));
            packet.Add((byte)(((int)pad.dwXpos - ushort.MaxValue / 2 -1) / 256));
            packet.Add((byte)(((int)pad.dwYpos - ushort.MaxValue / 2 -1) / 256));
            packet.Add((byte)(((int)pad.dwVpos - ushort.MaxValue / 2 -1) / 256));
            packet.Add((byte)(((int)pad.dwRpos - ushort.MaxValue / 2 -1) / 256));
            //checksum
            packet.Add(packet.Aggregate((sum, item) => (byte)(sum + item)));//byte列にはSum()が使えない

            return packet;
        }
        /// <summary>
        /// GUI入力のデータのパケット化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeHeader"></param>
        /// <param name="data"></param>
        /// <returns>未エンコードのデータパケット</returns>
        List<byte> MakeGUIDataPacket<T>(byte typeHeader, T data) where T : struct
        {
            int size = Marshal.SizeOf(data);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, ptr, false);
            byte[] bytes = new byte[size + 1];
            Marshal.Copy(ptr, bytes, 1, size);
            bytes[0] = typeHeader;

            return bytes.ToList();
        }
        /// <summary>
        /// GUI入力のデータのパケット化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeHeader"></param>
        /// <param name="data"></param>
        /// <param name="type">これ要る？</param>
        /// <returns>未エンコードのデータパケット</returns>
        List<byte> MakeGUIDataPacket(byte typeHeader, object data, Type type)
        {
            if (!type.IsValueType) throw new ArgumentException("Type must be a value type ", nameof(type));
            int size = Marshal.SizeOf(type);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, ptr, false);
            byte[] bytes = new byte[size + 1];
            Marshal.Copy(ptr, bytes, 1, size);
            bytes[0] = typeHeader;

            return bytes.ToList();
        }
        /// <summary>
        /// ここで受信データのパケット解読から振り分けまで一括でやる
        /// </summary>
        /// <param name="datas">デコード後のデータ列</param>
        void  ReadDataPacket(byte[] data) {
            string pName;
            Type type;
            try
            {
                pName = ((ControlTR.ReceiveDataType)data[0]).ToString();
                type = typeof(ControlTR).GetProperty(pName).PropertyType;

            }
            catch
            {
                throw;
            }

            if (!type.IsValueType) { throw new ArgumentException("Invalid received data", nameof(data)); }
            int size = Marshal.SizeOf(type);
            if (size != data.Count() - 1) { throw new ArgumentException("Invalid received data", nameof(data)); }
                
            //値の代入処理
            switch (_tr[pName])
            {
                case bool b:
                    _tr[pName] = BitConverter.ToBoolean(data, 1);
                    break;
                case float f:
                    _tr[pName] = BitConverter.ToSingle(data, 1);
                    break;
                case byte uint8:
                    _tr[pName] = data[1];
                    break;
                case char int8:
                    _tr[pName] = BitConverter.ToChar(data, 1);
                    break;
                case ushort uint16:
                    _tr[pName] = BitConverter.ToUInt16(data, 1);
                    break;
                case short int16:
                    _tr[pName] = BitConverter.ToInt16(data, 1);
                    break;
                case uint uint32:
                    _tr[pName] = BitConverter.ToUInt32(data, 1);
                    break;
                case int int32:
                    _tr[pName] = BitConverter.ToInt32(data, 1);
                    break;
                default:
                    throw new Exception("Undefined type");
            }
        }

        byte[] COBS_Encode(IReadOnlyList<byte> inData)
        {
            if (inData.Count() > 255 || inData.Count() < 1) throw new ArgumentException("The data size must be 1 to 255 bytes", nameof(inData));
            var encodeData = new List<byte>();
            int zeroIndex = 0;

            encodeData.Add(0);
            encodeData.AddRange(inData);
            for(int i = 1;i < encodeData.Count; ++i)
            {
                if (encodeData[i] == 0)
                {
                    encodeData[zeroIndex] = (byte)(i - zeroIndex);
                    zeroIndex = i;
                }
            }
            encodeData[zeroIndex] = (byte)(encodeData.Count - zeroIndex);
            encodeData.Add(0);

            return encodeData.ToArray();
        }
        byte[] COBS_Decode(IReadOnlyList<byte> inData)
        {
            if (inData.Last() != 0x00) throw new ArgumentException("The end of the data column must be 0", nameof(inData));
            if (inData.Count() >  257 || inData.Count() < 3) throw new ArgumentException("The data size must be 3 to 257 bytes", nameof(inData));
            var decodeData = inData.ToList();
            int zeroIndex = 0;

            while (zeroIndex < decodeData.Count - 1)
            {
                if (decodeData[zeroIndex] == 0) throw new ArgumentException("Data decoding failed", nameof(inData));
                var tmpIndex = zeroIndex;
                zeroIndex += decodeData[zeroIndex];
                decodeData[tmpIndex] = 0;
            }
            decodeData.RemoveAt(0);
            decodeData.RemoveAt(decodeData.Count - 1);

            return decodeData.ToArray();
        }
    }
}
