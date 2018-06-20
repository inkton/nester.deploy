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
                .ServicesViewModel
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

            Supplier.SelectedIndexChanged += Supplier_SelectedIndexChanged;

            if (_isUpgrading)
            {
                if (baseModels.TargetViewModel.ServicesViewModel.SelectedAppService.Tier.Service.Tag == "nest-redbud")
                {
                    Supplier.SelectedIndex = 0;
                }
                else
                {
                    Supplier.SelectedIndex = 1;
                }

                Supplier.IsEnabled = false;
            }
            else
            {
                Supplier.SelectedIndex = 0;
            }
        }

        public override void UpdateBindings()
        {
            if (_baseModels.TargetViewModel.EditApp != null)
            {
                Title = _baseModels.TargetViewModel.EditApp.Name;
            }

            BindingContext = _baseModels;

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

        private void AppTypeTierView_Loaded(object sender, Syncfusion.ListView.XForms.ListViewLoadedEventArgs e)
        {
            if (!_isUpgrading)
            {
                AppTypeTierView.SelectedItems.Clear();

                if (_baseModels.TargetViewModel.ServicesViewModel.SelectedAppService != null)
                {
                    ServicesViewModel.ServiceTableItem serviceTableItem = 
                        (AppTypeTierView.ItemsSource as ObservableCollection<ServicesViewModel.ServiceTableItem>).Where(
                                         x => x.Tier.Id == _baseModels.TargetViewModel.ServicesViewModel.SelectedAppService.Tier.Id).First();
                    AppTypeTierView.SelectedItems.Add(serviceTableItem);
                }
            }
        }

        private void Supplier_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((Supplier.SelectedItem as string) == "AWS")
            {
                _baseModels.TargetViewModel.ServicesViewModel.SelectedAppserviceTag = "nest-redbud";
            }
            else
            {
                _baseModels.TargetViewModel.ServicesViewModel.SelectedAppserviceTag = "nest-oak";
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
                _baseModels.TargetViewModel.Validated = (
                     e.AddedItems.Any()
                    );

                if (_baseModels.TargetViewModel.Validated)
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
            if (_selectedAppRow == null)
            {
                return;
            }
            
            // At present only the wizard mode brings up this page.
            if (_isUpgrading)
            {
                _baseModels.TargetViewModel.ServicesViewModel.UpgradeAppServiceTier = _selectedAppRow.Tier;
                AppSummaryView summaryView = new AppSummaryView(_baseModels);
                summaryView.MainSideView = MainSideView;
                MainSideView.Detail.Navigation.InsertPageBefore(summaryView, this);
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
                    _baseModels.TargetViewModel.ServicesViewModel.SelectedAppService == null ||
                    _baseModels.TargetViewModel.ServicesViewModel.SelectedAppService.Tier.Id != _selectedAppRow.Tier.Id))
                {
                    _baseModels.TargetViewModel.ServicesViewModel.SwitchAppServiceTierAsync(_selectedAppRow.Tier);
                }

                if (_baseModels.TargetViewModel.ServicesViewModel.SelectedStorageService == null && MariaDBEnabled.IsToggled)
                {
                    _baseModels.TargetViewModel.ServicesViewModel.CreateDefaultStorageServiceAsync();
                }
                else if (_baseModels.TargetViewModel.ServicesViewModel.SelectedStorageService != null && !MariaDBEnabled.IsToggled)
                {
                    _baseModels.TargetViewModel.ServicesViewModel.RemoveDefaultStorageServiceAsync();
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

                if (_baseModels.WizardMode)
                {
                    await _baseModels.TargetViewModel.NestViewModel.InitAsync();

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
