using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVVMLib;

namespace ABU2021_ControlAndDebug.Core
{
    /// <summary>
    /// ROS及びSTMとの通信インターフェースを隠蔽・抽象化する為の抽象ラッパークラス
    /// </summary>
    abstract class DeviceInterfaceBace : NotifyPropertyChanged
    {
        public delegate void DataReceivedEventHandler(object sender);


        ~DeviceInterfaceBace()
        {
            try
            {
                Disconnect();
            }
            catch
            {

            }
        }

        #region Property
        private bool _isConnected;

        public bool IsConnected
        {
            get => _isConnected;
            protected set { SetProperty(ref _isConnected, value); }
        }
        #endregion


        #region Method
        public abstract Task Connect();
        public abstract void Disconnect();

        public abstract void Write(string data);
        public abstract void Write(Byte[] buffer, Int32 size);
        public abstract void WriteLine(string data);
        public abstract int Read(Byte[] buffer, Int32 size);
        public abstract Task<int> ReadAsync(Byte[] buffer, Int32 size);
        public abstract string ReadLine();
        public abstract Task<string> ReadLineAsync();
        public abstract string ReadTo(string value);
        #endregion



        public event DataReceivedEventHandler DataReceived;

        protected void DataReceivedEventWrap()
        {
            DataReceived(this);
        }
    }
}
