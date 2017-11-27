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
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Syncfusion.SfChart.XForms;
using System.Collections.ObjectModel;
    using Inkton.Nester.Admin;
using System.Net;

namespace Inkton.Nester.Views
{
    public partial class AppView : View
    {
        public enum Status
        {
            Deploying,
            Refreshing,
            WaitingDeployment,
            Deployed
        }

        private Status _status = Status.Deployed;

        public AppView(Views.AppModelPair modelPair)
        {
            _modelPair = modelPair;
            _modelPair.WizardMode = false;

            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View>
                {
                });

            if (_modelPair.AppViewModel.EditApp != null)
            {
                Title = _modelPair.AppViewModel.EditApp.Name;
            }
            else
            {
                Title = "Hello World";
            }

            BindingContext = _modelPair.AppViewModel;

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

            _modelPair.AppViewModel.LogViewModel.SetupDiskSpaceSeries(
                DiskSpaceData.Series[0]
                );

            _modelPair.AppViewModel.LogViewModel.SetupRAMSeries(
                RamData.Series[0],
                RamData.Series[1],
                RamData.Series[2],
                RamData.Series[3]
                );

            _modelPair.AppViewModel.LogViewModel.SetupCPUSeries(
                CpuData.Series[0],
                CpuData.Series[1],
                CpuData.Series[2],
                CpuData.Series[3],
                CpuData.Series[4]
                );

            _modelPair.AppViewModel.LogViewModel.SetupIpv4Series(
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
                        ProgressControl.TitlePlacement = Syncfusion.SfBusyIndicator.XForms.TitlePlacement.Bottom;
                        ProgressControl.AnimationType = Syncfusion.SfBusyIndicator.XForms.AnimationTypes.Rectangle;

                        ProgressControl.IsVisible = true;
                        Logo.IsVisible = false;
                        InactiveApp.IsVisible = true;
                        Metrics.IsVisible = false;
                        break;

                    case Status.Deploying:
                        ProgressControl.Title = "Deploying ...";
                        ProgressControl.TitlePlacement = Syncfusion.SfBusyIndicator.XForms.TitlePlacement.Bottom;
                        ProgressControl.AnimationType = Syncfusion.SfBusyIndicator.XForms.AnimationTypes.Gear;

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

                status = await _modelPair.AppViewModel.InitAsync();
                if (status.Code < 0)
                {
                    await DisplayAlert("Nester", "Failed to get an update of " + App.Name, "OK");
                }

                GetAnalyticsAsync();

                OnPropertyChanged("App");

                State = oldState;
            }
            catch (Exception)
            {
                // The connection may fail
            }
        }

