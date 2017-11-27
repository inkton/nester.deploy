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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inkton.Nester.Views
{
    public class NavigationMessage
    {
        private string _type = string.Empty;

        public NavigationMessage(string type)
        {
            _type = type;
        }

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
    }

    public class AlertMessage : NavigationMessage
    {
        private string _message = string.Empty;

        public AlertMessage(string type, string message)
            :base(type)
        {
            _message = message;
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }

    public class ManagedObjectMessage<T> : NavigationMessage
    {
        private T _object = default(T);

        public ManagedObjectMessage(string type, T Object)
            : base(type)
        {
            _object = Object;
        }

        public T Object
        {
            get { return _object; }
            set { _object = value; }
        }
    }

}
