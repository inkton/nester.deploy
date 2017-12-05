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
    public partial class AppJoinDetailView : Inkton.Nester.Views.View
    {
        public AppJoinDetailView(Views.BaseModels baseModels)
        {
            _baseModels = baseModels;

            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                                ButtonDone,
                                ButtonMembership
                });

            if (_baseModels.AppViewModel.ContactModel.Invitations.Any())
            {
                _baseModels.AppViewModel.ContactModel.EditInvitation = _baseModels.AppViewModel.ContactModel.Invitations.First();
            }
            else
            {
                _baseModels.AppViewModel.ContactModel.EditInvitation = null;
            }

            BindingContext = _baseModels.AppViewModel;

            AppInviteList.Loaded += AppInviteList_Loaded;
            AppInviteList.SelectionChanged += AppInviteList_SelectionChanged;

            ButtonMembership.Clicked += ButtonMembership_ClickedAsync;
        }

        private void AppInviteList_Loaded(object sender, Syncfusion.ListView.XForms.ListViewLoadedEventArgs e)
        {
            if (_baseModels.AppViewModel.ContactModel.Invitations.Any())
            {
                AppInviteList.SelectedItem = _baseModels.AppViewModel.ContactModel.Invitations.First();
            }
        }

        private void AppInviteList_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            Admin.Invitation invitation = AppInviteList.SelectedItem as Admin.Invitation;

            if (invitation == null)
                return;

            ToggleMembershipButton(invitation);
        }

        private void ToggleMembershipButton(Admin.Invitation invitation)
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
                Admin.Invitation invitation = AppInviteList.SelectedItem as Admin.Invitation;

                if (invitation == null)
                    return;

                Admin.App searchApp = new Admin.App();
                searchApp.Tag = invitation.AppTag;

                Cloud.ServerStatus status = await _baseModels.AppViewModel.QueryAppAsync(
                    searchApp, false);

                if (status.Code != Cloud.Result.NEST_RESULT_SUCCESS)
                {
                    await DisplayAlert("Nester", "This app no longer exisit", "OK");
                    return;
                }
                else
                {
                    searchApp.Owner = NesterControl.User;

                    Admin.Contact myContact = new Admin.Contact();
                    Utils.Object.CopyPropertiesTo(invitation, myContact);
                    myContact.App = searchApp;
                    BaseModels baseModels = null;

                    status = await _baseModels.AppViewModel.ContactModel.UpdateContactAsync(myContact);
                    if (status.Code != Cloud.Result.NEST_RESULT_SUCCESS)
                    {
                        await DisplayAlert("Nester", "Could not confirm the invitation", "OK");
                        return;
                    }

                    Utils.Object.CopyPropertiesTo(myContact, invitation);
                    AppCollectionViewModel appCollection = NesterControl.BaseModels.AppViewModel as AppCollectionViewModel;

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

                    if (appCollection.AppModels.Any())
                    {
                        baseModels = new BaseModels(
                                _baseModels.AuthViewModel,
                                _baseModels.PaymentViewModel,
                                appCollection.AppModels.First());
                    }

                    NesterControl.ResetView(baseModels);

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
                ResetView();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
    }
}

