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
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Threading;

namespace Inkton.Nester.Views
{
    public class AuthViewModel : ViewModel
    {
        private Auth.Permit _permit;
        private ObservableCollection<Admin.UserEvent> _userEvents;
        private bool _canRecoverPassword = false;

        public AuthViewModel()
        {
            _permit = new Auth.Permit();
            _permit.Owner = NesterControl.User;

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
            NesterControl.NesterService.ClearSession();

            _permit.SecurityCode = null;
            _permit.Token = null;
        }

        public async Task<Cloud.ServerStatus> SignupAsync(
            bool throwIfError = true)
        {
            Cloud.ServerStatus status = await 
                NesterControl.NesterService.SignupAsync(_permit);

            if (status.Code < 0 && throwIfError)
            {
                Cloud.Result.ThrowError(status);
            }

            Utils.Object.CopyPropertiesTo(_permit.Owner, NesterControl.User);
            return status;
        }

        public async Task<Cloud.ServerStatus> RecoverPasswordAsync(
            bool throwIfError = true)
        {
            Cloud.ServerStatus status = await 
                NesterControl.NesterService.RecoverPasswordAsync(_permit);

            if (status.Code < 0 && throwIfError)
            {
                Cloud.Result.ThrowError(status);
            }

            Utils.Object.CopyPropertiesTo(_permit.Owner, NesterControl.User);
            return status;
        }

        public async Task<Cloud.ServerStatus> QueryTokenAsync(
            bool throwIfError = true)
        {
            Cloud.ServerStatus status = await 
                NesterControl.NesterService.QueryTokenAsync(_permit);

            if (status.Code < 0 && throwIfError)
            {
                Cloud.Result.ThrowError(status);
            }

            Utils.Object.CopyPropertiesTo(_permit.Owner, NesterControl.User);
            return status;
        }

        public async Task<Cloud.ServerStatus> ResetTokenAsync(
            bool throwIfError = true)
        {
            Cloud.ServerStatus status = await
                NesterControl.NesterService.ResetTokenAsync(_permit);

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
                user, new Cloud.CachedHttpRequest<Admin.User>(
                    NesterControl.NesterService.UpdateAsync), doCache);

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
