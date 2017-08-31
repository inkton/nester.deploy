using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using System.Globalization;
using System.Resources;
using System.Reflection;

namespace Nester.Utils
{
    public class TranslateConverter : IValueConverter
    {
        private ResourceManager _resmgr;
        private readonly CultureInfo _ci;

        public TranslateConverter()
        {
            _resmgr = new ResourceManager("Nester.AppResources"
                                , typeof(TranslateConverter).GetTypeInfo().Assembly);
            _ci = System.Globalization.CultureInfo.CurrentUICulture;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return _resmgr.GetString(value as string, _ci);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
