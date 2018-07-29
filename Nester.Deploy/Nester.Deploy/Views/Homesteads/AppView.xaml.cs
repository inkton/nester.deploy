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
            InitializeComponent();

            BaseModels = baseModels;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View>
                {
                    ButtonGetAnalytics,
                    ButtonAppSettings,
                    ButtonNotifications,
                    ButtonAppDeploy,
                    ButtonAppRestore,
                    ButtonAppDepRemove,
                    ButtonAppHide,
                    ButtonAppShow,
                    ButtonAppUpgrade,
                    ButtonAppDownload,
                    ButtonAppView,
                    ButtonAddToSlack,
                    ButtonAppAudit
                });

            ResetTimeFilter();

            ButtonAppSettings.Clicked += ButtonAppSettings_ClickedAsync;
            ButtonNotifications.Clicked += ButtonNotifications_ClickedAsync;
            ButtonAppRestore.Clicked += ButtonAppRestore_ClickedAsync;
            ButtonAppUpgrade.Clicked += ButtonAppUpgrade_ClickedAsync;
            ButtonAppDepRemove.Clicked += ButtonAppDepRemove_ClickedAsync;
            ButtonAppHide.Clicked += ButtonAppHide_Clicked;
            ButtonAppShow.Clicked += ButtonAppShow_ClickedAsync;
            ButtonAppDeploy.Clicked += ButtonAppDeploy_ClickedAsync;
            ButtonAppView.Clicked += ButtonAppView_ClickedAsync;
            ButtonAppDownload.Clicked += ButtonAppDownload_ClickedAsync;
            ButtonGetAnalytics.Clicked += ButtonGetLogs_Clicked;
            ButtonAddToSlack.Clicked += ButtonAddToSlack_ClickedAsync;
            ButtonAppAudit.Clicked += ButtonAppAudit_ClickedAsync; 

            NestLogs.SelectionChanged += NestLogs_SelectionChanged;
        }

        public Status State
        {
            set
            {
                _status = value;

                switch (_status)
                {
                    case Status.Deployed:
                        if (App.IsActive)
                        {
                            // Deplyed and active
                            InactiveApp.IsVisible = false;
                            Metrics.IsVisible = true;
                        }
                        else
                        {
                            // Deplyed but failed
                            ProgressControl.IsVisible = false;
                            Logo.IsVisible = true;
                            InactiveApp.IsVisible = true;
                            Metrics.IsVisible = false;
                        }
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

        public override void UpdateBindings()
        {
            base.UpdateBindings();

            UpdateState();
        }

        public void UpdateState()
        {
            ResetTimeFilter();
            Status newState = Status.Refreshing;

            if (App.IsBusy)
            {
                newState = Status.Updating;
            }
            else if (!App.IsDeployed)
            {
                newState = Status.WaitingDeployment;
            }
            else
            {
                newState = Status.Deployed;
            }

            State = newState;

            ButtonAppDeploy.IsEnabled = !App.IsBusy;
            ButtonAppRestore.IsEnabled = App.IsActive;
            ButtonAppDepRemove.IsEnabled = App.IsDeployed;
            ButtonAppHide.IsEnabled = App.IsDeployed;
            ButtonAppShow.IsEnabled = App.IsDeployed;
            ButtonAppDownload.IsEnabled = App.IsActive;
            ButtonAppView.IsEnabled = App.IsActive;
            ButtonAppAudit.IsEnabled = App.IsActive;
            ButtonAppUpgrade.IsEnabled = App.IsActive;
        }

        public void ReloadAnalytics()
        {
            try
            {
                ResetTimeFilter();

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await GetLogsAsync();
                });
            }
            catch (Exception)
            {
                FetchError.Text = "Failed to fetch logs";
            }
        }

        private async Task GetLogsAsync(int maxRows = 200,
            bool doCache = false, bool throwIfError = true)
        {
            IsServiceActive = true;

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
                FetchError.Text = "The begin time > end time";
            }
            else
            {
                await _baseModels.TargetViewModel
                      .LogViewModel.QueryAsync(beginId, endId,
                      maxRows, doCache, throwIfError);
            }

            IsServiceActive = false;
        }

        private void ResetTimeFilter()
        {
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
        }

        private void NestLogs_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            NestLog nestLog = e.AddedItems.FirstOrDefault() as NestLog;

            if (nestLog != null)
            {
                _baseModels.TargetViewModel.LogViewModel.EditNestLog = nestLog;
                /* 
                    Message.Text = nestLog.Message;

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

        private async void ButtonGetLogs_Clicked(object sender, EventArgs e)
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
                    await DisplayAlert("Nester", "The begin time is after the end time", "OK");
                }
                else
                {
                    await GetLogsAsync();
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Nester", "Failed to retrieve metrics from the app " +
                    _baseModels.TargetViewModel.EditApp.Name, "OK");
            }
        }

        async private void ButtonAppDownload_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                Devkit devkit = new Devkit();
                devkit.Contact = _baseModels.TargetViewModel.ContactViewModel.OwnerContact;

                await _baseModels.TargetViewModel.DeploymentViewModel.QueryDevkitAsync(devkit);

                await DisplayAlert("Nester", "The devkit has been emailed to you", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonNotifications_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _baseModels.TargetViewModel.QueryAppNotificationsAsync();
                MainSideView.StackViewAsync(new NotificationView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonAddToSlack_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.StackViewAsync(new AppWebView(_baseModels,
                    AppWebView.Pages.TargetSlackConnect));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonAppView_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.StackViewAsync(new AppWebView(_baseModels, 
                    AppWebView.Pages.TargetSite));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonAppUpgrade_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _baseModels.TargetViewModel
                    .ServicesViewModel
                    .QueryAppUpgradeServiceTiersAsync(
                    _baseModels
                        .TargetViewModel
                        .ServicesViewModel
                        .SelectedAppService
                        .Tier.Service);

                _baseModels.WizardMode = false;
                MainSideView.StackViewAsync(new AppTierView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
            }

        async private void ButtonAppRestore_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _baseModels.TargetViewModel.DeploymentViewModel.QueryAppBackupsAsync();

                MainSideView.StackViewAsync(new AppBackupView(_baseModels));
            }
            catch (Exception ex)
            {   
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonAppDepRemove_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                var yes = await DisplayAlert("Nester", "Would you like to remove this deployment", "Yes", "No");

                if (yes)
                {
                    if (_baseModels.TargetViewModel.EditApp.IsDeploymentValid)
                    {
                        await Process(_baseModels.TargetViewModel.EditApp.Deployment, true,
                            _baseModels.TargetViewModel.DeploymentViewModel.RemoveDeploymentAsync
                        );

                        await _baseModels.TargetViewModel.QueryStatusAsync();

                        UpdateState();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        async private void ButtonAppShow_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                Deployment deployment =
                    _baseModels.TargetViewModel
                        .DeploymentViewModel.Deployments.First();

                await _baseModels
                    .TargetViewModel
                    .DeploymentViewModel
                    .UpdateDeploymentAsync("show", deployment);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        async private void ButtonAppHide_Clicked(object sender, EventArgs e)
        {
            try
            {
                Deployment deployment =
                    _baseModels.TargetViewModel
                        .DeploymentViewModel.Deployments.First();

                await _baseModels
                    .TargetViewModel
                    .DeploymentViewModel
                    .UpdateDeploymentAsync("hide", deployment);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        async private void ButtonAppDeploy_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                if (_baseModels.TargetViewModel
                    .ServicesViewModel.UpgradableAppTiers != null)
                {
                    _baseModels.TargetViewModel
                        .ServicesViewModel.UpgradableAppTiers.Clear();
                }

                if (_baseModels.PaymentViewModel.EditPaymentMethod.Proof == null ||
                    _baseModels.PaymentViewModel.EditPaymentMethod.Proof.Last4 == 0)
                {
                    await DisplayAlert("Nester", "Please enter a payment method before app creation", "OK");
                    return;
                }

                NestPlatform workerPlatform = _baseModels.TargetViewModel.NestViewModel.Platforms.First(
                    x => x.Tag == "worker");

                var handlerNests = from nest in _baseModels.TargetViewModel.NestViewModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (handlerNests.ToArray().Length == 0)
                {
                    await DisplayAlert("Nester", "Please add a handler nest to process queries", "OK");
                    return;
                }

                await _baseModels.TargetViewModel.DeploymentViewModel.CollectInfoAsync();

                if (!_baseModels.TargetViewModel.DeploymentViewModel.Deployments.Any())
                {
                    Cloud.ServerStatus status = await _baseModels.TargetViewModel.QueryAppServiceTierLocationsAsync(
                        _baseModels.TargetViewModel.ServicesViewModel.SelectedAppService.Tier, false);
                    var forests = status.PayloadToList<Forest>();
                    MainSideView.StackViewAsync(new AppLocationView(_baseModels, forests));
                }
                else
                {
                    MainSideView.StackViewAsync(new AppSummaryView(_baseModels));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonAppSettings_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                BaseModels.WizardMode = false;
                MainSideView.StackViewAsync(new AppBasicDetailView(BaseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonAppAudit_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.StackViewAsync(new AppAuditView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
    }
}
