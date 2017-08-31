using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace Nester.Admin
{
    public class ChartsBasket : Cloud.ManagedEntity
    {
        private string _id;
        private object _basket;

        private Admin.App _application = null;

        public ChartsBasket()
            : base("charts_basket")
        {
        }

        public Admin.App App
        {
            get { return _application; }
            set
            {
                _application = value;
            }
        }

        public override string Key
        {
            get { return _id; }
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
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        [JsonProperty("basket")]
        public object Basket
        {
            get { return _basket; }
            set { _basket = value; }
        }

        public Chart GetChart(string id)
        {
            List<object> charts = _basket as List<object>;

            foreach (ExpandoObject obj in charts)
            {
                Chart chart = new Chart();
                Utils.Object.CopyExpandoPropertiesTo(obj, chart);
                if (chart.Id == id)
                {
                    return chart;
                }
            }

            return null;
        }
    }
}