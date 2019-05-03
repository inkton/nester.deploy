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
using System.Reflection;
using Xamarin.Forms;
using Inkton.Nest.Cloud;
using Inkton.Nest.Model;
using Inkton.Nester.Cloud;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Helpers;
using DeployApp = Nester.Deploy.App;

namespace Inkton.Nester.Views
{
    public partial class EntryView : View
    {
        public EntryView()
        {
            BindingContext = _baseViewModels.AuthViewModel;

            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    Email,
                    Password,
                    ButtonLogin,
                    ButtonSignoff,
                    ButtonSignup,
                    ButtonRecoverPassword
                });

            LoadHelpPage();
        }
            
        private void LoadHelpPage()
        {
            string page = @"
<html>
    <head> 
        <title> Nest </title> 
        <link href='https://fonts.googleapis.com/css?family=Open+Sans:300|Roboto' rel='stylesheet'>
        <style>
            body {
              background-color: #F3F9FF;
              color: #34495e;
              font-family: 'Open Sans';
              font-size: 12px;
            }
            .container
            {
              padding: 1%;
            }
            .content { font-size: 12px; font-family: 'Open Sans'; }
            .title { font-size: 18px; font-family: 'Roboto'; }
            .sub-title { font-size: 12px; color: #317589; }
            .content p { font-size: 11px; text-align: left; color: #48929B }
            .content table { margin-top:10px;  margin-bottom:10px;}
            .content table td { vertical-align:top;} 
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='content'>
                <div>
                   <span class='title'>Nester Deploy</span>&nbsp;
                   <span class='sub-title'>Version " + typeof(EntryView).GetTypeInfo()
                    .Assembly.GetName().Version.ToString() + @"</span>
                </div>
                <table>
                    <tr>
                        <td>
                            <div class='sub-title'>How to Register</div>
                            <p>
                                Enter email, password and click 'Sigin-In' below
                            </p>
                        </td>
                        <td>
                            <div class='sub-title'>How to Login</div>
                            <p>
                                Enter email, password and click 'Login' below
                            </p>
                        </td>
                        <td>
                            <div class='sub-title'>How to Unregister</div>
                            <p>
                                Enter email, password and click 'Sigin-Out' below
                            </p>
                        </td>
                        <td>
                            <div class='sub-title'>How to Recover Password</div>
                            <p>
                                Enter email and click 'Recover Password' below
                            </p>
                        </td>
                    </tr>
                </table>
                <div>
                    <a href='https://nestyt.com/blog/' target='_blank'>Blog</a>&nbsp;•&nbsp;
                    <a href='https://github.com/inkton/nester.deploy/wiki' target='_bl0nk'>Wiki</a>&nbsp;•&nbsp
                    <a href='https://github.com/inkton/nester.deploy/issues' target='_blank'>Discuss</a>&nbsp;•&nbsp
                    <a href='https://my.nest.yt/' target='_blank'>Support</a>
                </div><br/>
                <div class='sub-title'>By Inkton</div>
            </div>
        </div>
    </body>
</html>";
            var htmlSource = new HtmlWebViewSource();
            htmlSource.Html = page;
            StartHelp.Source = htmlSource;
        }

        void Validate()
        {
            if (EmailValidator != null)
            {
                _baseViewModels.AuthViewModel.Validated = (
                    EmailValidator.IsValid &&
                    PasswordValidator.IsValid);

                _baseViewModels.AuthViewModel.CanRecoverPassword = (
                    _baseViewModels.AuthViewModel.Platform.Permit.Owner.Email != null &&
                    _baseViewModels.AuthViewModel.Platform.Permit.Owner.Email.Length > 0 &&
                    EmailValidator.IsValid);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            BeginNewSession();

            Validate();
        }

        private void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }

        private void BeginNewSession()
        {
            _baseViewModels.AuthViewModel.Reset();
            _baseViewModels.AppCollectionViewModel.AppModels.Clear();
            _baseViewModels.WizardMode = true;

            // TODO: binding does not update sometimes
            // this a temp fix to capture inputs
            _baseViewModels.AuthViewModel.Platform.Permit.Password = Password.Text;
            _baseViewModels.AuthViewModel.Platform.Permit.Owner.Email = Email.Text;
        }

        private void PushEngageView()
        {
            AppViewModel newAppModel = new AppViewModel(BaseViewModels.Platform);   
            BaseViewModels.WizardMode = true;

            AppEngageView engageView = new AppEngageView(newAppModel);
            engageView.MainSideView = MainSideView;

            MainSideView.Detail.Navigation.InsertPageBefore(engageView, this);
        }

        private void PushUserUpdate()
        {
            DisplayAlert("Nester", "A security code was sent to the email address. Confirm by entering the code.", "OK");

            UserView userView = new UserView(_baseViewModels);
            userView.MainSideView = MainSideView;

            MainSideView.Detail.Navigation.InsertPageBefore(userView, this);
        }

        async private void OnLoginButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                ResultSingle<Permit> result = await _baseViewModels
                    .AuthViewModel.QueryTokenAsync(false);

                if (result.Code == Cloud.ServerStatus.NEST_RESULT_ERROR_AUTH_SECCODE)
                {
                    // the user can be hanging in inactive state
                    // if he/she did not confirm the security code
                    // in the second stage after registration.
                    // this result suggests the credentials were
                    // sound but need to confirm the security code.
                    // a new sec code would have been sent too.

                    PushUserUpdate();

                    await MainSideView.Detail.Navigation.PopAsync();
                }
                else if (result.Code == Cloud.ServerStatus.NEST_RESULT_SUCCESS)
                {
                    await _baseViewModels.PaymentViewModel.InitAsync();

                    await _baseViewModels.AppCollectionViewModel.LoadApps();

                    if (!_baseViewModels.AppCollectionViewModel.AppModels.Any())
                    {
                        PushEngageView();

                        await MainSideView.Detail.Navigation.PopAsync();
                    }
                    else
                    {
                        await MainSideView.Detail.Navigation.PopAsync();

                        ((DeployApp)Application.Current).RefreshView();
                    }
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
                await _baseViewModels.AuthViewModel.SignupAsync();

                PushUserUpdate();

                await MainSideView.Detail.Navigation.PopAsync();
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
                ResultSingle<Permit> result = await _baseViewModels
                    .AuthViewModel.QueryTokenAsync(false);

                if (result.Code == Cloud.ServerStatus.NEST_RESULT_SUCCESS)
                {
                    // the user can be hanging in inactive state
                    // if he/she did not confirm the security code
                    // in the second stage after registration.
                    // this result suggests the credentials were
                    // sound but need to confirm the security code.
                    // a new sec code would have been sent too.

                    ExitView exitView = new ExitView();
                    exitView.MainSideView = MainSideView;

                    await MainSideView.Detail.Navigation.PushAsync(exitView);
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
                ResultSingle<Permit> result = await _baseViewModels
                    .AuthViewModel.QueryTokenAsync(false);

                if (result.Code < 0)
                {
                    result = await _baseViewModels.AuthViewModel.RecoverPasswordAsync(false);

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
    }
}
