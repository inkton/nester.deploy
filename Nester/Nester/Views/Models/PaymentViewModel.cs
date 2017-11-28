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
using System.Text;
using System.Threading.Tasks;

namespace Inkton.Nester.Views
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
            _editPaymentMethod.Owner = NesterControl.User;
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
            bool dCache = false, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                _editPaymentMethod, new Cloud.CachedHttpRequest<Admin.PaymentMethod>(
                    NesterControl.NesterService.QueryAsync), dCache, null, null);

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
                _editPaymentMethod, new Cloud.CachedHttpRequest<Admin.PaymentMethod>(
                    NesterControl.NesterService.CreateAsync), doCache, data);

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
