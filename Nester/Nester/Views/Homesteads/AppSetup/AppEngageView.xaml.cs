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
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class AppEngageView : View
    {
        public AppEngageView(BaseModels baseModels)
        {
            _baseModels = baseModels;

            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                                ButtonCreate,
                                ButtonJoin
                });

            BindingContext = baseModels.TargetViewModel;
        }

        async private void OnCreateAppClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            _baseModels.WizardMode = true;
            AppBasicDetailView basicView = new AppBasicDetailView(_baseModels);
            basicView.MainSideView = MainSideView;
            MainSideView.Detail.Navigation.InsertPageBefore(basicView, this);
            await MainSideView.Detail.Navigation.PopAsync();

            IsServiceActive = false;
        }

        async private void OnJoinAppClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            await _baseModels.TargetViewModel.ContactModel.QueryInvitationsAsync();

            AppJoinDetailView joinView = new AppJoinDetailView(_baseModels);
            joinView.MainSideView = MainSideView;
            MainSideView.Detail.Navigation.InsertPageBefore(joinView, this);
            await MainSideView.Detail.Navigation.PopAsync();

            IsServiceActive = false;
        }
    }
}
