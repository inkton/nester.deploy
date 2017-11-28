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
using System.Net;
using System.Collections.ObjectModel;
using Inkton.Nester.Admin;
using System.Resources;
using System.Reflection;

namespace Inkton.Nester.Cloud
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
                ResourceManager resmgr = new ResourceManager("Inkon.Nester.Properties.Resources"
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
