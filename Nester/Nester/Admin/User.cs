﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace Nester.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;

    public class User : Cloud.ManagedEntity
    {
        private Int64 _id = 0;
        private string _email;
        private string _nickname;
        private string _territoryISOCode;
        private string _firstName;
        private string _lastName;
        private bool _active;

        public User() 
            : base("user")
        {
        }

        public override string Key
        {
            get { return _id.ToString(); }
        }

        [JsonProperty("id")]
        public Int64 Id
        {
            get { return _id; }
            set { _id = value; }
        }

        [JsonProperty("is_activated")]
        public bool IsActive
        {
            get { return _active; }
            set { SetProperty(ref _active, value); }
        }

        [JsonProperty("email")]
        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }

        [JsonProperty("nickname")]
        public string Nickname
        {
            get { return _nickname; }
            set { SetProperty(ref _nickname, value); }
        }

        [JsonProperty("territory_iso_code")]
        public string TerritoryISOCode
        {
            get { return _territoryISOCode; }
            set { SetProperty(ref _territoryISOCode, value); }
        }

        [JsonProperty("first_name")]
        public string FirstName
        {
            get { return _firstName; }
            set { SetProperty(ref _firstName, value); }
        }

        [JsonProperty("surname")]
        public string LastName
        {
            get { return _lastName; }
            set { SetProperty(ref _lastName, value); }
        }
    }
}
