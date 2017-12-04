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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Inkton.Nester.Views
{
    public class AppViewModel : ViewModel
    {
        private bool _mariaDBEnabled = false;

        private ContactViewModel _contactViewModel;
        private NestViewModel _nestViewModel;
        private DomainViewModel _domainViewModel;
        private DeploymentViewModel _deploymentViewModel;
        private ServicesViewModel _servicesViewModel;
        private LogViewModel _logViewModel;

        private Admin.AppServiceTier _selectedAppServiceTier;
        private ObservableCollection<Admin.Notification> _notifications;
        
        public class AppType
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Image { get; set; }
            public string Tag { get; set; }
        }

        private AppType _editApplicationType;
        private ObservableCollection<AppType> _applicationTypes;        
 
        public AppViewModel()
        {
            // when editing this will 
            // select uniflow default
            _editApp = new Admin.App();
            _editApp.Type = "uniflow";
            _editApp.Owner = NesterControl.User;

            _applicationTypes = new ObservableCollection<AppType> {
                new AppType {
                    Name ="Uniflow",
                    Description ="MVC Web server",
                    Image ="webnet32.png",
                    Tag = "uniflow"
                },
                new AppType {
                    Name ="Biflow",
                    Description ="API Web server",
                    Image ="websocketnet32.png",
                    Tag = "biflow"
                },
            };

            _contactViewModel = new ContactViewModel(_editApp);
            _nestViewModel = new NestViewModel(_editApp);
            _domainViewModel = new DomainViewModel(_editApp);
            _deploymentViewModel = new DeploymentViewModel(_editApp);
            _servicesViewModel = new ServicesViewModel(_editApp);
            _logViewModel = new LogViewModel(_editApp);
        }

        override public Admin.App EditApp
        {
            get
            {
                return _editApp;
            }
            set
            {
                SetProperty(ref _editApp, value);

                _contactViewModel.EditApp = value;
                _nestViewModel.EditApp = value;
                _domainViewModel.EditApp = value;
                _deploymentViewModel.EditApp = value;
                _servicesViewModel.EditApp = value;
                _logViewModel.EditApp = value;
            }
        }

        public bool IsAppOwner
        {
            get
            {
                bool isOwner = false;

                if (_editApp != null)
                {
                    isOwner = _editApp.UserId == NesterControl.User.Id;
                }

                return isOwner;
            }
        }

        public ContactViewModel ContactModel
        {
            get
            {
                return _contactViewModel;
            }
            set
            {
                SetProperty(ref _contactViewModel, value);
            }
        }

        public NestViewModel NestModel
        {
            get
            {
                return _nestViewModel;
            }
            set
            {
                SetProperty(ref _nestViewModel, value);
            }
        }

        public DomainViewModel DomainModel
        {
            get
            {
                return _domainViewModel;
            }
            set
            {
                SetProperty(ref _domainViewModel, value);
            }
        }

        public DeploymentViewModel DeploymentModel
        {
            get
            {
                return _deploymentViewModel;
            }
            set
            {
                SetProperty(ref _deploymentViewModel, value);
            }
        }

        public ServicesViewModel ServicesViewModel
        {
            get
            {
                return _servicesViewModel;
            }
            set
            {
                SetProperty(ref _servicesViewModel, value);
            }
        }

        public LogViewModel LogViewModel
        {
            get
            {
                return _logViewModel;
            }
            set
            {
                SetProperty(ref _logViewModel, value);
            }
        }

        public ObservableCollection<AppType> ApplicationTypes
        {
            get
            {
                return _applicationTypes;
            }
        }

        public AppType EditApplicationType
        {
            get
            {
                return _editApplicationType;
            }
            set
            {
                SetProperty(ref _editApplicationType, value);
            }
        }

        public ObservableCollection<Admin.Notification> Notifications
        {
            get
            {
                return _notifications;
            }
            set
            {
                SetProperty(ref _notifications, value);
            }
        }

        #region App Tier


        public ObservableCollection<Admin.AppServiceTier> AppServiceTiers
        {
            get
            {
                Admin.AppService service = _servicesViewModel.Services.FirstOrDefault(
                    x => x.Tag == "nest-oak");

                if (service != null)
                {
                    return service.Tiers;
                }

                return null;
            }
        }

        public Admin.AppServiceTier SelectedAppServiceTier
        {
            get
            {
                if (_editApp.Subscriptions != null)
                {
                    Admin.AppServiceSubscription subscription = _editApp.Subscriptions.FirstOrDefault(
                        x => x.ServiceTier.Service.Tag == "nest-oak");

                    if (subscription != null)
                    {
                        return subscription.ServiceTier;
                    }
                }
                else
                {
                    _selectedAppServiceTier = AppServiceTiers.First();
                }

                return _selectedAppServiceTier;
            }
            set
            {
                SetProperty(ref _selectedAppServiceTier, value);
            }
        }

        public string PaymentNotice
        {
            get
            {
                if (NesterControl.User.TerritoryISOCode == "AU")
                {
                    return "The prices are in US Dollars and do not include GST.";
                }
                else
                {
                    return "The prices are in US Dollars. ";
                }                
            }
        }

        #endregion  

        #region MariaDB Tier

        public bool MariaDBEnabled
        {
            get
            {
                return _mariaDBEnabled;
            }
            set
            {
                SetProperty(ref _mariaDBEnabled, value);
            }
        }

        public ObservableCollection<Admin.AppServiceTier> MariaDBTiers
        {
            get
            {
                Admin.AppService service = _servicesViewModel.Services.FirstOrDefault(
                    x => x.Tag == "mariadb");

                if (service != null)
                {
                    return service.Tiers;
                }

                return null;
            }
        }

        public Admin.AppServiceTier SelectedMariaDBTier
        {
            get
            {
                if (_editApp.Subscriptions != null)
                {
                    Admin.AppServiceSubscription subscription = _editApp.Subscriptions.FirstOrDefault(
                        x => x.ServiceTier.Service.Tag == "mariadb");

                    if (subscription != null)
                    {
                        return subscription.ServiceTier;
                    }
                }

                return null;
            }
        }

        #endregion

        #region Letsencrypt Tier

        public Admin.AppServiceTier SelectedLetsencryptTier
        {
            get
            {
                if (_editApp.Subscriptions != null)
                {
                    Admin.AppServiceSubscription subscription = _editApp.Subscriptions.FirstOrDefault(
                        x => x.ServiceTier.Service.Tag == "letsencrypt");

                    if (subscription != null)
                    {
                        return subscription.ServiceTier;
                    }
                }

                return null;
            }
        }

        #endregion

        #region Logging Tier

        public Admin.AppServiceTier SelectedLoggingTier
        {
            get
            {
                if (_editApp.Subscriptions != null)
                {
                    Admin.AppServiceSubscription subscription = _editApp.Subscriptions.FirstOrDefault(
                        x => x.ServiceTier.Service.Tag == "logging");

                    if (subscription != null)
                    {
                        return subscription.ServiceTier;
                    }
                }

                return null;
            }
        }

        #endregion

        #region RabbitMQ Tier

        public Admin.AppServiceTier SelectedRabbitMQTier
        {
            get
            {
                if (_editApp.Subscriptions != null)
                {
                    Admin.AppServiceSubscription subscription = _editApp.Subscriptions.FirstOrDefault(
                        x => x.ServiceTier.Service.Tag == "rabbitmq");

                    if (subscription != null)
                    {
                        return subscription.ServiceTier;
                    }
                }

                return null;
            }
        }

        #endregion

        #region Git Tier

        public Admin.AppServiceTier SelectedGitServiceTier
        {
            get
            {
                if (_editApp.Subscriptions != null)
                {
                    Admin.AppServiceSubscription subscription = _editApp.Subscriptions.FirstOrDefault(
                        x => x.ServiceTier.Service.Tag == "git");

                    if (subscription != null)
                    {
                        return subscription.ServiceTier;
                    }
                }

                return null;
            }
        }

        #endregion

        override public async Task<Cloud.ServerStatus> InitAsync()
        {
            Cloud.ServerStatus status;

            // The only blocking call
            status = QueryApp();
            if (status.Code < 0)
            {
                return status;
            }

            _mariaDBEnabled = SelectedMariaDBTier != null;

            await _deploymentViewModel.InitAsync();
            await _servicesViewModel.InitAsync();
            await _contactViewModel.InitAsync();
            await _nestViewModel.InitAsync();
            await _domainViewModel.InitAsync();

            OnPropertyChanged("EditApp");

            return status;
        }

        async public void NewAppAsync()
        {
            _editApp = new Admin.App();
            _editApp.Type = "uniflow";
            _editApp.Owner = NesterControl.User;

            _contactViewModel.EditApp = _editApp;
            _nestViewModel.EditApp = _editApp;
            _domainViewModel.EditApp = _editApp;
            _deploymentViewModel.EditApp = _editApp;
            _servicesViewModel.EditApp = _editApp;

            await ServicesViewModel.QueryServicesAsync();
        }

        async public void Reload()
        {
            await InitAsync();
            await InitAsync();

            await ServicesViewModel.QueryServicesAsync();
        }

        public async void QueryMetricsAsync(long beginId, long endId)
        {
            if (LogViewModel.NestLogs != null)
            {
                LogViewModel.NestLogs.Clear();
            }
            if (LogViewModel.SystemCPULogs != null)
            {
                LogViewModel.SystemCPULogs.Clear();
            }
            if (LogViewModel.DiskSpaceLogs != null)
            {
                LogViewModel.DiskSpaceLogs.Clear();
            }
            if (LogViewModel.SystemIPV4Logs != null)
            {
                LogViewModel.SystemIPV4Logs.Clear();
            }
            if (LogViewModel.SystemRAMLogs != null)
            {
                LogViewModel.SystemRAMLogs.Clear();
            }

            await LogViewModel.QueryNestLogsAsync(
                string.Format("id >= {0} and id < {1}",
                        beginId, endId
                    ));

            beginId /= 1000;
            endId /= 1000;

            string filter = string.Format("id >= {0} and id < {1}",
                        beginId, endId
                    );

            await LogViewModel.QuerySystemCPULogsAsync(filter);

            await LogViewModel.QueryDiskSpaceLogsAsync(filter);

            await LogViewModel.QuerSystemIPV4LogsAsync(filter);

            await LogViewModel.QuerSystemRAMLogsAsync(filter);
        }

        public async Task<Cloud.ServerStatus> QueryAppNotificationsAsync(Admin.App app = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.App theApp = app == null ? _editApp : app;
            Admin.Notification notificationSeed = new Admin.Notification();
            notificationSeed.App = theApp;

            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
               throwIfError, notificationSeed, doCache);

            if (status.Code == 0)
            {
                _notifications = status.PayloadToList<Admin.Notification>();
                _notifications.All(x => { x.App = theApp; return true; });
            }

            return status;
        }

        public Cloud.ServerStatus QueryApp(Admin.App app = null,
            bool bCache = false, bool throwIfError = true)
        {
            Admin.App theApp = app == null ? _editApp : app;
            Cloud.ServerStatus status = Cloud.Result.WaitForObject(throwIfError,
                theApp, new Cloud.CachedHttpRequest<Admin.App>(
                    NesterControl.NesterService.QueryAsync), bCache);

            if (status.Code == 0)
            {
                EditApp = status.PayloadToObject<Admin.App>();

                if (app != null)
                {
                    Utils.Object.PourPropertiesTo(_editApp, app);
                }

                if (_editApp.UserId == NesterControl.User.Id)
                {
                    _editApp.Owner = NesterControl.User;
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryAppAsync(Admin.App app = null,
            bool bCache = false, bool throwIfError = true)
        {
            Admin.App theApp = app == null ? _editApp : app;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theApp, new Cloud.CachedHttpRequest<Admin.App>(
                    NesterControl.NesterService.QueryAsync), bCache);
           
            if (status.Code == 0)
            {
                EditApp = status.PayloadToObject<Admin.App>();

                if (app != null)
                {
                    Utils.Object.PourPropertiesTo(_editApp, app);
                }

                if (_editApp.UserId == NesterControl.User.Id)
                {
                    _editApp.Owner = NesterControl.User;
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> RemoveAppAsync(Admin.App app = null,
             bool doCache = false, bool throwIfError = true)
        {
            Admin.App theApp = app == null ? _editApp : app;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theApp, new Cloud.CachedHttpRequest<Admin.App>(
                    NesterControl.NesterService.RemoveAsync), doCache);

            if (status.Code == 0)
            {
                EditApp = theApp;
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> UpdateAppAsync(Admin.App app = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.App theApp = app == null ? _editApp : app;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theApp, new Cloud.CachedHttpRequest<Admin.App>(
                    NesterControl.NesterService.UpdateAsync), doCache);

            if (status.Code == 0)
            {
                EditApp = status.PayloadToObject<Admin.App>();

                if (app != null)
                {
                    Utils.Object.PourPropertiesTo(_editApp, app);
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> CreateAppAsync(Admin.App app = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.App theApp = app == null ? _editApp : app;
            theApp.ServiceTierId = _selectedAppServiceTier.Id;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theApp, new Cloud.CachedHttpRequest<Admin.App>(
                    NesterControl.NesterService.CreateAsync), doCache);

            if (status.Code == 0)
            {
                EditApp = status.PayloadToObject<Admin.App>();
                _editApp.Owner = NesterControl.User;

                if (app != null)
                {
                    Utils.Object.PourPropertiesTo(_editApp, theApp);
                }

                if (throwIfError && _editApp.Status != "assigned")
                {
                    string message = "Failed to initialize the app. Please contact support.";
                    Utils.ErrorHandler.Exception(message, string.Empty);
                    throw new Exception(message);
                }

                await InitAsync();
            }
            return status;
        }

        public async Task<Cloud.ServerStatus> QueryAppServiceTierLocationsAsync(Admin.AppServiceTier teir,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.Forest forestSeeder = new Admin.Forest();
            forestSeeder.AppServiceTier = teir;

            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                 throwIfError, forestSeeder, doCache);

            return status;
        }
    }
}
