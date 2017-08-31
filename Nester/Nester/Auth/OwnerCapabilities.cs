using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Nester.Auth
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
