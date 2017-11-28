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
using Newtonsoft.Json;

namespace Inkton.Nester.Auth
{ 
    public class OwnerCapabilities : Cloud.ManagedEntity
    {
        private bool _canViewApp;
        private bool _canUpdateApp;
        private bool _canDeleteApp;

        private bool _canCreateNest;
        private bool _canViewNest;
        private bool _canDeleteNest;
        private bool _canUpdateNest;

        public OwnerCapabilities()
        {
            Reset();
        }

        public void Reset()
        {
            _canViewApp = false;
            _canUpdateApp = false;
            _canDeleteApp = false;

            _canCreateNest = false;
            _canViewNest = false;
            _canDeleteNest = false;
            _canUpdateNest = false;
        }

        public bool CanViewApp
        {
            get { return _canViewApp; }
            set { SetProperty(ref _canViewApp, value); }
        }

        public bool CanUpdateApp
        {
            get { return _canUpdateApp; }
            set { SetProperty(ref _canUpdateApp, value); }
        }

        public bool CanDeleteApp
        {
            get { return _canDeleteApp; }
            set { SetProperty(ref _canDeleteApp, value); }
        }

        public bool CanViewNest
        {
            get { return _canViewNest; }
            set { SetProperty(ref _canViewNest, value); }
        }

        public bool CanCreateNest
        {
            get { return _canCreateNest; }
            set { SetProperty(ref _canCreateNest, value); }
        }

        public bool CanUpdateNest
        {
            get { return _canUpdateNest; }
            set { SetProperty(ref _canUpdateNest, value); }
        }

        public bool CanDeleteNest
        {
            get { return _canDeleteNest; }
            set { SetProperty(ref _canDeleteNest, value); }
        }
    }

}
