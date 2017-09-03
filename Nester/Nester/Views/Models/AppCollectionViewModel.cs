using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nester.Views
{
    public class AppCollectionViewModel : AppViewModel
    {
        private ObservableCollection<AppViewModel> _appModels;
        private Func<Views.View, bool> _viewLoader;
        private Views.View _currentView = null;

        public AppCollectionViewModel()
        {
            _appModels = new ObservableCollection<AppViewModel>();
        }

        public ObservableCollection<AppViewModel> AppModels
        {
            get
            {
                return _appModels;
            }
            set
            {
                SetProperty(ref _appModels, value);
            }
        }

        public Views.View CurrentView
        {
            get
            {
                return _currentView;
            }
        }

        public void Init(Func<Views.View, bool> viewLoader)
        {
            _viewLoader = viewLoader;
        }

        public async Task<Cloud.ServerStatus> LoadApps(
            bool doCache = true, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                throwIfError, _editApp, doCache);

            if (status.Code == 0)
            {
                ObservableCollection<Admin.App> apps = status.PayloadToList<Admin.App>();

                if (apps.Any())
                {
                    foreach (Admin.App app in apps)
                    {
                        AppViewModel appModel = new AppViewModel();
                        appModel.EditApp = app;
                        await appModel.InitAsync();
                        _appModels.Add(appModel);
                    }

                    LoadApp(_appModels.First());
                    OnPropertyChanged("Apps");
                }
                else
                {
                    _currentView = new BannerView();
                    (_currentView as BannerView).State = BannerView.Status.BannerViewUndefined;
                    _viewLoader(_currentView);
                }
            }

            return status;
        }

        public void AddApp(Admin.App app)
        {
            AppViewModel appModel = new AppViewModel();
            appModel.EditApp = app;
            Task.Run(() => appModel.InitAsync());
            AddModel(appModel);
        }

        public void AddModel(AppViewModel appModel)
        {
            _appModels.Add(appModel);
            LoadApp(appModel);
        }

        public void RemoveApp(AppViewModel appModel)
        {
            _appModels.Remove(appModel);
            LoadApp(_appModels.First());
        }

        public bool LoadApp(AppViewModel appModel)
        {
            if (appModel.EditApp.IsBusy)
            {
                _currentView = new BannerView();
                (_currentView as BannerView).State = BannerView.Status.BannerViewUpdating;
            }
            else if (!appModel.EditApp.IsDeployed)
            {
                _currentView = new BannerView();
                (_currentView as BannerView).State = BannerView.Status.BannerViewWaitingDeployment;
            }
            else
            {
                _currentView = new Views.AppView(appModel);
            }

            _currentView.AppViewModel = appModel;
            _currentView.LoadView = _viewLoader;
            _viewLoader(_currentView);

            return true;
        }
    }
}
