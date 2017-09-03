using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Syncfusion.SfChart.XForms;
using System.Collections.ObjectModel;
using Nester.Admin;

namespace Nester.Views
{
    public partial class AppView : Nester.Views.View
    {
        public AppView(AppViewModel appViewModel)
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                });

            _appViewModel = appViewModel;
            _appViewModel.WizardMode = false;

            if (_appViewModel.EditApp != null)
            {
                Title = _appViewModel.EditApp.Name;
            }
            else
            {
                Title = "Hello World";
            }

            BindingContext = _appViewModel;

            DateTime endTime = DateTime.Now.ToUniversalTime();
            DateTime startTime = endTime.Subtract(new TimeSpan(0, 60, 0));

            StartTime.Time = new TimeSpan(startTime.Hour, startTime.Minute, startTime.Second);
            EndTime.Time = new TimeSpan(endTime.Hour, endTime.Minute, endTime.Second);

            ButtonAppSettings.Clicked += ButtonAppSettings_ClickedAsync;
            ButtonNotifications.Clicked += ButtonNotifications_ClickedAsync;
            ButtonAppDepRemove.Clicked += ButtonAppDepRemove_ClickedAsync;
            ButtonAppDeploy.Clicked += ButtonAppDeploy_ClickedAsync;
            ButtonAppView.Clicked += ButtonAppView_ClickedAsync;
            ButtonAppDownload.Clicked += ButtonAppDownload_ClickedAsync;
            ButtonAppMenu.Clicked += ButtonAppMenu_Clicked;
            ButtonGetAnalytics.Clicked += ButtonGetAnalytics_Clicked;

            NestLogs.SelectionChanged += NestLogs_SelectionChanged;

            _appViewModel.LogViewModel.SetupDiskSpaceSeries(
                DiskSpaceData.Series[0]
                );

            _appViewModel.LogViewModel.SetupRAMSeries(
                RamData.Series[0],
                RamData.Series[1],
                RamData.Series[2],
                RamData.Series[3]
                );

            _appViewModel.LogViewModel.SetupCPUSeries(
                CpuData.Series[0],
                CpuData.Series[1],
                CpuData.Series[2],
                CpuData.Series[3],
                CpuData.Series[4]
                );

            _appViewModel.LogViewModel.SetupIpv4Series(
                Ipv4Data.Series[0],
                Ipv4Data.Series[1]);
        }

        async public void UpdateAsync()
        {
            try
            {
                Cloud.ServerStatus status;

                status = await _appViewModel.InitAsync();
                if (status.Code < 0)
                {
                    await DisplayAlert("Nester", "Failed to get an update of " + App.Name, "OK");
                }

                OnPropertyChanged("App");
            }
            catch (Exception)
            {   
                // The connection may fail
            }
        }

        public async void GetAnalyticsAsync()
        {
            DateTime now = DateTime.Now, unixEpoch = new DateTime(1970, 1, 1);
            DateTime nowUTC = now.ToUniversalTime();

            long beginId = (long)(new DateTime(
                    nowUTC.Year, nowUTC.Month, nowUTC.Day,
                    StartTime.Time.Hours,
                    StartTime.Time.Minutes,
                    StartTime.Time.Seconds) - unixEpoch).TotalMilliseconds;
            long endId = (long)(new DateTime(
                    nowUTC.Year, nowUTC.Month, nowUTC.Day,
                    EndTime.Time.Hours,
                    EndTime.Time.Minutes,
                    EndTime.Time.Seconds) - unixEpoch).TotalMilliseconds;

            if (beginId >= endId)
            {
                await DisplayAlert("Nester", "Make sure begin time is less than end time", "OK");
                return;
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            _appViewModel.LogViewModel.QueryNestLogsAsync(
                string.Format("id >= {0} and id < {1}",
                        beginId, endId
                    ));

            beginId /= 1000;
            endId /= 1000;

            string filter = string.Format("id >= {0} and id < {1}",
                        beginId, endId
                    );

            _appViewModel.LogViewModel.QuerySystemCPULogsAsync(filter);

            _appViewModel.LogViewModel.QueryDiskSpaceLogsAsync(filter);

            _appViewModel.LogViewModel.QuerSystemIPV4LogsAsync(filter);

            _appViewModel.LogViewModel.QuerSystemRAMLogsAsync(filter);

#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void NestLogs_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            NestLog nestLog = e.AddedItems.FirstOrDefault() as NestLog;

            if (nestLog != null)
            {
                _appViewModel.LogViewModel.EditNestLog = nestLog;
                Message.Text = nestLog.Message;
                
                if (_appViewModel.LogViewModel.DiskSpaceLogs != null)
                {
                    var diskspaceLog = _appViewModel.LogViewModel.DiskSpaceLogs.FirstOrDefault(
                            x => x.Time >= nestLog.Time);
                    if (diskspaceLog != null)
                    {
                        FocusDisk.Text = string.Format("{0:P1}",
                            diskspaceLog.Used / (diskspaceLog.Used + diskspaceLog.Available + diskspaceLog.ReservedForRoot));
                    }
                }

                if (_appViewModel.LogViewModel.SystemCPULogs != null)
                {
                    var cpuLog = _appViewModel.LogViewModel.SystemCPULogs.FirstOrDefault(
                            x => x.Time >= nestLog.Time);
                    if (cpuLog != null)
                    {
                        FocusCPU.Text = string.Format("{0:P1}",
                            (cpuLog.User + cpuLog.System + cpuLog.IRQ + cpuLog.Nice + cpuLog.IOWait) / 100);
                    }
                }

                if (_appViewModel.LogViewModel.SystemIPV4Logs != null)
                {
                    var ipv4Log = _appViewModel.LogViewModel.SystemIPV4Logs.FirstOrDefault(
                            x => x.Time >= nestLog.Time);
                    if (ipv4Log != null)
                    {
                        FocusNet.Text = string.Format("{0:0}/{0:0}",
                             Math.Abs(ipv4Log.Sent / 1024),
                             Math.Abs(ipv4Log.Received / 1024));
                    }
                }

                if (_appViewModel.LogViewModel.SystemRAMLogs != null)
                {
                    var ramLog = _appViewModel.LogViewModel.SystemRAMLogs.FirstOrDefault(
                             x => x.Time >= nestLog.Time);
                    if (ramLog != null)
                    {
                        FocusRAM.Text = string.Format("{0:P1}",
                            ramLog.Used / (ramLog.Used + ramLog.Cached + ramLog.Buffers));
                    }
                }
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
                devkit.Contact = _appViewModel.ContactModel.OwnerContact;

                await _appViewModel.DeploymentModel.QueryDevkitAsync(devkit);

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
                await _appViewModel.QueryAppNotificationsAsync();

                await Navigation.PushAsync(new NotificationView(_appViewModel));
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
                await Navigation.PushAsync(new AppWebView(_appViewModel));
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
                    if (_appViewModel.EditApp.Deployment != null)
                    {
                        await Process(_appViewModel.EditApp.Deployment, true,
                            _appViewModel.DeploymentModel.RemoveDeploymentAsync
                        );

                        await _appViewModel.InitAsync();
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

        private void ButtonAppMenu_Clicked(object sender, EventArgs e)
        {
            _masterDetailPage.IsPresented = true;
        }
    }
}
