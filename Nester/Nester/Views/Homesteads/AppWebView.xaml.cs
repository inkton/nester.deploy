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
    public partial class AppWebView : Inkton.Nester.Views.View
    {
        public AppWebView(Views.BaseModels baseModels)
        {
            _baseModels = baseModels;
            _baseModels.WizardMode = false;

            InitializeComponent();

            _activityIndicator = ServiceActive;

            if (_baseModels.AppViewModel.EditApp.Id != 0)
            {
                Title = _baseModels.AppViewModel.EditApp.Name;
                Browser.Source = "https://" + _baseModels.AppViewModel.DomainModel.DefaultDomain.Name;
            }
            else
            {
                //var htmlSource = new HtmlWebViewSource();
                //htmlSource.Html = "ms-appx-web:///index.html";
                Browser.Source = "ms-appx-web:///index.html";
            }

            BindingContext = _baseModels.AppViewModel;

            //ButtonBrowserBack.Clicked += ButtonBrowserBack_Clicked;
            //ButtonBrowserForward.Clicked += ButtonBrowserForward_Clicked;
        }

        private string DefaultPage
        {
            get
            {
                return
                    @"
<html>
    <head> 
        <title> Nest </title> 
        <meta charset = 'UTF-8'>  
        <meta name = 'description' content = 'nest.yt Platform as a Service (PaaS) micro-container platform'>     
        <meta name = 'keywords'pass, sass, docker, platform, developers, build, deploy, scale, php, java, dot.net, python, ruby, nodejs'>
        <link href='https://fonts.googleapis.com/css?family=Open+Sans:300|Roboto' rel='stylesheet'>
        <style>
            html, body { height: 100 %; }
                body { margin: 0; padding: 0; width: 100 %; display: table; font-family: 'Open Sans'; }
            .container { text-align: center; display: table-cell; vertical-align: middle; }
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

            }
        }
        private void ButtonBrowserForward_Clicked(object sender, EventArgs e)
        {
            if (Browser.CanGoForward)
            {
                Browser.GoForward();
            }
        }

        private void ButtonBrowserBack_Clicked(object sender, EventArgs e)
        {
            if (Browser.CanGoBack)
            {
                Browser.GoBack();
            }
            else
            { // If not, leave the view
                ResetView();
            }
        }

        async private void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                ResetView();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}

