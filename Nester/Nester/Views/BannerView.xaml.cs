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
using System.Net;

namespace Nester.Views
{
    public partial class BannerView : Nester.Views.View
    {
        public enum Status
        {
            Undefined,
            Initializing,
            Updating,
            WaitingDeployment
        }

        private Status _status = Status.Undefined;

        public BannerView(Status status)
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View>
                {
                });

            ButtonAppSettings.Clicked += ButtonAppSettings_ClickedAsync;
            ButtonNotifications.Clicked += ButtonNotifications_ClickedAsync;
            ButtonAppDeploy.Clicked += ButtonAppDeploy_ClickedAsync;
            ButtonAddToSlack.Clicked += ButtonAddToSlack_ClickedAsync;
            ButtonAppMenu.Clicked += ButtonAppMenu_Clicked;

            ButtonAppSettings.IsVisible = false;
            ButtonNotifications.IsVisible = false;
            ButtonAppDeploy.IsVisible = false;
            ButtonAddToSlack.IsVisible = false;
            ButtonAppMenu.IsVisible = false;

            State = status;
        }

        public Status State
        {
            set
            {
                _status = value;
                progressControl.IsVisible = false;
                
                switch (_status)
                {
                    case Status.Undefined:
                        progressControl.IsVisible = false;
                        ButtonAppDeploy.IsVisible = false;

                        ButtonAppSettings.IsVisible = false;
                        ButtonNotifications.IsVisible = false;
                        ButtonAddToSlack.IsVisible = false;
                        ButtonAppMenu.IsVisible = false;
                        break;

                    case Status.Initializing:
                        progressControl.TitlePlacement = Syncfusion.SfBusyIndicator.XForms.TitlePlacement.Bottom;
                        progressControl.IsVisible = true;
                        progressControl.AnimationType = Syncfusion.SfBusyIndicator.XForms.AnimationTypes.SingleCircle;

                        ButtonAppDeploy.IsVisible = false;
                        ButtonAppDeploy.IsEnabled = false;

                        ButtonAppSettings.IsVisible = false;
                        ButtonNotifications.IsVisible = false;
                        ButtonAddToSlack.IsVisible = false;
                        ButtonAppMenu.IsVisible = false;
                        break;                        

                    case Status.Updating:
                        progressControl.Title = "Updating ...";
                        progressControl.TitlePlacement = Syncfusion.SfBusyIndicator.XForms.TitlePlacement.Bottom;
                        progressControl.IsVisible = true;
                        progressControl.AnimationType = Syncfusion.SfBusyIndicator.XForms.AnimationTypes.Gear;

                        ButtonAppDeploy.IsVisible = false;
                        ButtonAppDeploy.IsEnabled = false;

                        ButtonAppSettings.IsVisible = true;
                        ButtonNotifications.IsVisible = true;
                        ButtonAddToSlack.IsVisible = true;
                        ButtonAppMenu.IsVisible = true;
                        break;

                    case Status.WaitingDeployment:

                        ButtonAppDeploy.IsEnabled = true;
                        ButtonAppDeploy.IsVisible = true;

                        ButtonAppSettings.IsVisible = true;
                        ButtonNotifications.IsVisible = true;
                        ButtonAddToSlack.IsVisible = true;
                        ButtonAppMenu.IsVisible = true;
                        break;
                }
            }

            get
            {
                return _status;
            }
        }

        public override AppViewModel AppViewModel
        {
            get { return _appViewModel; }
            set {
                _appViewModel = value;
                BindingContext = _appViewModel;

                if (_appViewModel.EditApp != null)
                {
                    Title = _appViewModel.EditApp.Name;
                }
                else
                {
                    Title = "Hello World";
                }
            }
        }

        async private void ButtonAppSettings_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                _appViewModel.WizardMode = false;

                LoadView(new AppBasicDetailView(_appViewModel));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        async private void ButtonNotifications_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await _appViewModel.QueryAppNotificationsAsync();

                LoadView(new NotificationView(_appViewModel));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        async private void ButtonAddToSlack_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await _appViewModel.ContactModel.QueryContactCollaborateAccountAsync();

                string clientId = "237221988247.245551261632";
                string scope = "incoming-webhook,chat:write:bot";

                string url = "https://slack.com/oauth/authorize?" +
                    "&client_id=" + WebUtility.UrlEncode(clientId) +
                    "&scope=" + WebUtility.UrlEncode(scope) +
                    "&state=" + WebUtility.UrlEncode(_appViewModel.ContactModel.Collaboration.State);

                Device.OpenUri(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        async private void ButtonAppDeploy_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await _appViewModel.InitAsync();

                if (_appViewModel.PaymentModel.PaymentMethod.Proof == null ||
                    _appViewModel.PaymentModel.PaymentMethod.Proof.Last4 == 0)
                {
                    await DisplayAlert("Nester", "Please enter a payment method before app creation", "OK");
                    return;
                }

                Admin.NestPlatform workerPlatform = _appViewModel.NestModel.Platforms.First(
                    x => x.Tag == "worker");

                var handlerNests = from nest in _appViewModel.NestModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (handlerNests.ToArray().Length == 0)
                {
                    await DisplayAlert("Nester", "Please add a handler nest to process queries", "OK");
                    return;
                }

                await _appViewModel.DeploymentModel.CollectInfoAsync();

                if (!_appViewModel.DeploymentModel.Deployments.Any())
                {
                    Cloud.ServerStatus status = await _appViewModel.QueryAppServiceTierLocationsAsync(
                        _appViewModel.SelectedAppServiceTier, false);
                    var forests = status.PayloadToList<Admin.Forest>();
                    LoadView(new AppLocationView(_appViewModel, forests));
                }
                else
                {
                    LoadView(new AppSummaryView(_appViewModel));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        private void ButtonAppMenu_Clicked(object sender, EventArgs e)
        {
            _masterDetailPage.IsPresented = true;
        }
    }
}
