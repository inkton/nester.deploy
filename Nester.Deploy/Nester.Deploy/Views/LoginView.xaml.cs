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
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Xamarin.Forms;
using Inkton.Nest.Cloud;
using Inkton.Nest.Model;
using Inkton.Nester.Cloud;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Helpers;
using DeployApp = Nester.Deploy.App;

namespace Inkton.Nester.Views
{
    public partial class LoginView : View
    {
        public LoginView()
        {
            BindingContext = BaseViewModels.AuthViewModel;

            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    Email,
                    Password,
                    ButtonLogin,
                    ButtonSignoff,
                    ButtonSignup,
                    ButtonRecoverPassword,
                    ButtonAbout
                });
        }
            
        void Validate()
        {
            if (EmailValidator != null)
            {
                BaseViewModels.AuthViewModel.Validated = (
                    EmailValidator.IsValid &&
                    PasswordValidator.IsValid);

                BaseViewModels.AuthViewModel.CanRecoverPassword = (
                    BaseViewModels.AuthViewModel.Platform.Permit.Owner.Email != null &&
                    BaseViewModels.AuthViewModel.Platform.Permit.Owner.Email.Length > 0 &&
                    EmailValidator.IsValid);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _wizardMode = true;

            BeginNewSession();

            Validate();
        }

        private void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }

        private void BeginNewSession()
        {
            BaseViewModels.AuthViewModel.Platform.Permit.Invalid();
            BaseViewModels.AppCollectionViewModel.AppModels.Clear();

            BaseViewModels.AuthViewModel.Platform.Permit.Password = Password.Text;
            BaseViewModels.AuthViewModel.Platform.Permit.Owner.Email = Email.Text;
            BaseViewModels.AuthViewModel.Platform.Permit.Owner.Nickname = string.Empty;
            BaseViewModels.AuthViewModel.Platform.Permit.Owner.FirstName = string.Empty;
            BaseViewModels.AuthViewModel.Platform.Permit.Owner.LastName = string.Empty;
            BaseViewModels.AuthViewModel.Platform.Permit.Owner.TerritoryISOCode = string.Empty;
        }

        private async Task PushUserUpdateAsync()
        {
            await DisplayAlert("Nester", "A security code was sent to the email address. Confirm by entering the code.", "OK");

            await MainView.StackViewSkipBackAsync(
                new UserView(true));
        }

        async private void OnLoginButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                ResultSingle<Permit> result = await BaseViewModels
                    .AuthViewModel.QueryTokenAsync(false);

                if (result.Code == Cloud.ServerStatus.NEST_RESULT_ERROR_AUTH_SECCODE)
                {
                    // the user can be hanging in inactive state
                    // if he/she did not confirm the security code
                    // in the second stage after registration.
                    // this result suggests the credentials were
                    // sound but need to confirm the security code.
                    // a new sec code would have been sent too.

                    await PushUserUpdateAsync();
                }
                else if (result.Code == Cloud.ServerStatus.NEST_RESULT_SUCCESS)
                {
                    await BaseViewModels.PaymentViewModel.InitAsync();

                    await BaseViewModels.AppCollectionViewModel.LoadApps();

                    await MainView.GoHomeAsync();
                }
                else
                {
                    new ResultHandler<Permit>(result).Throw();
                }
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async void OnSignUpButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await BaseViewModels.AuthViewModel.SignupAsync();

                await PushUserUpdateAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
                IsServiceActive = false;
            }

            IsServiceActive = false;
        }

        async void OnSignOffButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                ResultSingle<Permit> result = await BaseViewModels
                    .AuthViewModel.QueryTokenAsync(false);

                if (result.Code == Cloud.ServerStatus.NEST_RESULT_SUCCESS)
                {
                    // the user can be hanging in inactive state
                    // if he/she did not confirm the security code
                    // in the second stage after registration.
                    // this result suggests the credentials were
                    // sound but need to confirm the security code.
                    // a new sec code would have been sent too.

                    await MainView.StackViewAsync(new ExitView());
                }
                else
                {
                    new ResultHandler<Permit>(result).Throw();
                }
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
                IsServiceActive = false;
            }

            IsServiceActive = false;
        }

        async void OnRecoverPasswordButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                ResultSingle<Permit> result = await BaseViewModels
                    .AuthViewModel.QueryTokenAsync(false);

                if (result.Code < 0)
                {
                    result = await BaseViewModels.AuthViewModel.RecoverPasswordAsync(false);

                    if (result.Code == Cloud.ServerStatus.NEST_RESULT_ERROR_USER_NFOUND)
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
                await ErrorHandler.ExceptionAsync(this, ex);
                IsServiceActive = false;
            }

            IsServiceActive = false;
        }

        async void OnAboutButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await MainView.StackViewAsync(
                    new WebView(WebView.Pages.AboutPage));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
                IsServiceActive = false;
            }

            IsServiceActive = false;
        }        
    }
}
