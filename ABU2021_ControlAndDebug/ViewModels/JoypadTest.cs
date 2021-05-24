using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using MVVMLib;

namespace ABU2021_ControlAndDebug.ViewModels
{
    class JoypadTest : ViewModel
    {
        private const int _checkTime = 50;
        private Timer _checkTimer;


        #region Model
        public Models.JoypadHandl Joypad;
        #endregion


        public JoypadTest()
        {
            #region get instance
            Joypad = Models.JoypadHandl.GetInstance;
            #endregion

            _checkTimer = new Timer(CheckButton, null, 0, _checkTime);
        }


        #region Property
        private int _anlogLeftX;
        private int _anlogLeftY;
        private int _anlogRightX;
        private int _anlogRightY;

        private bool _Button_L1;
        private bool _Button_L2;
        private bool _Button_L3;
        private bool _Button_R1;
        private bool _Button_R2;
        private bool _Button_R3;
        private bool _Button_Up;
        private bool _Button_Down;
        private bool _Button_Right;
        private bool _Button_Left;
        private bool _Button_A;
        private bool _Button_B;
        private bool _Button_X;
        private bool _Button_Y;

        private string _size;
        private string _flags;
        private string _xpos;
        private string _ypos;
        private string _zpos;
        private string _xrot;
        private string _yrot;
        private string _zrot;
        private string _button;
        private string _buttonNum;
        private string _pov;
        private string _reserved1;
        private string _reserved2;


        public int AnalogLeftX
        {
            get => _anlogLeftX;
            private set { SetProperty(ref _anlogLeftX, value); }
        }
        public int AnalogLeftY
        {
            get => _anlogLeftY;
            private set { SetProperty(ref _anlogLeftY, value); }
        }
        public int AnalogRightX
        {
            get => _anlogRightX;
            private set { SetProperty(ref _anlogRightX, value); }
        }
        public int AnalogRightY
        {
            get => _anlogRightY;
            private set { SetProperty(ref _anlogRightY, value); }
        }
        public bool Button_L1
        {
            get => _Button_L1;
            private set { SetProperty(ref _Button_L1, value); }
        }
        public bool Button_L2
        {
            get => _Button_L2;
            private set { SetProperty(ref _Button_L2, value); }
        }
        public bool Button_L3
        {
            get => _Button_L3;
            private set { SetProperty(ref _Button_L3, value); }
        }
        public bool Button_R1
        {
            get => _Button_R1;
            private set { SetProperty(ref _Button_R1, value); }
        }
        public bool Button_R2
        {
            get => _Button_R2;
            private set { SetProperty(ref _Button_R2, value); }
        }
        public bool Button_R3
        {
            get => _Button_R3;
            private set { SetProperty(ref _Button_R3, value); }
        }
        public bool Button_Up
        {
            get => _Button_Up;
            private set { SetProperty(ref _Button_Up, value); }
        }
        public bool Button_Down
        {
            get => _Button_Down;
            private set { SetProperty(ref _Button_Down, value); }
        }
        public bool Button_Right
        {
            get => _Button_Right;
            private set { SetProperty(ref _Button_Right, value); }
        }
        public bool Button_Left
        {
            get => _Button_Left;
            private set { SetProperty(ref _Button_Left, value); }
        }
        public bool Button_A
        {
            get => _Button_A;
            private set { SetProperty(ref _Button_A, value); }
        }
        public bool Button_B
        {
            get => _Button_B;
            private set { SetProperty(ref _Button_B, value); }
        }
        public bool Button_X
        {
            get => _Button_X;
            private set { SetProperty(ref _Button_X, value); }
        }
        public bool Button_Y
        {
            get => _Button_Y;
            private set { SetProperty(ref _Button_Y, value); }
        }

