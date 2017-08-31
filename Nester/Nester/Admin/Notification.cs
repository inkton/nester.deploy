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

    public class Notification : Cloud.ManagedEntity
    {
        private Int64 _id;
        private Int64 _appId;
        private string _tag;
        private string _activity;
        private string _support_text;
        private string _createdAt;

        private App _application = null;

        public Notification()
            : base("notification")
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
        
        [JsonProperty("tag")]
        public string Tag
        {
            get { return _tag; }
            set
            {
                SetProperty(ref _tag, value);
            }
        }

        [JsonProperty("app_id")]
        public Int64 AppId
        {
            get { return _appId; }
            set { SetProperty(ref _appId, value); }
        }

        [JsonProperty("activity")]
        public string Activity
        {
            get { return _activity; }
            set
            {
                SetProperty(ref _activity, value);
            }
        }

        [JsonProperty("support_text")]
        public string SupportText
        {
            get { return _support_text; }
            set
            {
                SetProperty(ref _support_text, value);
            }
        }

        [JsonProperty("created_at")]
        public string CreatedAt
        {
            get { return _createdAt; }
            set
            {
                SetProperty(ref _createdAt, value);
            }
        }
    }
}

