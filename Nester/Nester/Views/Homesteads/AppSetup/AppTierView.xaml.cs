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
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Inkton.Nester.Views
{
    public partial class AppTierView : Inkton.Nester.Views.View
    {
        public AppTierView(Views.BaseModels baseModels)
        {
            _baseModels = baseModels;

            InitializeComponent();

            AppTypeTierView.SelectionChanged += AppTypeTierView_SelectionChanged;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonDone
                });

            BindingContext = _baseModels.AppViewModel;
        }

        private void AppTypeTierView_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            Validate();
        }

        private void AppTypeTierView_Loaded(object sender, Syncfusion.ListView.XForms.ListViewLoadedEventArgs e)
        {
            AppTypeTierView.SelectedItem = _baseModels.AppViewModel.SelectedAppServiceTier;
        }

        void Validate()
        {
            if (_baseModels.AppViewModel != null)
            {
                _baseModels.AppViewModel.Validated = (
                     AppTypeTierView.SelectedItem != null
                    );

                if (_baseModels.AppViewModel.Validated)
                {
                    _baseModels.AppViewModel.SelectedAppServiceTier = 
                        AppTypeTierView.SelectedItem as Admin.AppServiceTier;
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

                if (_baseModels.AppViewModel.EditApp.Status != "assigned")
                {
                    await _baseModels.AppViewModel.CreateAppAsync();

                    (NesterControl.BaseModels.AppViewModel as AppCollectionViewModel).AddModel(_baseModels.AppViewModel);
                }

                if (_baseModels.AppViewModel.SelectedMariaDBTier == null && MariaDBEnabled.IsToggled)
                {
                    // Only one tier available at present
                    Admin.AppServiceTier mariaDBTier =_baseModels.AppViewModel.MariaDBTiers.First();
                    await _baseModels.AppViewModel.ServicesViewModel.CreateSubscription(mariaDBTier);
                }
                else if (_baseModels.AppViewModel.SelectedMariaDBTier != null && !MariaDBEnabled.IsToggled)
                {
                    Admin.AppServiceSubscription subscription = _baseModels.AppViewModel.EditApp.Subscriptions.FirstOrDefault(
                        x => x.ServiceTier.Service.Tag == "mariadb");

                    if (subscription != null)
                    {
                        await _baseModels.AppViewModel.ServicesViewModel.RemoveSubscription(subscription);
                    }
                }

                await _baseModels.AppViewModel.NestModel.InitAsync();

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
