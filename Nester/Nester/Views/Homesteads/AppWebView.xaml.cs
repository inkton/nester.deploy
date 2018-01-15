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
using Inkton.Nester.Models;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class AppWebView : View
    {
        public AppWebView(BaseModels baseModels)
        {
            _baseModels = baseModels;
            _baseModels.WizardMode = false;

            InitializeComponent();

            _activityIndicator = ServiceActive;

            if (_baseModels.TargetViewModel.EditApp.Id != 0)
            {
                Title = _baseModels.TargetViewModel.EditApp.Name;
                Browser.Source = "https://" + _baseModels.TargetViewModel.DomainModel.DefaultDomain.Name;
            }
            else
            {
                Browser.Source = "ms-appx-web:///index.html";
            }

            BindingContext = _baseModels.TargetViewModel;
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

        async private void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await NesterControl.ResetViewAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}

