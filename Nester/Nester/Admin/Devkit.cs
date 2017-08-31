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
    public class Devkit : Cloud.ManagedEntity
    {
        private Admin.Contact _contact;

        public Devkit() 
            : base("devkit")
        {
        }

        public Admin.Contact Contact
        {
            get { return _contact; }
            set
            {
                _contact = value;
            }
        }

        public override string Key
        {
            get { return "0"; }
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
    }
}
