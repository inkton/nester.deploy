using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Nester.Views
{
    public partial class AppWebView : Nester.Views.View
    {
        public AppWebView(AppViewModel appViewModel)
        {
            InitializeComponent();

            _activityIndicator = ServiceActive;

            _appViewModel = appViewModel;
            _appViewModel.WizardMode = false;

            if (_appViewModel.EditApp.Id != 0)
            {
                Title = _appViewModel.EditApp.Name;
                Browser.Source = "https://" + _appViewModel.DomainModel.DefaultDomain.Name;
            }
            else
            {
                //var htmlSource = new HtmlWebViewSource();
                //htmlSource.Html = "ms-appx-web:///index.html";
                Browser.Source = "ms-appx-web:///index.html";
            }

            BindingContext = _appViewModel;

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
                Navigation.PopAsync();
            }
        }

        async private void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                LoadView(new AppView(_appViewModel));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}

