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
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Inkton.Nest.Model;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Helpers;
using DeployApp = Nester.Deploy.App;

namespace Inkton.Nester.Views
{
    public partial class ContactsView : View
    {
        public ContactsView(AppViewModel appViewModel)
        {
            InitializeComponent();

            AppViewModel = appViewModel;

            SetActivityMonotoring(ServiceActive, 
                new List<Xamarin.Forms.View> {
                    ButtonHome,
                    ButtonBasicDetails,
                    ButtonNests,
                    ButtonDomains,
                    ButtonAdd,
                    ButtonRemove,
                    ButtonDone,
                    SwitchCanViewApp,
                    SwitchCanUpdateApp,
                    SwitchCanDeleteApp,
                    SwitchCanCreateNest,
                    SwitchCanViewNest,
                    SwitchCanUpdateNest,
                    SwitchCanDeleteNest,
                });

            AppContactsList.ItemSelected += AppContactsList_ItemSelected;

            ButtonDone.IsVisible = _baseViewModels.WizardMode;
            if (_baseViewModels.WizardMode)
            {
                // hide but do not collapse
                TopButtonPanel.Opacity = 0;
            }
        }

        public override void UpdateBindings()
        {
            base.UpdateBindings();

            BindingContext = AppViewModel.ContactViewModel;
        }

        async private void OnButtonBasicDetailsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                BaseViewModels.WizardMode = false;
                MainSideView.CurrentLevelViewAsync(new AppBasicDetailView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnButtonServiceClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.CurrentLevelViewAsync(new AppTierView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnButtonDomainsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.CurrentLevelViewAsync(new AppDomainView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnButtonNestsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.CurrentLevelViewAsync(new AppNestsView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        private void AppContactsList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            SetDefaults();
            Validate();
        }

        private void Clear()
        {
            AppContactsList.SelectedItem = null;
            AppViewModel.ContactViewModel.EditContact = new Contact();

            SetDefaults();

            Validate();
        }

        private void SetDefaults()
        {
            if (AppContactsList.SelectedItem == null)
            {
                return;
            }

            Contact browseContact = AppContactsList.SelectedItem as Contact;
            Contact copy = new Contact();
            browseContact.CopyTo(copy);
            AppViewModel.ContactViewModel.EditContact = copy;
        }

        void Validate()
        {
            AppViewModel.ContactViewModel.Validated = false;
            AppViewModel.ContactViewModel.CanUpdate = false;

            if (EmailValidator != null)
            {
                /* used to enable the add function. a domain can
                 * be added only if valid fields and no list item 
                 * has been selected and currenly receivng focus.
                 */
                AppViewModel.ContactViewModel.Validated = (
                    EmailValidator.IsValid 
                );

                /* used to enable the update function. a domain can
                 * be updaed only if valid fields has been selected 
                 * and an item from a list is selected.
                 */
                AppViewModel.ContactViewModel.CanUpdate =
                    AppViewModel.ContactViewModel.Validated &&
                    AppContactsList.SelectedItem != null;
            }
        }

        //protected override void SubscribeToMessages()
        //{
        //    base.SubscribeToMessages();

        //    ProcessMessage<Contact>("re-invite",
        //        new Func<Contact, bool, Task<Cloud.ServerStatus>>(
        //        _AppViewModel.ContactViewModel.ReinviteContact));


        //    MessagingCenter.Subscribe<ManagedObjectMessage<Contact>>(this, "remove", async (objMessage) =>
        //    {
        //        var yes = await DisplayAlert("Nester", "Would you like to remove this contact", "Yes", "No");

        //        if (yes)
        //        {
        //            try
        //            {
        //                await _AppViewModel.ContactViewModel.RemoveContact(objMessage.Object);
        //            }
        //            catch (Exception ex)
        //            {
        //                await DisplayAlert("Nester", ex.Message, "OK");
        //            }
        //        }
        //    });
        //}

        //protected override void UnsubscribeFromMessages()
        //{
        //    base.UnsubscribeFromMessages();

        //    MessagingCenter.Unsubscribe<ManagedObjectMessage<Contact>>(this, "re-invite");
        //    MessagingCenter.Unsubscribe<ManagedObjectMessage<Contact>>(this, "remove");
        //}

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                if (AppViewModel.ContactViewModel.Contacts.Any())
                {
                    Contact contact = AppViewModel.ContactViewModel.Contacts.First();
                    AppContactsList.SelectedItem = contact;
                }

                SetDefaults();
                Validate();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }
        }
       
        void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }
        
