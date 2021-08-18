using MVVMLib;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ABU2021_ControlAndDebug.Models
{
    /// <summary>
    /// フィールドとオブジェクトの情報
    /// ポット位置等はControlTRかControlDRから入力
    /// </summary>
    class MapProperty : NotifyPropertyChanged
    {
        public static readonly Vector MapSize = new Vector(12.0000, 12.0000);
        public static readonly Vector TabeleSize = new Vector(-1.1500, 0.3300);//もろもろの都合上符号反転

        public static readonly double PotOuterDiameter = 0.315;
        public static readonly double Pot1Diameter = 0.2980;
        public static readonly double Pot2Diameter = 0.2500;
        public static readonly double Pot3Diameter = 0.1580;
        public static readonly Vector Tabele2BackPoint = new Vector(0.0, 2.500);
        public static readonly Vector Tabele2FrontPoint = new Vector(0.0, -2.500);
        public static readonly Vector Tabele3Point = new Vector(0.0, 0.0);



        #region Singleton instance
        private static MapProperty _instance;
        public static MapProperty GetInstance
        {
            get
            {
                return _instance ?? (_instance = new MapProperty());
            }
        }
        #endregion


        private MapProperty()
        {
            MapPictureSoruce = CreateBitmapImg(Properties.Resources.Map);
            Table2PictureSoruce = CreateBitmapImg(Properties.Resources.Pot2);
            Table3PictureSoruce = CreateBitmapImg(Properties.Resources.Pot3);
        }


        #region Property
        private BitmapImage _mapPictureSoruce;
        private BitmapImage _table2PictureSoruce;
        private BitmapImage _table3PictureSoruce;
        private Vector _pot1RightPos;
        private Vector _pot1LeftPos;
        private Vector _pot2FrontPos;
        private Vector _pot2BackPos;
        private Vector _pot3Pos;
        private bool _isTeamRed = false;


        public BitmapImage MapPictureSoruce
        {
            get => _mapPictureSoruce;
            private set { SetProperty(ref _mapPictureSoruce, value); }
        }
        public BitmapImage Table2PictureSoruce
        {
            get => _table2PictureSoruce;
            private set { SetProperty(ref _table2PictureSoruce, value); }
        }
        public BitmapImage Table3PictureSoruce
        {
            get => _table3PictureSoruce;
            private set { SetProperty(ref _table3PictureSoruce, value); }
        }
        public Vector Pot1RightPos
        {
            get => _pot1RightPos;
            set { SetProperty(ref _pot1RightPos, value); }
        }
        public Vector Pot1LeftPos
        {
            get => _pot1LeftPos;
            set { SetProperty(ref _pot1LeftPos, value); }
        }
        public Vector Pot2FrontPos
        {
            get => _pot2FrontPos;
            set { SetProperty(ref _pot2FrontPos, value); }
        }
        public Vector Pot2BackPos
        {
            get => _pot2BackPos;
            set { SetProperty(ref _pot2BackPos, value); }
        }
        public Vector Pot3Pos
        {
            get => _pot3Pos;
            set { SetProperty(ref _pot3Pos, value); }
        }
        public bool IsTeamRed
        {
            get => _isTeamRed;
            set 
            { 
                if (SetProperty(ref _isTeamRed, value))
                {
                    MapPictureSoruce = CreateBitmapImg(Properties.Resources.Map, value ? Rotation.Rotate180 : Rotation.Rotate0);
                    Table2PictureSoruce = CreateBitmapImg(Properties.Resources.Pot2, value ? Rotation.Rotate180 : Rotation.Rotate0);
                    Table3PictureSoruce = CreateBitmapImg(Properties.Resources.Pot3, value ? Rotation.Rotate180 : Rotation.Rotate0);
                } 
            }
        }
        #endregion


        #region Method
        private static BitmapImage CreateBitmapImg(Bitmap bitmap, Rotation rotation = Rotation.Rotate0)
        {
            // BitmapImageを初期化
            var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
            using (var ms = new System.IO.MemoryStream())
            {
                // MemoryStreamに書き出す
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                // MemoryStreamのポジションを設定？
                ms.Position = 0;
                // MemoryStreamを書き込むために準備する
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapImage.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.None;
                // MemoryStreamを書き込む
                bitmapImage.StreamSource = ms;
                //
                bitmapImage.Rotation = rotation;
                bitmapImage.EndInit();
                // ここでFreezeしておくといいらしい(参考資料参照)<-どこ？
                bitmapImage.Freeze();
                ms.Dispose();
            }
            return bitmapImage;
        }
        #endregion
    }
}
