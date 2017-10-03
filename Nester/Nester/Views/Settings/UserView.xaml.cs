using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Nester.Views
{
    public partial class UserView : Nester.Views.View
    {
        public UserView(AuthViewModel authViewModel)
        {
            InitializeComponent();

            int selectedTerritoryIndex = -1;

            foreach (Admin.Geography.ISO3166Country territory in Admin.Geography.Territories)
            {
                Territories.Items.Add(territory.ToString());

                if (System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName == territory.Alpha2)
                {
                    selectedTerritoryIndex = Territories.Items.Count - 1;
                }
            }

            if (selectedTerritoryIndex < 0)
            {
                // random country
                Territories.SelectedIndex = 8;
            }

            Territories.SelectedIndex = selectedTerritoryIndex;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonDone
                });

            ButtonAppMenu.Clicked += ButtonAppMenu_Clicked;
            NickName.Unfocused += NickName_Unfocused;

            _authViewModel = authViewModel;
            BindingContext = _authViewModel;
        }

        private void NickName_Unfocused(object sender, FocusEventArgs e)
        {
/*            string nickname = (sender as Xamarin.Forms.Entry).Text;
            if (nickname != null && nickname.Length > 0)
            {
                Admin.User searchUser = new Admin.User();
                searchUser.Nickname = nickname;

                Cloud.ServerStatus status = await _appViewModel.QueryAppAsync(
                    searchApp, true, false);
                if (status.Code == Cloud.Result.NEST_RESULT_SUCCESS)
                {
                    NicknameValidator.IsValid = false;
                    NicknameValidator.Message = "The nickname is taken, try another nickname";
                }
            }*/
        }

        void Validate()
        {
            if (NicknameValidator != null)
            {
                _authViewModel.Validated = (
                     NicknameValidator.IsValid &&
                     FirstNameValidator.IsValid &&
                     LastNameValidator.IsValid
                     );

                SecurityCode.IsVisible = _authViewModel.WizardMode;
                SecurityCodeLabel.IsVisible = _authViewModel.WizardMode;
            }
        }

        void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                IsServiceActive = true;

                string territoryName = Territories.Items.ElementAt(Territories.SelectedIndex);

                foreach (Admin.Geography.ISO3166Country territory in Admin.Geography.Territories)
                {
                    if (territoryName == territory.ToString())
                    {
                        ThisUI.User.TerritoryISOCode = territory.Alpha2;
                        break;
                    }
                }

                IsServiceActive = false;

                if (_authViewModel.WizardMode)
                {
                    Cloud.ServerStatus status = await _authViewModel.SignupAsync(false);
                    if (status.Code == Cloud.Result.NEST_RESULT_ERROR_AUTH_SECCODE)
                    {
                        await DisplayAlert("Nester", "Invalid security code", "OK");
                    }
                    else if (status.Code == Cloud.Result.NEST_RESULT_ERROR)
                    {
                        await DisplayAlert("Nester", status.Notes, "OK");
                    }
                    else
                    {
                        await _authViewModel.QueryTokenAsync();

                        await Navigation.PopModalAsync();
                    }
                }
                else
                {
                    await _authViewModel.UpdateUserAsync(ThisUI.User);
                    await DisplayAlert("Nester", "Your information was saved", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
                IsServiceActive = false;
            }
        }

        private void ButtonAppMenu_Clicked(object sender, EventArgs e)
        {
            _masterDetailPage.IsPresented = true;
        }
    }
}
