using System;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Inkton.Nester
{
    public static class Settings
    {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string EnvironmentKey = "env";
        private static readonly string EnvironmentKeyDefault = "production";

        private const string AppStatusRefreshIntervalKey = "appStatusRefreshInterval";
        private static readonly Int64 AppStatusRefreshIntervalKDefault = 5;

        #endregion


        public static string Environment
        {
            get
            {
                return AppSettings.GetValueOrDefault(EnvironmentKey, EnvironmentKeyDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(EnvironmentKey, value);
            }
        }

        public static Int64 AppStatusRefreshInterval
        {
            get
            {
                return AppSettings.GetValueOrDefault(AppStatusRefreshIntervalKey, AppStatusRefreshIntervalKDefault);
            }
            set
            {
                AppSettings.AddOrUpdateValue(AppStatusRefreshIntervalKey, value);
            }
        }
    }
}
