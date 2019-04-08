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
using Xamarin.Forms;
using Inkton.Nest.Model;
using Inkton.Nest.Cloud;
using Inkton.Nester.Cloud;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Helpers;

namespace Inkton.Nester.Views
{
    public partial class ExitView : View
	{
        public ExitView(BaseViewModels baseModels)
        {
            InitializeComponent();

            _baseViewModels = baseModels;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonDone
                });

            Message.Text = "Click 'Close Account' to close the account.\n\nAll private details will be removed from the database and remaining credit balance refunded.";
            BindingContext = _baseViewModels.AuthViewModel;
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                IsServiceActive = true;

                var yes = await DisplayAlert("Nester", "This will permenently close the account, proceed ?", "Yes", "No");

                if (yes)
                {
                    ResultSingle<User> result = await _baseViewModels.AuthViewModel.DeleteUserAsync();
                    await DisplayAlert("Nester", new ResultHandler<User>(result).GetMessage(), "OK");
                    await MainSideView.Detail.Navigation.PopAsync();
                } 
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
                IsServiceActive = false;
            }
        }

        async private void OnCloseButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await MainSideView.Detail.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}