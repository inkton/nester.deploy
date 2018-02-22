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
using System.Resources;
using Xamarin.Forms;

namespace Inkton.Nester.Helpers
{
    public class BoolToDomainStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ResourceManager resmgr = (Application.Current as INesterControl)
                .GetResourceManager();

            if (value == null)
            {
                return resmgr.GetString("DomainNormal",
                     System.Globalization.CultureInfo.CurrentUICulture);
            }

            return (bool)value ?
                    resmgr.GetString("DomainPrimary",
                         System.Globalization.CultureInfo.CurrentUICulture) :
                    resmgr.GetString("DomainNormal",
                         System.Globalization.CultureInfo.CurrentUICulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ResourceManager resmgr = (Application.Current as INesterControl)
                .GetResourceManager();

            return resmgr.GetString("DomainPrimary",
                 System.Globalization.CultureInfo.CurrentUICulture);
        }
    }
}
