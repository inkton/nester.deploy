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
    public class Invitation : Contact
    {
        private string _appName;
        private string _appTag;
        private Admin.User _user = null;

        public Invitation() 
            : base("invitation", "invitations")
        {
        }

        override public string Collection
        {
            get
            {
                if (_user != null)
                {
                    return _user.CollectionKey + base.Collection;
                }
                else
                {
                    return base.Collection;
                }
            }
        }

        public Admin.User User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        public override string Icon
        {
            get
            {
                if (Status == "active") 
                {
                    return "invitejoin32.png";
                }
                else
                {
                    return "inviteleave32.png";
                }
            }
        }

        [JsonProperty("app_tag")]
        public string AppTag
        {
            get { return _appTag; }
            set { SetProperty(ref _appTag, value); }
        }

        [JsonProperty("app_name")]
        public string AppName
        {
            get { return _appName; }
            set { SetProperty(ref _appName, value); }
        }
    }
}