        public string Size
        {
            get => _size;
            set { SetProperty(ref _size, value); }
        }
        public string Flags
        {
            get => _flags;
            set { SetProperty(ref _flags, value); }
        }
        public string Xpos
        {
            get => _xpos;
            set { SetProperty(ref _xpos, value); }
        }
        public string Ypos
        {
            get => _ypos;
            set { SetProperty(ref _ypos, value); }
        }
        public string Zpos
        {
            get => _zpos;
            set { SetProperty(ref _zpos, value); }
        }
        public string Xrot
        {
            get => _xrot;
            set { SetProperty(ref _xrot, value); }
        }
        public string Yrot
        {
            get => _yrot;
            set { SetProperty(ref _yrot, value); }
        }
        public string Zrot
        {
            get => _zrot;
            set { SetProperty(ref _zrot, value); }
        }
        public string Button
        {
            get => _button;
            set { SetProperty(ref _button, value); }
        }
        public string ButtonNum
        {
            get => _buttonNum;
            set { SetProperty(ref _buttonNum, value); }
        }
        public string POV
        {
            get => _pov;
            set { SetProperty(ref _pov, value); }
        }
        public string Reserved1
        {
            get => _reserved1;
            set { SetProperty(ref _reserved1, value); }
        }
        public string Reserved2
        {
            get => _reserved2;
            set { SetProperty(ref _reserved2, value); }
        }
        #endregion


        #region Command
        #endregion


        #region Method
        private void CheckButton(object sender)
        {
            try
            {
                var pad = Joypad.GetPad();

                Size = pad.JoyInfoEx.dwSize.ToString();
                Flags = pad.JoyInfoEx.dwFlags.ToString();
                Xpos = pad.JoyInfoEx.dwXpos.ToString();
                Ypos = pad.JoyInfoEx.dwYpos.ToString();
                Zpos = pad.JoyInfoEx.dwZpos.ToString();
                Xrot = pad.JoyInfoEx.dwXrot.ToString();
                Yrot = pad.JoyInfoEx.dwYrot.ToString();
                Zrot = pad.JoyInfoEx.dwZrot.ToString();
                Button = pad.JoyInfoEx.dwButtons.ToString("X8");
                ButtonNum = pad.JoyInfoEx.dwButtonNumber.ToString();
                POV = pad.JoyInfoEx.dwPOV.ToString();
                Reserved1 = pad.JoyInfoEx.dwReserved1.ToString();
                Reserved2 = pad.JoyInfoEx.dwReserved2.ToString();

                Button_A = (pad.JoyInfoEx.dwButtons & 0x0001) != 0u;
                Button_B = (pad.JoyInfoEx.dwButtons & 0x0002) != 0u;
                Button_X = (pad.JoyInfoEx.dwButtons & 0x0004) != 0u;
                Button_Y = (pad.JoyInfoEx.dwButtons & 0x0008) != 0u;
                Button_L1 = (pad.JoyInfoEx.dwButtons & 0x0010) != 0u;
                Button_R1 = (pad.JoyInfoEx.dwButtons & 0x0020) != 0u;
                Button_L2 = (pad.JoyInfoEx.dwButtons & 0x0400) != 0u;
                Button_R2 = (pad.JoyInfoEx.dwButtons & 0x0800) != 0u;
                Button_L3 = (pad.JoyInfoEx.dwButtons & 0x0100) != 0u;
                Button_R3 = (pad.JoyInfoEx.dwButtons & 0x0200) != 0u;
                Button_Up = (pad.JoyInfoEx.dwPOV == 31500) || (pad.JoyInfoEx.dwPOV == 0) || (pad.JoyInfoEx.dwPOV == 4500);
                Button_Right = (pad.JoyInfoEx.dwPOV == 4500) || (pad.JoyInfoEx.dwPOV == 9000) || (pad.JoyInfoEx.dwPOV == 13500);
                Button_Down = (pad.JoyInfoEx.dwPOV == 13500) || (pad.JoyInfoEx.dwPOV == 18000) || (pad.JoyInfoEx.dwPOV == 22500);
                Button_Left = (pad.JoyInfoEx.dwPOV == 22500) || (pad.JoyInfoEx.dwPOV == 27000) || (pad.JoyInfoEx.dwPOV == 31500);

                AnalogLeftX = (int)(((float)pad.JoyInfoEx.dwXpos) / ushort.MaxValue * 75);
                AnalogLeftY = (int)(((float)pad.JoyInfoEx.dwYpos) / ushort.MaxValue * 75);
                AnalogRightX = (int)(((float)pad.JoyInfoEx.dwXrot) / ushort.MaxValue * 75);
                AnalogRightY = (int)(((float)pad.JoyInfoEx.dwYrot) / ushort.MaxValue * 75);
            }
            catch
            {
                _checkTimer.Change(Timeout.Infinite, Timeout.Infinite);
                //MessageBox.Show("Joypadが認識できません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        #endregion
    }
}
