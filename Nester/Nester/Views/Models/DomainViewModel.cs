using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Nester.Views
{
    public class DomainViewModel : ViewModel
    {
        private Admin.AppDomain _editDomain;
        private bool _primary;

        private ObservableCollection<Admin.AppDomain> _domains;
        private ObservableCollection<Admin.AppDomainCertificate> _certs;

        public ICommand EditCertCommand { get; private set; }

        public DomainViewModel(Admin.App app) : base(app)
        {
            _domains = new ObservableCollection<Admin.AppDomain>();
            _certs = new ObservableCollection<Admin.AppDomainCertificate>();
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
                _editApp = value;
                _editDomain.App = value;
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
                                   where domain.Primary == true
                                   select domain;
                return existDomains.First();
            }
        }

        override public async Task<Cloud.ServerStatus> InitAsync()
        {
            return await QueryDomainsAsync();
        }

        public void MakePrimary(Admin.AppDomain primary)
        {
            /* the primary flag is set in the app
             * this is just to reflect on the ui
             */
            _domains.All(
                doamain => {
                    doamain.Primary = (doamain.Tag == primary.Tag);
                    return true;
                });
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
                theDomain, new Cloud.NesterService.CachedHttpRequest<Admin.AppDomain>(
                    ThisUI.NesterService.QueryAsync), doCache, null, null);

            if (status.Code >= 0)
            {
                Utils.Object.PourPropertiesTo(status.PayloadToObject<Admin.AppDomain>(), theDomain);

                Admin.AppDomainCertificate seedCert = new Admin.AppDomainCertificate();
                seedCert.AppDomain = theDomain;

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
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> CreateDomainAsync(Admin.AppDomain domain,
            bool doCache = false, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                domain, new Cloud.NesterService.CachedHttpRequest<Admin.AppDomain>(
                    ThisUI.NesterService.CreateAsync), doCache);

            if (status.Code >= 0)
            {
                _editDomain = status.PayloadToObject<Admin.AppDomain>();
                _domains.Add(_editDomain);

                domain.Certificate = new Admin.AppDomainCertificate();
                domain.Certificate.AppDomain = domain;
                domain.Certificate.Tag = domain.Tag;
                domain.Certificate.Type = "free";

                status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                    domain.Certificate, new Cloud.NesterService.CachedHttpRequest<Admin.AppDomainCertificate>(
                        ThisUI.NesterService.CreateAsync));

                if (status.Code == 0)
                {
                    _editDomain.Certificate = status.PayloadToObject<Admin.AppDomainCertificate>();
                    _certs.Add(_editDomain.Certificate);

                    Utils.Object.PourPropertiesTo(_editDomain, domain);
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> RemoveDomainAsync(Admin.AppDomain domain = null,
             bool doCache = false, bool throwIfError = true)
         {
            Admin.AppDomain theDomain = domain == null ? _editDomain : domain;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theDomain, new Cloud.NesterService.CachedHttpRequest<Admin.AppDomain>(
                    ThisUI.NesterService.RemoveAsync), doCache);

            if (status.Code >= 0)
            {
                _certs.Remove(theDomain.Certificate);
                _domains.Remove(theDomain);
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> UpdateDomainAsync(Admin.AppDomain domain = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.AppDomain theDomain = domain == null ? _editDomain : domain;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theDomain, new Cloud.NesterService.CachedHttpRequest<Admin.AppDomain>(
                    ThisUI.NesterService.UpdateAsync), doCache);

            if (status.Code >= 0)
            {
                _editDomain = status.PayloadToObject<Admin.AppDomain>();

                status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                    theDomain.Certificate, new Cloud.NesterService.CachedHttpRequest<Admin.AppDomainCertificate>(
                        ThisUI.NesterService.UpdateAsync));

                if (status.Code >= 0)
                {
                    Utils.Object.PourPropertiesTo(
                        status.PayloadToObject<Admin.AppDomainCertificate>(), _editDomain.Certificate);

                    Utils.Object.PourPropertiesTo(_editDomain, theDomain);
                }
            }

            return status;
        }
    }
}
