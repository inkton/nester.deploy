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

using Xamarin.Forms;

namespace Inkton.Nester.Views
{
    public partial class AuthView : Inkton.Nester.Views.View
    {
        public AuthView(Views.BaseModels baseModels)
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonDone
                });

            Password.Unfocused += Password_Unfocused;
            PasswordVerify.Unfocused += Password_Unfocused;

            _baseModels = baseModels;
            BindingContext = _baseModels.AuthViewModel;
        }

        private void Password_Unfocused(object sender, FocusEventArgs e)
        {
            Validate();
        }

        void Validate()
        {
            if (PasswordValidator != null)
            {
                _baseModels.AuthViewModel.Validated = (
                     PasswordValidator.IsValid &&
                     PasswordRepeatValidator.IsValid
                     );
                
                if (_baseModels.AuthViewModel.Validated)
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

        void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                IsServiceActive = true;

                await _baseModels.AuthViewModel.ResetTokenAsync();
                await DisplayAlert("Nester", "Password was saved", "OK");

                IsServiceActive = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
                IsServiceActive = false;
            }
        }

        async private void OnCloseButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                ResetView();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}
