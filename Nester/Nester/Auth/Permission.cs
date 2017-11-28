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

namespace Inkton.Nester.Auth
{
    public class Permission : Cloud.ManagedEntity
    {
        private Int64 _id;
        private Int64 _appId;
        private Int64? _userId;
        private Int64 _contactId;
        private string _appPermissionTag;

        private Admin.Contact _contact = null;

        public Permission()
             : base("permission")
        {
        }

        public Admin.Contact Contact
        {
            get { return _contact; }
            set
            {
                _contact = value;
                if (_contact != null)
                {
                    this.ContactId = value.Id;
                }
            }
        }

        public override string Key
        {
            get { return _appPermissionTag; }
        }

        override public string Collection
        {
            get
            {
                if (_contact != null)
                {
                    return _contact.CollectionKey + base.Collection;
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
                if (_contact != null)
                {
                    return _contact.CollectionKey + base.CollectionKey;
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

        [JsonProperty("contact_id")]
        public Int64 ContactId
        {
            get { return _contactId; }
            set { SetProperty(ref _contactId, value); }
        }

        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public Int64? UserId
        {
            get { return _userId; }
            set { SetProperty(ref _userId, value); }
        }

        [JsonProperty("app_permission_tag")]
        public string AppPermissionTag
        {
            get { return _appPermissionTag; }
            set { SetProperty(ref _appPermissionTag, value); }
        }
    }
}

