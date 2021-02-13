using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ABU2021_ControlAndDebug.Core
{
    class InterfaceWifi : DeviceInterfaceBace
    {
        public InterfaceWifi()
        {

        }


        #region Property
        public NetworkStream Stream { get; private set; }
        public int Port { get; set; }
        private StreamReader _serverReader;
        #endregion


        #region Method

        #region Override method
        public override Task Connect()
        {
            if (IsConnected) throw new InvalidOperationException("Already connected to TCP/IP");

            return Task.Run(() =>
            {
                try
                {
                    var _client = new System.Net.Sockets.TcpClient(ControlType.TCP_IP_ADDRESS, (int)Port);

                    Stream = _client.GetStream();
                    _serverReader = new StreamReader(Stream, Encoding.UTF8);
                }
                catch { throw; }
                IsConnected = true;
            });

        }
        public override void Disconnect()
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to TCP/IP");

            try 
            {
                //_serverReader.Close();
                Stream.Close(); 
            }
            catch { throw; }
            finally { IsConnected = false; }
        }


        public override void Write(string data)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to TCP/IP");
            var sendBytes = Encoding.UTF8.GetBytes(data);
            try { Stream.Write(sendBytes, 0, sendBytes.Length); }
            catch { throw; }
        }
        public override void Write(Byte[] buffer, Int32 size)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to TCP/IP");
            try { Stream.Write(buffer, 0, size); }
            catch { throw; }
        }
        public override void WriteLine(string data)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to TCP/IP");
            var sendBytes = Encoding.UTF8.GetBytes(data + "\n");
            try { Stream.Write(sendBytes, 0, sendBytes.Length); }
            catch { throw; }
        }
        public override int Read(Byte[] buffer, Int32 size)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to TCP/IP");
            return Stream.Read(buffer, 0, size);
        }
        public override Task<int> ReadAsync(Byte[] buffer, Int32 size)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to TCP/IP");
            return Stream.ReadAsync(buffer, 0, size);
        }
        public override string ReadLine()
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to TCP/IP");
            return _serverReader.ReadLine();
        }
        public override Task<string> ReadLineAsync()
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to TCP/IP");
            return _serverReader.ReadLineAsync();
        }
        public override string ReadTo(string value)
        {
            throw new NotImplementedException("Don't use this");
        }
        #endregion
        #endregion
    }
}
