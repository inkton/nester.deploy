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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Inkton.Nester.Views
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        protected Admin.App _editApp;

        protected bool _validated = false;
        protected bool _canUpdate = false;
        protected bool _isBusy = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public ViewModel(Admin.App app = null)
        {
            _editApp = app;
        }

        virtual public Admin.App EditApp
        {
            get
            {
                return _editApp;
            }
            set
            {
                SetProperty(ref _editApp, value);
            }
        }

        public bool IsValidApp
        {
            get
            {
                return _editApp != null && _editApp.Id > 0;
            }
        }

        public Admin.INesterControl NesterControl
        {
            get
            {
                return Application.Current as Admin.INesterControl;
            }
        }

        public bool Validated
        {
            get { return _validated; }
            set { SetProperty(ref _validated, value); }
        }

        public bool CanUpdate
        {
            get { return _canUpdate; }
            set { SetProperty(ref _canUpdate, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }
        
        public virtual Task<Cloud.ServerStatus> InitAsync()
        {
            return Task.FromResult(default(Cloud.ServerStatus));
        }

        protected bool SetProperty<T>(ref T storage, T value,
                                        [CallerMemberName] string propertyName = null)
        {
            if (Object.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void HandleCommand<T>(T obj, string command)
        {
            ManagedObjectMessage<T> doThis =
                new ManagedObjectMessage<T>(command, obj);
            MessagingCenter.Send(doThis, doThis.Type);
        }
    }
}
