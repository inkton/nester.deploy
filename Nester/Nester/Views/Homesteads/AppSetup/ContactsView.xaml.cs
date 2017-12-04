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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Inkton.Nester.Views
{
    public partial class ContactsView : Inkton.Nester.Views.View
    {
        public ContactsView(Views.BaseModels baseModels)
        {
            _baseModels = baseModels;

            InitializeComponent();

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

            BindingContext = _baseModels.AppViewModel.ContactModel;
            AppContactsList.SelectionChanged += AppContactsList_SelectionChanged;

            ButtonDone.IsVisible = _baseModels.WizardMode;
            if (_baseModels.WizardMode)
            {
                // hide but do not collapse
                TopButtonPanel.Opacity = 0;
            }
        }

        async private void OnButtonBasicDetailsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.LoadView(new AppBasicDetailView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnButtonDomainsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _baseModels.AppViewModel.DomainModel.InitAsync();

                MainSideView.LoadView(new AppDomainView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnButtonNestsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _baseModels.AppViewModel.NestModel.InitAsync();

                MainSideView.LoadView(new AppNestsView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private void AppContactsList_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            SetDefaults();
            Validate();
        }

        private void Clear()
        {
            if (AppContactsList.SelectedItems.Any())
            {
                AppContactsList.SelectedItems.RemoveAt(0);
            }

            _baseModels.AppViewModel.ContactModel.EditContact = new Admin.Contact();

            SetDefaults();

            Validate();
        }

        private void SetDefaults()
        {
            if (AppContactsList.SelectedItem == null)
            {
                return;
            }

            Admin.Contact browseContact = AppContactsList.SelectedItem as Admin.Contact;
            Admin.Contact copy = new Admin.Contact();
            Utils.Object.CopyPropertiesTo(browseContact, copy);
            _baseModels.AppViewModel.ContactModel.EditContact = copy;
        }

        void Validate()
        {
            _baseModels.AppViewModel.ContactModel.Validated = false;
            _baseModels.AppViewModel.ContactModel.CanUpdate = false;

            if (EmailValidator != null)
            {
                /* used to enable the add function. a domain can
                 * be added only if valid fields and no list item 
                 * has been selected and currenly receivng focus.
                 */
                _baseModels.AppViewModel.ContactModel.Validated = (
                    EmailValidator.IsValid 
                );

                /* used to enable the update function. a domain can
                 * be updaed only if valid fields has been selected 
                 * and an item from a list is selected.
                 */
                _baseModels.AppViewModel.ContactModel.CanUpdate =
                    _baseModels.AppViewModel.ContactModel.Validated &&
                    AppContactsList.SelectedItem != null;
            }
        }

        //protected override void SubscribeToMessages()
        //{
        //    base.SubscribeToMessages();

        //    ProcessMessage<Admin.Contact>("re-invite",
        //        new Func<Admin.Contact, bool, Task<Cloud.ServerStatus>>(
        //        _baseModels.AppViewModel.ContactModel.ReinviteContact));


        //    MessagingCenter.Subscribe<ManagedObjectMessage<Admin.Contact>>(this, "remove", async (objMessage) =>
        //    {
        //        var yes = await DisplayAlert("Nester", "Would you like to remove this contact", "Yes", "No");

        //        if (yes)
        //        {
        //            try
        //            {
        //                await _baseModels.AppViewModel.ContactModel.RemoveContact(objMessage.Object);
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

        //    MessagingCenter.Unsubscribe<ManagedObjectMessage<Admin.Contact>>(this, "re-invite");
        //    MessagingCenter.Unsubscribe<ManagedObjectMessage<Admin.Contact>>(this, "remove");
        //}

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                if (_baseModels.AppViewModel.ContactModel.Contacts.Any())
                {
                    Admin.Contact contact = _baseModels.AppViewModel.ContactModel.Contacts.First();
                    AppContactsList.SelectedItem = contact;
                }

                SetDefaults();
                Validate();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
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
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async void OnAddButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                var existContacts = from contact in _baseModels.AppViewModel.ContactModel.Contacts
                                    where ((Admin.Contact)contact).Email == NewContactEmail.Text
                                    select contact;
                if (existContacts.ToArray().Length > 0)
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The user with this email already exist", "OK");
                    return;
                }

                Admin.Contact newContact = new Admin.Contact();
                newContact.App = _baseModels.AppViewModel.EditApp;
                newContact.Email = NewContactEmail.Text;

                await _baseModels.AppViewModel.ContactModel.CreateContactAsync(newContact);

                Clear();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        //async void OnSyncDiscordButtonClickedAsync(object sender, EventArgs e)
        //{
        //    IsServiceActive = true;

        //    try
        //    {
        //        Admin.Contact editContact = AppContactsList.SelectedItem as Admin.Contact;

        //        if (editContact.Status != "active")
        //        {
        //            IsServiceActive = false;
        //            await DisplayAlert("Nester", "The user hasn't accepted the invitation yet", "OK");
        //            return;
        //        }
        //        OnSyncDiscordButtonClickedAsync

        //       await Process(AppContactsList.SelectedItem as Admin.Contact, false,
        //            _baseModels.AppViewModel.ContactModel.UpdateContactDiscordAsync
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
                await Process(AppContactsList.SelectedItem as Admin.Contact, true,
                    _baseModels.AppViewModel.ContactModel.QueryContactAsync
                );

                SetDefaults();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
        
        async void OnRefreshAllButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _baseModels.AppViewModel.ContactModel.QueryContactsAsync();

                SetDefaults();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async void OnUpdateButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                Admin.Contact browseContact = AppContactsList.SelectedItem as Admin.Contact;
                browseContact.Email = NewContactEmail.Text;

                await Process(browseContact, true,
                    _baseModels.AppViewModel.ContactModel.UpdatePermissionAsync
                );

                SetDefaults();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async void OnRemoveButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await Process(AppContactsList.SelectedItem as Admin.Contact, true,
                    _baseModels.AppViewModel.ContactModel.RemoveContactAsync,
                       new Func<Admin.Contact, Task<bool>>(
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
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (_baseModels.WizardMode)
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
                    _baseModels.WizardMode = false;
                    MainSideView.ResetView(_baseModels);
                }
                else
                {
                    // Head back to homepage if the 
                    // page was called from here
                    ResetView();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}
