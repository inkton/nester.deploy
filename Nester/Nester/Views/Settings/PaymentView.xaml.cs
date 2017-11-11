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
