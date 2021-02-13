using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Management;
using System.Text.RegularExpressions;

namespace ABU2021_ControlAndDebug.Core
{
    /// <summary>
    /// Win32_PnPEntityを通してUSBポートにのみ接続するクラス
    /// </summary>
    class InterfaceUSB : DeviceInterfaceBace
    {
        public InterfaceUSB()
        {

        }


        #region Property
        public SerialPort Port { get; private set; }
        #endregion


        #region Method
        public static IReadOnlyList<Comport> GetComports()
        {
            var list = new List<Comport>();
            var management = new ManagementClass("Win32_PnPEntity");

            foreach (var m in management.GetInstances()) using (m)
                {
                    list.Add(new Comport()
                    {
                        DeviceID = (string)m.GetPropertyValue("DeviceID"),
                        Description = (string)m.GetPropertyValue("Caption"),
                        PNPDeviceID = (string)m.GetPropertyValue("PNPDeviceID")
                    });
                }

            return list.AsReadOnly();
        }


        public void DestinationSelection(int vid, int pid)
        {
            try { DestinationSelection(vid, pid, GetComports()); }
            catch { throw; }
        }
        public void DestinationSelection(int vid, int pid, IReadOnlyList<Comport> ports)
        {
            if (IsConnected) throw new InvalidOperationException("Already connected to USB port");
            if (ports.Count(i => i.VID == vid) == 0) throw new ArgumentException("No matching port was found", nameof(vid));
            var pnpPort = ports.FirstOrDefault(i => (i.VID == vid && i.PID == pid));
            if (pnpPort == null) throw new ArgumentException("No matching port was found", nameof(pid));

            Port = new SerialPort(pnpPort.PortName, 115200, Parity.None, 8, StopBits.One);
        }
        private void PortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataReceivedEventWrap();
        }


        #region Override method
        public override Task Connect()
        {
            if (IsConnected) throw new InvalidOperationException("Already connected to USB port");
            if (Port == null) throw new InvalidOperationException("The connection destination is not selected");

            return Task.Run(() => {
                try { Port.Open(); }
                catch { throw; }

                Port.DataReceived += PortDataReceived;
                IsConnected = true;
            });
        }
        public override void Disconnect()
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected to USB port");
            if (Port == null) throw new InvalidOperationException("The connection destination is not selected");

            try { Port.Close(); }
            catch { throw; }

            Port.DataReceived -= PortDataReceived;
            IsConnected = false;
        }


        public override void Write(string data)
        {
            if (Port == null) throw new InvalidOperationException("The connection destination is not selected");
            try { Port.Write(data); }
            catch { throw; }
        }
        public override void Write(Byte[] buffer, Int32 size)
        {
            if (Port == null) throw new InvalidOperationException("The connection destination is not selected");
            try { Port.Write(buffer, 0, size); }
            catch { throw; }
        }
        public override void WriteLine(string data)
        {
            if (Port == null) throw new InvalidOperationException("The connection destination is not selected");
            try { Port.WriteLine(data); }
            catch { throw; }
        }
        public override int Read(Byte[] buffer, Int32 size)
        {
            if (Port == null) throw new InvalidOperationException("The connection destination is not selected");
            try { return Port.Read(buffer, 0, size); }
            catch { throw; }
        }
        public override Task<int> ReadAsync(Byte[] buffer, Int32 size)
        {
            return Task.Run(() =>
            {
                if (Port == null) return 0;
                try { return Port.Read(buffer, 0, size); }
                catch { return 0; }
            });
        }
        public override string ReadLine()
        {
            if (Port == null) throw new InvalidOperationException("The connection destination is not selected");
            try { return Port.ReadLine(); }
            catch { throw; }
        }
        public override Task<string> ReadLineAsync()
        {
            return Task.Run(() =>
            {
                if (Port == null) return null;
                try { return Port.ReadLine(); }
                catch { return null; }
            });
        }
        public override string ReadTo(string value)
        {
            if (Port == null) return null;
            try { return Port.ReadTo(value); }
            catch { return null; }
        }
        #endregion
        #endregion
    }
}
