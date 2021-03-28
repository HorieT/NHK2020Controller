using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoypadControl
{
    public class Xbox360_JoyPad : Joypad
    {
        public bool ButtonA
        { get { return ((JoyInfoEx.dwButtons & JOY_BUTTON1) != 0); } }

        public bool ButtonB
        { get { return ((JoyInfoEx.dwButtons & JOY_BUTTON2) != 0); } }

        public bool ButtonX
        { get { return ((JoyInfoEx.dwButtons & JOY_BUTTON3) != 0); } }

        public bool ButtonY
        { get { return ((JoyInfoEx.dwButtons & JOY_BUTTON4) != 0); } }

        public bool ButtonLeftShoulder
        { get { return ((JoyInfoEx.dwButtons & JOY_BUTTON5) != 0); } }

        public bool ButtonRightShoulder
        { get { return ((JoyInfoEx.dwButtons & JOY_BUTTON6) != 0); } }

        public bool ButtonBack
        { get { return ((JoyInfoEx.dwButtons & JOY_BUTTON7) != 0); } }

        public bool ButtonStart
        { get { return ((JoyInfoEx.dwButtons & JOY_BUTTON8) != 0); } }

        public bool ButtonLeftStick
        { get { return ((JoyInfoEx.dwButtons & JOY_BUTTON9) != 0); } }

        public bool ButtonRightStick
        { get { return ((JoyInfoEx.dwButtons & JOY_BUTTON10) != 0); } }

        public float LeftStickX
        { get { return (((float)JoyInfoEx.dwXpos - 32767) / 32768); } }

        public float LeftStickY
        { get { return (((float)JoyInfoEx.dwYpos - 32767) / 32768); } }

        public float RightStickX
        { get { return (((float)JoyInfoEx.dwYrot - 32767) / 32768); } }

        public float RightStickY
        { get { return (((float)JoyInfoEx.dwZrot - 32767) / 32768); } }

        public float Trigger
        { get { return (((float)JoyInfoEx.dwZpos - 32767) / 32768); } }
    }
}
