using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Newtonsoft.Json;
using Inkton.Nest.Model;
using Inkton.Nester.Cloud;
using Inkton.Nester.Storage;

namespace Inkton.Nester.ViewModels
{
    public class BaseViewModels
    {
        private User _user;
        private AuthViewModel _authViewModel = null;
        private PaymentViewModel _paymentViewModel = null;
        private AppViewModel _appViewModel = null;
        private AppCollectionViewModel _appCollectionViewModel = null;

        private NesterService _platform;
        protected bool _wizardMode = false;

        public BaseViewModels()
        {
            SetupPlatform();

            _authViewModel = new AuthViewModel(_platform);
            _paymentViewModel = new PaymentViewModel(_platform);
            _appCollectionViewModel = new AppCollectionViewModel(_platform);
        }

        public BaseViewModels(
            AuthViewModel authViewModel,
            PaymentViewModel paymentViewModel,
            AppViewModel newAppModel)
        {
            SetupPlatform();

            _authViewModel = authViewModel;
            _paymentViewModel = paymentViewModel;

            _appCollectionViewModel = new AppCollectionViewModel(_platform);
            _appCollectionViewModel.AddModel(newAppModel);

            ResetApp();
        }

        public BaseViewModels(BaseViewModels other)
        {
            _platform = other._platform;

            _authViewModel = other._authViewModel;
            _paymentViewModel = other._paymentViewModel;
            _appCollectionViewModel = other._appCollectionViewModel;

            ResetApp();
        }


        public User User
        {
            get { return _user; }
            set {
                _user = value;
                if (PaymentViewModel != null && PaymentViewModel.EditPaymentMethod != null)
                    PaymentViewModel.EditPaymentMethod.OwnedBy = _user;
                if (AppCollectionViewModel != null)
                    AppCollectionViewModel.ResetOwner(_user);
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

        public AppViewModel AppViewModel
        {
            get { return _appViewModel; }
            set { _appViewModel = value; }
        }

        public AppCollectionViewModel AppCollectionViewModel
        {
            get { return _appCollectionViewModel; }
            set { _appCollectionViewModel = value; }
        }

        public bool WizardMode
        {
            get { return _wizardMode; }
            set { _wizardMode = value; }
        }

        public void ResetApp()
        {
            _appViewModel = _appCollectionViewModel
                .AppModels.FirstOrDefault();
        }

        public void SetupPlatform()
        {
            StorageService cache = new StorageService(Path.Combine(
                    Path.GetTempPath(), "NesterCache-" + DateTime.Now.Ticks.ToString()));
            cache.Clear();

            INesterClient client = Application.Current as INesterClient;
            _platform = new NesterService(
                client.ApiVersion, client.Signature, cache);
        }
    }
}
