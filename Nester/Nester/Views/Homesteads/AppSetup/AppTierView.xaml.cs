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

namespace Nester.Views
{
    public partial class AppTierView : Nester.Views.View
    {
        public AppTierView(AppViewModel appViewModel)
        {
            InitializeComponent();

            AppTypeTierView.SelectionChanged += AppTypeTierView_SelectionChanged;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonDone
                });

            _appViewModel = appViewModel;
            BindingContext = _appViewModel;
        }

        private void AppTypeTierView_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            Validate();
        }

        private void AppTypeTierView_Loaded(object sender, Syncfusion.ListView.XForms.ListViewLoadedEventArgs e)
        {
            AppTypeTierView.SelectedItem = _appViewModel.SelectedAppServiceTier;
        }

        void Validate()
        {
            if (_appViewModel != null)
            {
                _appViewModel.Validated = (
                     AppTypeTierView.SelectedItem != null
                    );

                if (_appViewModel.Validated)
                {
                    _appViewModel.SelectedAppServiceTier = 
                        AppTypeTierView.SelectedItem as Admin.AppServiceTier;
                }
            }
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                if (_appViewModel.WizardMode)
                {
                    if (_appViewModel.EditApp.Status != "assigned")
                    {
                        await _appViewModel.CreateAppAsync();

                        ThisUI.AppCollectionViewModel.AddModel(_appViewModel);
                    }

                    await _appViewModel.NestModel.InitAsync();
                    Navigation.InsertPageBefore(new AppNestsView(_appViewModel), this);
                }

                if (_appViewModel.SelectedMariaDBTier == null && MariaDBEnabled.IsToggled)
                {
                    // Only one tier available at present
                    Admin.AppServiceTier mariaDBTier =_appViewModel.MariaDBTiers.First();
                    await _appViewModel.ServicesViewModel.CreateSubscription(mariaDBTier);
                }
                else if (_appViewModel.SelectedMariaDBTier != null && !MariaDBEnabled.IsToggled)
                {
                    Admin.AppServiceSubscription subscription = _appViewModel.EditApp.Subscriptions.FirstOrDefault(
                        x => x.ServiceTier.Service.Tag == "mariadb");

                    if (subscription != null)
                    {
                        await _appViewModel.ServicesViewModel.RemoveSubscription(subscription);
                    }
                }
                
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
    }
}
