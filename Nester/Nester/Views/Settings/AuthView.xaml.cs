﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Nester.Views
{
    public partial class AuthView : Nester.Views.View
    {
        public AuthView()
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonDone
                });
        }

        void Validate()
        {
            if (PasswordValidator != null)
            {
                _authViewModel.Validated = (
                     PasswordValidator.IsValid &&
                     PasswordRepeatValidator.IsValid
                     );
                
                if (_authViewModel.Validated)
                {
                    if (Password.Text != PasswordVerify.Text)
                    {
                        PasswordValidator.Message = "The passwords do not match";
                        PasswordValidator.IsValid = false;

                        PasswordRepeatValidator.Message = "The passwords do not match";
                        PasswordRepeatValidator.IsValid = false;
                    }
                }
            }
        }

        protected override void OnAppearing()
        {
            BindingContext = _authViewModel;

            base.OnAppearing();
            Validate();
        }

        void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                IsServiceActive = true;

                await _authViewModel.ResetTokenAsync();
                await DisplayAlert("Nester", "Password was saved", "OK");

                IsServiceActive = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
                IsServiceActive = false;
            }
        }
    }
}