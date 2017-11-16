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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using Nester.Admin;
using Nester.Cloud;
using Syncfusion.SfChart.XForms;

namespace Nester.Views
{
    public class LogViewModel : ViewModel
    {
        private INesterService _nester;
        private ObservableCollection<Admin.NestLog> _nestLogs;
        private ObservableCollection<Admin.DiskSpaceLog> _diskSpaceLogs;
        private ObservableCollection<Admin.SystemRAMLog> _systemRamLogs;
        private ObservableCollection<Admin.SystemCPULog> _systemCpuLogs;
        private ObservableCollection<Admin.SystemIPV4Log> _ipv4Logs;

        private Admin.NestLog _editNestLog;

        private MultiCategoryData _diskSpaceData;
        private MultiSeriesData _ramSeries;
        private MultiSeriesData _cpuSeries;
        private MultiSeriesData _ipv4Series;

        #region Data Classes

        public class DataSeries
        {
            protected ChartSeries _series;

            public DataSeries(ChartSeries series)
            {
                _series = series;
            }
        }

        public class MultiCategoryData : DataSeries
        {
            protected Admin.Log _dataLog;

            public MultiCategoryData(ChartSeries series)
                :base(series)
            {
            }

            public void AddLog(Admin.Log log)
            {
                _dataLog = log;
                _series.ItemsSource = log.Fields;
            }
        }

        public class MultiSeriesData
        {
            public class DataSeriesPoints : DataSeries
            {
                public class Point
                {
                    public DateTime Time { get; set; }
                    public double Value { get; set; }
                }

                public ObservableCollection<Point> Values;

                public DataSeriesPoints(ChartSeries series)
                    :base(series)
                {
                    Values = new ObservableCollection<Point>();
                    _series.ItemsSource = Values;
                }

                public void Clear()
                {
                    Values.Clear();
                }
            }

            private Dictionary<string, DataSeriesPoints> _namedSeries = 
                new Dictionary<string, DataSeriesPoints>();

            public MultiSeriesData()
            {
            }

            public Dictionary<string, DataSeriesPoints> NamedSeries
            {
                get
                {
                    return _namedSeries;
                }
            }

            public void Init(string name, ChartSeries series)
            {
                series.Label = name;
                series.EnableTooltip = true;
                _namedSeries[name] = new MultiSeriesData.DataSeriesPoints(series);
            }

            public void AddLog(Admin.Log log)
            {
                foreach (string key in log.Fields.Keys)
                {
                    if (_namedSeries.Keys.Contains(key))
                    {
                        if (_namedSeries[key].Values == null)
                        {
                            _namedSeries[key].Values = new ObservableCollection<DataSeriesPoints.Point>();
                        }

                        DataSeriesPoints.Point point = new DataSeriesPoints.Point();
                        point.Time = log.Time;
                        point.Value = Convert.ToDouble(log.Fields[key]);
                        _namedSeries[key].Values.Add(point);
                    }
                }
            }

            public void Clear()
            {
                foreach (DataSeriesPoints points in _namedSeries.Values)
                {
                    points.Clear();
                }
            }
        }

        #endregion

        public LogViewModel(Admin.App app) : base(app)
        {
            _nester = DependencyService.Get<Cloud.INesterService>(DependencyFetchTarget.NewInstance);
            _nester.BasicAuth = new BasicAuth(
                true, app.Tag, app.NetworkPassword);
            _nester.Endpoint = app.ApiEndpoint;
        }

        override public Admin.App EditApp
        {
            get
            {
                return _editApp;
            }
            set
            {
                SetProperty(ref _editApp, value);

                _nester.BasicAuth = new BasicAuth(
                    true, value.Tag, value.NetworkPassword);
                _nester.Endpoint = value.ApiEndpoint;
            }
        }

        public Admin.NestLog EditNestLog
        {
            get
            {
                return _editNestLog;
            }
            set
            {
                SetProperty(ref _editNestLog, value);
            }
        }

        public ObservableCollection<Admin.NestLog> NestLogs
        {
            get
            {
                return _nestLogs;
            }
        }

        public ObservableCollection<Admin.SystemCPULog> SystemCPULogs
        {
            get
            {
                return _systemCpuLogs;
            }
        }

        public ObservableCollection<Admin.DiskSpaceLog> DiskSpaceLogs
        {
            get
            {
                return _diskSpaceLogs;
            }
        }

        public ObservableCollection<Admin.SystemIPV4Log> SystemIPV4Logs
        {
            get
            {
                return _ipv4Logs;
            }
        }

        public ObservableCollection<Admin.SystemRAMLog> SystemRAMLogs
        {
            get
            {
                return _systemRamLogs;
            }
        }

