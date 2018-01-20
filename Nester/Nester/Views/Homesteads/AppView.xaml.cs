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
using System.Net;
using Xamarin.Forms;
using Syncfusion.SfBusyIndicator.XForms;
using Inkton.Nester.Models;
using Inkton.Nester.ViewModels;
using System.Threading.Tasks;

namespace Inkton.Nester.Views
{
    public partial class AppView : View
    {
        public enum Status
        {
            Updating,
            Refreshing,
            WaitingDeployment,
            Deployed
        }

        private Status _status = Status.Deployed;

        public AppView(BaseModels baseModels)
        {
            _baseModels = baseModels;
            _baseModels.WizardMode = false;

            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View>
                {
                });

            if (_baseModels.TargetViewModel.EditApp != null)
            {
                Title = _baseModels.TargetViewModel.EditApp.Name;
            }
            else
            {
                Title = "Hello World";
            }

            BindingContext = _baseModels.TargetViewModel;

            DateTime endTime = DateTime.Now.ToUniversalTime();
            AnalyzeDateUTC.Date = endTime;
            DateTime dayBegin = new DateTime(
                endTime.Year, endTime.Month, endTime.Day, 0, 0, 0);
            TimeSpan dayElapsedTime = endTime - dayBegin;

            DateTime startTime;

            if (dayElapsedTime.TotalMinutes < 60)
            {
                startTime = dayBegin;
            }
            else
            {
                startTime = endTime.Subtract(new TimeSpan(0, 60, 0));
            }

            StartTime.Time = new TimeSpan(startTime.Hour, startTime.Minute, startTime.Second);
            EndTime.Time = new TimeSpan(endTime.Hour, endTime.Minute, endTime.Second);

            ButtonAppSettings.Clicked += ButtonAppSettings_ClickedAsync;
            ButtonNotifications.Clicked += ButtonNotifications_ClickedAsync;
            ButtonAppDepRemove.Clicked += ButtonAppDepRemove_ClickedAsync;
            ButtonAppDeploy.Clicked += ButtonAppDeploy_ClickedAsync;
            ButtonAppView.Clicked += ButtonAppView_ClickedAsync;
            ButtonAppDownload.Clicked += ButtonAppDownload_ClickedAsync;
            ButtonGetAnalytics.Clicked += ButtonGetAnalytics_Clicked;
            ButtonAddToSlack.Clicked += ButtonAddToSlack_ClickedAsync;

            NestLogs.SelectionChanged += NestLogs_SelectionChanged;

            _baseModels.TargetViewModel.LogViewModel.SetupDiskSpaceSeries(
                DiskSpaceData.Series[0]
                );

            _baseModels.TargetViewModel.LogViewModel.SetupRAMSeries(
                RamData.Series[0],
                RamData.Series[1],
                RamData.Series[2],
                RamData.Series[3]
                );

            _baseModels.TargetViewModel.LogViewModel.SetupCPUSeries(
                CpuData.Series[0],
                CpuData.Series[1],
                CpuData.Series[2],
                CpuData.Series[3],
                CpuData.Series[4]
                );

            _baseModels.TargetViewModel.LogViewModel.SetupIpv4Series(
                Ipv4Data.Series[0],
                Ipv4Data.Series[1]);
        }

        public Status State
        {
            set
            {
                _status = value;

                switch (_status)
                {
                    case Status.Deployed:
                        InactiveApp.IsVisible = false;
                        Metrics.IsVisible = true;
                        break;

                    case Status.Refreshing:
                        ProgressControl.Title = "Refreshing ...";
                        ProgressControl.AnimationType = AnimationTypes.Rectangle;

                        ProgressControl.IsVisible = true;
                        Logo.IsVisible = false;
                        InactiveApp.IsVisible = true;
                        Metrics.IsVisible = false;
                        break;

                    case Status.Updating:
                        ProgressControl.Title = "Updating ...";
                        ProgressControl.AnimationType = AnimationTypes.Gear;

                        ProgressControl.IsVisible = true;
                        Logo.IsVisible = false;
                        InactiveApp.IsVisible = true;
                        Metrics.IsVisible = false;
                        break;

                    case Status.WaitingDeployment:
                        ProgressControl.IsVisible = false;
                        Logo.IsVisible = true;
                        InactiveApp.IsVisible = true;
                        Metrics.IsVisible = false;
                        break;
                }
            }
            get
            {
                return _status;
            }
        }

