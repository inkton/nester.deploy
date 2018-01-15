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
using Inkton.Nester.Models;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class UserHistoryView : View
    {
        public UserHistoryView(BaseModels baseModels)
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                });

            _baseModels = baseModels;
            BindingContext = _baseModels.AuthViewModel;
        }

        protected async override void OnAppearing()
        {
            BindingContext = _baseModels.AuthViewModel;

            base.OnAppearing();

            await _baseModels.AuthViewModel.QueryUserEventsAsync(NesterControl.User);
        }

        async private void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await MainSideView.ResetViewAsync(_baseModels);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        async private void OnCloseButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await NesterControl.ResetViewAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}



