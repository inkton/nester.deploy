using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inkton.Nester.Views
{
    public class BaseModels
    {
        private Views.AuthViewModel _authViewModel = null;
        private Views.AppViewModel _appViewModel = null;
        private Views.PaymentViewModel _paymentViewModel = null;

        protected bool _wizardMode = false;

        public BaseModels(
            Views.AuthViewModel authViewModel = null,
            Views.PaymentViewModel paymentViewModel = null,
            Views.AppViewModel appViewModel = null)
        {
            _authViewModel = authViewModel;
            _paymentViewModel = paymentViewModel;
            _appViewModel = appViewModel;
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

        public bool WizardMode
        {
            get { return _wizardMode; }
            set { _wizardMode = value; }
        }
    }
}
