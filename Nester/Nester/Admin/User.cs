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

namespace Nester.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;

    public class User : Cloud.ManagedEntity
    {
        private Int64 _id = 0;
        private string _email;
        private string _nickname;
        private string _territoryISOCode;
        private string _firstName;
        private string _lastName;
        private bool _active;

        public User() 
            : base("user")
        {
        }

        public override string Key
        {
            get { return _id.ToString(); }
        }

        [JsonProperty("id")]
        public Int64 Id
        {
            get { return _id; }
            set { _id = value; }
        }

        [JsonProperty("is_activated")]
        public bool IsActive
        {
            get { return _active; }
            set { SetProperty(ref _active, value); }
        }

        [JsonProperty("email")]
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }

        [JsonProperty("nickname")]
        public string Nickname
        {
            get { return _nickname; }
            set { SetProperty(ref _nickname, value); }
        }

        [JsonProperty("territory_iso_code")]
        public string TerritoryISOCode
        {
            get { return _territoryISOCode; }
            set { SetProperty(ref _territoryISOCode, value); }
        }

        [JsonProperty("first_name")]
        public string FirstName
        {
            get { return _firstName; }
            set { SetProperty(ref _firstName, value); }
        }

        [JsonProperty("surname")]
        public string LastName
        {
            get { return _lastName; }
            set { SetProperty(ref _lastName, value); }
        }
    }
}