        async public void ReloadAsync()
        {
            try
            {
                Status oldState = _status;
                State = Status.Refreshing;

                Cloud.ServerStatus status;

                status = await _baseModels.TargetViewModel.InitAsync();
                if (status.Code < 0)
                {
                    await DisplayAlert("Nester", "Failed to get an update of " + App.Name, "OK");
                }

                await GetAnalyticsAsync();

                OnPropertyChanged("App");

                State = oldState;
            }
            catch (Exception)
            {
                // The connection may fail
            }
        }

        public async Task GetAnalyticsAsync()
        {
            try
            {
                DateTime unixEpoch = new DateTime(1970, 1, 1);
                DateTime analyzeDateUTC = AnalyzeDateUTC.Date;

                long beginId = (long)(new DateTime(
                        analyzeDateUTC.Year, analyzeDateUTC.Month, analyzeDateUTC.Day,
                        StartTime.Time.Hours,
                        StartTime.Time.Minutes,
                        StartTime.Time.Seconds) - unixEpoch).TotalSeconds;
                long endId = (long)(new DateTime(
                        analyzeDateUTC.Year, analyzeDateUTC.Month, analyzeDateUTC.Day,
                        EndTime.Time.Hours,
                        EndTime.Time.Minutes,
                        EndTime.Time.Seconds) - unixEpoch).TotalSeconds;

                if (beginId >= endId)
                {
                    await DisplayAlert("Nester", "Make sure begin time is less than end time", "OK");
                }
                else
                {
                    await _baseModels.TargetViewModel
                        .LogViewModel.QueryAsync(beginId, endId, 10000000);
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Nester", "Failed to retrieve metrics from the app " +
                    _baseModels.TargetViewModel.EditApp.Name , "OK");
            }
        }

        private void NestLogs_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            NestLog nestLog = e.AddedItems.FirstOrDefault() as NestLog;

