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
using Inkton.Nest;
using Inkton.Nest.Model;
using Inkton.Nester.ViewModels;
using Inkton.Nest.Cloud;
using Inkton.Nester.Helpers;
using DeployApp = Nester.Deploy.App;

namespace Inkton.Nester.Views
{
    public partial class AppJoinDetailView : View
    {
        ContactViewModel _contactsModel;

        public AppJoinDetailView(ContactViewModel contactsModel)
        {
            InitializeComponent();

            _contactsModel = contactsModel;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                                ButtonDone,
                                ButtonMembership
                });

            BindingContext = _contactsModel;

            AppInviteList.ItemSelected += AppInviteList_ItemSelected;

            ButtonMembership.Clicked += ButtonMembership_ClickedAsync;

            if (_contactsModel.Invitations.Any())
            {
                Invitation invitation = _contactsModel.Invitations.First();
                AppInviteList.SelectedItem = invitation;
                ToggleMembershipButton(invitation);
            }
        }

        private void AppInviteList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            Invitation invitation = e.SelectedItem as Invitation;

            if (invitation == null)
                return;

            ToggleMembershipButton(invitation);
        }

        private void ToggleMembershipButton(Invitation invitation)
        {
            if (invitation.Status == "active")
            {
                ButtonMembership.Text = "Leave";
            }
            else
            {
                ButtonMembership.Text = "Join";
            }
        }

        async private void ButtonMembership_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                Invitation invitation = AppInviteList.SelectedItem as Invitation;

                if (invitation == null)
                    return;

                AppViewModel appModel = BaseViewModels.AppCollectionViewModel.AppModels.Where(
                        m => m.EditApp.Tag == invitation.AppTag).FirstOrDefault(); ;

                App joinApp;

                if (appModel != null)
                {
                    joinApp = appModel.EditApp;
                }
                else
                {
                    App searchApp = new App();
                    searchApp.Tag = invitation.AppTag;

                    appModel = new AppViewModel(_contactsModel.Platform);
                    ResultSingle<App> appResult = await appModel.QueryAppAsync(
                        searchApp, false);

                    if (appResult.Code != Cloud.ServerStatus.NEST_RESULT_SUCCESS)
                    {
                        await ErrorHandler.ExceptionAsync(this, "This app no longer exisit");
                        return;
                    }

                    joinApp = searchApp;
                }

                Contact myContact = new Contact();
                invitation.CopyTo(myContact);

                joinApp.OwnedBy = BaseViewModels.Platform.Permit.Owner;
                myContact.OwnedBy = joinApp;

                ResultSingle<Contact> contactResult = await appModel
                    .ContactViewModel.UpdateContactAsync(myContact);

                if (contactResult.Code != Cloud.ServerStatus.NEST_RESULT_SUCCESS)
                {
                    await ErrorHandler.ExceptionAsync(this, "Could not confirm the invitation");
                    return;
                }

                contactResult.Data.Payload.CopyTo(invitation);
                AppCollectionViewModel appCollection = BaseViewModels.AppCollectionViewModel;

                if (invitation.Status == "active")
                {
                    appCollection.AddApp(joinApp);
                }
                else
                {
                    foreach (AppViewModel appRemoveeModel in appCollection.AppModels)
                    {
                        if (appRemoveeModel.EditApp.Id == joinApp.Id)
                        {
                            appCollection.RemoveApp(appRemoveeModel);
                            break;
                        }
                    }
                }

                ToggleMembershipButton(invitation);
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await ((DeployApp)Application.Current).RefreshViewAsync();

                await MainView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }
    }
}

