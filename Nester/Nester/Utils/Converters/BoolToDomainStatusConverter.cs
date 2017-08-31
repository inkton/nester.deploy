using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Nester.Utils
{
    public class BoolToDomainStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return AppResources.DomainNormal;

            return (bool)value ? AppResources.DomainPrimary : AppResources.DomainNormal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((string)value == AppResources.DomainPrimary);
        }
    }
}
