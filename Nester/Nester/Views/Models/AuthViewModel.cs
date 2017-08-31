using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Threading;

namespace Nester.Views
{
    public class AuthViewModel : ViewModel
    {
        private Auth.Permit _permit;
        private ObservableCollection<Admin.UserEvent> _userEvents;
        private bool _canRecoverPassword = false;

        public AuthViewModel()
        {
            _permit = new Auth.Permit();
            _permit.Owner = ThisUI.User;

            _userEvents = new ObservableCollection<Admin.UserEvent>();
        }

        public Auth.Permit Permit
        {
            get { return _permit; }
            set { SetProperty(ref _permit, value); }
        }

        public bool CanRecoverPassword
        {
            get { return _canRecoverPassword; }
            set { SetProperty(ref _canRecoverPassword, value); }
        }

        public void Reset()
        {
            ThisUI.NesterService.ClearSession();

            _permit.SecurityCode = null;
            _permit.Token = null;
        }

        public async Task<Cloud.ServerStatus> SignupAsync(
            bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Task<Cloud.ServerStatus>.Run(
                     () => ThisUI.NesterService.SignupAsync(_permit));

            if (status.Code < 0 && throwIfError)
            {
                Cloud.Result.ThrowError(status);
            }

            Utils.Object.CopyPropertiesTo(_permit.Owner, ThisUI.User);
            return status;
        }

        public async Task<Cloud.ServerStatus> RecoverPasswordAsync(
            bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Task<Cloud.ServerStatus>.Run(
                     () => ThisUI.NesterService.RecoverPasswordAsync(_permit));

            if (status.Code < 0 && throwIfError)
            {
                Cloud.Result.ThrowError(status);
            }

            Utils.Object.CopyPropertiesTo(_permit.Owner, ThisUI.User);
            return status;
        }

        public async Task<Cloud.ServerStatus> QueryTokenAsync(
            bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Task<Cloud.ServerStatus>.Run(
                     () => ThisUI.NesterService.QueryTokenAsync(_permit));

            if (status.Code < 0 && throwIfError)
            {
                Cloud.Result.ThrowError(status);
            }

            Utils.Object.CopyPropertiesTo(_permit.Owner, ThisUI.User);
            return status;
        }

        public async Task<Cloud.ServerStatus> ResetTokenAsync(
            bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Task<Cloud.ServerStatus>.Run(
                     () => ThisUI.NesterService.ResetTokenAsync(_permit));

            if (status.Code < 0 && throwIfError)
            {
                Cloud.Result.ThrowError(status);
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> UpdateUserAsync(Admin.User user,
            bool doCache = false, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                user, new Cloud.NesterService.CachedHttpRequest<Admin.User>(
                    ThisUI.NesterService.UpdateAsync), doCache);

            if (status.Code >= 0)
            {
                Utils.Object.PourPropertiesTo(status.PayloadToObject<Admin.User>(), user);
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryUserEventsAsync(Admin.User user,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.UserEvent userEventSeed = new Admin.UserEvent();
            userEventSeed.Owner = _permit.Owner;

            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                throwIfError, userEventSeed, doCache);

            if (status.Code >= 0)
            {
                _userEvents = status.PayloadToList<Admin.UserEvent>();
            }

            OnPropertyChanged("UserEvents");
            return status;
        }

        public ObservableCollection<Admin.UserEvent> UserEvents
        {
            get { return _userEvents; }
        }
    }
}
