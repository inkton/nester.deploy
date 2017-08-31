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

    public class Nest : Cloud.ManagedEntity
    {
        private Int64 _id;
        private Int64 _appId;
        private string _tag;
        private string _name;
        private Int64? _platformId;
        private string _status;
        private Int64 _scale;
        private string _scaleSize;
        private NestPlatform _platform;

        private Admin.App _application = null;

        public Nest() : 
            base("nest")
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

        public NestPlatform Platform
        {
            get { return _platform; }
            set
            {
                if (value != null)
                {
                    _platformId = value.Id;
                    OnPropertyChanged("Icon");
                }

                SetProperty(ref _platform, value);
            }
        }

        [JsonProperty("platform_id")]
        public Int64? PlatformId
        {
            get { return _platformId; }
            set
            {
                SetProperty(ref _platformId, value);
            }
        }

        [JsonProperty("status")]
        public string NestStatus
        {
            get { return _status; }
            set
            {
                SetProperty(ref _status, value);
            }
        }

        [JsonProperty("scale")]
        public Int64 Scale
        {
            get { return _scale; }
            set
            {
                SetProperty(ref _scale, value);
            }
        }

        [JsonProperty("scale_size")]
        public string ScaleSize
        {
            get { return _scaleSize; }
            set
            {
                SetProperty(ref _scaleSize, value);
            }
        }

        public string Icon
        {
            get
            {
                if (_platform == null)
                {
                    return string.Empty;
                }

                if (_platform.Tag == "mvc")
                {
                    return "webnet32.png";
                }
                else if (_platform.Tag == "api")
                {
                    return "websocketnet32.png";
                }
                else
                {
                    return "worker32.png";
                }
            }
        }
    }
}

