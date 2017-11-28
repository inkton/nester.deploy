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

namespace Inkton.Nester.Admin
{
    public class Contact : Cloud.ManagedEntity
    {
        private Int64 _id;
        private Int64 _appId;        
        private Int64? _userId;
        private string _email;
        private string _status;
        private bool _notifyUpdates;
        private string _nickname, _firstName, _lastName;

        private Admin.App _application = null;
        private Auth.OwnerCapabilities _ownerCapabilities;

        public Contact()
             : base("contact")
        {
            _ownerCapabilities = new Auth.OwnerCapabilities();
        }

        protected Contact(
            string entity = "contact", 
            string subject = "contacts") 
            : base(entity, subject)
        {
            _ownerCapabilities = new Auth.OwnerCapabilities();
        }

        public Admin.App App
        {
            get { return _application; }
            set
            {
                _application = value;
                if (_application != null)
                {
                    this.AppId = value.Id;
                }
            }
        }

        public Auth.OwnerCapabilities OwnerCapabilities
        {
            get { return _ownerCapabilities; }
            set { SetProperty(ref _ownerCapabilities, value); }
        }

        public override string Key
        {
            get { return _email.ToString(); }
        }

        override public string Collection
        {
            get
            {
                if (_application != null)
                {
                    return _application.CollectionKey + base.Collection;
                }
                else
                {
                    return base.Collection;
                }
            }
        }

        override public string CollectionKey
        {
            get
            {
                if (_application != null)
                {
                    return _application.CollectionKey + base.CollectionKey;
                }
                else
                {
                    return base.CollectionKey;
                }
            }
        }

        [JsonProperty("id")]
        public Int64 Id
        {
            get { return _id; }
            set { _id = value; }
        }

        [JsonProperty("app_id")]
        public Int64 AppId
        {
            get { return _appId; }
            set { SetProperty(ref _appId, value); }
        }

        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public Int64? UserId
        {
            get { return _userId; }
            set { SetProperty(ref _userId, value); }
        }

        [JsonProperty("email")]
        public string Email
		{
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }

        [JsonProperty("status")] 
        public string Status
		{
            get { return _status; }
            set {
                SetProperty(ref _status, value);
                OnPropertyChanged("Icon");
            }
        }

        [JsonProperty("notify_updates")]
        public bool NotifyUpdates
        {
            get { return _notifyUpdates; }
            set { SetProperty(ref _notifyUpdates, value); }
        }

        [JsonProperty("nickname", NullValueHandling = NullValueHandling.Ignore)]
        public string Nickname
        {
            get { return _nickname; }
            set {
                 SetProperty(ref _nickname, value);
                 OnPropertyChanged("DisplayName");
            }
        }

        [JsonProperty("first_name", NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName
        {
            get { return _firstName; }
            set {
                SetProperty(ref _firstName, value);
                OnPropertyChanged("DisplayName");
            }
        }

        [JsonProperty("surname", NullValueHandling = NullValueHandling.Ignore)]
        public string LastName
        {
            get { return _lastName; }
            set {
                SetProperty(ref _lastName, value);
                OnPropertyChanged("DisplayName");
            }
        }

        public string DisplayName
        {
            get
            {
                if (_status == "active")
                {
                    string name = string.Empty;
                    if ((_firstName != null && _firstName.Length > 0) ||
                        (_lastName != null && _lastName.Length > 0 ))
                    {
                        if (_firstName != null && _firstName.Length > 0)
                        {
                            name = _firstName;
                            name += " ";
                        }
                        if (_lastName != null && _lastName.Length > 0)
                        {
                            name += _lastName;
                        }

                        name += ", ";
                    }

                    name += _nickname;
                    return name.Trim();
                }
                else
                {
                    String[] elements = Regex.Split(_email, "@");
                    return elements[0];
                }
            }
        }

        public virtual string Icon
        {
            get {
                if (_status == "active")
                {
                    return "contactverified32.png";
                }
                else
                {
                    return "contact32.png";
                }
            }
        }

        public string Info
        {
            get
            {
                if (_status == "active")
                {
                    return "Active";
                }
                else
                {
                    return "Invited";
                }
            }
        }
    }
}

