﻿/*
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

using Xamarin.Forms;

namespace Nester.Views
{
    public partial class PaymentView : Nester.Views.View
    {
        private Views.PaymentViewModel _paymentViewModel;

        public PaymentView(PaymentViewModel paymentViewModel)
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonNew,
                    ButtonReenterDone
                });

            ButtonAppMenu.Clicked += ButtonAppMenu_Clicked;

            _paymentViewModel = paymentViewModel;
            BindingContext = paymentViewModel;
        }

        void Validate()
        {
            if (_paymentViewModel != null)
            {
                _paymentViewModel.Validated = (
                        CardNumberValidator.IsValid &&
                        ExpMonthValidator.IsValid &&
                        ExpYearValidator.IsValid &&
                        CVVNumberValidator.IsValid
                        );
            }
        }

        public Views.PaymentViewModel PaymentViewModel
        {
            get { return _paymentViewModel; }
            set { _paymentViewModel = value; }
        }

        void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }

        void OnNewClicked(object sender, EventArgs e)
        {
            IsServiceActive = true;

            _paymentViewModel.DisplayPaymentMethodProof = false;
            _paymentViewModel.DisplayPaymentMethodEntry = true;

            IsServiceActive = false;
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _paymentViewModel.CreatePaymentMethodAsync(CardNumber.Text,
                    int.Parse(ExpMonth.Text), int.Parse(ExpYear.Text), CVVNumber.Text);

                if (Navigation.ModalStack.Count > 0)
                {
                    await Navigation.PopModalAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private void ButtonAppMenu_Clicked(object sender, EventArgs e)
        {
            _masterDetailPage.IsPresented = true;
        }

        async private void OnCloseButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                LoadHomeView();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}
