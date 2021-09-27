using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MVVMLib;

namespace ABU2021_ControlAndDebug.ViewModels
{
    /// <summary>
    /// マップタブのVM
    /// 座標データを画像のピクセル位置に変換
    /// </summary>
    class MapControl : ViewModel
    {
        private static readonly double MagnificationScale = 1.15;//拡大縮小コントロール値
        private static readonly double MachineSizeDef = 1.000;//描画用の想定値なので実際とは関係ない
        private Point _scrollViewerDragStart;
        private Core.ControlType.Pot CapturedPot;
        private bool IsPotCaptured;

        #region Model
        public Models.OutputLog Log { get; private set; }
        public Models.MapProperty MapProperty { get; private set; }
        public Models.ControlTR TR{ get; private set; }
        public Models.ControlDR DR { get; private set; }
        public Models.Communicator Communicator { get; set; }
        public Models.DebugSate DebugSate { get; private set; }
        #endregion


        public MapControl()
        {
            #region get instance
            Log = Models.OutputLog.GetInstance;
            MapProperty = Models.MapProperty.GetInstance;
            TR = Models.ControlTR.GetInstance;
            DR = Models.ControlDR.GetInstance;
            Communicator = Models.Communicator.GetInstance;
            DebugSate = Models.DebugSate.GetInstance;
            #endregion

            #region set img
            MapHeight = MapProperty.MapPictureSoruce.PixelHeight;
            MapWidth = MapProperty.MapPictureSoruce.PixelWidth;
            TableHeight = MapProperty.Table2PictureSoruce.PixelHeight;
            TableWidth = MapProperty.Table2PictureSoruce.PixelWidth;
            MapScale = new Vector(MapWidth / Models.MapProperty.MapSize.X, MapHeight / Models.MapProperty.MapSize.Y);
            Core.Pot.CommonVal.Diameter = Models.MapProperty.PotOuterDiameter * MapScale.X;
            #endregion

            #region set property
            Pots.Add(new Core.Pot { Name = "0⃣", Tag = Core.ControlType.Pot._1Right});
            Pots.Add(new Core.Pot { Name = "1⃣", Tag = Core.ControlType.Pot._1Left });
            Pots.Add(new Core.Pot { Name = "2⃣", Tag = Core.ControlType.Pot._2Front });
            Pots.Add(new Core.Pot { Name = "3⃣", Tag = Core.ControlType.Pot._2Back });
            Pots.Add(new Core.Pot { Name = "4⃣", Tag = Core.ControlType.Pot._3 });
            #endregion


            MapProperty.PropertyChanged += MapProperty_PropertyChanged;
            TR.PropertyChanged += TR_PropertyChanged;
            DR.PropertyChanged += DR_PropertyChanged;
            Communicator.PropertyChanged += Communicator_PropertyChanged;
            DebugSate.PropertyChanged += DebugSate_PropertyChanged;
        }



        #region Property
        private Vector _mapScale;
        private double _mapWidth;
        private double _mapHeight;
        private double _tableWidth;
        private double _tableHeight;
        private double _table2FrontRot = 0.0;
        private double _table2BackRot = 90.0;
        private double _table3Rot = 0.0;
        private bool _isMachineEnabled;
        private Vector _machineCenter;
        private double _machineRot;
        private string _machineName;
        private Brush _teamColor = new SolidColorBrush(Colors.Blue);
        private ObservableNotifyCollection<Core.Pot> _pots = new ObservableNotifyCollection<Core.Pot>();
        //private Transform _canvasTransform;


