using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Nester.Views
{
    public partial class EntryView : Nester.Views.View
    {
        public EntryView()
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonLogin,
                    ButtonSignup,
                    ButtonRecoverPassword
                });

            // the app and auth models are the main
            // models. create and init here. it is
            // then kept in homeview for distribution
            // when appropriate. the two are properties
            // of the view base class.

            Views.MainSideView homeView = ThisUI.HomeView;

            _authViewModel = homeView.AuthViewModel;
            _appViewModel = homeView.AppViewModel;

            BindingContext = _authViewModel;
        }

        void Validate()
        {
            if (_authViewModel != null)
            {
                _authViewModel.Validated = (
                    EmailValidator.IsValid &&
                    PasswordValidator.IsValid);

                _authViewModel.CanRecoverPassword = (
                    _authViewModel.Permit.Owner.Email != null &&
                    _authViewModel.Permit.Owner.Email.Length > 0 &&
                    EmailValidator.IsValid);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Validate();
        }

        private void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }

        async private void OnLoginButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                _authViewModel.Reset();
                ThisUI.AppCollectionViewModel.AppModels.Clear();

                Cloud.ServerStatus status = await _authViewModel.QueryTokenAsync(false);

                if (status.Code == Cloud.Result.NEST_RESULT_ERROR_AUTH_SECCODE)
                {
                    await Navigation.PushAsync(ThisUI.HomeView);

                    // the user can be hanging in inactive state
                    // if he/she did not confirm the security code
                    // in the second stage after registration.
                    // this result suggests the credentials were
                    // sound but need to confirm the security code.
                    // a new sec code would have been sent too.

                    AppViewModel newAppModel = new AppViewModel();
                    newAppModel.NewAppAsync();
                    newAppModel.WizardMode = true;

                    await Navigation.PushAsync(
                        new AppEngageView(newAppModel));

                    Views.UserView userView = new Views.UserView();
                    userView.SetModels(_authViewModel, _appViewModel);

                    await Navigation.PushModalAsync(userView);
                }
                else if (status.Code == Cloud.Result.NEST_RESULT_SUCCESS)
                {
                    await Navigation.PushAsync(ThisUI.HomeView);

                    await ThisUI.AppCollectionViewModel.LoadApps();

                    if (!ThisUI.AppCollectionViewModel.AppModels.Any())
                    {
                        AppViewModel newAppModel = new AppViewModel();
                        newAppModel.WizardMode = true;
                        newAppModel.NewAppAsync();

                        await Navigation.PushAsync(
                          new AppEngageView(newAppModel));
                    }
                }
                else
                {
                    Cloud.Result.ThrowError(status);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async void OnSignUpButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                _authViewModel.Reset();
                ThisUI.AppCollectionViewModel.AppModels.Clear();

                await _authViewModel.SignupAsync();

                await Navigation.PushAsync(ThisUI.HomeView);

                AppViewModel newAppModel = new AppViewModel();
                newAppModel.NewAppAsync();
                newAppModel.WizardMode = true;
 
                await Navigation.PushAsync(
                    new AppEngageView(newAppModel));

                Views.UserView userView = new Views.UserView();
                _authViewModel.WizardMode = true;
                userView.SetModels(_authViewModel, _appViewModel);

                await Navigation.PushModalAsync(userView);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
                IsServiceActive = false;
            }

            IsServiceActive = false;
        }

        async void OnRecoverPasswordButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                Cloud.ServerStatus status = 
                    await _authViewModel.QueryTokenAsync(false);

                if (status.Code < 0)
                {
                    status = await _authViewModel.RecoverPasswordAsync(false);

                    if (status.Code == Cloud.Result.NEST_RESULT_ERROR_USER_NFOUND)
                    {
                        await DisplayAlert("Nester", "The user not found", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Nester", "The reset password has been emailed", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Nester", "The password is correct, recovery not needed", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
                IsServiceActive = false;
            }

            IsServiceActive = false;
        }
    }
}
