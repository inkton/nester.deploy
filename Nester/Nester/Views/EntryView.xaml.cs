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
            _baseModels = NesterControl.BaseModels;

            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    Email,
                    Password,
                    ButtonLogin,
                    ButtonSignup,
                    ButtonRecoverPassword
                });

            BindingContext = _baseModels.AuthViewModel;
        }

        void Validate()
        {
            if (EmailValidator != null)
            {
                _baseModels.AuthViewModel.Validated = (
                    EmailValidator.IsValid &&
                    PasswordValidator.IsValid);

                _baseModels.AuthViewModel.CanRecoverPassword = (
                    _baseModels.AuthViewModel.Permit.Owner.Email != null &&
                    _baseModels.AuthViewModel.Permit.Owner.Email.Length > 0 &&
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
            _baseModels.AuthViewModel.Reset();
            AppCollectionViewModel appCollection = _baseModels.AppViewModel as AppCollectionViewModel;
            appCollection.AppModels.Clear();
            _baseModels.WizardMode = true;
        }

        private void PushEngageView()
        {
            AppViewModel newAppModel = new AppViewModel();
            newAppModel.NewAppAsync();

            BaseModels baseModels = new BaseModels(
                _baseModels.AuthViewModel,
                _baseModels.PaymentViewModel,
                newAppModel);
            baseModels.WizardMode = true;

            AppEngageView engageView = new AppEngageView(baseModels);
            engageView.MainSideView = MainSideView;

            MainSideView.Detail.Navigation.InsertPageBefore(engageView, this);
        }

        private void PushEngageViewWithUserUpdate()
        {
            PushEngageView();

            Views.UserView userView = new Views.UserView(_baseModels);
            userView.MainSideView = MainSideView;

            MainSideView.Detail.Navigation.InsertPageBefore(userView, this);
        }

        async private void OnLoginButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                BeginNewSession();

                Cloud.ServerStatus status = _baseModels.AuthViewModel.QueryToken(false);

                if (status.Code == Cloud.Result.NEST_RESULT_ERROR_AUTH_SECCODE)
                {
                    // the user can be hanging in inactive state
                    // if he/she did not confirm the security code
                    // in the second stage after registration.
                    // this result suggests the credentials were
                    // sound but need to confirm the security code.
                    // a new sec code would have been sent too.

                    PushEngageViewWithUserUpdate();
                }
                else if (status.Code == Cloud.Result.NEST_RESULT_SUCCESS)
                {
                    await _baseModels.PaymentViewModel.InitAsync();

                    AppCollectionViewModel appCollection = _baseModels.AppViewModel as AppCollectionViewModel;

                    await appCollection.LoadApps();

                    if (!appCollection.AppModels.Any())
                    {
                        PushEngageView();

                        await MainSideView.Detail.Navigation.PopAsync();
                    }
                    else
                    {
                        await MainSideView.Detail.Navigation.PopAsync();

                        BaseModels baseModels = new BaseModels(
                                _baseModels.AuthViewModel,
                                _baseModels.PaymentViewModel,
                                appCollection.AppModels.First());

                        NesterControl.ResetView(baseModels);
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

                _baseModels.AuthViewModel.Signup();

                PushEngageViewWithUserUpdate();

                await MainSideView.Detail.Navigation.PopAsync();
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
                    _baseModels.AuthViewModel.QueryToken(false);

                if (status.Code < 0)
                {
                    status = await _baseModels.AuthViewModel.RecoverPasswordAsync(false);

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
