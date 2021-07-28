using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const int _checkTime = 30;
        private Timer _checkTimer;
        private bool _is_check_error = false;
        private SynchronizationContext _mainContext;


        #region Model
        public Models.JoypadHandl Joypad;
        #endregion


        public JoypadTest()
        {
            #region get instance
            Joypad = Models.JoypadHandl.GetInstance;
            _mainContext = SynchronizationContext.Current;
            #endregion

            _is_check_error = false;
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
                BitArray bit = new BitArray(pad.Buttons);
                int[] button = new int[bit.Count / 32];
                bit.CopyTo(button, 0);

                Xpos = pad.X.ToString();
                Ypos = pad.Y.ToString();
                Zpos = pad.Z.ToString();
                Xrot = pad.RotationX.ToString();
                Yrot = pad.RotationY.ToString();
                Zrot = pad.RotationZ.ToString();
                Button = button[0].ToString("X8");
                POV = pad.PointOfViewControllers[0].ToString();

                Button_A = pad.Buttons[0];
                Button_B = pad.Buttons[1];
                Button_X = pad.Buttons[2];
                Button_Y = pad.Buttons[3];
                Button_L1 = pad.Buttons[4];
                Button_R1 = pad.Buttons[5];
                Button_L2 = pad.Buttons[10];
                Button_R2 = pad.Buttons[11];
                Button_L3 = pad.Buttons[8];
                Button_R3 = pad.Buttons[9];
                Button_Up = (pad.PointOfViewControllers[0] == 31500) || (pad.PointOfViewControllers[0] == 0) || (pad.PointOfViewControllers[0] == 4500);
                Button_Right = (pad.PointOfViewControllers[0] == 4500) || (pad.PointOfViewControllers[0] == 9000) || (pad.PointOfViewControllers[0] == 13500);
                Button_Down = (pad.PointOfViewControllers[0] == 13500) || (pad.PointOfViewControllers[0] == 18000) || (pad.PointOfViewControllers[0] == 22500);
                Button_Left = (pad.PointOfViewControllers[0] == 22500) || (pad.PointOfViewControllers[0] == 27000) || (pad.PointOfViewControllers[0] == 31500);

                AnalogLeftX = (int)(((float)pad.X) / ushort.MaxValue * 75);
                AnalogLeftY = (int)(((float)pad.Y) / ushort.MaxValue * 75);
                AnalogRightX = (int)(((float)pad.RotationX) / ushort.MaxValue * 75);
                AnalogRightY = (int)(((float)pad.RotationY) / ushort.MaxValue * 75);
            }
            catch(Exception ex)
            {
                if (!_is_check_error)
                {
                    _mainContext.Post(_ => {
                        _is_check_error = true;
                        _checkTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        MessageBox.Show("Joypadが認識できません", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                        //Application.Current.MainWindow.Close();
                    }, null);

                    Trace.WriteLine("Joypad get state error. -> " + ex.ToString() + " : " + ex.Message);
                }
                return;
            }
        }
        #endregion
    }
}
