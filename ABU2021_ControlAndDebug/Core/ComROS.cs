using JoypadControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ABU2021_ControlAndDebug.Core
{
    class ComROS
    {
        //private InterfaceWifi _wifi;
        //private InterfaceBT _bluetooth;
        private NetworkStream _wifiStream;
        private StreamReader _wifiReader;//ネーミングセンス皆無
        private StreamWriter _wifiWriter;//ネーミングセンス皆無


        #region Property
        public bool IsConnected { get; private set; }
        public ControlType.TcpPort Port { get; private set; }
        #endregion



        public ComROS()
        {
        }
        ~ComROS()
        {
            if (IsConnected) Disconnect();
        }






        #region Method
        public Task Connect(ControlType.TcpPort port)
        {
            if (IsConnected) throw new InvalidOperationException("Already connected to TCP/IP on Wifi");

            Port = port;
            return Task.Run(() =>
            {
                try
                {
                    var _client = new System.Net.Sockets.TcpClient(ControlType.TCP_IP_ADDRESS, (int)Port);

                    _wifiStream = _client.GetStream();
                    _wifiReader = new StreamReader(_wifiStream, Encoding.UTF8);
                    _wifiWriter = new StreamWriter(_wifiStream, Encoding.UTF8);
                }
                catch { throw; }
                IsConnected = true;
            });
        }

        public void Disconnect()
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to TCP/IP on Wifi");
            try
            {
                //_wifiReader.Close();
                _wifiStream.Close();
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
        public void Send(SendDataMsg msg)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected");
            try
            {
                _wifiWriter.WriteLine(msg.ConvString());
            }
            catch
            {
                Disconnect();
                throw new InvalidOperationException("Connecton error");
            }
        }


        public async Task<ReceiveDataMsg> ReadMsgAsync()
        {
            while (true)
            {
                var data = await _wifiReader.ReadLineAsync();

                try
                {
                    return new ReceiveDataMsg(data);
                }
                catch
                {
                    continue;
                }
            }
        }
        #endregion
    }
}
