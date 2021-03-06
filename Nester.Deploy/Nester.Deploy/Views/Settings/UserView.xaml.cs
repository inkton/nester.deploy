﻿/*
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
using Xamarin.Forms;
using Inkton.Nest.Model;
using Inkton.Nest.Cloud;
using Inkton.Nester.Cloud;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class UserView : View
    {
        public UserView(bool wizardMode = false)
            :base(wizardMode)
        {
            InitializeComponent();

            SecurityCode.IsVisible = _wizardMode;
            SecurityCodeLabel.IsVisible = _wizardMode;
            int selectedTerritoryIndex = -1;

            foreach (Geography.ISO3166Country territory in Geography.Territories)
            {
                Territories.Items.Add(territory.ToString());

                if (_wizardMode == false)
                {
                    if (BaseViewModels.Platform.Permit.Owner.TerritoryISOCode == territory.Alpha2)
                    {
                        selectedTerritoryIndex = Territories.Items.Count - 1;
                    }
                }
                else
                {
                    if (System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName == territory.Alpha2)
                    {
                        selectedTerritoryIndex = Territories.Items.Count - 1;
                    }
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

            NickName.Unfocused += NickName_Unfocused;

            BindingContext = BaseViewModels.AuthViewModel;

            LoadExplainPage();
        }

        private void NickName_Unfocused(object sender, FocusEventArgs e)
        {
/*            string nickname = (sender as Xamarin.Forms.Entry).Text;
            if (nickname != null && nickname.Length > 0)
            {
                User searchUser = new User();
                searchUser.Nickname = nickname;

                Cloud.ServerStatus result = await _appViewModel.QueryAppAsync(
                    searchApp, true, false);
                if (result.Code == Cloud.Result.NEST_RESULT_SUCCESS)
                {
                    NicknameValidator.IsValid = false;
                    NicknameValidator.Message = "The nickname is taken, try another nickname";
                }
            }*/
        }

        private void LoadExplainPage()
        {
            string page = @"
<html>
    <head> 
        <title> Nest </title> 
        <link href='ms-appx-web:///googlefonts.css' rel='stylesheet'>
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
            .content { text-align: center; display: inline-block; }
            .title { font-size: 18px; font-family: 'Roboto'; }
            .sub-title { font-size: 12px; font-family: 'Roboto'; text-decoration: underline; }
            p { text-align: left; }
            ul {
              margin: 0
            }
            ul.dashed {
              list-style-type: none;
              text-align: left;
            }
            ul.dashed > li {
              text-indent: -8px;
            }
            ul.dashed > li:before {
              content: '-';
              text-indent: -8px;
            }
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='content'>
                <div class='title'>Terms</div>
                <p>Follow the links below for the terms of service and the privacy policy.</p>
                <p>
                    <ul class='dashed'>
                      <li><a href='https://nestyt.com/terms.html' target='_blank'>The Terms of Service</a></li>
                      <li><a href='https://nestyt.com/privacy.html' target='_blank'>The Privacy Policy.</a></li>
                    </ul>
                </p>
                <p>
                    Nest.yt complies with the <a href='https://www.eugdpr.org/ target='_blank'>EU General Data Protection Regulation (GDPR)</a>. Please read the privacy policy for details.
                </p>
                <div class='sub-title'>Support</div>
                <p>The Nester Deploy manual is found in the following link.</p>
                <a href='https://github.com/inkton/nester.deploy/wiki' target='_blank'>https://github.com/inkton/nester.deploy/wiki</a>
                <p>Open a ticket for issues relating to the open source project here. </p>
                <a href='https://github.com/inkton/nester.deploy/issues' target='_blank'>https://github.com/inkton/nester.deploy/issues</a>
                <p>Follow the link below and login with the Nester Deploy user and password for any support issues relating to the account. </p>
                <a href='https://my.nest.yt/' target='_blank'>https://my.nest.yt/</a>
            </div>
        </div>
    </body>
</html>";
            var htmlSource = new HtmlWebViewSource();
            htmlSource.Html = page;
            Browser.Source = htmlSource;
        }

        void Validate()
        {
            if (NicknameValidator != null)
            {
                BaseViewModels.AuthViewModel.Validated = (
                     NicknameValidator.IsValid &&
                     FirstNameValidator.IsValid &&
                     LastNameValidator.IsValid
                     );

                SecurityCode.IsVisible = _wizardMode;
                SecurityCodeLabel.IsVisible = _wizardMode;
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

                foreach (Geography.ISO3166Country territory in Geography.Territories)
                {
                    if (territoryName == territory.ToString())
                    {
                        BaseViewModels.Platform.Permit.Owner.TerritoryISOCode = territory.Alpha2;
                        break;
                    }
                }
                
                IsServiceActive = false;

                if (_wizardMode)
                {
                    ResultSingle<Permit> result = await BaseViewModels.AuthViewModel.SignupAsync(false);

                    if (result.Code < 0)
                    {
                        if (result.Code == Cloud.ServerStatus.NEST_RESULT_ERROR_AUTH_SECCODE)
                        {
                            await DisplayAlert("Nester", "Invalid security code", "OK");
                        }
                        else
                        {
                            await DisplayAlert("Nester", result.Notes, "OK");
                        }
                    }
                    else
                    {
                        await BaseViewModels.AuthViewModel.QueryTokenAsync();

                        await MainView.StackViewSkipBackAsync(
                            new AppLaunchView(new AppViewModel(BaseViewModels.Platform)));
                    }
                }
                else
                {
                    await BaseViewModels.AuthViewModel.UpdateUserAsync(BaseViewModels.Platform.Permit.Owner);
                    await DisplayAlert("Nester", "Your information was saved", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
                IsServiceActive = false;
            }
        }

        async private void OnCloseButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await MainView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}
