using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Nester.Views
{
    public class NestViewModel : ViewModel
    {
        private Admin.Nest _editNest;

        private ObservableCollection<Admin.Nest> _nests = null;        
        private ObservableCollection<Admin.NestPlatform> _platforms = null;

        public ICommand RemoveCommand { get; private set; }

        public NestViewModel(Admin.App app) : base(app)
        {
            _nests = new ObservableCollection<Admin.Nest>();
            _platforms = new ObservableCollection<Admin.NestPlatform>();

            _editNest = new Admin.Nest();
            _editNest.App = app;

            RemoveCommand = new Command<Admin.Nest>(SendRemoveMessage);
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
                _editNest.App = value;
            }
        }

        public void SendRemoveMessage(Admin.Nest nest)
        {
            ManagedObjectMessage<Admin.Nest> doThis =
                new ManagedObjectMessage<Admin.Nest>("remove", nest);
            MessagingCenter.Send(doThis, doThis.Type);
        }

        public Admin.Nest EditNest
        {
            get
            {
                return _editNest;
            }
            set
            {
                SetProperty(ref _editNest, value);
            }
        }

        public ObservableCollection<Admin.Nest> Nests
        {
            get
            {
                return _nests;
            }
            set
            {
                SetProperty(ref _nests, value);
            }
        }

        public ObservableCollection<Admin.NestPlatform> Platforms
        {
            get
            {
                return _platforms;
            }
            set
            {
                SetProperty(ref _platforms, value);
            }
        }

        override public async Task<Cloud.ServerStatus> InitAsync()
        {
            Cloud.ServerStatus status;

            status = await QueryNestPlatformsAsync();
            if (status.Code < 0)
            {
                return status;
            }

            status = await QueryNestsAsync();
            return status;
        }

        public async Task<Cloud.ServerStatus> QueryNestPlatformsAsync(
            bool doCache = false, bool throwIfError = true)
        {
            Admin.NestPlatform platformSeed = new Admin.NestPlatform();
            platformSeed.App = _editApp;

            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                throwIfError, platformSeed, doCache);

            if (status.Code >= 0)
            {
                _platforms = status.PayloadToList<Admin.NestPlatform>();
            }

            return status;
        }

        private void SetNestHosts(Admin.Nest nest)
        {
            foreach (Admin.NestPlatform platform in _platforms)
            {
                if (nest.PlatformId == platform.Id)
                {
                    nest.Platform = platform;
                    break;
                }
            }
        }

        private void SetNestsHosts()
        {
            foreach (Admin.Nest nest in _nests)
            {
                SetNestHosts(nest);
            }
        }

        public async Task<Cloud.ServerStatus> QueryNestsAsync(
            bool doCache = false, bool throwIfError = true)
        {
            _editNest.App = _editApp;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                throwIfError, _editNest, doCache);

            if (status.Code >= 0)
            {
                _nests = status.PayloadToList<Admin.Nest>();
                SetNestsHosts();
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryNestAsync(Admin.Nest nest,
             bool dCache = true, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                nest, new Cloud.NesterService.CachedHttpRequest<Admin.Nest>(
                    ThisUI.NesterService.QueryAsync), dCache, null, null);

            if (status.Code >= 0)
            {
                Utils.Object.PourPropertiesTo(status.PayloadToObject<Admin.Nest>(), nest);
                SetNestHosts(nest);
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> CreateNestAsync(Admin.Nest nest,
            bool doCache = false, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                nest, new Cloud.NesterService.CachedHttpRequest<Admin.Nest>(
                    ThisUI.NesterService.CreateAsync), doCache);

            if (status.Code >= 0)
            {
                _editNest = status.PayloadToObject<Admin.Nest>();
                SetNestHosts(_editNest);
                _nests.Add(_editNest);
                Utils.Object.PourPropertiesTo(_editNest, nest);
                OnPropertyChanged("Nests");
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> UpdateNestAsync(Admin.Nest nest,
            bool doCache = false, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                nest, new Cloud.NesterService.CachedHttpRequest<Admin.Nest>(
                    ThisUI.NesterService.UpdateAsync), doCache);

            if (status.Code >= 0)
            {
                _editNest = status.PayloadToObject<Admin.Nest>();
                Utils.Object.PourPropertiesTo(_editNest, nest);
                SetNestHosts(_editNest);
                OnPropertyChanged("Nests");
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> RemoveNestAsync(Admin.Nest nest,
            bool doCache = false, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                nest, new Cloud.NesterService.CachedHttpRequest<Admin.Nest>(
                    ThisUI.NesterService.RemoveAsync), doCache);

            if (status.Code >= 0)
            {
                _nests.Remove(nest);
                OnPropertyChanged("Nests");
            }

            return status;
        }
    }
}
