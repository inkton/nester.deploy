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
    public class ServicesViewModel : ViewModel
    {
        private ObservableCollection<Admin.AppService> _appServices;

        public ServicesViewModel(Admin.App app) : base(app)
        {
            _appServices = new ObservableCollection<Admin.AppService>();
        }

        public ObservableCollection<Admin.AppService> Services
        {
            get
            {
                return _appServices;
            }
            set
            {
                SetProperty(ref _appServices, value);
            }
        }

        override public async Task<Cloud.ServerStatus> InitAsync()
        {
            Cloud.ServerStatus status;

            status = await QueryAppSubscriptions();
            if (status.Code < 0)
            {
                return status;
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryServicesAsync(
            bool doCache = true, bool throwIfError = true)
        {
            Admin.AppService serviceSeed = new Admin.AppService();
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                throwIfError, serviceSeed, doCache);

            if (status.Code < 0)
            {
                return status;
            }

            _appServices = status.PayloadToList<Admin.AppService>();
            Admin.AppServiceTier tierSeed = new Admin.AppServiceTier();

            foreach (Admin.AppService service in _appServices)
            {
                tierSeed.Service = service;

                status = await Cloud.Result.WaitForObjectListAsync(
                    true, tierSeed);
                if (status.Code != 0)
                {
                    return status;
                }

                service.Tiers = status.PayloadToList<Admin.AppServiceTier>();
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryAppServiceTierLocationsAsync(Admin.AppServiceTier teir,
            bool doCache = true, bool throwIfError = true)
        {
            Admin.Forest forestSeeder = new Admin.Forest();
            forestSeeder.AppServiceTier = teir;

            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                 throwIfError, forestSeeder, doCache);

            return status;
        }

        public async Task<Cloud.ServerStatus> CreateSubscription(Admin.AppServiceTier tier,
            bool doCache = true, bool throwIfError = true)
        {
            Admin.AppServiceSubscription subscription = new Admin.AppServiceSubscription();
            subscription.App = _editApp;
            subscription.ServiceTier = tier;
            subscription.AppServiceTierId = tier.Id;

            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                subscription, new Cloud.NesterService.CachedHttpRequest<Admin.AppServiceSubscription>(
                    ThisUI.NesterService.CreateAsync), doCache);

            if (status.Code == 0)
            {
                Utils.Object.PourPropertiesTo(
                    status.PayloadToObject<Admin.AppServiceSubscription>(), subscription);
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> RemoveSubscription(Admin.AppServiceSubscription subscription,
             bool doCache = false, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                subscription, new Cloud.NesterService.CachedHttpRequest<Admin.AppServiceSubscription>(
                    ThisUI.NesterService.RemoveAsync), doCache);
            return status;
        }

        public async Task<Cloud.ServerStatus> QueryAppSubscriptions(Admin.App app = null,
            bool doCache = false, bool throwIfError = true)
        {
            if (!_appServices.Any())
            {
                await QueryServicesAsync();
            }

            Admin.AppServiceSubscription subSeeder = new Admin.AppServiceSubscription();
            subSeeder.App = (app == null ? _editApp : app);

            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                 throwIfError, subSeeder, doCache);

            if (status.Code >= 0)
            {
                ObservableCollection<Admin.AppServiceSubscription> serviceSubscriptions = 
                    status.PayloadToList<Admin.AppServiceSubscription>();

                foreach (Admin.AppServiceSubscription subscription in serviceSubscriptions)
                {
                    subscription.App = subSeeder.App;

                    foreach (Admin.AppService service in _appServices)
                    {
                        foreach (Admin.AppServiceTier tier in service.Tiers)
                        {
                            if (subscription.AppServiceTierId == tier.Id)
                            {
                                subscription.ServiceTier = tier;
                            }
                        }
                    }
                }

                subSeeder.App.Subscriptions = serviceSubscriptions;
            }

            return status;
        }
    }
}
