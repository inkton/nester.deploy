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
    public class SoftwareFramework : Cloud.ManagedEntity
    {
        private Int64 _id;
        private string _tag;
        private string _name;

        public class Version : Cloud.ManagedEntity
        {
            private Int64 _id;
            private string _tag;
            private string _name;
            SoftwareFramework _framework;

            public Version()
                : base("software_framework_version")
            {
            }

            public override string ToString()
            {
                return _name;
            }

            public SoftwareFramework Framework
            {
                get { return _framework; }
                set { _framework = value; }
            }

            public override string Key
            {
                get { return _id.ToString(); }
            }

            override public string Collection
            {
                get
                {
                    if (_framework != null)
                    {
                        return _framework.CollectionKey + base.Collection;
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
                    if (_framework != null)
                    {
                        return _framework.CollectionKey + base.CollectionKey;
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
                set { _tag = value; }
            }

            [JsonProperty("name")]
            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }
        }

        public SoftwareFramework()
            : base("software_framework")
        {
        }

        public override string Key
        {
            get { return _tag; }
        }

        override public string Collection
        {
            get
            {
                return base.Collection;
            }
        }

        override public string CollectionKey
        {
            get
            {
                return base.CollectionKey;
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

        [JsonProperty("name")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}
