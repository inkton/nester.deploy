using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Nester.Views
{
    public class AppViewModel : ViewModel
    {
        private bool _mariaDBEnabled = false;

        private PaymentViewModel _paymentModel;
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
            _editApp.Owner = this.ThisUI.User;

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

            _paymentModel = new PaymentViewModel();
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
                    isOwner = _editApp.UserId == this.ThisUI.User.Id;
                }

                return isOwner;
            }
        }

        override public bool WizardMode
        {
            get
            {
                return _wizardMode;
            }
            set
            {
                SetProperty(ref _wizardMode, value);

                _contactViewModel.WizardMode = value;
                _nestViewModel.WizardMode = value;
                _domainViewModel.WizardMode = value;
                _deploymentViewModel.WizardMode = value;
                _servicesViewModel.WizardMode = value;
                _logViewModel.WizardMode = value;
            }
        }

        public PaymentViewModel PaymentModel
        {
            get
            {
                return _paymentModel;
            }
            set
            {
                SetProperty(ref _paymentModel, value);
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

            status = await _paymentModel.InitAsync();
            if (status.Code < 0 &&
                status.Code != Cloud.Result.NEST_RESULT_ERROR_PMETHOD_NFOUND)
            {
                return status;
            }

            // Get the app when query all relations
            status = await QueryAppAsync();
            if (status.Code < 0)
            {
                return status;
            }

            status = await _deploymentViewModel.InitAsync();
            if (status.Code < 0)
            {
                return status;
            }

            status = await _servicesViewModel.InitAsync();
            if (status.Code < 0)
            {
                return status;
            }

            _mariaDBEnabled = SelectedMariaDBTier != null;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            _contactViewModel.InitAsync();
            _nestViewModel.InitAsync();
            _domainViewModel.InitAsync();

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            OnPropertyChanged("EditApp");

            return status;
        }

        async public void NewAppAsync()
        {
            _editApp = new Admin.App();
            _editApp.Type = "uniflow";
            _editApp.Owner = this.ThisUI.User;

            _contactViewModel.EditApp = _editApp;
            _nestViewModel.EditApp = _editApp;
            _domainViewModel.EditApp = _editApp;
            _deploymentViewModel.EditApp = _editApp;
            _servicesViewModel.EditApp = _editApp;

            await ServicesViewModel.QueryServicesAsync();
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

        public async Task<Cloud.ServerStatus> QueryAppAsync(Admin.App app = null,
            bool bCache = false, bool throwIfError = true)
        {
            Admin.App theApp = app == null ? _editApp : app;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theApp, new Cloud.CachedHttpRequest<Admin.App>(
                    ThisUI.NesterService.QueryAsync), bCache);
           
            if (status.Code == 0)
            {
                EditApp = status.PayloadToObject<Admin.App>();

                if (app != null)
                {
                    Utils.Object.PourPropertiesTo(_editApp, app);
                }

                if (_editApp.UserId == ThisUI.User.Id)
                {
                    _editApp.Owner = ThisUI.User;
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
                    ThisUI.NesterService.RemoveAsync), doCache);

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
                    ThisUI.NesterService.UpdateAsync), doCache);

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
                    ThisUI.NesterService.CreateAsync), doCache);

            if (status.Code == 0)
            {
                EditApp = status.PayloadToObject<Admin.App>();
                _editApp.Owner = this.ThisUI.User;

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
