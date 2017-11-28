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
        public AppTierView(Views.AppModelPair modelPair)
        {
            _modelPair = modelPair;

            InitializeComponent();

            AppTypeTierView.SelectionChanged += AppTypeTierView_SelectionChanged;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonDone
                });

            BindingContext = _modelPair.AppViewModel;
        }

        private void AppTypeTierView_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            Validate();
        }

        private void AppTypeTierView_Loaded(object sender, Syncfusion.ListView.XForms.ListViewLoadedEventArgs e)
        {
            AppTypeTierView.SelectedItem = _modelPair.AppViewModel.SelectedAppServiceTier;
        }

        void Validate()
        {
            if (_modelPair.AppViewModel != null)
            {
                _modelPair.AppViewModel.Validated = (
                     AppTypeTierView.SelectedItem != null
                    );

                if (_modelPair.AppViewModel.Validated)
                {
                    _modelPair.AppViewModel.SelectedAppServiceTier = 
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

                if (_modelPair.AppViewModel.EditApp.Status != "assigned")
                {
                    await _modelPair.AppViewModel.CreateAppAsync();

                    (NesterControl.AppModelPair.AppViewModel as AppCollectionViewModel).AddModel(_modelPair.AppViewModel);
                }

                if (_modelPair.AppViewModel.SelectedMariaDBTier == null && MariaDBEnabled.IsToggled)
                {
                    // Only one tier available at present
                    Admin.AppServiceTier mariaDBTier =_modelPair.AppViewModel.MariaDBTiers.First();
                    await _modelPair.AppViewModel.ServicesViewModel.CreateSubscription(mariaDBTier);
                }
                else if (_modelPair.AppViewModel.SelectedMariaDBTier != null && !MariaDBEnabled.IsToggled)
                {
                    Admin.AppServiceSubscription subscription = _modelPair.AppViewModel.EditApp.Subscriptions.FirstOrDefault(
                        x => x.ServiceTier.Service.Tag == "mariadb");

                    if (subscription != null)
                    {
                        await _modelPair.AppViewModel.ServicesViewModel.RemoveSubscription(subscription);
                    }
                }

                await _modelPair.AppViewModel.NestModel.InitAsync();

                AppNestsView nestsView = new AppNestsView(_modelPair);
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
