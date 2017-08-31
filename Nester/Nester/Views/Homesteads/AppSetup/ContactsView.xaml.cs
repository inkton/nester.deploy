using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Nester.Views
{
    public partial class ContactsView : Nester.Views.View
    {
        public ContactsView(AppViewModel appViewModel)
        {
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

            _appViewModel = appViewModel;

            BindingContext = _appViewModel.ContactModel;
            AppContactsList.SelectionChanged += AppContactsList_SelectionChanged;
        }

        async private void OnButtonBasicDetailsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                LoadView(new AppBasicDetailView(_appViewModel));
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
                LoadView(new AppDomainView(_appViewModel));
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
                LoadView(new AppNestsView(_appViewModel));
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

            _appViewModel.ContactModel.EditContact = new Admin.Contact();

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
            Utils.Object.CopyPropertiesTo(browseContact,
                _appViewModel.ContactModel.EditContact);
        }

        void Validate()
        {
            _appViewModel.ContactModel.Validated = false;
            _appViewModel.ContactModel.CanUpdate = false;

            if (_appViewModel != null &&
                _appViewModel.ContactModel != null)
            {
                /* used to enable the add function. a domain can
                 * be added only if valid fields and no list item 
                 * has been selected and currenly receivng focus.
                 */
                _appViewModel.ContactModel.Validated = (
                    EmailValidator.IsValid 
                );

                /* used to enable the update function. a domain can
                 * be updaed only if valid fields has been selected 
                 * and an item from a list is selected.
                 */
                _appViewModel.ContactModel.CanUpdate =
                    _appViewModel.ContactModel.Validated &&
                    AppContactsList.SelectedItem != null;
            }
        }

        //protected override void SubscribeToMessages()
        //{
        //    base.SubscribeToMessages();

        //    ProcessMessage<Admin.Contact>("re-invite",
        //        new Func<Admin.Contact, bool, Task<Cloud.ServerStatus>>(
        //        _appViewModel.ContactModel.ReinviteContact));


        //    MessagingCenter.Subscribe<ManagedObjectMessage<Admin.Contact>>(this, "remove", async (objMessage) =>
        //    {
        //        var yes = await DisplayAlert("Nester", "Would you like to remove this contact", "Yes", "No");

        //        if (yes)
        //        {
        //            try
        //            {
        //                await _appViewModel.ContactModel.RemoveContact(objMessage.Object);
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
                if (_appViewModel.ContactModel.Contacts.Any())
                {
                    Admin.Contact contact = _appViewModel.ContactModel.Contacts.First();
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
                var existContacts = from contact in _appViewModel.ContactModel.Contacts
                                    where ((Admin.Contact)contact).Email == NewContactEmail.Text
                                    select contact;
                if (existContacts.ToArray().Length > 0)
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The user with this email already exist", "OK");
                    return;
                }

                Admin.Contact newContact = new Admin.Contact();
                newContact.App = _appViewModel.EditApp;
                newContact.Email = NewContactEmail.Text;

                await _appViewModel.ContactModel.CreateContactAsync(newContact);

                Clear();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async void OnRefreshButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await Process(AppContactsList.SelectedItem as Admin.Contact, true,
                    _appViewModel.ContactModel.QueryContactAsync
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
                await _appViewModel.ContactModel.QueryContactsAsync();

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
                    _appViewModel.ContactModel.UpdatePermissionAsync
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
                    _appViewModel.ContactModel.RemoveContactAsync,
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
                if (_appViewModel.WizardMode)
                {
                    await _appViewModel.DomainModel.QueryDomainsAsync();

                    // if currently trvelling back and forth on the 
                    // app wizard - move to the next
                    await Navigation.PushAsync(new AppDomainView(_appViewModel));
                }
                else
                {
                    // Head back to homepage if the 
                    // page was called from here
                    LoadHomeView();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}
