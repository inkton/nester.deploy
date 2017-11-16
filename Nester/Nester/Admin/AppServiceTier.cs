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

    public class AppServiceTier : Cloud.ManagedEntity
    {
        private Int64 _id;
        private Int64 _appServiceId;
        private string _tag;
        private string _name;
        private string _type;
        private string _period;
        private decimal _itemCost;
        private string _featuresIncluded;
        private AppService _service = null;

        public AppServiceTier() 
            : base("app_service_tier")
        {
        }

        [JsonIgnore]
        public Admin.AppService Service
        {
            get { return _service; }
            set { SetProperty(ref _service, value); }
        }

        public override string Key
        {
            get { return _id.ToString(); }
        }

        override public string Collection
        {
            get
            {
                if (_service != null)
                {
                    return _service.CollectionKey + base.Collection;
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
                if (_service != null)
                {
                    return _service.CollectionKey + base.CollectionKey;
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

        [JsonProperty("app_service_id")]
        public Int64 AppServiceId
        {
            get { return _appServiceId; }
            set { _appServiceId = value; }
        }

        [JsonProperty("tag")]
        public string Tag
        {
            get { return _tag; }
            set { SetProperty(ref _tag, value); }
        }

        [JsonProperty("name")]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        [JsonProperty("type")]
        public string Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        [JsonProperty("period")]
        public string Period
        {
            get { return _period; }
            set { SetProperty(ref _period, value); }
        }

        [JsonProperty("item_cost")]
        public decimal ItemCost
        {
            get { return _itemCost; }
            set { SetProperty(ref _itemCost, value); }
        }

        [JsonProperty("features_included")]
        public string FeaturesIncluded
        {
            get { return _featuresIncluded; }
            set { SetProperty(ref _featuresIncluded, value); }
        }

        public string[] FeaturesIncludedArray
        {
            get
            {
                List<string> values = JsonConvert.DeserializeObject<List<string>>(
                    _featuresIncluded);
                return values.ToArray<string>();
            }
        }
    }
}

