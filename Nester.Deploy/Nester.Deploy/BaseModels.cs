using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Essentials;
using Xamarin.Forms;
using Newtonsoft.Json;
using Inkton.Nest.Model;
using Inkton.Nester.Cloud;
using Inkton.Nester.Storage;

namespace Inkton.Nester.ViewModels
{
    public class BaseViewModels
    {
        //private User _user;
        private AuthViewModel _authViewModel = null;
        private PaymentViewModel _paymentViewModel = null;
        private AppCollectionViewModel _appCollectionViewModel = null;

        const int ApiVersion = 2;
        private NesterService _platform;

        public BaseViewModels()
        {
            SetupPlatform();

            _authViewModel = new AuthViewModel(_platform);
            _paymentViewModel = new PaymentViewModel(_platform);
            _appCollectionViewModel = new AppCollectionViewModel(_platform);
        }

        public User User
        {
            get { return _platform.Permit.Owner; }
            set {
                _platform.Permit.Owner = value;
            }
        }

        public NesterService Platform
        {
            get { return _platform; }
            set { _platform = value; }
        }

        public AuthViewModel AuthViewModel
        {
            get { return _authViewModel; }
            set { _authViewModel = value; }
        }

        public PaymentViewModel PaymentViewModel
        {
            get { return _paymentViewModel; }
            set { _paymentViewModel = value; }
        }

        public AppCollectionViewModel AppCollectionViewModel
        {
            get { return _appCollectionViewModel; }
            set { _appCollectionViewModel = value; }
        }

        public void SetupPlatform()
        {
            StorageService cache = new StorageService(Path.Combine(
                    Path.GetTempPath(), "NesterCache-" + DateTime.Now.Ticks.ToString()));
            cache.Clear();

            var clientSignature = new
            {
                Model = DeviceInfo.Model,
                Manufacturer = DeviceInfo.Manufacturer,
                Name = DeviceInfo.Name,
                Platform = DeviceInfo.Platform,
                Idiom = DeviceInfo.Idiom,
                DeviceType = DeviceInfo.DeviceType,
                HardwareVersion = DeviceInfo.VersionString,
                SoftwareVersion = typeof(BaseViewModels).GetTypeInfo()
                    .Assembly.GetName().Version.ToString(),
                ApiVersion = ApiVersion
            };

            _platform = new NesterService(
                ApiVersion, JsonConvert.SerializeObject(clientSignature),
                cache);
        }
    }
}
