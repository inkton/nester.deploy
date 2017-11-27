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

namespace Inkton.Nester.Admin
{
    public class Deployment : Cloud.ManagedEntity
    {
        private Int64 _id = -1;  // _id == -1 is undeployed
        private Int64 _appId;
        private Int64 _forestId;
        private Int64 _frameworkVersionId;
        private string _status = "complete";

        private App _application = null;

        public Deployment()
            : base("deployment")
        {
        }

        public Admin.App App
        {
            get { return _application; }
            set
            {
                _application = value;
                if (value != null)
                {
                    this.AppId = value.Id;
                }
            }
        }

        public override string Key
        {
            get { return _id.ToString(); }
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

        public bool IsBusy
        {
            get
            {
                return (_status == "updating");
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

        [JsonProperty("forest_id")]
        public Int64 ForestId
        {
            get { return _forestId; }
            set
            {
                SetProperty(ref _forestId, value);
            }
        }

        [JsonProperty("software_framework_version_id")]
        public Int64 FrameworkVersionId
        {
            get { return _frameworkVersionId; }
            set
            {
                SetProperty(ref _frameworkVersionId, value);
            }
        }


        [JsonProperty("status")]
        public string Status
        {
            get { return _status; }
            set
            {
                SetProperty(ref _status, value);
            }
        }

        public string Icon
        {
            get
            {
                if (_status == "updating")
                {
                    return "deploymentbusy.png";
                }
                else if (_status == "complete")
                {
                    return "deploymentactive.png";
                }
                else
                {
                    // deployment failed icon needed
                    return "deploymentactive.png";
                }
            }
        }
    }
}

