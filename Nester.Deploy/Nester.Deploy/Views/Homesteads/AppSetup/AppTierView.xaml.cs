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
using System.Threading.Tasks;
using Inkton.Nester.Models;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class AppTierView : View
    {
        private ServicesViewModel.ServiceTableItem _selectedAppRow;
        private bool _isUpgrading;

        public AppTierView(BaseModels baseModels)
        {
            InitializeComponent();

            _isUpgrading = baseModels
                .TargetViewModel
                .DeploymentModel
                .UpgradableAppTiers.Any();

            BaseModels = baseModels;

            AppTypeTierView.SelectionChanged += AppTypeTierView_SelectionChanged;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {  
                    ButtonDone
                });

            ButtonDone.IsVisible = _baseModels.WizardMode || _isUpgrading;
            if (_baseModels.WizardMode || _isUpgrading)
            {
                // hide but do not collapse
                TopButtonPanel.Opacity = 0;
            }

            AppTypeTierView.Loaded += AppTypeTierView_Loaded;
        }

        public override void UpdateBindings()
        {
            if (_baseModels.TargetViewModel.EditApp != null)
            {
                Title = _baseModels.TargetViewModel.EditApp.Name;
            }

            if (_isUpgrading)
            {
                // The app is being created and deployed. Only selected services are applicable
                AppTierTitle.Text = "Select an App Tier to Upgrade";
                BindingContext = _baseModels.TargetViewModel
                    .DeploymentModel;
                DatabaseOption.Opacity = 0;
            }
            else
            {
                // The app is being created. All services are applicable
                AppTierTitle.Text = "Select an App Tier to Install";
                BindingContext = _baseModels.TargetViewModel;
            }
        }

        private void AppTypeTierView_Loaded(object sender, Syncfusion.ListView.XForms.ListViewLoadedEventArgs e)
        {
            if (!_isUpgrading)
            {
                AppTypeTierView.SelectedItems.Clear();

                if (_baseModels.TargetViewModel.SelectedAppService != null)
                {
                    ServicesViewModel.ServiceTableItem serviceTableItem = 
                        (AppTypeTierView.ItemsSource as ObservableCollection<ServicesViewModel.ServiceTableItem>).Where(
                                         x => x.Tier.Id == _baseModels.TargetViewModel.SelectedAppService.Tier.Id).First();
                    AppTypeTierView.SelectedItems.Add(serviceTableItem);
                }
            }
        }

        async private void OnButtonBasicDetailsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                BaseModels.WizardMode = false;
                await UpdateServicesAsync();
                MainSideView.CurrentLevelViewAsync(new AppBasicDetailView(BaseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnButtonDomainsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await UpdateServicesAsync();
                MainSideView.CurrentLevelViewAsync(new AppDomainView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnButtonNestsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await UpdateServicesAsync();
                MainSideView.CurrentLevelViewAsync(new AppNestsView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnButtonContactsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await UpdateServicesAsync();
                MainSideView.CurrentLevelViewAsync(new ContactsView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private void AppTypeTierView_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            if (_isUpgrading)
            {
                _baseModels.TargetViewModel.DeploymentModel.Validated = (
                     e.AddedItems.Any()
                    );

                if (_baseModels.TargetViewModel.DeploymentModel.Validated)
                {
                    _selectedAppRow = e.AddedItems.First() as ServicesViewModel.ServiceTableItem;
                }
            }
            else if (_baseModels.TargetViewModel != null)
            {
                _baseModels.TargetViewModel.Validated = (
                    e.AddedItems.Any()
                );

                if (_baseModels.TargetViewModel.Validated)
                {
                    _selectedAppRow = e.AddedItems.First() as ServicesViewModel.ServiceTableItem;
                }
            }
        }

        async Task UpdateServicesAsync()
        {
            // At present only the wizard mode brings up this page.
            if (_isUpgrading)
            {
                if (_selectedAppRow != null)
                {
                    _baseModels.TargetViewModel.DeploymentModel.SelectedAppService = _selectedAppRow;
                    _baseModels.TargetViewModel.DeploymentModel.MariaDBEnabled =
                            _baseModels.TargetViewModel.MariaDBEnabled;
                    AppSummaryView summaryView = new AppSummaryView(_baseModels);
                    summaryView.MainSideView = MainSideView;
                    MainSideView.Detail.Navigation.InsertPageBefore(summaryView, this);
                }
            }
            else
            {
                if (_baseModels.TargetViewModel.EditApp.Status != "assigned")
                {
                    await _baseModels.TargetViewModel.CreateAppAsync(_selectedAppRow.Tier);

                    NesterControl.Target = _baseModels.TargetViewModel;
                    NesterControl.BaseModels.AllApps.AddModel(_baseModels.TargetViewModel);
                }

                if (_selectedAppRow != null && (
                    _baseModels.TargetViewModel.SelectedAppService == null ||
                    _baseModels.TargetViewModel.SelectedAppService.Tier.Id != _selectedAppRow.Tier.Id))
                {
                    AppServiceSubscription subscription = _baseModels.TargetViewModel.EditApp.Subscriptions.FirstOrDefault(
                        x => x.ServiceTier.Service.Tag == "nest-oak");

                    if (subscription != null)
                    {
                        await _baseModels.TargetViewModel.ServicesViewModel.RemoveSubscription(subscription);
                    }

                    await _baseModels.TargetViewModel.ServicesViewModel.CreateSubscription(_selectedAppRow.Tier);
                    await _baseModels.TargetViewModel.ServicesViewModel.QueryAppSubscriptions();
                }
                else if (_baseModels.TargetViewModel.SelectedMariaDBService != null && !MariaDBEnabled.IsToggled)
                {
                    AppServiceSubscription subscription = _baseModels.TargetViewModel.EditApp.Subscriptions.FirstOrDefault(
                        x => x.ServiceTier.Service.Tag == "mariadb");

                    if (subscription != null)
                    {
                        await _baseModels.TargetViewModel.ServicesViewModel.RemoveSubscription(subscription);
                    }
                }

                if (_baseModels.TargetViewModel.SelectedMariaDBService == null && MariaDBEnabled.IsToggled)
                {
                    // Only one tier available at present
                    AppServiceTier mariaDBTier = _baseModels.TargetViewModel.MariaDBTiers.First();
                    await _baseModels.TargetViewModel.ServicesViewModel.CreateSubscription(mariaDBTier);
                }
                else if (_baseModels.TargetViewModel.SelectedMariaDBService != null && !MariaDBEnabled.IsToggled)
                {
                    AppServiceSubscription subscription = _baseModels.TargetViewModel.EditApp.Subscriptions.FirstOrDefault(
                        x => x.ServiceTier.Service.Tag == "mariadb");

                    if (subscription != null)
                    {
                        await _baseModels.TargetViewModel.ServicesViewModel.RemoveSubscription(subscription);
                    }
                }
            }
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await UpdateServicesAsync();

                if (_baseModels.WizardMode)
                {
                    await _baseModels.TargetViewModel.NestModel.InitAsync();

                    AppNestsView nestsView = new AppNestsView(_baseModels);
                    nestsView.MainSideView = MainSideView;
                    MainSideView.Detail.Navigation.InsertPageBefore(nestsView, this);

                    await MainSideView.Detail.Navigation.PopAsync();
                }
                else
                {
                    // Head back to homepage if the 
                    // page was called from here
                    MainSideView.UnstackViewAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
    }
}
