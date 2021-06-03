using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MVVMLib;

namespace ABU2021_ControlAndDebug.Models
{
    /// <summary>
    /// TRの状態とTRへの入力モデル
    /// プロパティの変更権を雑に外側に握らせてるのが気に食わないけど
    /// いい方法が思いつかなかった
    /// </summary>
    class ControlTR0 : NotifyPropertyChanged
    {
        public static readonly double INJECT_SPEED_MAX = 7.5;//m/s
        public static readonly double INJECT_ANGLE_MAX = 47.0;//deg
        public static readonly double INJECT_ANGLE_MIN = 27.0;//deg


        private OutputLog _log;
        private Communicator _communicator;
        private DebugSate _debugSate;


        private struct DebugData
        {
            public double time_ms;
            public double od_x;
            public double od_y;
            public double od_theta;
            public double pos_x;
            public double pos_y;
            public double pos_theta;
        }
        private Stopwatch _stopwatch;
        private List<DebugData> _debugDatas;


        #region Singleton instance
        private static ControlTR0 _instance;
        public static ControlTR0 GetInstance
        {
            get
            {
                return _instance ?? (_instance = new ControlTR0());
            }
        }
        private ControlTR0()
        {
            _log = OutputLog.GetInstance;
            _communicator = Communicator.GetInstance;
            _debugSate = DebugSate.GetInstance;

            _communicator.PropertyChanged += _communicator_PropertyChanged;
            _stopwatch = Stopwatch.StartNew();
        }

        #endregion



        #region Property
        #region private
        private bool _isEnabled;
        private double _injectHeight = 0.8;
        // OneWay (input from device only / output to VM) : ReceiveDataType
        private bool _isEmergencyStopped;
        private byte _loaderPoint;
        private bool _isOpenedRecovery;
        private float _injectAngleNow;
        private bool _isMoveEndInjectAngle;
        private bool _isMoveEndInjecInit;
        private bool _isMoveEndInject;
        private bool _isMoveEndRecovery;
        private bool _isMoveEndLoader;
        private Vector _position = new Vector(2000, 2000);
        private double _positionRot;


        #endregion
        #region public
        public bool IsEnabaled
        {
            get => _isEnabled;
            set { SetProperty(ref _isEnabled, value); }
        }
        public double InjectHeight
        {
            get => _injectHeight;
            set { SetProperty(ref _injectHeight, value); }
        }

        #region Get only
        public bool IsEmergencyStopped
        {
            get => _isEmergencyStopped;
            private set { SetProperty(ref _isEmergencyStopped, value); }
        }
        public byte LoaderPoint
        {
            get => _loaderPoint;
            private set { SetProperty(ref _loaderPoint, value); }
        }
        public bool IsOpenedRecovery
        {
            get => _isOpenedRecovery;
            private set { SetProperty(ref _isOpenedRecovery, value); }
        }
        public float InjectAngleNow
        {
            get => _injectAngleNow;
            private set { SetProperty(ref _injectAngleNow, value); }
        }
        public bool IsMoveEndInjectAngle
        {
            get => _isMoveEndInjectAngle;
            private set { SetProperty(ref _isMoveEndInjectAngle, value); }
        }
        public bool IsMoveEndInjecInit
        {
            get => _isMoveEndInjecInit;
            private set { SetProperty(ref _isMoveEndInjecInit, value); }
        }
        public bool IsMoveEndInject
        {
            get => _isMoveEndInject;
            private set { SetProperty(ref _isMoveEndInject, value); }
        }
        public bool IsMoveEndRecovery
        {
            get => _isMoveEndRecovery;
            private set { SetProperty(ref _isMoveEndRecovery, value); }
        }
        public bool IsMoveEndLoader
        {
            get => _isMoveEndLoader;
            private set { SetProperty(ref _isMoveEndLoader, value); }
        }
        public Vector Positon
        {
            get => _position;
            private set { SetProperty(ref _position, value); }
        }
        public double PositonRot
        {
            get => _positionRot;
            private set { SetProperty(ref _positionRot, value); }
        }
        #endregion
        #endregion
        #endregion



        #region Method
        public void Inject(double speed)
        {
            if (speed < 0.0) throw new ArgumentOutOfRangeException(nameof(speed), "Negative value cannot be set");
            _communicator.SendMsg(new Core.SendDataMsg(Core.SendDataMsg.HeaderType.INJECT, (float)speed));
        }
        public void ChangeAngle(double deg)
        {
            if (deg < 0.0) throw new ArgumentOutOfRangeException(nameof(deg), "Negative value cannot be set");
            _communicator.SendMsg(new Core.SendDataMsg(Core.SendDataMsg.HeaderType.I_ANGLE, (float)(deg * Math.PI / 180.0)));
        }


        public double GetArrowFallPos(double height, double speed, double deg)
        {
            double tmp1 = Math.Pow(speed * Math.Cos(deg * Math.PI/180), 2);
            double tmp2 = Math.Tan(deg * Math.PI / 180);
            return -tmp1 * (-tmp2 - Math.Sqrt(Math.Pow(tmp2, 2) + 2 * 9.8 * (InjectHeight - height) / tmp1)) / 9.8;
        }


