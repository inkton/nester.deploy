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
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Inkton.Nester.Views
{
    public partial class EntryView : Inkton.Nester.Views.View
    {
        public EntryView()
        {
            _modelPair = NesterControl.AppModelPair;

            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonLogin,
                    ButtonSignup,
                    ButtonRecoverPassword
                });

            BindingContext = _modelPair.AuthViewModel;
        }

        void Validate()
        {
            if (EmailValidator != null)
            {
                _modelPair.AuthViewModel.Validated = (
                    EmailValidator.IsValid &&
                    PasswordValidator.IsValid);

                _modelPair.AuthViewModel.CanRecoverPassword = (
                    _modelPair.AuthViewModel.Permit.Owner.Email != null &&
                    _modelPair.AuthViewModel.Permit.Owner.Email.Length > 0 &&
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

        private void BeginNewSession()
        {
            _modelPair.AuthViewModel.Reset();
            AppCollectionViewModel appCollection = _modelPair.AppViewModel as AppCollectionViewModel;
            appCollection.AppModels.Clear();
            _modelPair.WizardMode = true;
        }

        async private void PushEngageViewAsync()
        {
            AppViewModel newAppModel = new AppViewModel();
            newAppModel.NewAppAsync();

            AppModelPair modelPair = new AppModelPair(
                _modelPair.AuthViewModel, newAppModel);
            modelPair.WizardMode = true;

            AppEngageView engageView = new AppEngageView(modelPair);
            engageView.MainSideView = MainSideView;

            await MainSideView.Detail.Navigation.PushAsync(engageView);
        }

        async private void PushEngageViewWithUserUpdateAsync()
        {
            PushEngageViewAsync();

            Views.UserView userView = new Views.UserView(_modelPair);
            userView.MainSideView = MainSideView;
            await MainSideView.Detail.Navigation.PushModalAsync(userView);
        }

        async private void OnLoginButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                BeginNewSession();

                Cloud.ServerStatus status = await _modelPair.AuthViewModel.QueryTokenAsync(false);

                if (status.Code == Cloud.Result.NEST_RESULT_ERROR_AUTH_SECCODE)
                {
                    // the user can be hanging in inactive state
                    // if he/she did not confirm the security code
                    // in the second stage after registration.
                    // this result suggests the credentials were
                    // sound but need to confirm the security code.
                    // a new sec code would have been sent too.

                    PushEngageViewWithUserUpdateAsync();
                }
                else if (status.Code == Cloud.Result.NEST_RESULT_SUCCESS)
                {
                    AppCollectionViewModel appCollection = _modelPair.AppViewModel as AppCollectionViewModel;

                    await appCollection.LoadApps();

                    if (!appCollection.AppModels.Any())
                    {
                        PushEngageViewAsync();
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
                BeginNewSession();

                await _modelPair.AuthViewModel.SignupAsync();

                PushEngageViewWithUserUpdateAsync();
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
                    await _modelPair.AuthViewModel.QueryTokenAsync(false);

                if (status.Code < 0)
                {
                    status = await _modelPair.AuthViewModel.RecoverPasswordAsync(false);

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
