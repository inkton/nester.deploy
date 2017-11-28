/*
    Copyright (c) 2017 Inkton.

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the "Software"),
    to deal in the Software without restriction, including without limitation
    the rights to use, copy, modify, merge, publish, distribute, sublicense,
    and/or sell copies of the Software, and to permit persons to whom the Software
    is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace Inkton.Nester.Utils
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
