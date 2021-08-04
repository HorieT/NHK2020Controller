using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ABU2021_ControlAndDebug.Core
{
    class ComRos : ComDevice
    {
        //private InterfaceWifi _wifi;
        //private InterfaceBT _bluetooth;
        private NetworkStream _wifiStream;
        private StreamReader _wifiReader;//ネーミングセンス皆無
        //private StreamWriter _wifiWriter;//ネーミングセンス皆無
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
                    //var _client = new System.Net.Sockets.TcpClient(ControlType.TCP_IP_ADDRESS, (int)Port);
                    var _client = new System.Net.Sockets.TcpClient(GetMyIpaddress(), (int)Port);

                    _wifiStream = _client.GetStream();
                    _wifiReader = new StreamReader(_wifiStream, Encoding.UTF8);
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
            catch(Exception ex)
            {
                Trace.WriteLine("Disconnect error. -> " + ex.ToString() + " : " + ex.Message);
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
            if (!IsConnected) throw new InvalidOperationException("Nonconnected");
            
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(msg.ConvString() + "\n");
                await _wifiStream.WriteAsync(data, 0, data.Length);
            }
            catch
            {
                Disconnect();
                throw;
            }
        }


        public async Task<ReceiveDataMsg> ReadMsgAsync()
        {
            while (_isConencted)
            {
                var data = await _wifiReader.ReadLineAsync();
                if (data == null) continue;
                //Trace.WriteLine("ReceiveDataMsg log. ->" + data);
                try
                {
                    return new ReceiveDataMsg(data);
                }
                catch(Exception ex)
                {
                    //読み取り失敗
                    Trace.WriteLine("Message reading failed. -> " + ex.ToString() + " : " + ex.Message + "\n" + data);
                    continue;
                }
            }
            throw new InvalidOperationException("Dissconnected");
        }
        #region private method
        private static string GetMyIpaddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return null;
        }
        #endregion
        #endregion
    }
}
