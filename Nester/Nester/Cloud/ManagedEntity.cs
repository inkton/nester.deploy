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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Inkton.Nester.Cloud
{
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
