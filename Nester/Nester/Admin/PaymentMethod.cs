using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nester.Admin
{
    public class ChargeCardToken : Cloud.ManagedEntity
    {
        public ChargeCardToken()
             : base("token")
        {
        }

        [JsonProperty("brand")]
        public string Brand
        {
            get;
            set;
        }
        [JsonProperty("country")]
        public string Country
        {
            get;
            set;
        }

        [JsonProperty("last4")]
        public Int64 Last4
        {
            get;
            set;
        }

        [JsonProperty("exp_month")]
        public Int64 ExpMonth
        {
            get;
            set;
        }

        [JsonProperty("exp_year")]
        public Int64 ExpYear
        {
            get;
            set;
        }
    }

    public class PaymentMethod : Cloud.ManagedEntity
    {
        private Int64 _id;
        private string _type = "cc";
        private string _tag = "stripe_cc";
        private string _name;
        private ChargeCardToken _proof = null;
        private Admin.User _user = null;

        public PaymentMethod() 
            : base("payment_method")
        {
        }

        public Admin.User Owner
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        public override string Key
        {
            get { return _tag; }
        }

        override public string Collection
        {
            get
            {
                return _user.CollectionKey + base.Collection;
            }
        }

        override public string CollectionKey
        {
            get {
                return _user.CollectionKey + base.CollectionKey;
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
            set { _tag = value; }
        }

        [JsonProperty("type")]
        public string type
        {
            get { return _type; }
            set { _type = value; }
        }

        [JsonProperty("name")]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        [JsonProperty("token")]
        public ChargeCardToken Proof
        {
            get { return _proof; }
            set { SetProperty(ref _proof, value); }
        }
    }
}
