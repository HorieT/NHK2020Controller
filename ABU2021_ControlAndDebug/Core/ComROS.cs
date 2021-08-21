using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace ABU2021_ControlAndDebug.Core
{
    class ComRos : ComDevice
    {
        //private InterfaceWifi _wifi;
        //private InterfaceBT _bluetooth;
        private TcpClient _client;
        private NetworkStream _wifiStream;
        private StreamReader _wifiReader;//ネーミングセンス皆無
        //private StreamWriter _wifiWriter;//ネーミングセンス皆無
        private static System.Threading.SemaphoreSlim _semaphore = new System.Threading.SemaphoreSlim(1, 1);

        #region Property
        public bool IsConnected { get => _client?.Connected ?? false; }
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
#if DEBUG
                    var ip = GetMyIpaddress();
#else
                    var ip = ControlType.TCP_IP_ADDRESS;
#endif
                    Trace.WriteLine("Tcp cliant IP : " + ip);
                    _client = new System.Net.Sockets.TcpClient();
                    if (!_client.ConnectAsync(ip, (int)Port).Wait(1000)) throw new TimeoutException("TCP connection timeouted : 1000ms");
                }
                catch (Exception ex){
                    _client = null;
                    Trace.WriteLine("TCP server connect failed ->" + ex.Message);
                    throw;
                }
                _wifiStream = _client.GetStream();
                _wifiReader = new StreamReader(_wifiStream, Encoding.UTF8);
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
            byte[] data;
            try
            {
                 data = Encoding.UTF8.GetBytes(msg.ConvString() + "\n");
            }
            catch(Exception)
            {
                Trace.WriteLine("SendMsg convert error -> " + msg.Header.ToString() + ":" + msg.Data.ToString());
                return;
            }
            await _semaphore.WaitAsync(); // ロックを取得する
            try
            {
                await _wifiStream.WriteAsync(data, 0, data.Length);
            }
            catch
            {
                Disconnect();
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }


        public async Task<ReceiveDataMsg> ReadMsgAsync()
        {
            while (IsConnected)
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
