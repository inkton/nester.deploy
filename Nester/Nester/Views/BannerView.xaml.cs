using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Nester.Views
{
    public partial class BannerView : Nester.Views.View
    {
        public enum Status
        {
            BannerViewUndefined,
            BannerViewUpdating,
            BannerViewWaitingDeployment
        }

        private Status _status = Status.BannerViewUndefined;

        public BannerView()
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View>
                {
                });

            ButtonAppSettings.Clicked += ButtonAppSettings_ClickedAsync;
            ButtonNotifications.Clicked += ButtonNotifications_ClickedAsync;
            ButtonAppDeploy.Clicked += ButtonAppDeploy_ClickedAsync;
            ButtonAppMenu.Clicked += ButtonAppMenu_Clicked;

            ButtonAppSettings.IsVisible = false;
            ButtonNotifications.IsVisible = false;
            ButtonAppDeploy.IsVisible = false;
            ButtonAppMenu.IsVisible = false;
        }

        public Status State
        {
            set
            {
                _status = value;
                progressControl.IsVisible = false;
                
                switch (_status)
                {
                    case Status.BannerViewUndefined:
                        progressControl.IsVisible = false;
                        ButtonAppDeploy.IsVisible = false;

                        ButtonAppSettings.IsVisible = false;
                        ButtonNotifications.IsVisible = false;
                        ButtonAppMenu.IsVisible = false;
                        break;

                    case Status.BannerViewUpdating:
                        progressControl.Title = "Please Wait ...";
                        progressControl.TitlePlacement = Syncfusion.SfBusyIndicator.XForms.TitlePlacement.Top;
                        progressControl.IsVisible = true;

                        ButtonAppDeploy.IsVisible = false;
                        ButtonAppDeploy.IsEnabled = false;

                        ButtonAppSettings.IsVisible = true;
                        ButtonNotifications.IsVisible = true;
                        ButtonAppMenu.IsVisible = true;
                        break;

                    case Status.BannerViewWaitingDeployment:

                        ButtonAppDeploy.IsEnabled = true;
                        ButtonAppDeploy.IsVisible = true;

                        ButtonAppSettings.IsVisible = true;
                        ButtonNotifications.IsVisible = true;
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

                await Navigation.PushAsync(new NotificationView(_appViewModel));
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
                    await Navigation.PushAsync(new AppLocationView(_appViewModel, forests));
                }
                else
                {
                    await Navigation.PushAsync(new AppSummaryView(_appViewModel));
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
