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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Inkton.Nester.Views
{
    public class DomainViewModel : ViewModel
    {
        private Admin.AppDomain _editDomain;
        private bool _primary;

        private ObservableCollection<Admin.AppDomain> _domains;

        public ICommand EditCertCommand { get; private set; }

        public DomainViewModel(Admin.App app) : base(app)
        {
            _domains = new ObservableCollection<Admin.AppDomain>();
            _primary = false;

            _editDomain = new Admin.AppDomain();
            _editDomain.App = app;
        }

        override public Admin.App EditApp
        {
            get
            {
                return _editApp;
            }
            set
            {
                _editDomain.App = value;
                SetProperty(ref _editApp, value);
            }
        }

        public bool Primary
        {
            get { return _primary; }
            set
            {
                SetProperty(ref _primary, value);
            }
        }

        public Admin.AppDomain EditDomain
        {
            get
            {
                return _editDomain;
            }
            set
            {
                SetProperty(ref _editDomain, value);
            }
        }

        public ObservableCollection<Admin.AppDomain> Domains
        {
            get
            {
                return _domains;
            }
            set
            {
                SetProperty(ref _domains, value);
            }
        }

        public Admin.AppDomain DefaultDomain
        {
            get
            {
                var existDomains = from domain in _domains
                                   where domain.Default == true
                                   select domain;
                Admin.AppDomain defaultDomain = existDomains.FirstOrDefault();
                return defaultDomain;
            }
        }

        override public async Task<Cloud.ServerStatus> InitAsync()
        {
            return await QueryDomainsAsync();
        }

        public async Task<Cloud.ServerStatus> QueryDomainsAsync(
            bool doCache = false, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                throwIfError, _editDomain, doCache);

            if (status.Code >= 0)
            {
                _domains = status.PayloadToList<Admin.AppDomain>();

                foreach (Admin.AppDomain domain in _domains)
                {
                    domain.App = _editApp;
                    domain.Primary = (_editApp.PrimaryDomainId == domain.Id);
                    domain.Ip = await NesterControl.NesterService.GetIPAsync(domain.Name);

                    Admin.AppDomainCertificate seedCert = new Admin.AppDomainCertificate();
                    seedCert.AppDomain = domain;

                    status = await Cloud.Result.WaitForObjectListAsync(
                        throwIfError, seedCert);

                    if (status.Code >= 0)
                    {
                        ObservableCollection<Admin.AppDomainCertificate> list = 
                            status.PayloadToList<Admin.AppDomainCertificate>();

                        if (list.Any())
                        {
                            domain.Certificate = list.First();
                            domain.Certificate.AppDomain = domain;
                        }
                    }
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryDomainAsync(Admin.AppDomain domain = null, 
            bool doCache = false, bool throwIfError = true)
        {
            Admin.AppDomain theDomain = domain == null ? _editDomain : domain;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theDomain, new Cloud.CachedHttpRequest<Admin.AppDomain>(
                    NesterControl.NesterService.QueryAsync), doCache, null, null);

            if (status.Code >= 0)
            {
                _editDomain = status.PayloadToObject<Admin.AppDomain>();

                Admin.AppDomainCertificate seedCert = new Admin.AppDomainCertificate();
                seedCert.AppDomain = _editDomain;

                status = await Cloud.Result.WaitForObjectListAsync(
                    throwIfError, seedCert);

                if (status.Code >= 0)
                {
                    ObservableCollection<Admin.AppDomainCertificate> list =
                        status.PayloadToList<Admin.AppDomainCertificate>();

                    if (list.Any())
                    {
                        theDomain.Certificate = list.First();
                        theDomain.Certificate.AppDomain = theDomain;
                    }
                }

                if (domain != null)
                {
                    Utils.Object.PourPropertiesTo(_editDomain, domain);
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> CreateDomainAsync(Admin.AppDomain domain = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.AppDomain theDomain = domain == null ? _editDomain : domain;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theDomain, new Cloud.CachedHttpRequest<Admin.AppDomain>(
                    NesterControl.NesterService.CreateAsync), doCache);

            if (status.Code >= 0)
            {
                _editDomain = status.PayloadToObject<Admin.AppDomain>();

                if (domain != null)
                {
                    Utils.Object.PourPropertiesTo(_editDomain, domain);
                    _domains.Add(domain);
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> CreateDomainCertificateAsync(Admin.AppDomainCertificate cert = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.AppDomainCertificate theCert = cert == null ? _editDomain.Certificate : cert;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                    theCert, new Cloud.CachedHttpRequest<Admin.AppDomainCertificate>(
                        NesterControl.NesterService.CreateAsync));

            if (status.Code >= 0)
            {
                _editDomain.Certificate = status.PayloadToObject<Admin.AppDomainCertificate>();

                if (cert != null)
                {
                    Utils.Object.PourPropertiesTo(_editDomain.Certificate, cert);
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> RemoveDomainAsync(Admin.AppDomain domain = null,
             bool doCache = false, bool throwIfError = true)
         {
            Admin.AppDomain theDomain = domain == null ? _editDomain : domain;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theDomain, new Cloud.CachedHttpRequest<Admin.AppDomain>(
                    NesterControl.NesterService.RemoveAsync), doCache);

            if (status.Code >= 0)
            {
                if (domain == null)
                {
                    // any cert that belong to the domain
                    // are automatically removed in the server
                    _domains.Remove(theDomain);
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> RemoveDomainCertificateAsync(Admin.AppDomainCertificate cert = null,
             bool doCache = false, bool throwIfError = true)
        {
            Admin.AppDomainCertificate theCert = cert == null ? _editDomain.Certificate : cert;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theCert, new Cloud.CachedHttpRequest<Admin.AppDomainCertificate>(
                    NesterControl.NesterService.RemoveAsync), doCache);

            if (status.Code >= 0)
            {
                if (cert == null)
                {
                    _editDomain.Certificate = null;
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> UpdateDomainAsync(Admin.AppDomain domain = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.AppDomain theDomain = domain == null ? _editDomain : domain;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theDomain, new Cloud.CachedHttpRequest<Admin.AppDomain>(
                    NesterControl.NesterService.UpdateAsync), doCache);

            if (status.Code >= 0)
            {
                _editDomain = status.PayloadToObject<Admin.AppDomain>();

                /* updates to the domain invalidates attached
                 * certificates.
                */
                _editDomain.Certificate = null;

                if (domain != null)
                {
                    Utils.Object.PourPropertiesTo(_editDomain, domain);
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> UpdateDomainCertificateAsync(Admin.AppDomainCertificate cert = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.AppDomainCertificate theCert = cert == null ? _editDomain.Certificate : cert;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theCert, new Cloud.CachedHttpRequest<Admin.AppDomainCertificate>(
                    NesterControl.NesterService.UpdateAsync), doCache);

            if (status.Code >= 0)
            {
                _editDomain.Certificate = status.PayloadToObject<Admin.AppDomainCertificate>();

                if (cert != null)
                {
                    Utils.Object.PourPropertiesTo(
                        _editDomain.Certificate, cert);
                }
            }

            return status;
        }
    }
}
