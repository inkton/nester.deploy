using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Nester.Admin
{
    public class Collaboration : Cloud.ManagedEntity
    {
        private string _accountId;
        private string _state;
        private Contact _contact;

        public Collaboration()
            : base("collaboration")
        {
        }

        public Contact Contact
        {
            get { return _contact; }
            set { _contact = value; }
        }

        public override string Key
        {
            get { return _accountId; }
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

        [JsonProperty("account_id")]
        public string AccountId
        {
            get { return _accountId; }
            set { SetProperty(ref _accountId, value); }
        }

        [JsonProperty("state")]
        public string State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }
    }
}