            if (nestLog != null)
            {
                _baseModels.TargetViewModel.LogViewModel.EditNestLog = nestLog;
                Message.Text = nestLog.Message;
                /*                
                                if (_baseModels.AppViewModel.LogViewModel.DiskSpaceLogs != null)
                                {
                                    var diskspaceLog = _baseModels.AppViewModel.LogViewModel.DiskSpaceLogs.FirstOrDefault(
                                            x => x.Time >= nestLog.Time);
                                    if (diskspaceLog != null)
                                    {
                                        FocusDisk.Text = string.Format("{0:P1}",
                                            diskspaceLog.Used / (diskspaceLog.Used + diskspaceLog.Available + diskspaceLog.ReservedForRoot));
                                    }
                                }

                                if (_baseModels.AppViewModel.LogViewModel.SystemCPULogs != null)
                                {
                                    var cpuLog = _baseModels.AppViewModel.LogViewModel.SystemCPULogs.FirstOrDefault(
                                            x => x.Time >= nestLog.Time);
                                    if (cpuLog != null)
                                    {
                                        FocusCPU.Text = string.Format("{0:P1}",
                                            (cpuLog.User + cpuLog.System + cpuLog.IRQ + cpuLog.Nice + cpuLog.IOWait) / 100);
                                    }
                                }

                                if (_baseModels.AppViewModel.LogViewModel.SystemIPV4Logs != null)
                                {
                                    var ipv4Log = _baseModels.AppViewModel.LogViewModel.SystemIPV4Logs.FirstOrDefault(
                                            x => x.Time >= nestLog.Time);
                                    if (ipv4Log != null)
                                    {
                                        FocusNet.Text = string.Format("{0:0}/{0:0}",
                                             Math.Abs(ipv4Log.Sent / 1024),
                                             Math.Abs(ipv4Log.Received / 1024));
                                    }
                                }

                                if (_baseModels.AppViewModel.LogViewModel.SystemRAMLogs != null)
                                {
                                    var ramLog = _baseModels.AppViewModel.LogViewModel.SystemRAMLogs.FirstOrDefault(
                                             x => x.Time >= nestLog.Time);
                                    if (ramLog != null)
                                    {
                                        FocusRAM.Text = string.Format("{0:P1}",
                                            ramLog.Used / (ramLog.Used + ramLog.Cached + ramLog.Buffers));
                                    }
                                }
                */
            }
        }

        private async void ButtonGetAnalytics_Clicked(object sender, EventArgs e)
        {
            try
            {
                await GetAnalyticsAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        async private void ButtonAppDownload_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                Devkit devkit = new Devkit();
                devkit.Contact = _baseModels.TargetViewModel.ContactModel.OwnerContact;

                await _baseModels.TargetViewModel.DeploymentModel.QueryDevkitAsync(devkit);

                await DisplayAlert("Nester", "The devkit has been emailed to you", "OK");
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
                await _baseModels.TargetViewModel.QueryAppNotificationsAsync();

                MainSideView.LoadView(new NotificationView(_baseModels));
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
                MainSideView.LoadView(new AppWebView(_baseModels,
                    AppWebView.Pages.TargetSlackConnect));
/*
                await _baseModels.TargetViewModel.ContactModel.QueryContactCollaborateAccountAsync();

                string clientId = "237221988247.245551261632";
                string scope = "incoming-webhook,chat:write:bot";

                string url = "https://slack.com/oauth/authorize?" +
                    "&client_id=" + WebUtility.UrlEncode(clientId) +
                    "&scope=" + WebUtility.UrlEncode(scope) +
                    "&state=" + WebUtility.UrlEncode(_baseModels.TargetViewModel.ContactModel.Collaboration.State);

                Device.OpenUri(new Uri(url));
                */
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        async private void ButtonAppView_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.LoadView(new AppWebView(_baseModels, 
                    AppWebView.Pages.TargetSite));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonAppDepRemove_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                var yes = await DisplayAlert("Nester", "Would you like to remove this deployment", "Yes", "No");

                if (yes)
                {
                    if (_baseModels.TargetViewModel.EditApp.IsDeploymentValid)
                    {
                        State = Status.Updating;

                        await Process(_baseModels.TargetViewModel.EditApp.Deployment, true,
                            _baseModels.TargetViewModel.DeploymentModel.RemoveDeploymentAsync
                        );

                        await _baseModels.TargetViewModel.InitAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonAppDeploy_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await _baseModels.TargetViewModel.InitAsync();

                if (_baseModels.PaymentViewModel.EditPaymentMethod.Proof == null ||
                    _baseModels.PaymentViewModel.EditPaymentMethod.Proof.Last4 == 0)
                {
                    await DisplayAlert("Nester", "Please enter a payment method before app creation", "OK");
                    return;
                }

                NestPlatform workerPlatform = _baseModels.TargetViewModel.NestModel.Platforms.First(
                    x => x.Tag == "worker");

                var handlerNests = from nest in _baseModels.TargetViewModel.NestModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (handlerNests.ToArray().Length == 0)
                {
                    await DisplayAlert("Nester", "Please add a handler nest to process queries", "OK");
                    return;
                }

                await _baseModels.TargetViewModel.DeploymentModel.CollectInfoAsync();

                if (!_baseModels.TargetViewModel.DeploymentModel.Deployments.Any())
                {
                    Cloud.ServerStatus status = await _baseModels.TargetViewModel.QueryAppServiceTierLocationsAsync(
                        _baseModels.TargetViewModel.SelectedAppService.Tier, false);
                    var forests = status.PayloadToList<Forest>();
                    MainSideView.LoadView(new AppLocationView(_baseModels, forests));
                }
                else
                {
                    MainSideView.LoadView(new AppSummaryView(_baseModels));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        async private void ButtonAppSettings_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                BaseModels.WizardMode = false; ;
                MainSideView.LoadView(new AppBasicDetailView(BaseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}
