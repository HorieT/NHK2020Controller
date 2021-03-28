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
    class ComRos : ComDevice
    {
        //private InterfaceWifi _wifi;
        //private InterfaceBT _bluetooth;
        private NetworkStream _wifiStream;
        private StreamReader _wifiReader;//ネーミングセンス皆無
        private StreamWriter _wifiWriter;//ネーミングセンス皆無
        private bool _isConencted = false;


        #region Property
        public bool IsConnected { get => _isConencted; }
        public ControlType.TcpPort Port { get; set; }
        #endregion



        public ComRos()
        {
        }
        ~ComRos()
        {
            if (IsConnected) Disconnect();
        }






        #region Method
        public Task Connect()
        {
            if (IsConnected) throw new InvalidOperationException("Already connected to TCP/IP on Wifi");

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
                _isConencted = true;
            });
        }

        public void Disconnect()
        {
            if (!IsConnected) return;//throw new InvalidOperationException("Not connected to TCP/IP on Wifi");
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
                _isConencted = false;
            }
        }


        /// <summary>
        /// 送信
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="header"></param>
        /// <param name="data"></param>
        public async Task SendMsgAsync(SendDataMsg msg)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected");
            
            try
            {
                await _wifiWriter.WriteLineAsync(msg.ConvString());
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
