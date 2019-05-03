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
using Xamarin.Forms;
using Inkton.Nest.Model;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Helpers;

namespace Inkton.Nester.Views
{
    public partial class AppTierView : View
    {
        private ServicesViewModel.ServiceTableItem _selectedAppRow;
        private bool _isUpgrading;

        public AppTierView(AppViewModel appViewModel)
        {
            InitializeComponent();

            AppViewModel = appViewModel;

            _isUpgrading = AppViewModel
                .ServicesViewModel
                .UpgradableAppTiers.Any();

            AppViewModel
                .ServicesViewModel
                .ResetAppServiceTable();

            AppTypeTierView.ItemSelected += AppTypeTierView_ItemSelected;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonHome,
                    ButtonBasicDetails,
                    ButtonNests,
                    ButtonDomains,
                    ButtonContacts,
                    ButtonDone
                });
        }

        public override void UpdateBindings()
        {
            AppTypeTierView.ItemsSource = AppViewModel.ServicesViewModel.AppTierTable;

            if (AppViewModel.EditApp != null)
            {
                Title = AppViewModel.EditApp.Name;
            }

            if (_isUpgrading)
            {
                // The app is being created and deployed. Only selected services are applicable
                AppTierTitle.Text = "Select an App Tier to Upgrade";
                DatabaseOption.Opacity = 0;
            }
            else
            {
                // The app is being created. All services are applicable
                AppTierTitle.Text = "Select an App Tier to Install";
            }

            if (BaseViewModels.Platform.Permit.Owner.TerritoryISOCode == "AU")
            {
                PaymentNotice.Text = "The prices are in US Dollars and do not include GST.";
            }
            else
            {
                PaymentNotice.Text = "The prices are in US Dollars. ";
            }

            if (BaseViewModels.WizardMode || _isUpgrading)
            {
                // hide but do not collapse
                TopButtonPanel.Opacity = 0;
            }

            if (AppViewModel.ServicesViewModel
                .SelectedAppServiceTableItem != null)
            {
                if ((AppViewModel.ServicesViewModel
                    .SelectedAppServiceTableItem.Tier.OwnedBy as AppService).Tag == "nest-redbud")
                {
                    Supplier.SelectedIndex = 0;
                }
                else
                {
                    Supplier.SelectedIndex = 1;
                }
            }
            else
            {
                Supplier.SelectedIndex = 0;
            }

            Supplier.IsEnabled = !_isUpgrading;

            if (Supplier.IsEnabled)
            {
                Supplier.SelectedIndexChanged += Supplier_SelectedIndexChanged;
            }

            if (!_isUpgrading)
            {
                ResetSelections();
            }
        }

        private void ResetSelections()
        {
            ServicesViewModel.ServiceTableItem serviceTableItem = AppViewModel
                .ServicesViewModel.SelectedAppServiceTableItem;

            if (serviceTableItem != null)
            {
                AppTypeTierView.SelectedItem = serviceTableItem;
                _selectedAppRow = serviceTableItem;

                MariaDBEnabled.IsToggled = (AppViewModel
                    .ServicesViewModel.SelectedStorageServiceTableItem != null);

                SetMariaDBSupport();
            }
        }

        private void AppTypeTierView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            AppViewModel.Validated = e.SelectedItem != null;
            
            if (AppViewModel.Validated)
            {
                _selectedAppRow = e.SelectedItem as ServicesViewModel.ServiceTableItem;
            }

            SetMariaDBSupport();
        }

        private void Supplier_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((Supplier.SelectedItem as string) == "AWS")
            {
                AppViewModel
                    .ServicesViewModel.BuildAppServiceTable("nest-redbud");
            }
            else
            {
                AppViewModel
                    .ServicesViewModel.BuildAppServiceTable("nest-oak");
            }

            ResetSelections();
        }

        async private void OnButtonBasicDetailsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                BaseViewModels.WizardMode = false;
                await UpdateServicesAsync();
                MainSideView.CurrentLevelViewAsync(new AppBasicDetailView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnButtonDomainsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await UpdateServicesAsync();
                MainSideView.CurrentLevelViewAsync(new AppDomainView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnButtonNestsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await UpdateServicesAsync();
                MainSideView.CurrentLevelViewAsync(new AppNestsView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnButtonContactsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await UpdateServicesAsync();
                MainSideView.CurrentLevelViewAsync(new ContactsView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        private void SetMariaDBSupport()
        {
            const int FEATURE_MEMORY = 3;

            // MariaDB on instances less than 1024 MB not supported.
            bool isMariaDBSupported = int.Parse(_selectedAppRow.FeaturesIncluded[FEATURE_MEMORY]) > 1024;

            if (!isMariaDBSupported)
            {
                MariaDBEnabled.IsToggled = false;
            }

            MariaDBEnabled.IsEnabled = isMariaDBSupported;
        }

        async Task UpdateServicesAsync()
        {
            if (_selectedAppRow == null)
            {
                return;
            }
            
            // At present only the wizard mode brings up this page.
            if (_isUpgrading)
            {
                AppViewModel.ServicesViewModel.UpgradeAppServiceTier(_selectedAppRow.Tier);

                AppSummaryView summaryView = new AppSummaryView(AppViewModel);
                summaryView.MainSideView = MainSideView;
                MainSideView.Detail.Navigation.InsertPageBefore(summaryView, this);
            }
            else
            {
                if (AppViewModel.EditApp.Status != "assigned")
                {
                    await AppViewModel.CreateAppAsync(_selectedAppRow.Tier);
                    BaseViewModels.AppCollectionViewModel.AddModel(AppViewModel);
                }

                if (_selectedAppRow != null && (
                    AppViewModel.ServicesViewModel.SelectedAppServiceTableItem == null ||
                    AppViewModel.ServicesViewModel.SelectedAppServiceTableItem.Tier.Id != _selectedAppRow.Tier.Id))
                {
                    await AppViewModel.ServicesViewModel.SwitchAppServiceTierAsync(_selectedAppRow.Tier);
                }

                if (AppViewModel.ServicesViewModel.SelectedStorageServiceTableItem == null && MariaDBEnabled.IsToggled)
                {
                    await AppViewModel.ServicesViewModel.CreateDefaultStorageServiceAsync();
                }
                else if (AppViewModel.ServicesViewModel.SelectedStorageServiceTableItem != null && !MariaDBEnabled.IsToggled)
                {
                    await AppViewModel.ServicesViewModel.RemoveDefaultStorageServiceAsync();
                }
            }
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                if (_selectedAppRow == null)
                {
                    await ErrorHandler.ExceptionAsync(this, "Select an App Tier first");
                    return;
                }

                await UpdateServicesAsync();

                if (BaseViewModels.WizardMode)
                {
                    await AppViewModel.NestViewModel.InitAsync();

                    AppNestsView nestsView = new AppNestsView(AppViewModel);
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
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }
    }
}
