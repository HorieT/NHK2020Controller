using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ABU2021_ControlAndDebug
{
    /// <summary>
    /// -1/2倍するだけ
    /// </summary>
    public class CenterOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (value)
            {
                case int i:
                    return -i / 2;
                case double d:
                    return -d / 2.0;
                case float f:
                    return -f / 2.0;
                case Point point:
                    return new Point(-point.X / 2, -point.Y / 2);
                case Vector vec:
                    return -vec / 2.0;
                default:
                    throw new NotImplementedException("Can't convert");
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch (value)
            {
                case int i:
                    return -i * 2;
                case double d:
                    return -d * 2.0;
                case float f:
                    return -f * 2.0;
                case Point point:
                    return new Point(-point.X * 2, -point.Y * 2);
                case Vector vec:
                    return -vec * 2.0;
                default:
                    throw new NotImplementedException("Can't convert");
            }
        }
    }
}
