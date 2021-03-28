using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MVVMLib;

namespace ABU2021_ControlAndDebug.ViewModels
{
    class MapControl : ViewModel
    {
        private static readonly double MagnificationScale = 1.15;
        private Point _scrollViewerDragStart;

        #region Model
        public Models.OutputLog Log { get; private set; }
        public Models.MapProperty MapProperty { get; private set; }
        public Models.ControlTR TR{ get; private set; }
        public Models.ControlDR DR { get; private set; }
        #endregion


        public MapControl()
        {
            #region get instance
            Log = Models.OutputLog.GetInstance;
            MapProperty = Models.MapProperty.GetInstance;
            TR = Models.ControlTR.GetInstance;
            DR = Models.ControlDR.GetInstance;
            #endregion

            MapHeight = MapProperty.MapPictureSoruce.PixelHeight;
            MapWidth = MapProperty.MapPictureSoruce.PixelWidth;
            TableHeight = MapProperty.Table2PictureSoruce.PixelHeight;
            TableWidth = MapProperty.Table2PictureSoruce.PixelWidth;
            MapScale = new Vector(MapWidth / Models.MapProperty.MapSize.X, MapHeight / Models.MapProperty.MapSize.Y);
        }


        #region Property
        private Vector _mapScale;
        private double _mapWidth;
        private double _mapHeight;
        private double _tableWidth;
        private double _tableHeight;

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
        public Vector Tabele2aOffset { 
            get
            {
                var v = Models.MapProperty.Tabele2aPoint - Models.MapProperty.TabeleSize * 0.5;
                v.X *= MapScale.X;
                v.Y *= MapScale.Y;
                return v;
            }
        }
        public Vector Tabele2bOffset
        {
            get
            {
                var v = Models.MapProperty.Tabele2bPoint - Models.MapProperty.TabeleSize * 0.5;
                v.X *= MapScale.X;
                v.Y *= MapScale.Y;
                return v;
            }
        }
        public Vector Tabele3Offset
        {
            get
            {
                var v = Models.MapProperty.Tabele3Point - Models.MapProperty.TabeleSize * 0.5;
                v.X *= MapScale.X;
                v.Y *= MapScale.Y;
                return v;
            }
        }
        public Vector Tabele2aCenter
        {
            get
            {
                var v = Models.MapProperty.Tabele2aPoint;
                v.X *= MapScale.X;
                v.Y *= MapScale.Y;
                return v;
            }
        }
        public Vector Tabele2bCenter
        {
            get
            {
                var v = Models.MapProperty.Tabele2bPoint;
                v.X *= MapScale.X;
                v.Y *= MapScale.Y;
                return v;
            }
        }
        public Vector Tabele3Center
        {
            get
            {
                var v = Models.MapProperty.Tabele3Point;
                v.X *= MapScale.X;
                v.Y *= MapScale.Y;
                return v;
            }
        }
        #endregion


        #region Command
        private ICommand _scrollViewer_PreviewMouseWheel;
        private ICommand _scrollViewer_MouseDown;
        private ICommand _scrollViewer_MouseUp;
        private ICommand _scrollViewer_MouseMove;
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

                          viewer.ScrollToHorizontalOffset((Mouse.GetPosition(viewer).X + viewHOffset) * scaleing - Mouse.GetPosition(viewer).X);
                          viewer.ScrollToVerticalOffset((Mouse.GetPosition(viewer).Y + viewVOffset) * scaleing - Mouse.GetPosition(viewer).Y);
                      }));
            }
        }
        /// <summary>
        /// ScrollViewer上でセンター押下時位置をキャプチャ
        /// </summary>
        public ICommand ScrollViewer_MouseDown
        {
            get
            {
                return _scrollViewer_MouseDown ??
                  (_scrollViewer_MouseDown = CreateCommand(
                      (MouseButtonEventArgs e) => {
                          #region get control
                          var control = e.Source as UIElement;
                          var viewer = control?.Ancestor<ScrollViewer>();
                          if (viewer == null) return;
                          #endregion

                          if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
                          {
                              _scrollViewerDragStart = e.GetPosition(viewer);
                              control.CaptureMouse();
                          }
                          e.Handled = true;
                      }));
            }
        }
        /// <summary>
        /// ScrollViewer上でセンターリリース時位置をキャプチャ
        /// </summary>
        public ICommand ScrollViewer_MouseUp
        {
            get
            {
                return _scrollViewer_MouseUp ??
                    (_scrollViewer_MouseUp = CreateCommand(
                        (MouseButtonEventArgs e) => {
                            #region get control
                            var control = e.Source as UIElement;
                            var viewer = control?.Ancestor<ScrollViewer>();
                            if (viewer == null) return;
                            #endregion

                            if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
                                control.ReleaseMouseCapture();

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
                          var viewer = control?.Ancestor<ScrollViewer>();
                          if (viewer == null) return;
                          #endregion

                          if (control.IsMouseCaptured && (e.MiddleButton == MouseButtonState.Pressed))
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
        public ICommand ScrollViewer_ManipulationDelta
        {
            get
            {
               return _scrollViewer_ManipulationDelta ??
               (_scrollViewer_ManipulationDelta = CreateCommand(
                     (ManipulationDeltaEventArgs e) =>
                     {
                         e.Handled = true;
                         double scaleing = e.DeltaManipulation.Scale.Length / Math.Sqrt(2);//デフォで(1,1)なので2^(-0.5)倍
                         if (scaleing == 0.0 || (scaleing == 1.0 && e.DeltaManipulation.Translation.Length == 0.0)) return;

                         #region get control
                         var viewer = e.Source as ScrollViewer;//こいつはScrollViewerから直接発火
                         if (viewer == null) return;
                         #endregion

                         var canvas = viewer.Descendants<Canvas>().ToArray()[0];

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
                         }

                         viewer.ScrollToHorizontalOffset((e.ManipulationOrigin.X + viewHOffset) * scaleing - e.ManipulationOrigin.X - e.DeltaManipulation.Translation.X);
                         viewer.ScrollToVerticalOffset((e.ManipulationOrigin.Y + viewVOffset) * scaleing - e.ManipulationOrigin.Y - e.DeltaManipulation.Translation.Y);
                     }));
            }
            set { }
        }
        #endregion


        #region Method
        #endregion
    }
}
