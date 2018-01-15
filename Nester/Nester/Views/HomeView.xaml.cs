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
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class HomeView : View
    {
        private long _updatInterval;
        private long _lastUpdate = 0;

        public HomeView()
        {
            _baseModels = NesterControl.BaseModels;

            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonAppReload,
                    ButtonAppRemove,
                    ButtonAppAdd,
                    ButtonAppJoin,
                    ButtonUser,
                    ButtonAuth,
                    ButtonPayment,
                    ButtonHistory,
                    AppModels
                });

            AppModels.SelectionChanged += AppModels_SelectionChangedAsync;

            ButtonAppJoin.Clicked += ButtonAppJoin_ClickedAsync;
            ButtonAppReload.Clicked += ButtonAppReload_ClickedAsync;
            ButtonAppAdd.Clicked += ButtonAppAdd_ClickedAsync;
            ButtonAppRemove.Clicked += ButtonAppRemove_ClickedAsync;

            ButtonUser.Clicked += ButtonUser_Clicked;
            ButtonAuth.Clicked += ButtonAuth_Clicked;
            ButtonPayment.Clicked += ButtonPayment_ClickedAsync;
            ButtonHistory.Clicked += ButtonHistory_ClickedAsync;

            _updatInterval = Settings.AppStatusRefreshInterval;

            BindingContext = _baseModels.AllApps;

            Monitor();
        }

        public void Monitor()
        {
            try
            {
                Cloud.ServerStatus status = new Cloud.ServerStatus(0);
                AppView.Status newState;
                bool viewLoadNeeded = true;

                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    if (_lastUpdate > _updatInterval)
                    {
                        Task.Factory.StartNew(async () =>
                        {
                            foreach (AppViewModel appModel in BaseModels.AllApps.AppModels)
                            {
                                if (!appModel.EditApp.IsBusy)
                                {
                                    continue;
                                }

                                await appModel.QueryAppAsync();
                                await appModel.DeploymentModel.QueryDeploymentsAsync();

                                viewLoadNeeded = false;

                                if (appModel.EditApp.IsBusy)
                                {
                                    newState = AppView.Status.Updating;
                                }
                                else if (!appModel.EditApp.IsDeployed)
                                {
                                    newState = AppView.Status.WaitingDeployment;
                                }
                                else
                                {
                                    newState = AppView.Status.Deployed;
                                }

                                System.Diagnostics.Debug.WriteLine("Polling App - {0}, currently Deployed - {1}, Busy - {2}, View - {3}",
                                    appModel.EditApp.Tag, appModel.EditApp.IsDeployed, appModel.EditApp.IsBusy, newState);

                                if (MainSideView.CurrentView != null && MainSideView.CurrentView is AppView && 
                                    MainSideView.CurrentView.BaseModels != null && 
                                    MainSideView.CurrentView.BaseModels.TargetViewModel.EditApp.Id == appModel.EditApp.Id)
                                {
                                    System.Diagnostics.Debug.WriteLine("Displayed App - {0}, currently Deployed - {1}, Busy - {2}, View - {3}",
                                        MainSideView.CurrentView.BaseModels.TargetViewModel.EditApp.Tag,
                                        MainSideView.CurrentView.BaseModels.TargetViewModel.EditApp.IsDeployed, 
                                        MainSideView.CurrentView.BaseModels.TargetViewModel.EditApp.IsBusy,
                                        (MainSideView.CurrentView as AppView).State);

                                    viewLoadNeeded = ((MainSideView.CurrentView as AppView).State != newState);
                                }

                                if (viewLoadNeeded)
                                {
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        NesterControl.ResetViewAsync(appModel);
                                    });
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

        private async void LoadSettingsPage(Type pageType)
        {
            IsServiceActive = true;

            try
            {
                _baseModels.WizardMode = false;

                AppViewModel appModel = AppModels.SelectedItem as AppViewModel;
                if (appModel != null)
                {
                    NesterControl.Target = appModel;
                }

                MainSideView.LoadView(
                    Activator.CreateInstance(pageType, new object[] { _baseModels }) as View);
                MainSideView.IsPresented = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private async void ButtonAppJoin_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                _baseModels.TargetViewModel.ContactModel.EditInvitation.User = NesterControl.User;

                await _baseModels.TargetViewModel.ContactModel.QueryInvitationsAsync();

                MainSideView.LoadView(
                   new AppJoinDetailView(_baseModels));
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

                    if (appModel.EditApp.IsActive)
                        MainSideView.Reload();
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
                    if (appModel.EditApp.IsDeploymentValid)
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
                            BaseModels.AllApps.RemoveApp(appModel);
                            await NesterControl.ResetViewAsync(BaseModels.AllApps.AppModels.FirstOrDefault());
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
        
        private void ButtonUser_Clicked(object sender, EventArgs e)
        {
            LoadSettingsPage(typeof(UserView));
        }

        private void ButtonAuth_Clicked(object sender, EventArgs e)
        {
            LoadSettingsPage(typeof(AuthView));
        }

        private async void ButtonPayment_ClickedAsync(object sender, EventArgs e)
        {
            await _baseModels.PaymentViewModel.QueryPaymentMethodAsync(false, false);

            LoadSettingsPage(typeof(PaymentView));
        }

        private void ButtonHistory_ClickedAsync(object sender, EventArgs e)
        {
            LoadSettingsPage(typeof(UserHistoryView));
        }

        private async void ButtonAppAdd_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                AppViewModel newAppModel = new AppViewModel();
                newAppModel.NewAppAsync();

                NesterControl.Target = newAppModel;
                BaseModels.WizardMode = true;

                MainSideView.LoadView(
                   new AppBasicDetailView(BaseModels));

                MainSideView.IsPresented = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private async void AppModels_SelectionChangedAsync(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            AppViewModel appModel = e.AddedItems.FirstOrDefault() as AppViewModel;
            if (appModel != null)
            {
                await NesterControl.ResetViewAsync(appModel);
            }
        }
    }
}
