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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Nester.Admin
{
    public class Invitation : Contact
    {
        private string _appName;
        private string _appTag;
        private Admin.User _user = null;

        public Invitation() 
            : base("invitation", "invitations")
        {
        }

        override public string Collection
        {
            get
            {
                if (_user != null)
                {
                    return _user.CollectionKey + base.Collection;
                }
                else
                {
                    return base.Collection;
                }
            }
        }

        public Admin.User User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        public override string Icon
        {
            get
            {
                if (Status == "active") 
                {
                    return "invitejoin32.png";
                }
                else
                {
                    return "inviteleave32.png";
                }
            }
        }

        [JsonProperty("app_tag")]
        public string AppTag
        {
            get { return _appTag; }
            set { SetProperty(ref _appTag, value); }
        }

        [JsonProperty("app_name")]
        public string AppName
        {
            get { return _appName; }
            set { SetProperty(ref _appName, value); }
        }
    }
}
