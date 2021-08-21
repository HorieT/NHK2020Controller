using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    class ControlTR : NotifyPropertyChanged
    {
        public static readonly double INJECT_SPEED_MAX = 10.0;//m/s

        #region Model
        private OutputLog _log;
        private Communicator _communicator;
        private MapProperty _mapProperty;
        private DebugSate _debugSate;
        #endregion

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
        private List<DebugData> _debugDatas = new List<DebugData>();


        #region Singleton instance
        private static ControlTR _instance;
        public static ControlTR GetInstance
        {
            get
            {
                return _instance ?? (_instance = new ControlTR());
            }
        }
        private ControlTR()
        {
            _log = OutputLog.GetInstance;
            _communicator = Communicator.GetInstance;
            _mapProperty = MapProperty.GetInstance;
            _debugSate = DebugSate.GetInstance;

            _communicator.PropertyChanged += _communicator_PropertyChanged;
            _stopwatch = Stopwatch.StartNew();

#if DEBUG
            /*
            PotsQueue.Add(Core.ControlType.Pot._1Left);
            PotsQueue.Add(Core.ControlType.Pot._1Left);
            PotsQueue.Add(Core.ControlType.Pot._1Left);
            */
#endif 
        }

        #endregion



        #region Property
        #region private
        private bool _isEnabled;
        private double _injectHeight = 0.8;
        // OneWay (input from device only / output to VM) : ReceiveDataType
        private bool _isEmergencyStopped;
        private bool _isMoveEndInject;
        private Vector _position = new Vector(2000, 2000);
        private double _positionRot;
        private string _machineState;
        private string _machineSequence;
        private ObservableCollection<Core.ControlType.Pot> _potsQueue = new ObservableCollection<Core.ControlType.Pot>();
        private double _offsetRad;


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
        public bool IsMoveEndInject
        {
            get => _isMoveEndInject;
            private set { SetProperty(ref _isMoveEndInject, value); }
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
        public string MachineState
        {
            get => _machineState;
            set { SetProperty(ref _machineState, value); }
        }
        public string MachineSequence
        {
            get => _machineSequence;
            set { SetProperty(ref _machineSequence, value); }
        }
        public ObservableCollection<Core.ControlType.Pot> PotsQueue { 
            get => _potsQueue;
            private set { 
                if(!_potsQueue.SequenceEqual(value))SetProperty(ref _potsQueue, value); } 
        }
        public double OffsetRad
        {
            get => _offsetRad;
            private set { SetProperty(ref _offsetRad, value); }
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


        public double GetArrowFallPos(double height, double speed, double deg)
        {
            double tmp1 = Math.Pow(speed * Math.Cos(deg * Math.PI/180), 2);
            double tmp2 = Math.Tan(deg * Math.PI / 180);
            return -tmp1 * (-tmp2 - Math.Sqrt(Math.Pow(tmp2, 2) + 2 * 9.8 * (InjectHeight - height) / tmp1)) / 9.8;
        }

        public void AddInjectQueue(Core.ControlType.Pot pot, int index)
        {
            if (!IsEnabaled) throw new InvalidOperationException("TR is not enbale");
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Negative value cannot be set");
            _communicator.SendMsg(new Core.SendDataMsg(Core.SendDataMsg.HeaderType.INJECT_Q_INS, new int[]{ (int)pot, index }));
            _log.WiteLine("ポットをキューに追加 : index " + index.ToString() + "->" + ((int)pot).ToString() + "[" + pot.ToString() + "]");
        }
        public void AddInjectQueue(Core.ControlType.Pot pot)
        {
            AddInjectQueue(pot, PotsQueue.Count);
        }
        public void DeleatInjectQueue(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "Negative value cannot be set");
            _communicator.SendMsg(new Core.SendDataMsg(Core.SendDataMsg.HeaderType.INJECT_Q_DEL, index));
        }

        public void SetArrowNumOnMachine(int num) 
        {
            if (0 > num && num > 5) throw new ArgumentOutOfRangeException("Arrow num range is 0~5. ");
            _communicator.SendMsg(new Core.SendDataMsg(Core.SendDataMsg.HeaderType.M_ARROW, num));
            _log.WiteLine("マシン上の矢数 : " + num.ToString() + " 本");

        }
        public void SetArrowNumOnRack(int num)
        {
            if (0 > num && num > 5) throw new ArgumentOutOfRangeException("Arrow num range is 0~5. ");
            _communicator.SendMsg(new Core.SendDataMsg(Core.SendDataMsg.HeaderType.RACK_ARROW, num));
            _log.WiteLine("ラック上の矢数 : " + num.ToString() + " 本");
        }
        public void SetRadOffset(double rad)
        {
            if (double.IsNaN(rad) || double.IsInfinity(rad)) throw new ArgumentException("Rad value isn't finite.");
            _communicator.SendMsg(new Core.SendDataMsg(Core.SendDataMsg.HeaderType.OFFSET_RAD, rad));
            _log.WiteLine("オフセットの更新要求 : " + rad.ToString() + " rad");
        }


        public void SaveDebugLog(string fileName)
        {

            try
            {
                using (StreamWriter stream = new StreamWriter(fileName, false))
                {
                    System.Reflection.FieldInfo[] fieldInfos = typeof(DebugData).GetFields();

                    foreach(var fieldInfo in fieldInfos)
                    {
                        stream.Write(fieldInfo.Name);
                        stream.Write(", ");
                    }
                    stream.WriteLine();


                    /*
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
                        case Core.ReceiveDataMsg.HeaderType.M_POS:
                            Positon = ((ValueTuple<Vector, double, int>)msg.Data).Item1;
                            PositonRot = ((ValueTuple<Vector, double, int>)msg.Data).Item2;
                            break;
                        case Core.ReceiveDataMsg.HeaderType.INJECT_Q:
                            PotsQueue = new ObservableCollection<Core.ControlType.Pot>((msg.Data as int[]).Cast<Core.ControlType.Pot>());
                            break;
                        case Core.ReceiveDataMsg.HeaderType.M_STATE:
                            MachineState = (msg.Data as string);
                            break;
                        case Core.ReceiveDataMsg.HeaderType.M_SEQUENCE:
                            MachineSequence = (msg.Data as string);
                            break;
                        case Core.ReceiveDataMsg.HeaderType.OFFSET_RAD:
                            OffsetRad = (double)msg.Data;
                            break;
                        case Core.ReceiveDataMsg.HeaderType.POT_POS:
                            {
                                var pots = (msg.Data as double[]);
                                _mapProperty.Pot1RightPos = new Vector(pots[0], pots[1]);
                                _mapProperty.Pot1LeftPos =  new Vector(pots[2], pots[3]);
                                _mapProperty.Pot2FrontPos = new Vector(pots[4], pots[5]);
                                _mapProperty.Pot2BackPos =  new Vector(pots[6], pots[7]);
                                _mapProperty.Pot3Pos =      new Vector(pots[8], pots[9]);
                            }
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
                    Trace.WriteLine("Message reading error");
                }
            }
        }
        private void _communicator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(_communicator.IsConnected))
            {
                IsEnabaled = _communicator.Device == Core.ControlType.Device.TR && _communicator.IsConnected;
                Task.Run(async () => { await ReadMsg(); });
            }
        }
        #endregion
    }
}
