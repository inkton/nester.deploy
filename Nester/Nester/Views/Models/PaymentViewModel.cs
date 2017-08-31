using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nester.Views
{
    public class PaymentViewModel : ViewModel
    {
        private Admin.PaymentMethod _editPaymentMethod;
        private bool _displayPaymentMethodProof = false;
        private bool _displayPaymentMethodEntry = true;
        private string _paymentMethodProofDetail;

        public PaymentViewModel()
        {
            _editPaymentMethod = new Admin.PaymentMethod();
            _editPaymentMethod.Owner = ThisUI.User;
        }

        public Admin.PaymentMethod PaymentMethod
        {
            get
            {
                return _editPaymentMethod;
            }
        }

        public bool DisplayPaymentMethodProof
        {
            get { return _displayPaymentMethodProof; }
            set { SetProperty(ref _displayPaymentMethodProof, value); }
        }

        public bool DisplayPaymentMethodEntry
        {
            get { return _displayPaymentMethodEntry; }
            set { SetProperty(ref _displayPaymentMethodEntry, value); }
        }

        public string PaymentMethodProofDetail
        {
            get { return _paymentMethodProofDetail; }
            set { SetProperty(ref _paymentMethodProofDetail, value); }
        }

        override public async Task<Cloud.ServerStatus> InitAsync()
        {
            return await QueryPaymentMethodAsync(false, false);
        }

        public async Task<Cloud.ServerStatus> QueryPaymentMethodAsync(
            bool dCache = true, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                _editPaymentMethod, new Cloud.NesterService.CachedHttpRequest<Admin.PaymentMethod>(
                    ThisUI.NesterService.QueryAsync), dCache, null, null);

            if (status.Code >= 0)
            {                
                _editPaymentMethod = status.PayloadToObject<Admin.PaymentMethod>();
                DisplayPaymentMethodProof = _editPaymentMethod.Proof != null;

                if (DisplayPaymentMethodProof)
                {
                    DisplayPaymentMethodEntry = false;

                    PaymentMethodProofDetail = string.Format("{0}\nLast 4 Digits {1}\nExpiry {2}/{3}",
                                    _editPaymentMethod.Proof.Brand,
                                    _editPaymentMethod.Proof.Last4,
                                    _editPaymentMethod.Proof.ExpMonth, _editPaymentMethod.Proof.ExpYear);
                }
                else
                {
                    DisplayPaymentMethodEntry = true;

                    PaymentMethodProofDetail = "";
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> CreatePaymentMethodAsync(
            string cardNumber, int expiryMonth, int expiryYear, string cvc, 
            bool doCache = false, bool throwIfError = true)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("type", "cc");
            data.Add("number", cardNumber);
            data.Add("exp_month", expiryMonth.ToString());
            data.Add("exp_year", expiryYear.ToString());
            data.Add("cvc", cvc);

            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                _editPaymentMethod, new Cloud.NesterService.CachedHttpRequest<Admin.PaymentMethod>(
                    ThisUI.NesterService.CreateAsync), doCache, data);

            if (status.Code >= 0)
            {
                _editPaymentMethod = status.PayloadToObject<Admin.PaymentMethod>();
                DisplayPaymentMethodProof = _editPaymentMethod.Proof != null;

                if (DisplayPaymentMethodProof)
                {
                    DisplayPaymentMethodEntry = false;

                    PaymentMethodProofDetail = string.Format("{0}\nLast 4 Digits {1}\nExpiry {2}/{3}",
                                    _editPaymentMethod.Proof.Brand,
                                    _editPaymentMethod.Proof.Last4,
                                    _editPaymentMethod.Proof.ExpMonth, _editPaymentMethod.Proof.ExpYear);
                }
                else
                {
                    DisplayPaymentMethodEntry = true;

                    PaymentMethodProofDetail = "";
                }
            }

            return status;
        }
    }

}
