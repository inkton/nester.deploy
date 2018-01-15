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
using Inkton.Nester.Models;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class AppJoinDetailView : View
    {
        public AppJoinDetailView(BaseModels baseModels)
        {
            _baseModels = baseModels;

            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                                ButtonDone,
                                ButtonMembership
                });

            if (_baseModels.TargetViewModel.ContactModel.Invitations.Any())
            {
                _baseModels.TargetViewModel.ContactModel.EditInvitation = _baseModels.TargetViewModel.ContactModel.Invitations.First();
            }
            else
            {
                _baseModels.TargetViewModel.ContactModel.EditInvitation = null;
            }

            BindingContext = _baseModels.TargetViewModel;

            AppInviteList.Loaded += AppInviteList_Loaded;
            AppInviteList.SelectionChanged += AppInviteList_SelectionChanged;

            ButtonMembership.Clicked += ButtonMembership_ClickedAsync;
        }

        private void AppInviteList_Loaded(object sender, Syncfusion.ListView.XForms.ListViewLoadedEventArgs e)
        {
            if (_baseModels.TargetViewModel.ContactModel.Invitations.Any())
            {
                AppInviteList.SelectedItem = _baseModels.TargetViewModel.ContactModel.Invitations.First();
            }
        }

        private void AppInviteList_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            Invitation invitation = AppInviteList.SelectedItem as Invitation;

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

                App searchApp = new App();
                searchApp.Tag = invitation.AppTag;

                Cloud.ServerStatus status = await _baseModels.TargetViewModel.QueryAppAsync(
                    searchApp, false);

                if (status.Code != Cloud.ServerStatus.NEST_RESULT_SUCCESS)
                {
                    await DisplayAlert("Nester", "This app no longer exisit", "OK");
                    return;
                }
                else
                {
                    searchApp.Owner = NesterControl.User;

                    Contact myContact = new Contact();
                    Cloud.Object.CopyPropertiesTo(invitation, myContact);
                    myContact.App = searchApp;

                    status = await _baseModels.TargetViewModel.ContactModel.UpdateContactAsync(myContact);
                    if (status.Code != Cloud.ServerStatus.NEST_RESULT_SUCCESS)
                    {
                        await DisplayAlert("Nester", "Could not confirm the invitation", "OK");
                        return;
                    }

                    Cloud.Object.CopyPropertiesTo(myContact, invitation);
                    AppCollectionViewModel appCollection = NesterControl.BaseModels.AllApps;

                    if (invitation.Status == "active")
                    {
                        await appCollection.AddAppAsync(searchApp);
                    }
                    else
                    {
                        foreach (AppViewModel appModel in appCollection.AppModels)
                        {
                            if (appModel.EditApp.Id == searchApp.Id)
                            {
                                appCollection.RemoveApp(appModel);
                                break;
                            }
                        }
                    }

                    await NesterControl.ResetViewAsync(
                        NesterControl.BaseModels.AllApps.AppModels.FirstOrDefault());

                    ToggleMembershipButton(invitation);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await NesterControl.ResetViewAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
    }
}

