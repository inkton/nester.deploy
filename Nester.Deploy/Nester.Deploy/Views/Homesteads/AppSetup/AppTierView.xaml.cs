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
using Inkton.Nest.Model;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class AppTierView : View
    {
        private ServicesViewModel.ServiceTableItem _selectedAppRow;
        private bool _isUpgrading;

        public AppTierView(BaseViewModels baseModels)
        {
            InitializeComponent();

            _isUpgrading = baseModels
                .AppViewModel
                .ServicesViewModel
                .UpgradableAppTiers.Any();

            baseModels
                .AppViewModel
                .ServicesViewModel
                .ResetAppServiceTable();

            ViewModels = baseModels;

            AppTypeTierView.SelectionChanged += AppTypeTierView_SelectionChanged;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonHome,
                    ButtonBasicDetails,
                    ButtonNests,
                    ButtonDomains,
                    ButtonContacts,
                    ButtonDone
                });

            if (_baseViewModels.WizardMode || _isUpgrading)
            {
                // hide but do not collapse
                TopButtonPanel.Opacity = 0;
            }

            AppTypeTierView.Loaded += AppTypeTierView_Loaded;

            if (baseModels.AppViewModel
                .ServicesViewModel
                .SelectedAppServiceTableItem != null)
            {
                if ((baseModels.AppViewModel
                    .ServicesViewModel
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
        }

        public override void UpdateBindings()
        {
            if (App != null)
            {
                Title = App.Name;
            }

            BindingContext = _baseViewModels;

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
        }

        private void ResetSelections()
        {
            AppTypeTierView.SelectedItems.Clear();

            ServicesViewModel.ServiceTableItem serviceTableItem = _baseViewModels
                .AppViewModel.ServicesViewModel.SelectedAppServiceTableItem;

            if (serviceTableItem != null)
            {
                AppTypeTierView.SelectedItems.Add(serviceTableItem);
                _selectedAppRow = serviceTableItem;

                MariaDBEnabled.IsToggled = (_baseViewModels.AppViewModel.ServicesViewModel.SelectedStorageServiceTableItem != null);

                SetMariaDBSupport();
            }
        }

        private void AppTypeTierView_Loaded(object sender, Syncfusion.ListView.XForms.ListViewLoadedEventArgs e)
        {
            if (!_isUpgrading)
            {
                ResetSelections();
            }
        }

        private void Supplier_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((Supplier.SelectedItem as string) == "AWS")
            {
                _baseViewModels.AppViewModel
                    .ServicesViewModel.BuildAppServiceTable("nest-redbud");
            }
            else
            {
                _baseViewModels.AppViewModel
                    .ServicesViewModel.BuildAppServiceTable("nest-oak");
            }

            ResetSelections();
        }

        async private void OnButtonBasicDetailsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                ViewModels.WizardMode = false;
                await UpdateServicesAsync();
                MainSideView.CurrentLevelViewAsync(new AppBasicDetailView(ViewModels));
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
                MainSideView.CurrentLevelViewAsync(new AppDomainView(_baseViewModels));
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
                MainSideView.CurrentLevelViewAsync(new AppNestsView(_baseViewModels));
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
                MainSideView.CurrentLevelViewAsync(new ContactsView(_baseViewModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private void AppTypeTierView_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            _baseViewModels.AppViewModel.Validated = (
                 e.AddedItems.Any()
                );

            if (_baseViewModels.AppViewModel.Validated)
            {
                _selectedAppRow = e.AddedItems.First() as ServicesViewModel.ServiceTableItem;
            }

            SetMariaDBSupport();
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
                _baseViewModels.AppViewModel.ServicesViewModel.UpgradeAppServiceTier(_selectedAppRow.Tier);

                AppSummaryView summaryView = new AppSummaryView(_baseViewModels);
                summaryView.MainSideView = MainSideView;
                MainSideView.Detail.Navigation.InsertPageBefore(summaryView, this);
            }
            else
            {
                if (App.Status != "assigned")
                {
                    await _baseViewModels.AppViewModel.CreateAppAsync(_selectedAppRow.Tier);
                    ViewModels.AppCollectionViewModel.AddModel(_baseViewModels.AppViewModel);
                }

                if (_selectedAppRow != null && (
                    _baseViewModels.AppViewModel.ServicesViewModel.SelectedAppServiceTableItem == null ||
                    _baseViewModels.AppViewModel.ServicesViewModel.SelectedAppServiceTableItem.Tier.Id != _selectedAppRow.Tier.Id))
                {
                    await _baseViewModels.AppViewModel.ServicesViewModel.SwitchAppServiceTierAsync(_selectedAppRow.Tier);
                }

                if (_baseViewModels.AppViewModel.ServicesViewModel.SelectedStorageServiceTableItem == null && MariaDBEnabled.IsToggled)
                {
                    await _baseViewModels.AppViewModel.ServicesViewModel.CreateDefaultStorageServiceAsync();
                }
                else if (_baseViewModels.AppViewModel.ServicesViewModel.SelectedStorageServiceTableItem != null && !MariaDBEnabled.IsToggled)
                {
                    await _baseViewModels.AppViewModel.ServicesViewModel.RemoveDefaultStorageServiceAsync();
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
                    await DisplayAlert("Nester", "Select an App Tier first", "OK");
                    return;
                }

                await UpdateServicesAsync();

                if (_baseViewModels.WizardMode)
                {
                    await _baseViewModels.AppViewModel.NestViewModel.InitAsync();

                    AppNestsView nestsView = new AppNestsView(_baseViewModels);
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
