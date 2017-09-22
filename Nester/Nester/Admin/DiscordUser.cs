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
    public class DiscordUser : Contact
    {
        private string _accountId;

        public DiscordUser()
            : base("discord_user", "discord_users")
        {
        }

        [JsonProperty("account_id")]
        public string AccountId
        {
            get { return _accountId; }
            set { SetProperty(ref _accountId, value); }
        }
    }
}
