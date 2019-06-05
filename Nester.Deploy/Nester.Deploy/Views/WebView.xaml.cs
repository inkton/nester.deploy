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
using System.Reflection;
using Xamarin.Forms;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Helpers;

namespace Inkton.Nester.Views
{
    public partial class WebView : View
    {
        public enum Pages
        {
            AboutPage,
            AppPage,
            AppSlackConnect
        }

        public WebView(Pages page = Pages.AboutPage)
        {
            InitializeComponent();

            _activityIndicator = ServiceActive;

            LoadAboutPage();
        }

        public WebView(Pages page, AppViewModel appViewModel)
        {
            InitializeComponent();

            AppViewModel = appViewModel;

            _activityIndicator = ServiceActive;

            switch (page)
            {
                case Pages.AppPage:
                    // TODO : prevent the page from being cached
                    // the 'under-construction' page is cached 
                    // even when the site is up
                    Browser.Source = "https://" + AppViewModel.DomainViewModel.DefaultDomain.Name + "/?cache=no";                    
                    break;

                case Pages.AppSlackConnect:
                    LoadSlackPageAsync();
                    break;
            }
        }

        private string BuildHead()
        {
            string page = @"
    <head> 
        <title>Nest.yt</title> 
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
            .content { font-size: 12px; font-family: 'Open Sans'; width: 100%;}
            .title { font-size: 18px; font-family: 'Roboto'; }
            .sub-title { font-size: 12px; color: #317589; }
            .content p { font-size: 11px; text-align: left; color: #48929B }
            .content table { margin-top:10px;  margin-bottom:10px;}
            .content table td { vertical-align:top;} 
            .center { margin-left: auto; margin-right: auto;}
        </style>
    </head>";
            return page;
        }

        private void LoadAboutPage()
        {
            string page = "<html>" + 
               BuildHead() + 
  @"<body>
        <div class='container'>
            <div class='content'>
                <div>
                   <span class='title'>Nester Deploy</span>&nbsp;
                   <span class='sub-title'>Version " + typeof(WebView).GetTypeInfo()
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

            string page = "<html>" +
               BuildHead() +
  @"<body>
        <div class='container'>
            <div class='content'>
                <div style='width: 28em;text-align: center;' class='center'>
                   <div class='title'>Connect " + AppViewModel.EditApp.Name + @" to Slack and stay updated</div><br>
                    <div class='sub-title'>
                        <a target='_blank' href='" + url + @"' >
                            <img alt='Add to Slack height='40' width='139' src='https://platform.slack-edge.com/img/add_to_slack.png' srcset='https://platform.slack-edge.com/img/add_to_slack.png 1x, https://platform.slack-edge.com/img/add_to_slack@2x.png 2x' />
                        </a>
                    </div>
                </div>
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
                await MainView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }
        }
    }
}