        #region oneTime bind
        public double MapWidth
        {
            get => _mapWidth;
            private set { SetProperty(ref _mapWidth, value); }
        }
        public double MapHeight
        {
            get => _mapHeight;
            private set { SetProperty(ref _mapHeight, value); }
        }
        public double TableWidth
        {
            get => _tableWidth;
            private set { SetProperty(ref _tableWidth, value); }
        }
        public double TableHeight
        {
            get => _tableHeight;
            private set { SetProperty(ref _tableHeight, value); }
        }
        public Vector MapScale
        {
            get => _mapScale;
            set
            {
                if (SetProperty(ref _mapScale, value))
                {
                    RaisePropertyChanged("Tabele2aOffset");
                    RaisePropertyChanged("Tabele2bOffset");
                    RaisePropertyChanged("Tabele3Offset");
                    RaisePropertyChanged("Tabele2aCenter");
                    RaisePropertyChanged("Tabele2bCenter");
                    RaisePropertyChanged("Tabele3Center");
                }
            }
        }
        public Vector Tabele2FrontOffset { 
            get => RealToCanvas(Models.MapProperty.Tabele2FrontPoint + Models.MapProperty.TabeleSize * 0.5);
        }
        public Vector Tabele2BackOffset
        {
            get => RealToCanvas(Models.MapProperty.Tabele2BackPoint + Models.MapProperty.TabeleSize * 0.5);
        }
        public Vector Tabele3Offset
        {
            get => RealToCanvas(Models.MapProperty.Tabele3Point + Models.MapProperty.TabeleSize * 0.5);
        }
        public Vector Tabele2FrontCenter
        {
            get => RealToCanvas(Models.MapProperty.Tabele2FrontPoint);
        }
        public Vector Tabele2BackCenter
        {
            get => RealToCanvas(Models.MapProperty.Tabele2BackPoint);
        }
        public Vector Tabele3Center
        {
            get  => RealToCanvas(Models.MapProperty.Tabele3Point);
        }
        public Vector MachineSize
        {
            get => MachineSizeDef * MapScale;
        }
        #endregion
        #region oneWay bind
        public bool IsMachineEnabled
        {
            get => _isMachineEnabled;
            set { if (SetProperty(ref _isMachineEnabled, value)) Core.Pot.CommonVal.IsEnabled = value; }
        }
        public Vector MachineCenter
        {
            get => _machineCenter;
            private set { SetProperty(ref _machineCenter, value); }
        }
        public double MachineRot
        {
            get => _machineRot;
            private set { SetProperty(ref _machineRot, value); }
        }
        public string MachineName {
            get => _machineName;
            set { SetProperty(ref _machineName, value); }
        }
        public double Table2FrontRot
        {
            get => _table2FrontRot;
            private set { SetProperty(ref _table2FrontRot, value); }
        }
        public double Table2BackRot
        {
            get => _table2BackRot;
            private set { SetProperty(ref _table2BackRot, value); }
        }
        public double Table3Rot
        {
            get => _table3Rot;
            private set { SetProperty(ref _table3Rot, value); }
        }
        public ObservableNotifyCollection<Core.Pot> Pots
        {
            get => _pots;
            private set { SetProperty(ref _pots, value); }
        }
        public Brush TeamColor
        {
            get => _teamColor;
            private set { if (SetProperty(ref _teamColor, value)) Core.Pot.CommonVal.Color = value; }
        }
        #endregion
        #endregion




        #region Command
        private ICommand _scrollViewer_PreviewMouseWheel;
        private ICommand _scrollViewer_MouseDown;
        private ICommand _scrollViewer_MouseUp;
        private ICommand _scrollViewer_MouseMove;
        private ICommand _scrollViewer_ManipulationStarting;
        private ICommand _scrollViewer_ManipulationCompleted;
        private ICommand _scrollViewer_ManipulationDelta;

