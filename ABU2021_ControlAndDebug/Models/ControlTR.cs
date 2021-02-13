using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public static readonly double INJECT_SPEED_MAX = 7.5;//m/s
        public static readonly double INJECT_ANGLE_MAX = 47.0;//deg
        public static readonly double INJECT_ANGLE_MIN = 27.0;//deg

        /// <summary>
        /// 通信インデックス定義の為にプロパティと同名の列挙型を定義しコンバータとして使う
        /// </summary>
        public enum SendDataType : int
        {
            IsEmergencyStopped = 1,
            InjectSpeedTag,
            InjectAngleTag,
            IsClickedInjectButton,
            LoaderPoint,
            IsOpenedRecovery,
        }
        public enum ReceiveDataType : int
        {
            IsEmergencyStopped = 1,
            InjectAngleNow,
            IsMoveEndInjectAngle,
            IsMoveEndInjecInit,
            IsMoveEndInject,
            IsMoveEndRecovery,
            IsMoveEndLoader,
        }


        private OutputLog _log;
        private Communicator _communicator;

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
        }
        #endregion


        //マシンとの相互依存プロパティの型に注意
        #region Property
        #region private
        private bool _isEnabled;
        private double _injectHeight = 0.8;
        // bind VM and dvice : SendDataType & ReceiveDataType
        private bool _isEmergencyStopped;
        // OneWay (input from VM / output to dvice only) : SendDataType
        private float _injectSpeedTag;
        private float _injectAngleTag;
        private bool _isClickedInjectButton;
        private byte _loaderPoint;
        private bool _isOpenedRecovery;
        // OneWay (input from device only / output to VM) : ReceiveDataType
        private float _injectAngleNow;
        private bool _isMoveEndInjectAngle;
        private bool _isMoveEndInjecInit;
        private bool _isMoveEndInject;
        private bool _isMoveEndRecovery;
        private bool _isMoveEndLoader;


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
        public bool IsEmergencyStopped
        {
            get => _isEmergencyStopped;
            set { SetProperty(ref _isEmergencyStopped, value); }
        }
        public float InjectSpeedTag
        {
            get => _injectSpeedTag;
            set 
            {
                if (value < 0.0) throw new ArgumentOutOfRangeException(nameof(_injectSpeedTag), "Negative value cannot be set");
                SetProperty(ref _injectSpeedTag, value); 
            }
        }
        public float InjectAngleTag
        {
            get => _injectAngleTag;
            set
            {
                if (value < 0.0) throw new ArgumentOutOfRangeException(nameof(_injectAngleTag), "Negative value cannot be set"); 
                SetProperty(ref _injectAngleTag, value); 
            }
        }
        public bool IsClickedInjectButton
        {
            get => _isClickedInjectButton;
            set { SetProperty(ref _isClickedInjectButton, value); }
        }
        public byte LoaderPoint
        {
            get => _loaderPoint;
            set { SetProperty(ref _loaderPoint, value); }
        }
        public bool IsOpenedRecovery
        {
            get => _isOpenedRecovery;
            set { SetProperty(ref _isOpenedRecovery, value); }
        }
        public float InjectAngleNow
        {
            get => _injectAngleNow;
            set { SetProperty(ref _injectAngleNow, value); }
        }
        public bool IsMoveEndInjectAngle
        {
            get => _isMoveEndInjectAngle;
            set { SetProperty(ref _isMoveEndInjectAngle, value); }
        }
        public bool IsMoveEndInjecInit
        {
            get => _isMoveEndInjecInit;
            set { SetProperty(ref _isMoveEndInjecInit, value); }
        }
        public bool IsMoveEndInject
        {
            get => _isMoveEndInject;
            set { SetProperty(ref _isMoveEndInject, value); }
        }
        public bool IsMoveEndRecovery
        {
            get => _isMoveEndRecovery;
            set { SetProperty(ref _isMoveEndRecovery, value); }
        }
        public bool IsMoveEndLoader
        {
            get => _isMoveEndLoader;
            set { SetProperty(ref _isMoveEndLoader, value); }
        }


        /// <summary>
        /// プロパティ名からプロパティにアクセスできるようにする拡張
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public object this[string propertyName]
        {
            get
            {
                try
                {
                    return typeof(ControlTR).GetProperty(propertyName).GetValue(this);
                }
                catch
                {
                    throw;
                }
            }
            set
            {
                try
                {
                    typeof(ControlTR).GetProperty(propertyName).SetValue(this, value);
                }
                catch
                {
                    throw;
                }
            }
        }
        #endregion
        #endregion



        #region Method

        public void Inject(float speed)
        {

        }
        public void ChangeAngle(float deg)
        {

        }


        public double GetArrowFallPos(double height)
        {
            double tmp1 = Math.Pow(InjectSpeedTag * Math.Cos(InjectAngleTag*Math.PI/180), 2);
            double tmp2 = Math.Tan(InjectAngleTag * Math.PI / 180);
            return -tmp1 * (-tmp2 - Math.Sqrt(Math.Pow(tmp2, 2) + 2 * 9.8 * (InjectHeight - height) / tmp1)) / 9.8;
        }
        #endregion
    }
}
