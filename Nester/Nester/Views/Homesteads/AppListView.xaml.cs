 using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nester.Admin;
using Xamarin.Forms;
using PCLAppConfig;

namespace Nester.Views
{
    public partial class AppListView : Nester.Views.View
    {
        private Admin.App _editApp;
        private long _updatInterval;
        private long _lastUpdate = 0;

        public AppListView()
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonAppReload,
                    ButtonAppRemove,
                    ButtonAppAdd,
                    ButtonAppJoin,
                    AppModels
                });

            AppModels.SelectionChanged += AppModels_SelectionChangedAsync;

            ButtonAppJoin.Clicked += ButtonAppJoin_ClickedAsync;
            ButtonAppReload.Clicked += ButtonAppReload_ClickedAsync;
            ButtonAppAdd.Clicked += ButtonAppAdd_ClickedAsync;
            ButtonAppRemove.Clicked += ButtonAppRemove_ClickedAsync;

            AuthViewModel = new AuthViewModel();
            AppViewModel = new AppCollectionViewModel();

            BindingContext = AppViewModel;

            _updatInterval = Convert.ToInt64(
                ConfigurationManager.AppSettings["appStatusRefreshInterval"]);
        }

        public AppCollectionViewModel AppCollectionModel
        {
            get
            {
                return AppViewModel as AppCollectionViewModel;
            }
        }

        public Admin.App EditApp
        {
            set
            {
                _editApp = value;
            }
            get
            {
                return _editApp;
            }
        }

        public void Init(Func<Views.View, bool> viewLoader)
        {
            try
            {
                _viewLoader = viewLoader;
                AppCollectionModel.Init(viewLoader);
                Cloud.ServerStatus status = new Cloud.ServerStatus(0);

                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    if (_lastUpdate > _updatInterval)
                    {
                        Task.Factory.StartNew(async () =>
                        {
                            foreach (AppViewModel appModel in AppCollectionModel.AppModels)
                            {
                                if (appModel.EditApp.IsBusy)
                                {
                                    await appModel.InitAsync();
                                }

                                if (AppCollectionModel.CurrentView != null &&
                                    appModel.EditApp.Id == AppCollectionModel.CurrentView.App.Id)
                                {
                                    if (appModel.EditApp.IsBusy && 
                                            (!(AppCollectionModel.CurrentView is Views.BannerView ) ||
                                            (AppCollectionModel.CurrentView is Views.BannerView && 
                                                (AppCollectionModel.CurrentView as Views.BannerView).State == 
                                                    BannerView.Status.BannerViewWaitingDeployment)
                                            ) ||
                                        !appModel.EditApp.IsBusy &&
                                            ((!appModel.EditApp.IsDeployed && 
                                                (!(AppCollectionModel.CurrentView is Views.BannerView)) ||
                                                    (AppCollectionModel.CurrentView is Views.BannerView && 
                                                        (AppCollectionModel.CurrentView as Views.BannerView).State !=
                                                    BannerView.Status.BannerViewWaitingDeployment )
                                                ) ||
                                            (appModel.EditApp.IsDeployed && AppCollectionModel.CurrentView is Views.BannerView)))
                                    {
                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            // Update the UI
                                            AppCollectionModel.LoadApp(appModel);
                                        });
                                    }
                                }
                            }
                        });

                        _lastUpdate = 0;
                    }

                    ++_lastUpdate;

                    return true;
                });
            }
            catch (Exception ex)
            {
                DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        private async void ButtonAppJoin_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                AppViewModel appViewModel = new AppViewModel();

                appViewModel.ContactModel.EditInvitation.User = ThisUI.User;

                await appViewModel.ContactModel.QueryInvitationsAsync();
                
                await Navigation.PushAsync(
                    new AppJoinDetailView(appViewModel));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        private async void ButtonAppReload_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                if (AppModels.SelectedItem != null)
                {
                    AppViewModel appModel = AppModels.SelectedItem as AppViewModel;
                    await appModel.InitAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private async void ButtonAppRemove_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                if (AppModels.SelectedItem == null)
                {
                    IsServiceActive = false;
                    return;
                }

                AppViewModel appModel = AppModels.SelectedItem as AppViewModel;
                if (appModel != null)
                {
                    if (appModel.EditApp.Deployment != null)
                    {
                        await DisplayAlert("Nester", "Please remove the deployment before removing the app", "OK");
                        IsServiceActive = false;
                        return;
                    }

                    var yes = await DisplayAlert("Nester", "Are you certain to remove this app?", "Yes", "No");

                    if (yes)
                    {
                        try
                        {
                            await appModel.RemoveAppAsync();
                            AppCollectionModel.RemoveApp(appModel);
                        }
                        catch (Exception ex)
                        {
                            await DisplayAlert("Nester", ex.Message, "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private async void ButtonAppAdd_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                AppViewModel.NewAppAsync();
                AppViewModel.WizardMode = true;

                await Navigation.PushAsync(
                    new AppBasicDetailView(AppViewModel));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private void AppModels_SelectionChangedAsync(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            AppViewModel appModel = e.AddedItems.FirstOrDefault() as AppViewModel;
            if (appModel != null)
            {
                AppCollectionModel.LoadApp(appModel);
            }
        }

        public void InitViews()
        {
            AppViewModel appModel = AppCollectionModel.AppModels.FirstOrDefault();
            if (appModel != null)
            {
                AppCollectionModel.LoadApp(appModel);
            }
        }
    }
}
