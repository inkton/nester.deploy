using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Resources;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Nester.Admin
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

                ResourceManager resmgr = new ResourceManager("Nester.AppResources"
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
