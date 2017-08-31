namespace Nester.Auth
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

