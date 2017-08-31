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