        /// <summary>
        /// ScrollViewer上でのホイール回転をキャプチャ
        /// </summary>
        public ICommand ScrollViewer_PreviewMouseWheel
        {
            get
            {
                return _scrollViewer_PreviewMouseWheel ??
                  (_scrollViewer_PreviewMouseWheel = CreateCommand(
                      (MouseWheelEventArgs e) => {
                          #region get control
                          var control = e.Source as UIElement;
                          var viewer = control?.Ancestor<ScrollViewer>();
                          if (viewer == null) return;
                          #endregion

                          var canvas = viewer.Descendants<Canvas>().ToArray()[0];

                          double scaleing = (e.Delta > 0) ? MagnificationScale : 1.0 / MagnificationScale;
                          double viewHOffset = viewer.HorizontalOffset,
                              viewVOffset = viewer.VerticalOffset;
                          var matrix = canvas.LayoutTransform.Value;

                          //スクロールイベントの無効化
                          e.Handled = true;

                          matrix.ScaleAt(scaleing, scaleing, Mouse.GetPosition(canvas).X, Mouse.GetPosition(canvas).Y);
                          canvas.LayoutTransform = new MatrixTransform(matrix);
                          Core.Pot.CommonVal.HitboxDiameter /= scaleing;

                          viewer.ScrollToHorizontalOffset((Mouse.GetPosition(viewer).X + viewHOffset) * scaleing - Mouse.GetPosition(viewer).X);
                          viewer.ScrollToVerticalOffset((Mouse.GetPosition(viewer).Y + viewVOffset) * scaleing - Mouse.GetPosition(viewer).Y);
                      }));
            }
        }
        /// <summary>
        /// ScrollViewer上で押下時位置をキャプチャ
        /// </summary>
        public ICommand ScrollViewer_MouseDown
        {
            get
            {
                return _scrollViewer_MouseDown ??
                  (_scrollViewer_MouseDown = CreateCommand(
                      (MouseButtonEventArgs e) => {


                          #region get control

                          //potCanvasをキャッチ
                          var canvas = (e.OriginalSource as UIElement)?.Ancestor<Canvas>();
                          if (canvas != null)
                          {
                              if (canvas.Tag is Core.ControlType.Pot)
                              {
                                  Core.ControlType.Pot pot = (Core.ControlType.Pot)canvas.Tag;
                                  CapturedPot = pot;
                                  IsPotCaptured = true;
                                  e.Handled = true;
                                  return;
                              }
                          }

                          var control = e.Source as UIElement;
                          var viewer = control?.Ancestor<ScrollViewer>();
                          if (viewer == null) return;
                          #endregion

                          if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
                          {
                              _scrollViewerDragStart = e.GetPosition(viewer);
                              viewer.CaptureMouse();
                          }
                          e.Handled = true;
                      }));
            }
        }
        /// <summary>
        /// ScrollViewer上でリリース時位置をキャプチャ
        /// </summary>
        public ICommand ScrollViewer_MouseUp
        {
            get
            {
                return _scrollViewer_MouseUp ??
                    (_scrollViewer_MouseUp = CreateCommand(
                        (MouseButtonEventArgs e) => {
                            #region get control
                            //potCanvasをキャッチ
                            var canvas = (e.OriginalSource as UIElement)?.Ancestor<Canvas>();
                            if (canvas != null)
                            {
                                if (canvas.Tag is Core.ControlType.Pot && e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Released)
                                {
                                    Core.ControlType.Pot pot = (Core.ControlType.Pot)canvas.Tag;
                                    if (IsPotCaptured && pot == CapturedPot)
                                    {
                                        IsPotCaptured = false;
                                        PotClick(pot);
                                        return;
                                    }
                                }
                            }



                            var control = e.Source as UIElement;
                            var viewer = control?.Ancestor<ScrollViewer>() ?? control as ScrollViewer;
                            if (viewer == null) return;
                            #endregion

                            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
                                viewer.ReleaseMouseCapture();

                        }));
            }
        }
        /// <summary>
        /// ScrollViewer上でのマウス移動をキャプチャ
        /// </summary>
        public ICommand ScrollViewer_MouseMove
        {
            get
            {
                return _scrollViewer_MouseMove ??
                  (_scrollViewer_MouseMove = CreateCommand(
                      (MouseEventArgs e) => {
                          #region get control
                          var control = e.Source as UIElement;
                          var viewer = control?.Ancestor<ScrollViewer>() ?? control as ScrollViewer;
                          if (viewer == null) return;
                          #endregion

                          if (viewer.IsMouseCaptured && (e.MiddleButton == MouseButtonState.Pressed))
                          {
                              var move = _scrollViewerDragStart - Mouse.GetPosition(viewer);

                              viewer.ScrollToHorizontalOffset(move.X + viewer.HorizontalOffset);
                              viewer.ScrollToVerticalOffset(move.Y + viewer.VerticalOffset);

                              _scrollViewerDragStart = e.GetPosition(viewer);
                          }
                      }));
            }
        }
        /// <summary>
        /// タッチ操作
        /// </summary>
        public ICommand ScrollViewer_ManipulationStarting
        {
            get
            {
                return _scrollViewer_ManipulationStarting ??
                (_scrollViewer_ManipulationStarting = CreateCommand(
                      (ManipulationStartingEventArgs e) =>
                      {
                          switch (e.OriginalSource)
                          {
                              case Canvas canvas:
                                  {
                                      Core.ControlType.Pot pot = (Core.ControlType.Pot)canvas.Tag;
                                      CapturedPot = pot;
                                      IsPotCaptured = true;
                                  }
                                break;
                              default:
                                return;
                          }
                          e.Handled = true;
                      }));
            }
        }
        /// <summary>
        /// タッチ操作
        /// </summary>
        public ICommand ScrollViewer_ManipulationCompleted
        {
            get
            {
                return _scrollViewer_ManipulationCompleted ??
                (_scrollViewer_ManipulationCompleted = CreateCommand(
                      (ManipulationCompletedEventArgs e) =>
                      {
                          switch (e.OriginalSource)
                          {
                              case Canvas canvas:
                                  {
                                      Core.ControlType.Pot pot = (Core.ControlType.Pot)canvas.Tag;
                                      if (IsPotCaptured && pot == CapturedPot)
                                      {
                                          IsPotCaptured = false;
                                          if (!PotClick(pot)) return;
                                      }
                                  }
                                  break;
                              default:
                                  return;
                          }
                          e.Handled = true;
                      }));
            }
        }
        /// <summary>
        /// タッチ操作
        /// </summary>
        public ICommand ScrollViewer_ManipulationDelta
        {
            get
            {
               return _scrollViewer_ManipulationDelta ??
               (_scrollViewer_ManipulationDelta = CreateCommand(
                     (ManipulationDeltaEventArgs e) =>
                     {
                         double scaleing = e.DeltaManipulation.Scale.Length / Math.Sqrt(2);//デフォで(1,1)なので2^(-0.5)倍
                         if (scaleing == 0.0 || (scaleing == 1.0 && e.DeltaManipulation.Translation.Length == 0.0)) return;

                         #region get control
                         var viewer = e.Source as ScrollViewer;//PotCanvas以外から発火
                         if (viewer == null)
                         {
                             return;
                         }
                         #endregion

                         var canvas = viewer.Descendants<Canvas>().ToArray()[0];
                         if (!canvas.IsStylusOver) return;//バー上でのタッチイベントは無視
                         e.Handled = true;

                         double viewHOffset = viewer.HorizontalOffset,
                             viewVOffset = viewer.VerticalOffset;
                         var matrix = canvas.LayoutTransform.Value;

                         /*
                         Log.WiteDebugMsg(" time:" + DateTime.Now.ToString("s.fff") +
                                            ", ScaleX:" + e.DeltaManipulation.Scale.X.ToString("F4") +
                                            ", ScaleY:" + e.DeltaManipulation.Scale.Y.ToString("F4") +
                                            ", Scale:" + scaleing.ToString("F4") +
                                            ", X:" + e.ManipulationOrigin.X.ToString("F4") +
                                            ", Y:" + e.ManipulationOrigin.Y.ToString("F4"));
                         */

                         if (scaleing != 1.0)
                         {
                             matrix.ScaleAt(scaleing, scaleing, e.ManipulationOrigin.X, e.ManipulationOrigin.Y);
                             canvas.LayoutTransform = new MatrixTransform(matrix);
                             Core.Pot.CommonVal.HitboxDiameter /= scaleing;
                         }

                         viewer.ScrollToHorizontalOffset((e.ManipulationOrigin.X + viewHOffset) * scaleing - e.ManipulationOrigin.X - e.DeltaManipulation.Translation.X);
                         viewer.ScrollToVerticalOffset((e.ManipulationOrigin.Y + viewVOffset) * scaleing - e.ManipulationOrigin.Y - e.DeltaManipulation.Translation.Y);
                     }));
            }
        }
        #endregion


