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

    public class UserEvent : Cloud.ManagedEntity
    {
        private Int64 _id;
        private string _tag;
        private string _activity;
        private string _support_text;
        private string _createdAt;

        private Admin.User _owner = null;

        public UserEvent()
            : base("user_event")
        {
        }

        public Admin.User Owner
        {
            get { return _owner; }
            set { SetProperty(ref _owner, value); }
        }

        public override string Key
        {
            get { return _id.ToString(); }
        }

        override public string Collection
        {
            get
            {
                if (_owner != null && _owner.Id > 0)
                {
                    return _owner.CollectionKey + base.Collection;
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
                if (_owner != null && _owner.Id > 0)
                {
                    return _owner.CollectionKey + base.CollectionKey;
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

