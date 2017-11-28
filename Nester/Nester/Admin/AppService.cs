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
using Newtonsoft.Json;
using System.Resources;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Inkton.Nester.Admin
{
    public class AppService : Cloud.ManagedEntity
    {
        private Int64 _id;
        private string _tag;
        private string _name;
        private string _type;
        private string _rules;
        private string _featuresAll;
        private Int64? _port;
        private App _application = null;
        private ObservableCollection<Admin.AppServiceTier> _tiers;

        public AppService() 
            : base("app_service")
        {
        }

        public Admin.App App
        {
            get { return _application; }
            set { SetProperty(ref _application, value); }
        }

        public override string Key
        {
            get { return _tag; }
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

        [JsonProperty("rules")]
        public string Rules
        {
            get { return _rules; }
            set { SetProperty(ref _rules, value); }
        }

        [JsonProperty("features_all")]
        public string FeaturesAll
        {
            get { return _featuresAll; }
            set { SetProperty(ref _featuresAll, value); }
        }

        public string[] FeaturesAllArray
        {
            get
            {
                List<string> values = JsonConvert.DeserializeObject<List<string>>(
                    _featuresAll);

                ResourceManager resmgr = new ResourceManager("Inkon.Nester.Properties.Resources"
                                    , typeof(AppService).GetTypeInfo().Assembly);

                List<string> TranslatedValues = new List<string>();
                foreach (string value in values)
                {
                    var translation = resmgr.GetString(value, 
                        System.Globalization.CultureInfo.CurrentUICulture);
                    TranslatedValues.Add(translation);
                }

                return TranslatedValues.ToArray<string>();
            }
        }

        [JsonProperty("port")]
        public Int64? Port
        {
            get { return _port; }
            set { SetProperty(ref _port, value); }
        }

        public ObservableCollection<Admin.AppServiceTier> Tiers
        {
            get { return _tiers; }
            set { SetProperty(ref _tiers, value); }
        }
    }
}
