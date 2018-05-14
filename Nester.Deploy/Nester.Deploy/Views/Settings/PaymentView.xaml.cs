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
using System.Threading.Tasks;
using Inkton.Nester.Models;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class PaymentView : View
    {
        public PaymentView(BaseModels baseModels)
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonNew,
                    ButtonReenterDone,
                    BillingCycle
                });

            _baseModels = baseModels;
            BindingContext = _baseModels.PaymentViewModel;

            BillingCycle.SelectedIndex = 0;
            UpdateBillingInfoAsync();

            BillingCycle.SelectedIndexChanged += BillingCycle_SelectedIndexChanged;
        }

        private void BillingCycle_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateBillingInfoAsync();
        }

        private async void UpdateBillingInfoAsync()
        {
            BillingCycle cycle = BillingCycle.SelectedItem as BillingCycle;

            if (cycle != null)
            {
                Dictionary<string, string> filter = new Dictionary<string, string>();
                filter.Add("billing_cycle_id", cycle.Id.ToString());
                await _baseModels.PaymentViewModel.QueryUserBillingTasksAsync(filter);
            }
        }

        void Validate()
        {
            if (CardNumberValidator != null)
            {
                _baseModels.PaymentViewModel.Validated = (
                        CardNumberValidator.IsValid &&
                        ExpMonthValidator.IsValid &&
                        ExpYearValidator.IsValid &&
                        CVVNumberValidator.IsValid
                        );
            }
        }

        void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }

        void OnNewClicked(object sender, EventArgs e)
        {
            IsServiceActive = true;

            _baseModels.PaymentViewModel.DisplayPaymentMethodProof = false;
            _baseModels.PaymentViewModel.DisplayPaymentMethodEntry = true;

            IsServiceActive = false;
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _baseModels.PaymentViewModel.CreatePaymentMethodAsync(CardNumber.Text,
                    int.Parse(ExpMonth.Text), int.Parse(ExpYear.Text), CVVNumber.Text);

                MainSideView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnCloseButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                MainSideView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}
