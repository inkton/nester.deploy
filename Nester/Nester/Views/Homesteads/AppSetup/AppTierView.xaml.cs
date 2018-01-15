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
using Inkton.Nester.Models;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class AppTierView : View
    {
        public AppTierView(BaseModels baseModels)
        {
            _baseModels = baseModels;

            InitializeComponent();

            AppTypeTierView.SelectionChanged += AppTypeTierView_SelectionChanged;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonDone
                });

            BindingContext = _baseModels.TargetViewModel;
        }

        private void AppTypeTierView_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            Validate();
        }

        void Validate()
        {
            if (_baseModels.TargetViewModel != null)
            {
                _baseModels.TargetViewModel.Validated = (
                     AppTypeTierView.SelectedItem != null
                    );

                if (_baseModels.TargetViewModel.Validated)
                {
                    _baseModels.TargetViewModel.SelectedAppService =
                        AppTypeTierView.SelectedItem as ServicesViewModel.ServiceTableItem;
                }
            }
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                // At present only the wizard mode brings up this page.
                // the app cannot change app tier afterwards as 
                // there is no support in the backend.

                if (_baseModels.TargetViewModel.EditApp.Status != "assigned")
                {
                    await _baseModels.TargetViewModel.CreateAppAsync();

                    NesterControl.BaseModels.AllApps.AddModel(_baseModels.TargetViewModel);
                }

                if (_baseModels.TargetViewModel.SelectedMariaDBService == null && MariaDBEnabled.IsToggled)
                {
                    // Only one tier available at present
                    AppServiceTier mariaDBTier =_baseModels.TargetViewModel.MariaDBTiers.First();
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

                await _baseModels.TargetViewModel.NestModel.InitAsync();

                AppNestsView nestsView = new AppNestsView(_baseModels);
                nestsView.MainSideView = MainSideView;
                MainSideView.Detail.Navigation.InsertPageBefore(nestsView, this);
                await MainSideView.Detail.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
    }
}