        public async void GetAnalyticsAsync()
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1);
            DateTime analyzeDateUTC = AnalyzeDateUTC.Date;

            long beginId = (long)(new DateTime(
                    analyzeDateUTC.Year, analyzeDateUTC.Month, analyzeDateUTC.Day,
                    StartTime.Time.Hours,
                    StartTime.Time.Minutes,
                    StartTime.Time.Seconds) - unixEpoch).TotalMilliseconds;
            long endId = (long)(new DateTime(
                    analyzeDateUTC.Year, analyzeDateUTC.Month, analyzeDateUTC.Day,
                    EndTime.Time.Hours,
                    EndTime.Time.Minutes,
                    EndTime.Time.Seconds) - unixEpoch).TotalMilliseconds;

            if (beginId >= endId)
            {
                await DisplayAlert("Nester", "Make sure begin time is less than end time", "OK");
                return;
            }

            _modelPair.AppViewModel.QueryMetricsAsync(beginId, endId);
        }

        private void NestLogs_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            NestLog nestLog = e.AddedItems.FirstOrDefault() as NestLog;

            if (nestLog != null)
            {
                _modelPair.AppViewModel.LogViewModel.EditNestLog = nestLog;
                Message.Text = nestLog.Message;
                /*                
                                if (_modelPair.AppViewModel.LogViewModel.DiskSpaceLogs != null)
                                {
                                    var diskspaceLog = _modelPair.AppViewModel.LogViewModel.DiskSpaceLogs.FirstOrDefault(
                                            x => x.Time >= nestLog.Time);
                                    if (diskspaceLog != null)
                                    {
                                        FocusDisk.Text = string.Format("{0:P1}",
                                            diskspaceLog.Used / (diskspaceLog.Used + diskspaceLog.Available + diskspaceLog.ReservedForRoot));
                                    }
                                }

                                if (_modelPair.AppViewModel.LogViewModel.SystemCPULogs != null)
                                {
                                    var cpuLog = _modelPair.AppViewModel.LogViewModel.SystemCPULogs.FirstOrDefault(
                                            x => x.Time >= nestLog.Time);
                                    if (cpuLog != null)
                                    {
                                        FocusCPU.Text = string.Format("{0:P1}",
                                            (cpuLog.User + cpuLog.System + cpuLog.IRQ + cpuLog.Nice + cpuLog.IOWait) / 100);
                                    }
                                }

                                if (_modelPair.AppViewModel.LogViewModel.SystemIPV4Logs != null)
                                {
                                    var ipv4Log = _modelPair.AppViewModel.LogViewModel.SystemIPV4Logs.FirstOrDefault(
                                            x => x.Time >= nestLog.Time);
                                    if (ipv4Log != null)
                                    {
                                        FocusNet.Text = string.Format("{0:0}/{0:0}",
                                             Math.Abs(ipv4Log.Sent / 1024),
                                             Math.Abs(ipv4Log.Received / 1024));
                                    }
                                }

                                if (_modelPair.AppViewModel.LogViewModel.SystemRAMLogs != null)
                                {
                                    var ramLog = _modelPair.AppViewModel.LogViewModel.SystemRAMLogs.FirstOrDefault(
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
                GetAnalyticsAsync();
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
                Admin.Devkit devkit = new Admin.Devkit();
                devkit.Contact = _modelPair.AppViewModel.ContactModel.OwnerContact;

                await _modelPair.AppViewModel.DeploymentModel.QueryDevkitAsync(devkit);

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
                await _modelPair.AppViewModel.QueryAppNotificationsAsync();

                MainSideView.LoadView(new NotificationView(_modelPair));
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
                await _modelPair.AppViewModel.ContactModel.QueryContactCollaborateAccountAsync();

                string clientId = "237221988247.245551261632";
                string scope = "incoming-webhook,chat:write:bot";

                string url = "https://slack.com/oauth/authorize?" +
                    "&client_id=" + WebUtility.UrlEncode(clientId) +
                    "&scope=" + WebUtility.UrlEncode(scope) +
                    "&state=" + WebUtility.UrlEncode(_modelPair.AppViewModel.ContactModel.Collaboration.State);

                Device.OpenUri(new Uri(url));

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
                MainSideView.LoadView(new AppWebView(_modelPair));
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
                    if (_modelPair.AppViewModel.EditApp.IsDeploymentValid)
                    {
                        await Process(_modelPair.AppViewModel.EditApp.Deployment, true,
                            _modelPair.AppViewModel.DeploymentModel.RemoveDeploymentAsync
                        );

                        await _modelPair.AppViewModel.InitAsync();
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
                await _modelPair.AppViewModel.InitAsync();

                if (_modelPair.AppViewModel.PaymentModel.PaymentMethod.Proof == null ||
                    _modelPair.AppViewModel.PaymentModel.PaymentMethod.Proof.Last4 == 0)
                {
                    await DisplayAlert("Nester", "Please enter a payment method before app creation", "OK");
                    return;
                }

                Admin.NestPlatform workerPlatform = _modelPair.AppViewModel.NestModel.Platforms.First(
                    x => x.Tag == "worker");

                var handlerNests = from nest in _modelPair.AppViewModel.NestModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (handlerNests.ToArray().Length == 0)
                {
                    await DisplayAlert("Nester", "Please add a handler nest to process queries", "OK");
                    return;
                }

                await _modelPair.AppViewModel.DeploymentModel.CollectInfoAsync();

                if (!_modelPair.AppViewModel.DeploymentModel.Deployments.Any())
                {
                    Cloud.ServerStatus status = await _modelPair.AppViewModel.QueryAppServiceTierLocationsAsync(
                        _modelPair.AppViewModel.SelectedAppServiceTier, false);
                    var forests = status.PayloadToList<Admin.Forest>();
                    MainSideView.LoadView(new AppLocationView(_modelPair, forests));
                }
                else
                {
                    MainSideView.LoadView(new AppSummaryView(_modelPair));
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
                _modelPair.WizardMode = false;

                MainSideView.LoadView(new AppBasicDetailView(_modelPair));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}
