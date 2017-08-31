namespace Nester.Cloud
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Text;
    using System.Net;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class ManagedEntity : INotifyPropertyChanged
    {
        private string _entity, _collection;
        public event PropertyChangedEventHandler PropertyChanged;
        
        public ManagedEntity(string entity = null, string collection = null)
        {
            _entity = entity;
            if (collection != null)
            {
                _collection = collection;
            }
            else if (entity != null)
            {
                _collection = entity + "s";
            }
        }

        virtual public string Entity
        {
            get { return _entity; }
        }

        virtual public string Collection
        {
            get { return _collection + "/"; }
        }

        virtual public string CollectionKey
        {
            get { return _collection + "/" + this.Key + "/"; }
        }

        public virtual string Key
        {
            get;
        }

        protected virtual bool SetProperty<T>(ref T storage, T value,
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
    }
}
