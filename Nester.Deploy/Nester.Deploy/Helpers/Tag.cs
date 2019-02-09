using System;
using System.Text;

namespace Inkton.Nester.Helpers
{
    public class Tag
    {
        public const int DefaultMaxLength = 16;

        static public string Clean(string value)
        {
            /* 
             * https://blog.maartenballiauw.be/post/2017/04/24/making-string-validation-faster-no-regular-expressions.html
             */

            string valueFinal = value.ToString();

            if (valueFinal.Length == 0)
                return value;

            // adjust string case
            valueFinal = valueFinal.ToLower();

            // id types must begin with an alpha
            if (valueFinal[0] >= 48 && valueFinal[0] <= 57) // 0-9
            {
                valueFinal = valueFinal.Substring(1, valueFinal.Length - 1);
            }

            // adjust string length
            var len = valueFinal.Length;
            if (len > DefaultMaxLength)
            {
                valueFinal = valueFinal.Substring(0, DefaultMaxLength);
                len = DefaultMaxLength;
            }

            StringBuilder myStringBuilder = new StringBuilder();

            for (int i = 0; i < len; i++)
            {
                if ((valueFinal[i] >= 48 && valueFinal[i] <= 57) // 0-9
                          || (valueFinal[i] >= 97 && valueFinal[i] <= 122)) // a-z)
                {
                    myStringBuilder.Append(valueFinal[i]);
                }
            }

            return myStringBuilder.ToString();
        }
    }
}
