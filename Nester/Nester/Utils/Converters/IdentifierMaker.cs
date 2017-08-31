using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace Nester.Utils
{
    public class IdentifierMaker : IValueConverter
    {
        public int DefaultMaxLength = 16;

        public object ChangeValue(object value, object parameter)
        {
            if (value == null)
                return value;

            int maxLength = DefaultMaxLength;

            if (!int.TryParse(parameter as string, out maxLength))
                maxLength = DefaultMaxLength;

            //string valueFinal = value.ToString().ToLower().Trim().Replace(' ', '_');
            string valueFinal = value.ToString().ToLower();
            if (valueFinal.Length > maxLength)
            {
                 valueFinal = valueFinal.Substring(0, maxLength);
            }

            try
            {                
                valueFinal = Regex.Replace(valueFinal, @"[^A-Za-z0-9]", "",
                                         RegexOptions.None, TimeSpan.FromSeconds(1.5));
                if (valueFinal.Length > 0)
                {
                    // id types cannot begin with a number
                    string beginsWith = valueFinal.Substring(0, 1);
                    int.Parse(beginsWith);
                    valueFinal = "a" + valueFinal;
                }
            }
            catch (FormatException)
            {
            }
            catch (Exception ex)
            {
                if (ex is RegexMatchTimeoutException)
                {
                    valueFinal = string.Empty;
                }
            }

            return valueFinal;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.ChangeValue(value, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.ChangeValue(value, parameter);
        }
    }
}
