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
    public class DeploymentViewModel : ViewModel
    {
        private Admin.Deployment _editDeployment;

        private ObservableCollection<Admin.Deployment> _deployments;
        private ObservableCollection<Admin.Forest> _forests;
        private Dictionary<string, Admin.Forest> _forestByTag;
        private ObservableCollection<Admin.SoftwareFramework.Version> _dotnetVersions;

        public ICommand SelectForestCommand { get; private set; }

        public DeploymentViewModel(Admin.App app) : base(app)
        {
            _deployments = new ObservableCollection<Admin.Deployment>();
            _forests = new ObservableCollection<Admin.Forest>();
            _forestByTag = new Dictionary<string, Admin.Forest>();
            SelectForestCommand = new Command<Admin.Forest>( 
                (forest) => HandleCommand<Admin.Forest>(forest, "select") );

            _editDeployment = new Admin.Deployment();
            _editDeployment.App = app;
            app.Deployment = _editDeployment;
        }

        override public Admin.App EditApp
        {
            get
            {
                return _editApp;
            }
            set
            {
                SetProperty(ref _editApp, value);
                _editApp.Deployment = _editDeployment;
                _editDeployment.App = value;
            }
        }

        public Admin.Deployment EditDeployment
        {
            get
            {
                return _editDeployment;
            }
            set
            {
                SetProperty(ref _editDeployment, value);
                _editApp.Deployment = value;
            }
        }

        public Dictionary<string, Admin.Forest> ForestsByTag
        {
            get
            {
                return _forestByTag;
            }
            set
            {
                SetProperty(ref _forestByTag, value);
            }
        }

        public ObservableCollection<Admin.SoftwareFramework.Version> DotnetVersions
        {
            get
            {
                return _dotnetVersions;
            }
            set
            {
                SetProperty(ref _dotnetVersions, value);
            }
        }

        public ObservableCollection<Admin.Deployment> Deployments
        {
            get
            {
                return _deployments;
            }
            set
            {
                SetProperty(ref _deployments, value);
            }
        }

        override public async Task<Cloud.ServerStatus> InitAsync()
        {
            Cloud.ServerStatus status;

            status = await QueryDeploymentsAsync();
            if (status.Code < 0)
            {
                return status;
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> CollectInfoAsync()
        {
            Cloud.ServerStatus status;

            status = await QueryForestsAsync();
            if (status.Code < 0)
            {
                return status;
            }

            status = await QuerySoftwareFrameworkVersionsAsync();
            if (status.Code < 0)
            {
                return status;
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryForestsAsync(
            bool doCache = true, bool throwIfError = true)
        {
            Admin.Forest forestSeed = new Admin.Forest();
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                throwIfError, forestSeed, doCache);

            if (status.Code >= 0)
            {
                _forests = status.PayloadToList<Admin.Forest>();
                _forestByTag.Clear();

                foreach (Admin.Forest forest in _forests)
                {
                    _forestByTag[forest.Tag] = forest;
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QuerySoftwareFrameworkVersionsAsync(
            bool doCache = true, bool throwIfError = true)
        {
            Admin.SoftwareFramework frameworkSeed = new Admin.SoftwareFramework();
            frameworkSeed.Tag = "aspdotnetcore";
            Admin.SoftwareFramework.Version versionSeed = new Admin.SoftwareFramework.Version();
            versionSeed.Framework = frameworkSeed;

            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                throwIfError, versionSeed, doCache);

            if (status.Code >= 0)
            {
                _dotnetVersions = status.PayloadToList<Admin.SoftwareFramework.Version>();
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryDeploymentsAsync(
            Admin.Deployment deployment = null, bool doCache = false, bool throwIfError = true)
        {
            Admin.Deployment theDeployment = deployment == null ? _editApp.Deployment : deployment;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                throwIfError, theDeployment, doCache);
            _editApp.Deployment = null;

            if (status.Code >= 0)
            {
                _deployments = status.PayloadToList<Admin.Deployment>();
                if (_deployments.Any())
                {
                    _editDeployment = _deployments.First();
                    _editApp.Deployment = _editDeployment;
                }

                if (deployment != null)
                {
                    Utils.Object.PourPropertiesTo(_editApp.Deployment, deployment);
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> QueryDevkitAsync(Admin.Devkit devkit,
            bool bCache = false, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                devkit, new Cloud.CachedHttpRequest<Admin.Devkit>(
                    ThisUI.NesterService.QueryAsync), bCache);

            return status;
        }

        public async Task<Cloud.ServerStatus> CreateDeployment(Admin.Deployment deployment = null,
            bool doCache = true, bool throwIfError = true)
        {
            Admin.Deployment theDeployment = deployment == null ? _editApp.Deployment : deployment;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theDeployment, new Cloud.CachedHttpRequest<Admin.Deployment>(
                    ThisUI.NesterService.CreateAsync), doCache);

            if (status.Code >= 0)
            {
                _editDeployment = status.PayloadToObject<Admin.Deployment>();
                _editApp.Deployment = _editDeployment;

                if (deployment != null)
                {
                    Utils.Object.PourPropertiesTo(_editDeployment, deployment);
                    _deployments.Add(_editDeployment);
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> UpdateDeploymentAsync(Admin.Deployment deployment = null,
             bool doCache = true, bool throwIfError = true)
        {
            Admin.Deployment theDeployment = deployment == null ? _editApp.Deployment : deployment;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theDeployment, new Cloud.CachedHttpRequest<Admin.Deployment>(
                    ThisUI.NesterService.UpdateAsync), doCache);

            if (status.Code >= 0)
            {
                _editDeployment = status.PayloadToObject<Admin.Deployment>();
                _editApp.Deployment = _editDeployment;

                if (deployment != null)
                {
                    Utils.Object.PourPropertiesTo(_editDeployment, deployment);
                }
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> RemoveDeploymentAsync(Admin.Deployment deployment = null,
            bool doCache = false, bool throwIfError = true)
        {
            Admin.Deployment theDeployment = deployment == null ? _editApp.Deployment : deployment;
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectAsync(throwIfError,
                theDeployment, new Cloud.CachedHttpRequest<Admin.Deployment>(
                    ThisUI.NesterService.RemoveAsync), doCache);

            if (status.Code == 0)
            {
                if (deployment == null)
                {
                    _deployments.Remove(deployment);
                }
            }

            return status;
        }        
    }
}
