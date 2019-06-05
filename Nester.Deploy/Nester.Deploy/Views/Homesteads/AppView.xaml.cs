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
using System.IO;
using System.Net;
using Xamarin.Forms;
using Syncfusion.SfBusyIndicator.XForms;
using Inkton.Nest.Model;
using Inkton.Nester.ViewModels;
using System.Threading.Tasks;
using Inkton.Nest.Cloud;
using Inkton.Nester.Cloud;
using Inkton.Nester.Helpers;

namespace Inkton.Nester.Views
{
    public partial class AppView : View
    {
        public enum ViewStatus
        {
            Updating,
            WaitingDeployment,
            Deployed
        }

        private ViewStatus _status = ViewStatus.WaitingDeployment;

        public AppView(AppViewModel appViewModel)
        {
            InitializeComponent();

            AppViewModel = appViewModel;

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
            ButtonAppHide.Clicked += ButtonAppHide_ClickedAsync;
            ButtonAppShow.Clicked += ButtonAppShow_ClickedAsync;
            ButtonAppDeploy.Clicked += ButtonAppDeploy_ClickedAsync;
            ButtonAppView.Clicked += ButtonAppView_ClickedAsync;
            ButtonAppDownload.Clicked += ButtonAppDownload_ClickedAsync;
            ButtonGetAnalytics.Clicked += ButtonGetLogs_Clicked;
            ButtonAddToSlack.Clicked += ButtonAddToSlack_ClickedAsync;
            ButtonAppAudit.Clicked += ButtonAppAudit_ClickedAsync;

            NestLogs.ItemSelected += NestLogs_ItemSelected;

            MessagingCenter.Subscribe<App>(
                this, "Updated", (updateApp) => {
                    Device.BeginInvokeOnMainThread(() => {

                        System.Diagnostics.Debug.WriteLine(
                            string.Format("Notify arrived for app - {0} to {1}", 
                            updateApp.Tag, AppViewModel.EditApp.Tag));

                        if (updateApp.Tag == AppViewModel.EditApp.Tag)
                        {
                            // Set the backend address for querying logs and metrics
                            // The IP address is only available after an update recvd
                            AppViewModel.LogViewModel.ResetBackend();

                            UpdateStatus();

                            if (Status == ViewStatus.Deployed)
                            {
                                ReloadAnalytics();
                            }
                        }
                    });
                });

            SetRefresing();
        }

        public ViewStatus Status
        {
            get
            {
                return _status;
            }
        }

        public void SetRefresing()
        {
            System.Diagnostics.Debug.WriteLine(
                string.Format("Refreshing {0}", AppViewModel.EditApp.Tag));

            ProgressControl.Title = "Refreshing ...";
            ProgressControl.AnimationType = AnimationTypes.Rectangle;

            ProgressControl.IsVisible = true;
            Logo.IsVisible = false;
            InactiveApp.IsVisible = true;
            Metrics.IsVisible = false;

            AppViewModel.IsBusy = true;

            ToggleOperations();
        }

        public void UpdateStatus()
        {
            /* -> App Busy (Installing/Updating/Upgrading)
             * -> App Deployed
             *      Active - Working
             *      Not Active - Failed
             * -> App Not Deployed
             */

            if (AppViewModel.EditApp.IsBusy)
            {
                _status = ViewStatus.Updating;
            }
            else
            {
                if (AppViewModel.EditApp.IsDeployed)
                {
                    if (AppViewModel.EditApp.IsActive)
                    {
                        _status = ViewStatus.Deployed;
                    }
                    else
                    {
                        _status = ViewStatus.WaitingDeployment;
                    }
                }
                else
                {
                    _status = ViewStatus.WaitingDeployment;
                }

                System.Diagnostics.Debug.WriteLine(
                    string.Format("Set Status {0}, {1}", AppViewModel.EditApp.Tag, _status));

                ToggleOperations();
            }

            switch (_status)
            {
                case ViewStatus.Updating:
                    ProgressControl.Title = "Updating ...";
                    ProgressControl.AnimationType = AnimationTypes.Gear;

                    ProgressControl.IsVisible = true;
                    Logo.IsVisible = false;
                    InactiveApp.IsVisible = true;
                    Metrics.IsVisible = false;

                    AppViewModel.IsBusy = true;
                    break;

                case ViewStatus.Deployed:
                    // Deployed and active
                    InactiveApp.IsVisible = false;
                    Metrics.IsVisible = true;

                    AppViewModel.IsBusy = false;
                    break;

                case ViewStatus.WaitingDeployment:
                    ProgressControl.IsVisible = false;
                    Logo.IsVisible = true;
                    InactiveApp.IsVisible = true;
                    Metrics.IsVisible = false;

                    AppViewModel.IsBusy = false;
                    break;
            }

            ToggleOperations();
        }

