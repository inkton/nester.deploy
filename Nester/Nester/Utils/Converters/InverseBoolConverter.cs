using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Nester.Utils
{
    public class InverseBoolConverter : IValueConverter
    {
        private bool Invert(object value)
        {
            if (value == null)
                return false;
            return !(bool)value;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Invert(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Invert(value);
        }
    }
}