        public void SetupDiskSpaceSeries(ChartSeries series)
        {
            _diskSpaceData = new MultiCategoryData(series);
        }

        public void SetupCPUSeries(
            ChartSeries user,
            ChartSeries system,
            ChartSeries irq,
            ChartSeries nice,
            ChartSeries iowait
            )
        {
            _cpuSeries = new MultiSeriesData();

            _cpuSeries.Init("User", user);
            _cpuSeries.Init("System", system);
            _cpuSeries.Init("IRQ", irq);
            _cpuSeries.Init("Nice", nice);
            _cpuSeries.Init("IOWait", iowait);
        }

        public void SetupRAMSeries(
            ChartSeries free,
            ChartSeries used,
            ChartSeries cached,
            ChartSeries buffers
            )
        {
            _ramSeries = new MultiSeriesData();

            _ramSeries.Init("Free", free);
            _ramSeries.Init("Used", used);
            _ramSeries.Init("Cached", cached);
            _ramSeries.Init("Buffers", buffers);
        }

        public void SetupIpv4Series(
            ChartSeries sent,
            ChartSeries received
            )
        {
            _ipv4Series = new MultiSeriesData();

            _ipv4Series.Init("Sent", sent);
            _ipv4Series.Init("Received", received);
        }

        public async Task<Cloud.ServerStatus> QueryNestLogsAsync(string filter = null,
            bool throwIfError = true)
        {
            string sql = "select * from nest_log";
            if (filter != null)
            {
                sql += " where " + filter;
            }
            sql += " limit 100";

            Cloud.ServerStatus status = await QueryLogsAsync<Admin.NestLog>(
                sql, throwIfError);

            _nestLogs = status.PayloadToList<Admin.NestLog>();

            OnPropertyChanged("NestLogs");
            return status;
        }

        public async Task<Cloud.ServerStatus> QuerySystemCPULogsAsync(string filter = null,
            bool throwIfError = true)
        {
            string sql = "select * from system_cpu";
            if (filter != null)
            {
                sql += " where " + filter;
            }
            sql += " limit 100";

            Cloud.ServerStatus status = await QueryLogsAsync<Admin.SystemCPULog>(
                sql, throwIfError);

            _systemCpuLogs = status.PayloadToList<Admin.SystemCPULog>();
            _cpuSeries.Clear();

            if (_systemCpuLogs.Any())
            {
                _systemCpuLogs.All(log => { _cpuSeries.AddLog(log); return true; });

                OnPropertyChanged("SystemCPULogs");
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryDiskSpaceLogsAsync(string filter = null,
            bool throwIfError = true)
        {
            string sql = "select * from disk_space";
            if (filter != null)
            {
                sql += " where " + filter;
            }
            sql += " limit 100";

            Cloud.ServerStatus status = await QueryLogsAsync<Admin.DiskSpaceLog>(
                sql, throwIfError);

            _diskSpaceLogs = status.PayloadToList<Admin.DiskSpaceLog>();

            if (_diskSpaceLogs.Any())
            {
                _diskSpaceData.AddLog(_diskSpaceLogs.Last());

                OnPropertyChanged("DiskSpaceLogs");
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QuerSystemIPV4LogsAsync(string filter = null,
            bool throwIfError = true)
        {
            string sql = "select * from system_ipv4";
            if (filter != null)
            {
                sql += " where " + filter;
            }
            sql += " limit 100";

            Cloud.ServerStatus status = await QueryLogsAsync<Admin.SystemIPV4Log>(
                sql, throwIfError);

            _ipv4Logs = status.PayloadToList<Admin.SystemIPV4Log>();
            _ipv4Series.Clear();

            if (_ipv4Logs.Any())
            {
                _ipv4Logs.All(log => { _ipv4Series.AddLog(log); return true; });

                OnPropertyChanged("SystemIPV4Logs");
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QuerSystemRAMLogsAsync(string filter = null,
            bool throwIfError = true)
        {
            string sql = "select * from system_ram";
            if (filter != null)
            {
                sql += " where " + filter;
            }
            sql += " limit 100";

            Cloud.ServerStatus status = await QueryLogsAsync<Admin.SystemRAMLog>(
                sql, throwIfError);

            _systemRamLogs = status.PayloadToList<Admin.SystemRAMLog>();
            _ramSeries.Clear();

            if (_systemRamLogs.Any())
            {
                _systemRamLogs.All(log => { _ramSeries.AddLog(log); return true; });

                OnPropertyChanged("SystemRAMLogs");
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryLogsAsync<T>(string sql,
            bool throwIfError = true) where T : Log, new()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("sql", sql);

            T logsSeed = new T();
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                _nester, throwIfError, logsSeed, false, data);

            if (status.Code < 0)
            {
                return status;
            }

            return status;
        }
    }
}
