/*
    Copyright (c) 2017 Inkton.

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the "Software"),
    to deal in the Software without restriction, including without limitation
    the rights to use, copy, modify, merge, publish, distribute, sublicense,
    and/or sell copies of the Software, and to permit persons to whom the Software
    is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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