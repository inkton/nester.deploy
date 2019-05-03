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
using Xamarin.Forms;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Helpers;
using DeployApp = Nester.Deploy.App;

namespace Inkton.Nester.Views
{
    public partial class AppEngageView : View
    {
        public AppEngageView(AppViewModel appViewModel)
        {
            InitializeComponent();

            AppViewModel = appViewModel;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                                ButtonCreate,
                                ButtonJoin,
                                ButtonSkip
                });
        }

        async private void OnCreateAppClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                _baseViewModels.WizardMode = true;
                AppBasicDetailView basicView = new AppBasicDetailView(AppViewModel);
                basicView.MainSideView = MainSideView;
                MainSideView.Detail.Navigation.InsertPageBefore(basicView, this);
                await MainSideView.Detail.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnJoinAppClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                ContactViewModel contactsModel = new ContactViewModel(BaseViewModels.Platform, null);
                contactsModel.EditInvitation.OwnedBy = BaseViewModels.Platform.Permit.Owner;
                await contactsModel.QueryInvitationsAsync();

                AppJoinDetailView joinView = new AppJoinDetailView(contactsModel);
                joinView.MainSideView = MainSideView;
                MainSideView.Detail.Navigation.InsertPageBefore(joinView, this);
                await MainSideView.Detail.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnSkipButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                ((DeployApp)Application.Current).RefreshView();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }
    }
}
