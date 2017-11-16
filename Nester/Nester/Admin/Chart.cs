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
    public class Chart : Cloud.ManagedEntity
    {
        private string _id;
        private string _name;
        private string _type;
        private string _family;
        private string _context;
        private string _title;
        private Int64 _priority;
        private bool _enabled;
        private string _units;
        private string _chartType;
        private Int64 _duration;
        private Int64 _firstEntry;
        private Int64 _lastEntry;
        private Int64 _updateEvery;
        private double _min, _max;
        private Points _result = null;

        private Admin.App _application = null;

        public class Points : Cloud.ManagedEntity
        {
            private string[] _labels;
            private object[,] _data;

            public Points()
            {
            }

            [JsonProperty("labels")]
            public string[] Labels
            {
                get { return _labels; }
                set { _labels = value; }
            }

            [JsonProperty("data")]
            public object[,] Data
            {
                get { return _data; }
                set { _data = value; }
            }
        }

        public Chart()
            : base("chart")
        {
        }

        protected Chart(
            string entity = "chart",
            string subject = "charts") 
            : base(entity, subject)
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
                    string collection = _application.CollectionKey + base.Collection;

                    if (_result != null)
                    {
                        collection += "/data";
                    }

                    return collection;
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
                    string collectionKey = _application.CollectionKey + base.CollectionKey;

                    if (_result != null)
                    {
                        collectionKey += "/data";
                    }

                    return collectionKey;
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

        [JsonProperty("name")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        [JsonProperty("family", NullValueHandling = NullValueHandling.Ignore)]
        public string Family
        {
            get { return _family; }
            set { _family = value; }
        }

        [JsonProperty("context", NullValueHandling = NullValueHandling.Ignore)]
        public string Context
        {
            get { return _context; }
            set { _context = value; }
        }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        [JsonProperty("priority", NullValueHandling = NullValueHandling.Ignore)]
        public Int64 Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        [JsonProperty("units", NullValueHandling = NullValueHandling.Ignore)]
        public string Units
        {
            get { return _units; }
            set { _units = value; }
        }

        [JsonProperty("chart_type", NullValueHandling = NullValueHandling.Ignore)]
        public string ChartType
        {
            get { return _chartType; }
            set { _chartType = value; }
        }

        [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
        public Int64 Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        [JsonProperty("first_entry")]
        public Int64 FirstEntry
        {
            get { return _firstEntry; }
            set { _firstEntry = value; }
        }

        [JsonProperty("last_entry")]
        public Int64 LastEntry
        {
            get { return _lastEntry; }
            set { _lastEntry = value; }
        }

        [JsonProperty("update_every")]
        public Int64 UpdateEvery
        {
            get { return _updateEvery; }
            set { _updateEvery = value; }
        }

        [JsonProperty("min")]
        public double Min
        {
            get { return _min; }
            set { _min = value; }
        }

        [JsonProperty("max")]
        public double Max
        {
            get { return _max; }
            set { _max = value; }
        }

        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public Points Result
        {
            get { return _result; }
            set { _result = value; }
        }
    }
}