        async void OnClearButtonClicked(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                Clear();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async void OnAddButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                var existContacts = from contact in AppViewModel.ContactViewModel.Contacts
                                    where ((Contact)contact).Email == NewContactEmail.Text
                                    select contact;
                if (existContacts.ToArray().Length > 0)
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The user with this email already exist", "OK");
                    return;
                }

                Contact newContact = new Contact();
                newContact.OwnedBy = AppViewModel.EditApp;
                newContact.Email = NewContactEmail.Text;

                await AppViewModel.ContactViewModel.CreateContactAsync(newContact);

                Clear();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        //async void OnSyncDiscordButtonClickedAsync(object sender, EventArgs e)
        //{
        //    IsServiceActive = true;

        //    try
        //    {
        //        Contact editContact = AppContactsList.SelectedItem as Contact;

        //        if (editContact.Status != "active")
        //        {
        //            IsServiceActive = false;
        //            await DisplayAlert("Nester", "The user hasn't accepted the invitation yet", "OK");
        //            return;
        //        }
        //        OnSyncDiscordButtonClickedAsync

        //       await Process(AppContactsList.SelectedItem as Contact, false,
        //            _AppViewModel.ContactViewModel.UpdateContactDiscordAsync
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Nester", ex.Message, "OK");
        //    }

        //    IsServiceActive = false;
        //}

        async void OnRefreshButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await Process(AppContactsList.SelectedItem as Contact, true,
                    AppViewModel.ContactViewModel.QueryContactAsync
                );

                SetDefaults();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }
        
        async void OnRefreshAllButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await AppViewModel
                    .ContactViewModel
                    .QueryContactsAsync();

                SetDefaults();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async void OnUpdateButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                Contact browseContact = AppContactsList.SelectedItem as Contact;

                if (browseContact.OwnerCapabilities.CanCreateNest ||
                    browseContact.OwnerCapabilities.CanDeleteNest ||
                    browseContact.OwnerCapabilities.CanUpdateNest ||
                    browseContact.OwnerCapabilities.CanViewNest)
                {
                    // if nest operations are permitted then
                    // the app must me at least viewable
                    browseContact.OwnerCapabilities.CanViewApp = true;
                }

                browseContact.Email = NewContactEmail.Text;

                await Process(browseContact, true,
                    AppViewModel
                        .ContactViewModel
                        .UpdatePermissionAsync
                );

                SetDefaults();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async void OnRemoveButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await Process(AppContactsList.SelectedItem as Contact, true,
                    AppViewModel.ContactViewModel.RemoveContactAsync,
                       new Func<Contact, Task<bool>>(
                            async (obj) =>
                            {
                                return await DisplayAlert("Nester", "Would you like to remove this contact", "Yes", "No");
                            }
                        )
                );

                Clear();
            }   
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (_baseViewModels.WizardMode)
                {
                    // Pop this to go to Homeview <->
                    foreach (var page in MainSideView.Detail.Navigation.NavigationStack.ToList())
                    {
                        if (!(page is ContactsView))
                        {
                            MainSideView.Detail.Navigation.RemovePage(page);
                        }
                    }

                    await MainSideView.Detail.Navigation.PopAsync();
                    _baseViewModels.WizardMode = false;

                    ((DeployApp)Application.Current).RefreshView();

                }
                else
                {
                    // Head back to homepage if the 
                    // page was called from here
                    MainSideView.UnstackViewAsync();
                }
            } 
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }
        }
    }
}
