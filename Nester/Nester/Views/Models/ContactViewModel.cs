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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Inkton.Nester.Views
{
    public class ContactViewModel : ViewModel
    {
        private Admin.Contact _editContact;
        private Admin.Contact _ownerContact;
        private Admin.Collaboration _collaboration;

        private ObservableCollection<Admin.Contact> _contacts;

        private Admin.Invitation _editInvitation;

        private ObservableCollection<Admin.Invitation> _invitations;

        struct PermissionSwitch
        {
            public PermissionSwitch(Expression<Func<Auth.OwnerCapabilities, bool>> check, string permissionTag)
            {
                Check = check;
                PermissionTag = permissionTag;
            }

            public Func<Auth.OwnerCapabilities, bool> Getter
            {
                get
                {
                    return Check.Compile();
                }
            }

            public Expression<Func<Auth.OwnerCapabilities, bool>> Check;
            public string PermissionTag;
        }

        public ContactViewModel(Admin.App app) : base(app)
        {
            _contacts = new ObservableCollection<Admin.Contact>();
            _editContact = new Admin.Contact();
            _editContact.App = app;

            _invitations = new ObservableCollection<Admin.Invitation>();
            _editInvitation = new Admin.Invitation();
            _editInvitation.User = NesterControl.User;

            _collaboration = new Admin.Collaboration();
            _collaboration.Contact = _editContact;
        }

        override public Admin.App EditApp
        {
            get
            {
                return _editApp;
            }
            set
            {
                _editContact.App = value; 
                _editInvitation.User = NesterControl.User;

                SetProperty(ref _editApp, value);
            }
        }

        public Admin.Contact EditContact    
        {
            get
            {
                return _editContact;
            }
            set
            {
                SetProperty(ref _editContact, value);
                _collaboration.Contact = value;
                OnPropertyChanged("EditContact.OwnerCapabilities");
            }
        }

        public Admin.Invitation EditInvitation
        {
            get
            {
                return _editInvitation;
            }
            set
            {
                SetProperty(ref _editInvitation, value);
            }
        }

        public Admin.Contact OwnerContact
        {
            get
            {
                return _ownerContact;
            }
            set
            {
                SetProperty(ref _ownerContact, value);
            }
        }

        public Admin.Collaboration Collaboration
        {
            get
            {
                return _collaboration;
            }
            set
            {
                _collaboration.Contact = _editContact;
                SetProperty(ref _collaboration, value);
            }
        }

        public int ContactsCount
        {
            get
            {
                return _contacts.Count();
            }
        }

        public ObservableCollection<Admin.Contact> Contacts
        {
            get
            {
                return _contacts;
            }
            set
            {
                SetProperty(ref _contacts, value);
            }
        }

        public ObservableCollection<Admin.Invitation> Invitations
        {
            get
            {
                return _invitations;
            }
            set
            {
                SetProperty(ref _invitations, value);
            }
        }

        override public async Task<Cloud.ServerStatus> InitAsync()
        {
            Cloud.ServerStatus status;

            status = await QueryContactsAsync();
            if (status.Code < 0)
            {
                return status;
            }
            
            return status;
        }

        public async Task<Cloud.ServerStatus> QueryInvitationsAsync(
            bool doCache = false, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                throwIfError, _editInvitation, doCache);

            if (status.Code >= 0)
            {
                _invitations = status.PayloadToList<Admin.Invitation>();
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryContactsAsync(
            bool doCache = false, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                throwIfError, _editContact, doCache);

            if (status.Code >= 0)
            {
                _contacts = status.PayloadToList<Admin.Contact>();
                _editApp.OwnerCapabilities = null;

                /* The owner is to be treated differently
                 * The owner contact does not need invitation
                 * for example.
                 */
                foreach (Admin.Contact contact in _contacts)
                {
                    contact.App = _editContact.App;
                    await QueryPermissionsAsync(contact, throwIfError);

                    if (contact.UserId != null &&
                        contact.UserId == NesterControl.User.Id)
                    {
                        _ownerContact = contact;
                        _editContact = contact;
                        _collaboration.Contact = contact;
                        _editApp.OwnerCapabilities = contact.OwnerCapabilities;
                    }
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryContactAsync(Admin.Contact contact = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.Contact theContact = contact == null ? _editContact : contact;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theContact, new Cloud.CachedHttpRequest<Admin.Contact>(
                    NesterControl.NesterService.QueryAsync), doCache, null, null);

            if (status.Code >= 0)
            {
                _editContact = status.PayloadToObject<Admin.Contact>();

                if (contact != null)
                {
                    Utils.Object.PourPropertiesTo(_editContact, contact);
                }

                await QueryPermissionsAsync(_editContact, throwIfError);
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> UpdateContactAsync(Admin.Contact contact = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.Contact theContact = contact == null ? _editContact : contact;
            bool leaving = theContact.UserId != null;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theContact, new Cloud.CachedHttpRequest<Admin.Contact>(
                    NesterControl.NesterService.UpdateAsync), doCache);

            if (status.Code >= 0)
            {
                _editContact = status.PayloadToObject<Admin.Contact>();

                if (contact != null)
                {
                    Utils.Object.PourPropertiesTo(_editContact, contact);
                }

                if (!leaving)
                {
                    await QueryPermissionsAsync(_editContact, throwIfError);
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> CreateContactAsync(Admin.Contact contact = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.Contact theContact = contact == null ? _editContact : contact;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theContact, new Cloud.CachedHttpRequest<Admin.Contact>(
                    NesterControl.NesterService.CreateAsync), doCache);

            if (status.Code >= 0)
            {
                _editContact = status.PayloadToObject<Admin.Contact>();

                if (contact != null)
                {
                    Utils.Object.PourPropertiesTo(_editContact, contact);
                    _contacts.Add(_editContact);
                }

                await QueryPermissionsAsync(contact, throwIfError);
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> RemoveContactAsync(Admin.Contact contact = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.Contact theContact = contact == null ? _editContact : contact;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theContact, new Cloud.CachedHttpRequest<Admin.Contact>(
                    NesterControl.NesterService.RemoveAsync), doCache);

            if (status.Code >= 0)
            {
                _contacts.Remove(theContact);
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> ReinviteContactAsync(Admin.Contact contact = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.Contact theContact = contact == null ? _editContact : contact;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theContact, new Cloud.CachedHttpRequest<Admin.Contact>(
                    NesterControl.NesterService.UpdateAsync), doCache);
            return status;
        }

        public async Task<Cloud.ServerStatus> QueryPermissionsAsync(Admin.Contact contact = null,
              bool doCache = false, bool throwIfError = true)
        {
            Admin.Contact theContact = contact == null ? _editContact : contact;
            Auth.Permission seedPermission = new Auth.Permission();
            seedPermission.Contact = theContact;
            ObservableCollection<Auth.Permission> permissions = new ObservableCollection<Auth.Permission>();

            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                 throwIfError, seedPermission, doCache);

            if (status.Code >= 0)
            {
                permissions = status.PayloadToList<Auth.Permission>();
                Auth.OwnerCapabilities caps = new Auth.OwnerCapabilities();
                caps.Reset();

                foreach (Auth.Permission permission in permissions)
                {
                    switch (permission.AppPermissionTag)
                    {
                        case "view-app": caps.CanViewApp = true; break;
                        case "update-app": caps.CanUpdateApp = true; break;
                        case "delete-app": caps.CanDeleteApp = true; break;

                        case "create-nest": caps.CanCreateNest = true; break;
                        case "update-nest": caps.CanUpdateNest = true; break;
                        case "delete-nest": caps.CanDeleteNest = true; break;
                        case "view-nest": caps.CanViewNest = true; break;
                    }
                }

                _editContact.OwnerCapabilities = caps;

                if (contact != null)
                {
                    Utils.Object.PourPropertiesTo(_editContact, contact);
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> UpdatePermissionAsync(Admin.Contact contact = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.Contact theContact = contact == null ? _editContact : contact;
            Auth.Permission seedPermission = new Auth.Permission();
            seedPermission.Contact = theContact;

            PermissionSwitch[] switches = new PermissionSwitch[] {
                new PermissionSwitch(caps => caps.CanViewApp, "view-app"),
                new PermissionSwitch(caps => caps.CanUpdateApp, "update-app"),
                new PermissionSwitch(caps => caps.CanUpdateApp, "delete-app"),
                new PermissionSwitch(caps => caps.CanCreateNest, "create-nest"),
                new PermissionSwitch(caps => caps.CanUpdateNest, "update-nest"),
                new PermissionSwitch(caps => caps.CanDeleteNest, "delete-nest"),
                new PermissionSwitch(caps => caps.CanViewNest, "view-nest")
            };

            Cloud.ServerStatus status = new Cloud.ServerStatus(0);
            Dictionary<string, string> permission = new Dictionary<string, string>();
             
            foreach (PermissionSwitch permSwitch in switches)
            {
                var capable = permSwitch.Getter(theContact.OwnerCapabilities);
                seedPermission.AppPermissionTag = permSwitch.PermissionTag;

                if (capable)
                {
                    permission["app_permission_tag"] = seedPermission.AppPermissionTag;

                    status = await Cloud.Result.WaitForObjectAsync(false,
                        seedPermission, new Cloud.CachedHttpRequest<Auth.Permission>(
                            NesterControl.NesterService.CreateAsync), doCache, permission);

                    if (status.Code != Cloud.Result.NEST_RESULT_SUCCESS &&
                        status.Code != Cloud.Result.NEST_RESULT_ERROR_PERM_FOUND)
                    {
                        break;
                    }

                    permission.Remove("app_permission_tag");
                }
                else
                {
                    status = await Cloud.Result.WaitForObjectAsync(false,
                        seedPermission, new Cloud.CachedHttpRequest<Auth.Permission>(
                            NesterControl.NesterService.RemoveAsync), doCache);

                    if (status.Code != Cloud.Result.NEST_RESULT_SUCCESS &&
                        status.Code != Cloud.Result.NEST_RESULT_ERROR_PERM_NFOUND)
                    {
                        break;
                    }
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryContactCollaborateAccountAsync(Admin.Collaboration collaboration = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.Collaboration theCollaboration = collaboration == null ? _collaboration : collaboration;
            theCollaboration.AccountId = "0";

            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theCollaboration, new Cloud.CachedHttpRequest<Admin.Collaboration>(
                    NesterControl.NesterService.QueryAsync), doCache, null, null);

            if (status.Code >= 0)
            {
                _collaboration = status.PayloadToObject<Admin.Collaboration>();
                Utils.Object.PourPropertiesTo(_collaboration, theCollaboration);
            }

            return status;
        }
    }
}
