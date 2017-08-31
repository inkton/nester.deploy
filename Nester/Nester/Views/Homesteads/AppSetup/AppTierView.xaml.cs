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
                        await _appViewModel.CreateAppAsync(_appViewModel.EditApp);

                        (_appViewModel as AppCollectionViewModel).AddApp(
                            _appViewModel.EditApp);
                    }

                    await _appViewModel.NestModel.InitAsync();
                    Navigation.InsertPageBefore(new AppNestsView(_appViewModel), this);
                }

                if (_appViewModel.SelectedMariaDBTier == null && _appViewModel.MariaDBEnabled)
                {
                    await _appViewModel.ServicesViewModel.CreateSubscription(_appViewModel.SelectedMariaDBTier);
                }
                else if (_appViewModel.SelectedMariaDBTier != null && !_appViewModel.MariaDBEnabled)
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
