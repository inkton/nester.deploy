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

        public AppView(BaseViewModels baseModels)
        {
            InitializeComponent();

            ViewModels = baseModels;

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

            MessagingCenter.Subscribe<AppViewModel, App>(
                baseModels.AppViewModel, "Updated", (sender, updateApp) => {
                    Device.BeginInvokeOnMainThread(() => {

                        System.Diagnostics.Debug.WriteLine(
                            string.Format("MessagingCenter @ AppView - {0}", App.Tag));

                        if (updateApp.Id == App.Id)
                        {
                            // Set the backend address for querying logs and metrics
                            NesterControl.Backend.Endpoint = string.Format(
                                    "https://{0}/", App.Hostname);
                            NesterControl.Backend.BasicAuth = new Inkton.Nester.Cloud.BasicAuth(true,
                                    App.Tag, App.NetworkPassword);

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
                string.Format("Refreshing {0}", App.Tag));

            ProgressControl.Title = "Refreshing ...";
            ProgressControl.AnimationType = AnimationTypes.Rectangle;

            ProgressControl.IsVisible = true;
            Logo.IsVisible = false;
            InactiveApp.IsVisible = true;
            Metrics.IsVisible = false;
        }

        public void UpdateStatus()
        {
            /* -> App Busy (Installing/Updating/Upgrading)
             * -> App Deployed
             *      Active - Working
             *      Not Active - Failed
             * -> App Not Deployed
             */

            ButtonAppDeploy.IsEnabled = !App.IsBusy;
            ButtonAppRestore.IsEnabled = App.IsActive;
            ButtonAppDepRemove.IsEnabled = App.IsDeployed;
            ButtonAppHide.IsEnabled = App.IsDeployed;
            ButtonAppShow.IsEnabled = App.IsDeployed;
            ButtonAppDownload.IsEnabled = App.IsActive;
            ButtonAppView.IsEnabled = App.IsActive;
            ButtonAppAudit.IsEnabled = App.IsActive;
            ButtonAppUpgrade.IsEnabled = App.IsActive;

            if (App.IsBusy)
            {
                _status = ViewStatus.Updating;
            }
            else
            {
                if (App.IsDeployed)
                {
                    if (App.IsActive)
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
                    string.Format("Set Status {0}, {1}", App.Tag, _status));
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
                    break;

                case ViewStatus.Deployed:
                    // Deployed and active
                    InactiveApp.IsVisible = false;
                    Metrics.IsVisible = true;
                    break;

                case ViewStatus.WaitingDeployment:
                    ProgressControl.IsVisible = false;
                    Logo.IsVisible = true;
                    InactiveApp.IsVisible = true;
                    Metrics.IsVisible = false;
                    break;
            }
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
                await _baseViewModels.AppViewModel
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
                _baseViewModels.AppViewModel.LogViewModel.EditNestLog = nestLog;
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
                await DisplayAlert("Nester", "Failed to retrieve metrics from the app " + App.Name, "OK");
            }
        }

        async private void ButtonAppDownload_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var filePath = Path.Combine(folder, string.Format("{0}.devkit", App.Tag));

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
                devkit.OwnedBy = _baseViewModels.AppViewModel.ContactViewModel.OwnerContact;

                await _baseViewModels.AppViewModel.DeploymentViewModel.QueryDevkitAsync(devkit);

                File.WriteAllText(filePath, devkit.Script);

                await DisplayAlert("Nester", "The Devkit was emailed. A copy was downloaded here:\n" + filePath, "OK");

                Device.OpenUri(new Uri("file://" + folder.Replace("\\", "/")));
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
                await _baseViewModels.AppViewModel.QueryAppNotificationsAsync();
                MainSideView.StackViewAsync(new NotificationView(_baseViewModels));
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
                MainSideView.StackViewAsync(new AppWebView(_baseViewModels,
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
                MainSideView.StackViewAsync(new AppWebView(_baseViewModels, 
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
                await _baseViewModels.AppViewModel
                    .ServicesViewModel
                    .SetAppUpgradingAsync();

                _baseViewModels.WizardMode = false;
                MainSideView.StackViewAsync(new AppTierView(_baseViewModels));
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
                await _baseViewModels.AppViewModel.DeploymentViewModel.QueryAppBackupsAsync();

                MainSideView.StackViewAsync(new AppBackupView(_baseViewModels));
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
                    if (App.IsDeploymentValid)
                    {
                        await Process(App.Deployment, true,
                            _baseViewModels.AppViewModel.DeploymentViewModel.RemoveDeploymentAsync
                        );

                        await _baseViewModels.AppViewModel.QueryStatusAsync();

                        UpdateStatus();
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
                    _baseViewModels.AppViewModel
                        .DeploymentViewModel.Deployments.First();

                await _baseViewModels
                    .AppViewModel
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
                    _baseViewModels.AppViewModel
                        .DeploymentViewModel.Deployments.First();

                await _baseViewModels
                    .AppViewModel
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
                await _baseViewModels.AppViewModel
                    .ServicesViewModel
                    .SetAppUpgradingAsync(false);

                NestPlatform workerPlatform = _baseViewModels.AppViewModel.NestViewModel.Platforms.First(
                    x => x.Tag == "worker");

                var handlerNests = from nest in _baseViewModels.AppViewModel.NestViewModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (handlerNests.ToArray().Length == 0)
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "Please add a handler nest to process queries", "OK");
                    return;
                }

                await _baseViewModels.AppViewModel.DeploymentViewModel.CollectInfoAsync();

                if (!_baseViewModels.AppViewModel.DeploymentViewModel.Deployments.Any())
                {
                    ResultMultiple<Forest> result = await _baseViewModels.AppViewModel.QueryAppServiceTierLocationsAsync(
                        _baseViewModels.AppViewModel.ServicesViewModel.SelectedAppServiceTableItem.Tier, false);
                    MainSideView.StackViewAsync(new AppLocationView(_baseViewModels, result.Data.Payload));
                }
                else
                {
                    MainSideView.StackViewAsync(new AppSummaryView(_baseViewModels));
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
                await _baseViewModels.AppViewModel
                    .ServicesViewModel
                    .SetAppUpgradingAsync(false);

                ViewModels.WizardMode = false;
                MainSideView.StackViewAsync(new AppBasicDetailView(ViewModels));
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
                MainSideView.StackViewAsync(new AppAuditView(_baseViewModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
    }
}
