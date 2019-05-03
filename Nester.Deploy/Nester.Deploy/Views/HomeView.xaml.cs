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
using Nester.Deploy;
using Inkton.Nester.ViewModels;
using DeployApp = Nester.Deploy.App;

namespace Inkton.Nester.Views
{
    public partial class HomeView : View
    {
        private long _updatInterval;
        private long _lastUpdate = 0;

        public HomeView()
        {
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

            AppModels.ItemSelected += AppModels_ItemSelected;

            ButtonAppJoin.Clicked += ButtonAppJoin_ClickedAsync;
            ButtonAppReload.Clicked += ButtonAppReload_ClickedAsync;
            ButtonAppAdd.Clicked += ButtonAppAdd_ClickedAsync;
            ButtonAppRemove.Clicked += ButtonAppRemove_ClickedAsync;

            ButtonUser.Clicked += ButtonUser_Clicked;
            ButtonAuth.Clicked += ButtonAuth_Clicked;
            ButtonPayment.Clicked += ButtonPayment_ClickedAsync;
            ButtonHistory.Clicked += ButtonHistory_ClickedAsync;

            _updatInterval = Settings.AppStatusRefreshInterval;

            BindingContext = _baseViewModels.AppCollectionViewModel;

            Monitor();
        }

        public void Monitor()
        {
            try
            {
                const int StatusUpdateIntervalSec = 5;

                Device.StartTimer(TimeSpan.FromSeconds(StatusUpdateIntervalSec), () =>
                {
                    if (_lastUpdate > _updatInterval)
                    {
                        Task.Run(async () =>
                        {
                            foreach (AppViewModel appModel in BaseViewModels.AppCollectionViewModel.AppModels)
                            {
                                if (!appModel.EditApp.IsBusy)
                                {
                                    continue;
                                }

                                try
                                {
                                    await appModel.QueryStatusAsync();

                                    System.Diagnostics.Debug.WriteLine("Polling App - {0}, currently Deployed - {1}, Busy - {2}",
                                        appModel.EditApp.Tag, appModel.EditApp.IsDeployed, appModel.EditApp.IsBusy);

                                    if (!appModel.EditApp.IsBusy)
                                    {
                                        await appModel.ReloadAsync();
                                    }
                                }
                                catch(Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(
                                        "Exception raised in Monitor - {0}", ex.ToString());
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
                _baseViewModels.WizardMode = false;

                MainSideView.StackViewAsync(
                    Activator.CreateInstance(pageType, new object[] { _baseViewModels }) as View);
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
                ContactViewModel contactsModel = new ContactViewModel(_baseViewModels.Platform, null);
                contactsModel.EditInvitation.OwnedBy = BaseViewModels.Platform.Permit.Owner;
                await contactsModel.QueryInvitationsAsync();

                MainSideView.StackViewAsync(
                   new AppJoinDetailView(contactsModel));
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
                await MainSideView.ReloadAsync();
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

                            BaseViewModels.AppCollectionViewModel.RemoveApp(appModel);

                            ((DeployApp)Application.Current).RefreshView();

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
            await _baseViewModels.PaymentViewModel.QueryPaymentMethodAsync(false, false);
            await _baseViewModels.PaymentViewModel.QueryBillingCyclesAsync();

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
                AppViewModel = new AppViewModel(_baseViewModels.Platform);
                _baseViewModels.WizardMode = true;

                MainSideView.StackViewAsync(
                   new AppBasicDetailView(AppViewModel));

                MainSideView.IsPresented = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private void AppModels_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            AppViewModel appModel = e.SelectedItem as AppViewModel;
            if (appModel != null)
            {
                ((DeployApp)Application.Current).RefreshView(appModel);
            }
        }
    }
}
