using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Nester.Utils
{
    public class ObjectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }

            Admin.AppDomain domain = value as Admin.AppDomain;
            if (domain != null && domain.Default)
            {
                return false;
            }

            //Admin.AppDomainCertificate cert = value as Admin.AppDomainCertificate;
            //if (cert != null && cert.Type == "free")
            //{
            //    return false;
            //}

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
