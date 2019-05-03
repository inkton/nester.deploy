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
using Xamarin.Forms;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Helpers;

namespace Inkton.Nester.Views
{
    public partial class AppWebView : View
    {
        public enum Pages
        {
            DefaultPage,
            TargetSite,
            TargetSlackConnect
        }

        public AppWebView(AppViewModel appViewModel, 
            Pages page = Pages.DefaultPage)
        {
            InitializeComponent();

            AppViewModel = appViewModel;
            _baseViewModels.WizardMode = false;

            _activityIndicator = ServiceActive;

            if (page == Pages.DefaultPage)
            {
                LoadDefaultPage();
            }
            else if (page == Pages.TargetSlackConnect)
            {
                LoadSlackPageAsync();
            }
            else
            {
                if (AppViewModel.EditApp.Id != 0)
                {
                    Title = AppViewModel.EditApp.Name;
                    Browser.Source = "https://" + AppViewModel.DomainViewModel.DefaultDomain.Name;
                }
            }
        }

        private void LoadDefaultPage()
        {
            string page = @"
<html>
    <head> 
        <title> Nest </title> 
        <link href='https://fonts.googleapis.com/css?family=Open+Sans:300|Roboto' rel='stylesheet'>
        <style>
            body {
              background-color: #fcfcfc;
              font-family: 'Open Sans';
            }
            .container
            {
              position: fixed;
              top: 10%;
              left: 50%;
              transform: translate(-50%, -50%);
            }
            .content { text-align: center; display: inline-block; }
            .title { font-size: 66px; font-family: 'Roboto'; }
            .sub-title { font-size: 20px; }
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='content'>
                <div class='title'>Nester</div>
                <div class='sub-title'>Easy Web Enabler</div>
            </div>
        </div>
    </body>
</html>";
            var htmlSource = new HtmlWebViewSource();
            htmlSource.Html = page;
            Browser.Source = htmlSource;
        }

        private async void LoadSlackPageAsync()
        {
            await AppViewModel.ContactViewModel.QueryContactCollaborateAccountAsync();

            string clientId = "237221988247.245551261632";
            string scope = "incoming-webhook,chat:write:bot";
            
            string url = "https://slack.com/oauth/authorize?" +
                "&client_id=" + WebUtility.UrlEncode(clientId) +
                "&scope=" + WebUtility.UrlEncode(scope) +
                "&state=" + WebUtility.UrlEncode(AppViewModel.ContactViewModel.Collaboration.State);

            string page = @"
<html>
    <head> 
        <title> Nest </title> 
        <meta charset = 'UTF-8'>  
        <meta name = 'description' content = 'nest.yt Platform as a Service (PaaS) micro-container platform'>     
        <meta name = 'keywords'pass, sass, docker, platform, developers, build, deploy, scale, php, java, dot.net, python, ruby, nodejs'>
        <link href='https://fonts.googleapis.com/css?family=Open+Sans:300|Roboto' rel='stylesheet'>
        <style>
            body {
              background-color: #fcfcfc;
              font-family: 'Open Sans';
            }
            .container
            {
              position: fixed;
              top: 10%;
              left: 50%;
              transform: translate(-50%, -50%);
            }
            .content { text-align: center; display: inline-block; }
            .title { font-size: 20px; font-family:'Roboto';margin-bottom:15px; }
            .sub-title { font-size: 10px; }
            .slack {  }
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='content'>
                <div class='slack'>
                    <div class='title'>Connect " + AppViewModel.EditApp.Name + @" to Slack and stay updated</div>
                    <a target='_blank' href='" + url + @"' ><img alt='Add to Slack height='40' width='139' src='https://platform.slack-edge.com/img/add_to_slack.png' srcset='https://platform.slack-edge.com/img/add_to_slack.png 1x, https://platform.slack-edge.com/img/add_to_slack@2x.png 2x' /></a>
                <div>
            </div>
        </div>
    </body>
</html>";
            var htmlSource = new HtmlWebViewSource();
            htmlSource.Html = page;
            Browser.Source = htmlSource;
        }

        async private void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                MainSideView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }
        }
    }
}

