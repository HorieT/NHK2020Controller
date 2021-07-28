using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;
using System.Diagnostics;

namespace ABU2021_ControlAndDebug.Core
{
    /// <summary>
    /// SharpDX.DirectInputを利用したこのアプリ用にチューンしたjoypadクラス
    /// </summary>
    class JoyPad
    {
        private DirectInput _directInput;
        private Joystick _joystick;
        private Guid _joystickGuid = Guid.Empty;// 使用するゲームパッドのID


        public bool IsEnabled{get => _joystick != null;}



        public JoyPad()
        {
            _directInput = new DirectInput();
        }

        ~JoyPad()
        {

        }


        public  List<DeviceInstance> GetDevices()
        {
            List<DeviceInstance> devices = new List<DeviceInstance>();
            devices.AddRange(_directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices));
            devices.AddRange(_directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices));
            devices.AddRange(_directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AllDevices));
            return devices;
        }

        public void SetDevice(DeviceInstance device, IntPtr windowHandle)
        {
            if (device == null) throw new ArgumentNullException();
            if (_joystick != null) DisposeDevice();

            _joystickGuid = device.InstanceGuid;// パッド入力周りの初期化
            _joystick = new Joystick(_directInput, _joystickGuid);
            if (_joystick != null)
            {
                // バッファサイズを指定
                _joystick.Properties.BufferSize = 128;
                //バックグラウンドで非占有にする(この場合はnullでもよさげ？)
                _joystick.SetCooperativeLevel(windowHandle, CooperativeLevel.Background | CooperativeLevel.NonExclusive);

                // 相対軸・絶対軸の最小値と最大値を
                // 指定した値の範囲に設定する
                foreach (DeviceObjectInstance deviceObject in _joystick.GetObjects())
                {
                    switch (deviceObject.ObjectId.Flags)
                    {
                        case DeviceObjectTypeFlags.Axis:
                        // 絶対軸or相対軸
                        case DeviceObjectTypeFlags.AbsoluteAxis:
                        // 絶対軸
                        case DeviceObjectTypeFlags.RelativeAxis:
                            // 相対軸
                            var ir = _joystick.GetObjectPropertiesById(deviceObject.ObjectId);
                            if (ir != null)
                            {
                                ir.Range = new InputRange(0, ushort.MaxValue);
                            }
                            break;
                    }
                }

                _joystick.Acquire();
                _joystick.Poll();
            }
            else
            {
                throw new InvalidOperationException("Connect to jpypad failed");
            }
        }
        public void DisposeDevice()
        {
            if (_joystick == null) return;//すでに開放済み

            _joystick.Dispose();
            _joystick = null;
            _joystickGuid = Guid.Empty;
        }



        public JoystickState GetJoy()
        {
            if (_joystick == null) throw new InvalidOperationException("Nonconeccted");

            try
            {
                _joystick.Poll();
                //_joystick.GetBufferedData();//こいつしか例外吐かない？？？
                return _joystick.GetCurrentState();
            }
            catch(Exception ex)
            {
                Trace.WriteLine("Joypad get state error. -> " + ex.ToString() + " : " + ex.Message);
                DisposeDevice();
                throw;
            }

        }




        public static uint ButtonConv(JoystickState joy)
        {
            return (
                (uint)(joy.PointOfViewControllers[0] == 0 ? 0x0010 : 0x0000) |
                (uint)(joy.PointOfViewControllers[0] == 4500 ? 0x0030 : 0x0000) |
                (uint)(joy.PointOfViewControllers[0] == 9000 ? 0x0020 : 0x0000) |
                (uint)(joy.PointOfViewControllers[0] == 13500 ? 0x0060 : 0x0000) |
                (uint)(joy.PointOfViewControllers[0] == 18000 ? 0x0040 : 0x0000) |
                (uint)(joy.PointOfViewControllers[0] == 22500 ? 0x00C0 : 0x0000) |
                (uint)(joy.PointOfViewControllers[0] == 27000 ? 0x0080 : 0x0000) |
                (uint)(joy.PointOfViewControllers[0] == 31500 ? 0x0090 : 0x0000) |
                (joy.Buttons[7] ? 0x00001u : 0u) |   //select
                (joy.Buttons[8] ? 0x00002u : 0u) |   //L3
                (joy.Buttons[9] ? 0x00004u : 0u) |   //R3
                (joy.Buttons[6] ? 0x00008u : 0u) |   //start
                (joy.Buttons[10] ? 0x00100u : 0u) |   //L2
                (joy.Buttons[11] ? 0x00200u : 0u) |   //R2
                (joy.Buttons[4] ? 0x00400u : 0u) |   //L1
                (joy.Buttons[5] ? 0x00800u : 0u) |   //R1
                (joy.Buttons[3] ? 0x01000u : 0u) |   //Y(三角)
                (joy.Buttons[1] ? 0x02000u : 0u) |   //B(丸)
                (joy.Buttons[0] ? 0x04000u : 0u) |   //A(バツ)
                (joy.Buttons[2] ? 0x08000u : 0u) |   //X(四角)
                (joy.Buttons[12] ? 0x10000u : 0u)     //HOME
                );
        }

        public static float AnalogToFloat(int analogData)
        {
            return ((float)analogData - ushort.MaxValue / 2 - 1) / ushort.MaxValue * 2;
        }
        public static byte AnalogToByte(int analogData)
        {
            return (byte)(((int)analogData - ushort.MaxValue / 2 - 1) / 256);
        }
    }
}
