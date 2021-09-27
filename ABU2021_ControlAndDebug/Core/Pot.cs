using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MVVMLib;

namespace ABU2021_ControlAndDebug.Core
{
    class Pot : NotifyPropertyChanged
    {   
        public class CommonValue : NotifyPropertyChanged
        {
            private static CommonValue _instance;
            public static CommonValue Instance { get=> _instance ?? (_instance = new CommonValue()); }

            private double _diameter;
            private double _hitboxDiameter = 500.0;
            private bool _isEnabled;
            private Brush _color = new SolidColorBrush(Colors.Blue); 
            public double Diameter { get => _diameter; set => SetProperty(ref _diameter, value); }
            public double HitboxDiameter { get => _hitboxDiameter; set => SetProperty(ref _hitboxDiameter, value); }
            public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }
            public Brush Color { get => _color; set => SetProperty(ref _color, value); }
        }
        
        
        private string _name;
        private Vector _point;
        private bool _isHighlighted;
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public Vector Point { get => _point; set => SetProperty(ref _point, value); }
        public bool IsHighlighted { get => _isHighlighted; set => SetProperty(ref _isHighlighted, value); }
        public static CommonValue CommonVal { get; private set; } = CommonValue.Instance;
        public ControlType.Pot Tag { get; set; }
    }
}
