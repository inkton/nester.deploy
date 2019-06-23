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

using System.Collections.Generic;
using Syncfusion.SfBusyIndicator.XForms;

namespace Inkton.Nester.Views
{
    public partial class BannerView : View
    {
        public BannerView()
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View>
                {
                });

            ProgressControl.IsVisible = false;

            if (BaseViewModels.AuthViewModel.IsAuthenticated)
            {
                ButtonAuthenticate.Text = "Logout";
            }
            else
            {
                ButtonAuthenticate.Text = "Login";
            }

            ButtonAuthenticate.Clicked += ButtonAuthenticate_ClickedAsync;
        }

        private async void ButtonAuthenticate_ClickedAsync(object sender, System.EventArgs e)
        {
            await MainView.LogoutAsync();
        }

        public string Text
        {
            set
            {
                ProgressControl.Title = value;
            }
            get
            {
                return ProgressControl.Title;
            }
        }

        public bool ShowProgress
        {
            set
            {
                ProgressControl.IsVisible = value;
            }
            get
            {
                return ProgressControl.IsVisible;
            }
        }

        public AnimationTypes AnimationType
        {
            set
            {
                ProgressControl.AnimationType = value;
            }
            get
            {
                return ProgressControl.AnimationType;
            }
        }
    }
}
