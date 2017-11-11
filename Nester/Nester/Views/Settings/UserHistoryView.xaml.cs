using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Nester.Views
{
    public partial class UserHistoryView : Nester.Views.View
    {
        public UserHistoryView(AuthViewModel authViewModel)
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                });

            ButtonAppMenu.Clicked += ButtonAppMenu_Clicked;

            _authViewModel = authViewModel;
            BindingContext = _authViewModel;
        }

        protected async override void OnAppearing()
        {
            BindingContext = _authViewModel;

            base.OnAppearing();

            await _authViewModel.QueryUserEventsAsync(ThisUI.User);
        }

        async private void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        private void ButtonAppMenu_Clicked(object sender, EventArgs e)
        {
            _masterDetailPage.IsPresented = true;
        }

        async private void OnCloseButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                LoadHomeView();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}