        public void SaveDebugLog(string fileName)
        {

            try
            {
                using (StreamWriter stream = new StreamWriter(fileName, false))
                {/*
                    stream.WriteLine("!option");
                    stream.Write("<point ");
                    stream.Write("size=\"" + _userDotList.Size.ToString(CultureInfo.GetCultureInfo("ja-JP")) + "\" ");
                    stream.Write("color=\"" + _userDotList.Brush.ToString(CultureInfo.GetCultureInfo("ja-JP")) + "\" ");
                    stream.WriteLine(">");
                    stream.Write("<interpolation ");
                    stream.Write("size=\"" + _generatedPathDotList.Size.ToString(CultureInfo.GetCultureInfo("ja-JP")) + "\" ");
                    stream.Write("color=\"" + _generatedPathDotList.Brush.ToString(CultureInfo.GetCultureInfo("ja-JP")) + "\" ");
                    stream.WriteLine(">");
                    stream.Write("<origen ");
                    stream.Write("size=\"" + DotOreginHarf.ToString(CultureInfo.GetCultureInfo("ja-JP")) + "\" ");
                    stream.Write("color=\"" + DotOregin.Fill.ToString(CultureInfo.GetCultureInfo("ja-JP")) + "\" ");
                    stream.WriteLine(">");

                    stream.WriteLine("!map");
                    stream.Write("<map ");
                    stream.Write("path=\"" + mapPath + "\" ");
                    stream.Write("oregin=\"" + MapOrigen.X.ToString("N", CultureInfo.GetCultureInfo("ja-JP")) + "," + MapOrigen.Y.ToString("N", CultureInfo.GetCultureInfo("ja-JP")) + "\" ");
                    stream.Write("scale=\"" + MapScale.ToString("N", CultureInfo.GetCultureInfo("ja-JP")) + "\" ");
                    stream.WriteLine(">");

                    stream.WriteLine("!point");
                    stream.Write("<point ");
                    stream.Write("item=\"");
                    foreach(PointDot dot in _userDotList.Point)
                    {
                        stream.Write("(" + dot.X.ToString(CultureInfo.GetCultureInfo("ja-JP")) + "," + dot.Y.ToString(CultureInfo.GetCultureInfo("ja-JP")) + "," + dot.Angle.ToString(CultureInfo.GetCultureInfo("ja-JP")) + ")");
                    }
                    stream.Write("\" ");
                    stream.WriteLine(">");

                    stream.WriteLine("!file");
                    stream.Write("<file ");
                    stream.Write("name=\"" + genFileName.Text + "\" ");
                    //stream.Write("type=\"" +  "\" ");
                    stream.Write("root=\"" + generatedPath + "\" ");
                    stream.WriteLine(">");

                    stream.WriteLine("!interpolation");
                    stream.Write("<menu ");
                    //stream.Write("method=\"" + genFileName.Text + "\" ");
                    //stream.Write("option=\"" +  "\" ");
                    stream.Write("div=\"" + divText.Text + "\" ");
                    stream.Write("vel=\"" + topSpeedText.Text + "\" ");
                    stream.Write("acc=\"" + accText.Text + "\" ");
                    stream.WriteLine(">");

                    stream.WriteLine("!end");*/
                }
            }
            catch
            {
                //outputLogText.Text += "パスが不適切か、またはファイルへのアクセスが拒否されました。\n";
                throw;
            }
            //outputLogText.Text += "プロジェクトを保存しました。\n\n";
        }


        private async Task ReadMsg()
        {
            while (IsEnabaled)
            {
                var msg = await _communicator.ReadMsgAsync();

                try
                {
                    switch (msg.Header)
                    {
                    case Core.ReceiveDataMsg.HeaderType.POSITION:
                            Positon = ((ValueTuple<Vector, double>)msg.Data).Item1;
                            PositonRot = ((ValueTuple<Vector, double>)msg.Data).Item2;
                        break;
                    case Core.ReceiveDataMsg.HeaderType.DEBUG_POS:
                            var taple = (ValueTuple<Vector, double, Vector, double>)msg.Data;
                            _debugDatas.Add(new DebugData() { 
                                time_ms = _stopwatch.Elapsed.TotalMilliseconds,
                                od_x = taple.Item1.X,
                                od_y = taple.Item1.Y,
                                od_theta = taple.Item2,
                                pos_x = taple.Item3.X,
                                pos_y = taple.Item3.Y,
                                pos_theta = taple.Item4
                            });
                        break;
                    default:
                        break;
                    }
                }
                catch (Exception e)
                {
                    _log.WiteDebugMsg(e.Message);
                    throw;
                }
            }
        }
        private void _communicator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(_communicator.IsConnected))
            {
                IsEnabaled = _communicator.Device == Core.ControlType.Device.TR0 && _communicator.IsConnected;
                Task.Run(async () => { await ReadMsg(); });
            }
        }
        #endregion
    }
}
