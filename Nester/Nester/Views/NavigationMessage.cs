using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nester.Views
{
    public class NavigationMessage
    {
        private string _type = string.Empty;

        public NavigationMessage(string type)
        {
            _type = type;
        }

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
    }

    public class AlertMessage : NavigationMessage
    {
        private string _message = string.Empty;

        public AlertMessage(string type, string message)
            :base(type)
        {
            _message = message;
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }

    public class ManagedObjectMessage<T> : NavigationMessage
    {
        private T _object = default(T);

        public ManagedObjectMessage(string type, T Object)
            : base(type)
        {
            _object = Object;
        }

        public T Object
        {
            get { return _object; }
            set { _object = value; }
        }
    }

}
