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
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using System.Runtime.Serialization;
    using Newtonsoft.Json.Linq;

    public class Forest : Cloud.ManagedEntity
    {
        private Int64 _id;
        private string _tag;
        private string _name;
        private string _region;
        private string _territory;
        private Admin.AppServiceTier _tier = null;

        public Forest() :
            base("forest")
        {
        }

        public override string Key
        {
            get { return _id.ToString(); }
        }

        override public string Collection
        {
            get
            {
                if (_tier != null)
                {
                    return _tier.CollectionKey + base.Collection;
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
                if (_tier != null)
                {
                    return _tier.CollectionKey + base.CollectionKey;
                }
                else
                {
                    return base.CollectionKey;
                }
            }
        }

        public Admin.AppServiceTier AppServiceTier
        {
            get { return _tier; }
            set
            {
                _tier = value;
            }
        }

        [JsonProperty("id")]
        public Int64 Id
        {
            get { return _id; }
            set { _id = value; }
        }

        [JsonProperty("tag")]
        public string Tag
        {
            get { return _tag; }
            set
            {
                SetProperty(ref _tag, value);
            }
        }

        [JsonProperty("name")]
        public string Name
        {
            get { return _name; }
            set
            {
                SetProperty(ref _name, value);
            }
        }

        [JsonProperty("region")]
        public string Region
        {
            get { return _region; }
            set
            {
                SetProperty(ref _region, value);
            }
        }

        [JsonProperty("territory")]
        public string Territory
        {
            get { return _territory; }
            set
            {
                SetProperty(ref _territory, value);
                OnPropertyChanged("Icon");
            }
        }

        public string Icon
        {
            get
            {
                switch(_territory)
                {
                    case "SG": return "singapore.png";
                    case "JP": return "japan.png";
                    case "FR": return "france.png";
                    case "NL": return "netherlands.png";
                    case "DE": return "germany.png";
                    case "GB": return "uk.png";
                    case "US": return "usa.png";
                    case "AU": return "australia.png";
                }

                return "usa.png";
            }
        }
    }
}

