using MVVMLib;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ABU2021_ControlAndDebug.Models
{
    /// <summary>
    /// 地図情報
    /// マシン位置はマシンのモデルへ
    /// 表示についてはVMへ
    /// </summary>
    class MapProperty : NotifyPropertyChanged
    {
        public static readonly Vector MapSize = new Vector(12000.0, 12000.0);
        public static readonly Vector TabeleSize = new Vector(1150.0, 330.0);

        public static readonly double Pot1Diameter = 298.0;
        public static readonly double Pot2Diameter = 250.0;
        public static readonly double Pot3Diameter = 158.0;
        public static readonly Vector Tabele2aPoint = new Vector(6000, 3500);
        public static readonly Vector Tabele2bPoint = new Vector(6000, 8500);
        public static readonly Vector Tabele3Point = new Vector(6000, 6000);



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

        private double _table2aRot = 0.0;
        private double _table2bRot = 0.0;
        private double _table3Rot = 90.0;

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
        public double Table2aRot
        {
            get => _table2aRot;
            private set { SetProperty(ref _table2aRot, value); }
        }
        public double Table2bRot
        {
            get => _table2bRot;
            private set { SetProperty(ref _table2bRot, value); }
        }
        public double Table3Rot
        {
            get => _table3Rot;
            private set { SetProperty(ref _table3Rot, value); }
        }
        #endregion


        #region Method
        private static BitmapImage CreateBitmapImg(Bitmap bitmap)
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
                bitmapImage.EndInit();
                // ここでFreezeしておくといいらしい(参考資料参照)
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }
        #endregion
    }
}