        public void ToggleOperations()
        {
            // Actions availabe all the time
            ButtonNotifications.IsEnabled = true;

            // Actions when the app is not busy
            ButtonAppSettings.IsEnabled = AppViewModel.IsInteractive;
            ButtonAddToSlack.IsEnabled = AppViewModel.IsInteractive;
            ButtonAppDeploy.IsEnabled = AppViewModel.IsInteractive;

            // Actions when the app has been deployed - may not be active due to failure
            ButtonAppDepRemove.IsEnabled = AppViewModel.IsDeployed;
            ButtonAppHide.IsEnabled = AppViewModel.IsDeployed;
            ButtonAppShow.IsEnabled = AppViewModel.IsDeployed;

            // -Actions when the app is running and active
            ButtonAppRestore.IsEnabled = AppViewModel.IsActive;
            ButtonAppUpgrade.IsEnabled = AppViewModel.IsActive;
            ButtonAppDownload.IsEnabled = AppViewModel.IsActive;
            ButtonAppView.IsEnabled = AppViewModel.IsActive;
            ButtonAppAudit.IsEnabled = AppViewModel.IsActive;
        }

        private void ReloadAnalytics()
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

        private async Task GetLogsAsync(int maxRows = 10,
            bool doCache = false, bool throwIfError = true)
        {
            System.Diagnostics.Debug.WriteLine(
                string.Format("GetLogsAsync"));

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
                await AppViewModel
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

        private void NestLogs_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            NestLog nestLog = e.SelectedItem as NestLog;

            if (nestLog != null)
            {
                AppViewModel.LogViewModel.EditNestLog = nestLog;
                /* 
                    Message.Text = nestLog.Message;

                if (_AppViewModel.LogViewModel.DiskSpaceLogs != null)
                {
                    var diskspaceLog = _AppViewModel.LogViewModel.DiskSpaceLogs.FirstOrDefault(
                            x => x.Time >= nestLog.Time);
                    if (diskspaceLog != null)
                    {
                        FocusDisk.Text = string.Format("{0:P1}",
                            diskspaceLog.Used / (diskspaceLog.Used + diskspaceLog.Available + diskspaceLog.ReservedForRoot));
                    }
                }

                if (_AppViewModel.LogViewModel.SystemCPULogs != null)
                {
                    var cpuLog = _AppViewModel.LogViewModel.SystemCPULogs.FirstOrDefault(
                            x => x.Time >= nestLog.Time);
                    if (cpuLog != null)
                    {
                        FocusCPU.Text = string.Format("{0:P1}",
                            (cpuLog.User + cpuLog.System + cpuLog.IRQ + cpuLog.Nice + cpuLog.IOWait) / 100);
                    }
                }

                if (_AppViewModel.LogViewModel.SystemIPV4Logs != null)
                {
                    var ipv4Log = _AppViewModel.LogViewModel.SystemIPV4Logs.FirstOrDefault(
                            x => x.Time >= nestLog.Time);
                    if (ipv4Log != null)
                    {
                        FocusNet.Text = string.Format("{0:0}/{0:0}",
                             Math.Abs(ipv4Log.Sent / 1024),
                             Math.Abs(ipv4Log.Received / 1024));
                    }
                }

                if (_AppViewModel.LogViewModel.SystemRAMLogs != null)
                {
                    var ramLog = _AppViewModel.LogViewModel.SystemRAMLogs.FirstOrDefault(
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
                    await ErrorHandler.ExceptionAsync(this, "The begin time is after the end time");
                }
                else
                {
                    await GetLogsAsync();
                }
            }
            catch (Exception)
            {
                await ErrorHandler.ExceptionAsync(this, "Failed to retrieve metrics from the app " + AppViewModel.EditApp.Name);
            }
        }

        async private void ButtonAppDownload_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var filePath = Path.Combine(folder, string.Format("{0}.devkit", AppViewModel.EditApp.Tag));

                var alreadyExist = File.Exists(filePath);

                if (alreadyExist)
                {
                    var yes = await DisplayAlert("Nester", "A downloaded Devkit already exists here. Overwrite? \n" + filePath, "Yes", "No");

                    if (!yes)
                    {
                        return;
                    }

                    File.Delete(filePath);
                }

                Devkit devkit = new Devkit();
                devkit.OwnedBy = AppViewModel.ContactViewModel.OwnerContact;

                await AppViewModel.DeploymentViewModel.QueryDevkitAsync(devkit);

                File.WriteAllText(filePath, devkit.Script);

                await DisplayAlert("Nester", "The Devkit was emailed. A copy was downloaded here:\n" + filePath, "OK");

                Device.OpenUri(new Uri("file://" + folder.Replace("\\", "/")));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void ButtonNotifications_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await AppViewModel.QueryAppNotificationsAsync();
                await MainView.StackViewAsync(new NotificationView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void ButtonAddToSlack_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await MainView.StackViewAsync(
                    new WebView(WebView.Pages.AppSlackConnect, AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void ButtonAppView_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await MainView.StackViewAsync(
                    new WebView(WebView.Pages.AppPage, AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void ButtonAppUpgrade_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await AppViewModel
                    .ServicesViewModel
                    .SetAppUpgradingAsync();

                _wizardMode = false;
                await MainView.StackViewAsync(new AppTierView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void ButtonAppRestore_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await AppViewModel.DeploymentViewModel.QueryAppBackupsAsync();

                await MainView.StackViewAsync(new AppBackupView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
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
                    if (AppViewModel.EditApp.IsDeploymentValid)
                    {
                        await Process(AppViewModel.EditApp.Deployment, true,
                            AppViewModel.DeploymentViewModel.RemoveDeploymentAsync
                        );

                        await AppViewModel.QueryStatusAsync();

                        UpdateStatus();
                    }
                }
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }
        }

        async private void ButtonAppShow_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                var yes = await DisplayAlert("Nester", "This will remove the 'under construction' banner for the site.\nDo you want to continue?", "Yes", "No");

                if (!yes)
                {
                    return;
                }

                Deployment deployment =
                    AppViewModel
                        .DeploymentViewModel.Deployments.First();

                await AppViewModel
                    .DeploymentViewModel
                    .UpdateDeploymentAsync("show", deployment);
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }
        }

        async private void ButtonAppHide_ClickedAsync(object sender, EventArgs e)
        {
            try
            {
                var yes = await DisplayAlert("Nester", "This will place an 'under construction' banner on the site.\nDo you want to continue?", "Yes", "No");

                if (!yes)
                {
                    return;
                }

                Deployment deployment =
                    AppViewModel
                        .DeploymentViewModel.Deployments.First();

                await AppViewModel
                    .DeploymentViewModel
                    .UpdateDeploymentAsync("hide", deployment);
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }
        }

        async private void ButtonAppDeploy_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await AppViewModel
                    .ServicesViewModel
                    .SetAppUpgradingAsync(false);

                NestPlatform workerPlatform = AppViewModel.NestViewModel.Platforms.First(
                    x => x.Tag == "worker");

                var handlerNests = from nest in AppViewModel.NestViewModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (handlerNests.ToArray().Length == 0)
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "Please add a handler nest to process queries", "OK");
                    return;
                }

                await AppViewModel.DeploymentViewModel.CollectInfoAsync();

                if (!AppViewModel.DeploymentViewModel.Deployments.Any())
                {
                    ResultMultiple<Forest> result = await AppViewModel.QueryAppServiceTierLocationsAsync(
                        AppViewModel.ServicesViewModel.SelectedAppServiceTableItem.Tier, false);
                    await MainView.StackViewAsync(new AppLocationView(AppViewModel, result.Data.Payload));
                }
                else
                {
                    await MainView.StackViewAsync(new AppSummaryView(AppViewModel));
                }
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void ButtonAppSettings_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await AppViewModel
                    .ServicesViewModel
                    .SetAppUpgradingAsync(false);

                _wizardMode = false;
                await MainView.StackViewAsync(new AppBasicDetailView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void ButtonAppAudit_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await MainView.StackViewAsync(new AppAuditView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }
    }
}
