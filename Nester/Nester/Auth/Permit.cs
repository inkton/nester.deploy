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

namespace Inkton.Nester.Auth
{
    /*
     * Cloud objects needed for the app
     * will loaded as used and a reference
     * will be kept here.  The idea is only
     * loading objects as needed given small
     * device constraints.  The server is
     * also only contacted per object per 
     * session. The object is kept cached
     * locally within a single session.
     */

    public class Permit : Cloud.ManagedEntity
    {
        private string _token = "<-token->";
        private string _password = null;
        private string _securityCode = null;
        private Admin.User _user;

        public Permit() 
            : base("permit")
        {
        }

        public override string Key
        {
            get { return _user.Email; }
        }

        [JsonProperty("token")]
        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public string SecurityCode
        {
            get { return _securityCode; }
            set { _securityCode = value; }
        }

        [JsonProperty("owner")]
        public Admin.User Owner
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }
    }
}

