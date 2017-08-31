using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net;
using System.Collections.ObjectModel;
using Nester.Admin;
using System.Resources;
using System.Reflection;

namespace Nester.Cloud
{
    public struct ServerStatus
    {
        private int _code;
        private string _description;
        private string _notes;
        private HttpStatusCode _httpStatus;
        private object _payload;

        public ServerStatus(int code = -999)
        {
            _code = code;
            _description = "unknown";
            _notes = "none";
            _httpStatus = HttpStatusCode.NotFound;
            _payload = null;
        }

        public static ServerStatus FromServerResult(object payload, Result result)
        {
            ServerStatus status = new ServerStatus();
            status.Code = result.ResultCode;
            status.Description = result.ResultText;
            status.Notes = result.Notes;
            status.Payload = payload;
            return status;
        }

        public T PayloadToObject<T>() where T : Cloud.ManagedEntity
        {
            return _payload as T;
        }

        public ObservableCollection<T> PayloadToList<T>() where T : Cloud.ManagedEntity
        {
            return _payload as ObservableCollection<T>;
        }

        public int Code
        {
            get { return _code; }
            set { _code = value; }
        }

        public HttpStatusCode HttpStatus
        {
            get { return _httpStatus; }
            set { _httpStatus = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string LocalDescription
        {
            get
            {
                ResourceManager resmgr = new ResourceManager("Nester.AppResources"
                                    , typeof(ServerStatus).GetTypeInfo().Assembly);
                return resmgr.GetString(_description,
                                      System.Globalization.CultureInfo.CurrentUICulture);
            }
        }

        public string Notes
        {
            get { return _notes; }
            set { _notes = value; }
        }

        public object Payload
        {
            get { return _payload; }
            set { _payload = value; }
        }
    }
}
