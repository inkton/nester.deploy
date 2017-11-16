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

namespace Nester.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using System.Collections.ObjectModel;

    public class App : Cloud.ManagedEntity
    {
        private Int64 _id;
        private string _tag;
        private string _name;
        private string _type;
        private string _status;
        private Int64 _userId;
        private Int64? _treeId;
        private Int64? _primaryDomainTag;
        private string _unifiedPassword, _networkPassword, _servicesPassword;
        private Int64? _serviceTierId;

        private Admin.User _owner = null;
        private Admin.Deployment _deployment = null;
        private Auth.OwnerCapabilities _ownerCapabilities = null;
        private ObservableCollection<Admin.AppServiceSubscription> _subscriptions;

        public App()
            : base("app")
        {
            _status = "init";
        }

        public Admin.User Owner
        {
            get { return _owner; }
            set { SetProperty(ref _owner, value); }
        }

        public string Hostname
        {
            get { return _tag + ".nestapp.yt"; }
        }

        public string ApiEndpoint
        {
            get
            {
                return "http://" + Hostname + "/v1/";
            }
        }

        public override string Key
        {
            get { return _tag; }
        }

        override public string Collection
        {
            get
            {
                if (_owner != null && _owner.Id > 0)
                {
                    return _owner.CollectionKey + base.Collection;
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
                if (_owner != null && _owner.Id > 0)
                {
                    return _owner.CollectionKey + base.CollectionKey;
                }
                else
                {
                    return base.CollectionKey;
                }
            }
        }

        public Auth.OwnerCapabilities OwnerCapabilities
        {
            get { return _ownerCapabilities; }
            set { SetProperty(ref _ownerCapabilities, value); }
        }

        public ObservableCollection<Admin.AppServiceSubscription> Subscriptions
        {
            get { return _subscriptions; }
            set { SetProperty(ref _subscriptions, value); }
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
            set { SetProperty(ref _tag, value); }
        }

        [JsonProperty("name")]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        [JsonProperty("type")]
        public string Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        [JsonProperty("status")]
        public string Status
        {
            get { return _status; }
            set
            {
                SetProperty(ref _status, value);
                OnPropertyChanged("CurrentActivity");
            }
        }

        public bool IsDeploymentValid
        {
            get
            {
                return (_deployment != null && _deployment.Id >= 0);
            }
        }

        public string CurrentActivity
        {
            get
            {
                if (IsDeploymentValid)
                {
                    if (_deployment.Status == "updating")
                    {
                        return "Updating";
                    }
                    else if (_deployment.Status == "failed")
                    {
                        return "Failed";
                    }
                    else
                    {
                        return char.ToUpper(_status[0]) +
                            _status.Substring(1);
                    }
                }
                else
                {
                    return char.ToUpper(_status[0]) +
                        _status.Substring(1);
                }
            }
        }

        public bool IsDeployed
        {
            get
            {
                if (IsDeploymentValid)
                {
                    return (_deployment.Status == "complete" ||
                            _deployment.Status == "failed");
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsBusy
        {
            get
            {
                if (IsDeploymentValid)
                {
                    return (_deployment.Status == "updating");
                }
                return false;
            }
        }

        public bool IsActive
        {
            get
            {
                if (IsDeploymentValid)
                {
                    return (_deployment.Status == "complete" &&
                        _status == "deployed");
                }
                return false;
            }
        }

        [JsonProperty("primary_domain_id")]
        public Int64? PrimaryDomainId
        {
            get { return _primaryDomainTag; }
            set { SetProperty(ref _primaryDomainTag, value); }
        }

        [JsonProperty("network_password")]
        public string NetworkPassword
        {
            get { return _networkPassword; }
            set { SetProperty(ref _networkPassword, value); }
        }

        [JsonProperty("services_password")]
        public string ServicesPassword
        {
            get { return _servicesPassword; }
            set
            {
                SetProperty(ref _servicesPassword, value);

                // the default is the services 
                // password
                _unifiedPassword = _servicesPassword;
            }
        }

        public string UnifiedPassword
        {
            get { return _unifiedPassword; }
            set
            {
                SetProperty(ref _unifiedPassword, value);
                _networkPassword = _unifiedPassword;
                _servicesPassword = _unifiedPassword;
            }
        }

        [JsonProperty("user_id")]
        public Int64 UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        [JsonProperty("tree_id", NullValueHandling = NullValueHandling.Ignore)]
        public Int64? TreeId
        {
            get { return _treeId; }
            set { _treeId = value; }
        }

        [JsonProperty("app_service_tier_id", NullValueHandling = NullValueHandling.Ignore)]
        public Int64? ServiceTierId
        {
            get { return _serviceTierId; }
            set { SetProperty(ref _serviceTierId, value); }
        }

        [JsonIgnore]
        public Admin.Deployment Deployment
        {
            get { return _deployment; }
            set
            {
                SetProperty(ref _deployment, value);

                OnPropertyChanged("IsDeployed");
                OnPropertyChanged("IsBusy");
                OnPropertyChanged("IsActive");

                OnPropertyChanged("CurrentActivity");
            }
        }

        public string Icon
        {
            get
            {
                if (_status == "deployed")
                {
                    return "homesteadactive32.png";
                }
                else
                {
                    return "homesteadinactive32.png";
                }
            }
        }
    }
}

