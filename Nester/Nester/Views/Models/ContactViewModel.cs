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

namespace Nester.Views
{
    public class ContactViewModel : ViewModel
    {
        private Admin.Contact _editContact;
        private Admin.Contact _ownerContact;

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
            _editInvitation.User = ThisUI.User;
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
                _editInvitation.User = ThisUI.User;

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
                        contact.UserId == ThisUI.User.Id)
                    {
                        _ownerContact = contact;
                        _editApp.OwnerCapabilities = contact.OwnerCapabilities;
                    }
                }

                _contacts.Remove(_ownerContact);
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryContactAsync(Admin.Contact contact,
            bool doCache = true, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                contact, new Cloud.NesterService.CachedHttpRequest<Admin.Contact>(
                    ThisUI.NesterService.QueryAsync), doCache, null, null);

            if (status.Code >= 0)
            {
                _editContact = status.PayloadToObject<Admin.Contact>();
                Utils.Object.PourPropertiesTo(_editContact, contact);
                await QueryPermissionsAsync(_editContact, throwIfError);
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> UpdateContactAsync(Admin.Contact contact,
            bool doCache = true, bool throwIfError = true)
        {
            bool leaving = contact.UserId != null;

            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                contact, new Cloud.NesterService.CachedHttpRequest<Admin.Contact>(
                    ThisUI.NesterService.UpdateAsync), doCache);

            if (status.Code >= 0)
            {
                _editContact = status.PayloadToObject<Admin.Contact>();
                Utils.Object.PourPropertiesTo(_editContact, contact);

                if (!leaving)
                {
                    await QueryPermissionsAsync(_editContact, throwIfError);
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> CreateContactAsync(Admin.Contact contact,
            bool doCache = true, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                contact, new Cloud.NesterService.CachedHttpRequest<Admin.Contact>(
                    ThisUI.NesterService.CreateAsync), doCache);

            if (status.Code >= 0)
            {
                _editContact = status.PayloadToObject<Admin.Contact>();
                _contacts.Add(_editContact);

                Utils.Object.PourPropertiesTo(_editContact, contact);

                await QueryPermissionsAsync(contact, throwIfError);
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> RemoveContactAsync(Admin.Contact contact,
            bool doCache = false, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                contact, new Cloud.NesterService.CachedHttpRequest<Admin.Contact>(
                    ThisUI.NesterService.RemoveAsync), doCache);

            if (status.Code >= 0)
            {
                _contacts.Remove(contact);
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> ReinviteContactAsync(Admin.Contact contact,
            bool doCache = true, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                contact, new Cloud.NesterService.CachedHttpRequest<Admin.Contact>(
                    ThisUI.NesterService.UpdateAsync), doCache);
            return status;
        }

        public async Task<Cloud.ServerStatus> QueryPermissionsAsync(Admin.Contact contact,
              bool doCache = true, bool throwIfError = true)
        {
            Auth.Permission seedPermission = new Auth.Permission();
            seedPermission.Contact = contact;
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

                contact.OwnerCapabilities = caps;
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> UpdatePermissionAsync(Admin.Contact contact,
            bool doCache = true, bool throwIfError = true)
        {
            Auth.Permission seedPermission = new Auth.Permission();
            seedPermission.Contact = contact;

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
                var capable = permSwitch.Getter(contact.OwnerCapabilities);
                seedPermission.AppPermissionTag = permSwitch.PermissionTag;

                if (capable)
                {
                    permission["app_permission_tag"] = seedPermission.AppPermissionTag;

                    status = await Cloud.Result.WaitForObjectAsync(false,
                        seedPermission, new Cloud.NesterService.CachedHttpRequest<Auth.Permission>(
                            ThisUI.NesterService.CreateAsync), doCache, permission);

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
                        seedPermission, new Cloud.NesterService.CachedHttpRequest<Auth.Permission>(
                            ThisUI.NesterService.RemoveAsync), doCache);

                    if (status.Code != Cloud.Result.NEST_RESULT_SUCCESS &&
                        status.Code != Cloud.Result.NEST_RESULT_ERROR_PERM_NFOUND)
                    {
                        break;
                    }
                }
            }

            return status;
        }
    }
}