        #region Method
        private bool PotClick(Core.ControlType.Pot pot)
        {
            if (TR.IsEnabaled)
            {
                TR.TurnPot(pot);
                return true;
            }
            return false;
        }




        private Vector RealToCanvas(Point realPoint)
        {
            return new Vector(realPoint.X * MapScale.X + MapWidth * 0.5, MapHeight - realPoint.Y * MapScale.Y - MapHeight * 0.5);
        }
        private Vector RealToCanvas(Vector realPoint)
        {
            return new Vector(realPoint.X * MapScale.X + MapWidth * 0.5, MapHeight - realPoint.Y * MapScale.Y - MapHeight * 0.5);
        }





        private void MapProperty_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MapProperty.Pot1RightPos))
            {
                Pots[0].Point = RealToCanvas(MapProperty.Pot1RightPos);
            }
            else if (e.PropertyName == nameof(MapProperty.Pot1LeftPos))
            {
                Pots[1].Point = RealToCanvas(MapProperty.Pot1LeftPos);
            }
            else if (e.PropertyName == nameof(MapProperty.Pot2FrontPos))
            {
                Pots[2].Point = RealToCanvas(MapProperty.Pot2FrontPos);
                var v = MapProperty.Pot2FrontPos - Models.MapProperty.Tabele2FrontPoint;
                Table2FrontRot = Math.Atan2(v.Y, -v.X) * 180.0 / Math.PI;
            }
            else if (e.PropertyName == nameof(MapProperty.Pot2BackPos))
            {
                Pots[3].Point = RealToCanvas(MapProperty.Pot2BackPos);
                var v = MapProperty.Pot2BackPos - Models.MapProperty.Tabele2BackPoint;
                Table2BackRot = Math.Atan2(v.Y, -v.X) * 180.0 / Math.PI;
            }
            else if (e.PropertyName == nameof(MapProperty.Pot3Pos))
            {
                Pots[4].Point = RealToCanvas(MapProperty.Pot3Pos);
                var v = MapProperty.Pot3Pos - Models.MapProperty.Tabele3Point;
                Table3Rot = Math.Atan2(v.Y, -v.X) * 180.0 / Math.PI;
            }
            else if (e.PropertyName == nameof(MapProperty.IsTeamRed))
            {
                TeamColor = new SolidColorBrush(MapProperty.IsTeamRed ? Colors.Red : Colors.Blue);
            }
        }
        private void TR_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(TR.Positon))
            {
                MachineCenter = RealToCanvas(TR.Positon);
            }
            if (e.PropertyName == nameof(TR.PositonRot))
            {
                MachineRot = -TR.PositonRot * 180.0 / Math.PI;
            }
            if(e.PropertyName == nameof(TR.IsEnabaled))
            {
                IsMachineEnabled = TR.IsEnabaled;
            }
            if(e.PropertyName == nameof(TR.PotsQueue))
            {
                foreach (var pot in Pots) pot.IsHighlighted = false;
                if(TR.PotsQueue.Count != 0) Pots[(int)TR.PotsQueue[0]].IsHighlighted = true;
            }
        }
        private void DR_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DR.Positon))
            {
                MachineCenter = RealToCanvas(DR.Positon);
            }
            else if (e.PropertyName == nameof(DR.PositonRot))
            {
                MachineRot = -DR.PositonRot * 180.0 / Math.PI;
            }
            else if (e.PropertyName == nameof(DR.IsEnabaled))
            {
                IsMachineEnabled = DR.IsEnabaled;
            }
        }
        private void Communicator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Communicator.Device))
            {
                MachineName = Communicator.Device.ToString();
            }
        }
        private void DebugSate_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        #region Converter
        #endregion
        #endregion
    }
}
