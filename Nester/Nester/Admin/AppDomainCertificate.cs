using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class AppDomainCertificate : Cloud.ManagedEntity
    {
        private Int64 _id;
        private string _tag;
        private string _privateKey;
        private string _certificateChain;
        private string _type;

        private Admin.AppDomain _appDomain = null;

        public AppDomainCertificate()
            : base("app_domain_certificate")
        {
        }

        public Admin.AppDomain AppDomain
        {
            get { return _appDomain; }
            set
            {
                _appDomain = value;
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
                if (_appDomain != null)
                {
                    return _appDomain.CollectionKey + base.Collection;
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
                if (_appDomain != null)
                {
                    return _appDomain.CollectionKey + base.CollectionKey;
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

        [JsonProperty("private_key")]
        public string PrivateKey
        {
            get { return _privateKey; }
            set
            {
                SetProperty(ref _privateKey, value);
            }
        }

        [JsonProperty("certificate_chain")]
        public string CertificateChain
        {
            get { return _certificateChain; }
            set
            {
                SetProperty(ref _certificateChain, value);
            }
        }

        [JsonProperty("type")]
        public string Type
        {
            get { return _type; }
            set
            {
                SetProperty(ref _type, value);
                OnPropertyChanged("Icon");
            }
        }

        public string Icon
        {
            get
            {
                if (_type == "custom")
                {
                    return "customdomaincert.png";
                }
                else
                {
                    return "freedomaincert.png";
                }
            }
        }
    }
}

