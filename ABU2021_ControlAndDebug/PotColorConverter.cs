using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ABU2021_ControlAndDebug
{
    class PotColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is Core.ControlType.Pot)) throw new ArgumentException("\"value\" can't convert");

            try
            {
                return new SolidColorBrush(Core.ControlType.PotsColor[(int)value]);
            }
            catch
            {
                throw new NotImplementedException("Can't convert");
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Can't convert");
        }
    }
}
