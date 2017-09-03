using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Nester.Views
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        protected Admin.App _editApp;

        protected bool _validated = false;
        protected bool _canUpdate = false;
        protected bool _wizardMode = false;

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

        virtual public bool WizardMode
        {
            get
            {
                return _wizardMode;
            }
            set
            {
                SetProperty(ref _wizardMode, value);
            }
        }

        public NesterUI ThisUI
        {
            get
            {
                return ((NesterUI)NesterUI.Current);
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
