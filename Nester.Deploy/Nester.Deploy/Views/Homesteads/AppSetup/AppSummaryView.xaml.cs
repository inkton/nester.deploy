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
using System.Threading.Tasks;
using Inkton.Nester.Models;
using Inkton.Nester.ViewModels;
using Nester.Deploy.Helpers;

namespace Inkton.Nester.Views
{
    public partial class AppSummaryView : View
    {
        SoftwareFramework.Version _selVersion = null;

        public AppSummaryView(BaseModels baseModels)
        {
            InitializeComponent();

            BaseModels = baseModels;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {                   
                    ButtonDone,
                    ButtonCancel
                });            
        }

        public override void UpdateBindings()
        {
            if (_baseModels.TargetViewModel.EditApp != null)
            {
                Title = _baseModels.TargetViewModel.EditApp.Name;
            }

            if (_baseModels.TargetViewModel.DeploymentModel.UpgradableAppTiers.Any())
            {
                // The app is being created and deployed. Only selected services are applicable
                BindingContext = _baseModels.TargetViewModel
                    .DeploymentModel;
                SoftwareVersion.IsVisible = false;
                ButtonDone.Text = "Upgrade";
            }
            else
            {
                // The app is being created or updated. All services are applicable
                BindingContext = _baseModels.TargetViewModel;

                _baseModels.TargetViewModel.DeploymentModel.DotnetVersions.All(version =>
                {
                    SoftwareVersion.Items.Add(version.Name);
                    return true;
                });

                SoftwareVersion.SelectedIndex = 0;

                DeployWarning.Text =  "- Read and understand the terms for deployed apps.\n";
                DeployWarning.Text += "- The deployment progress will be reported on Slack if integrated.\n";

                if (_baseModels.TargetViewModel.EditApp.IsDeployed)
                {
                    DeployWarning.Text += "- New DevKits will be rebuilt with new access keys.\n";
                    DeployWarning.Text += "- Download the Devkits again once the deployment is complete.\n";
                    DeployWarning.Text += "- Ensure the custom domains and aliases point to the IP address " + _baseModels.TargetViewModel.EditApp.IPAddress + ".\n";
                    DeployWarning.Text += "- Ensure the custom domain IP has been fully propagted.\n";
                    DeployWarning.Text += "- The app has a backup to restore state.\n";
                }
            }
        }

        private async Task<bool> IsDnsOkAsync()
        {
            Nester.Models.AppDomain defaultDomain = (from domain in _baseModels.TargetViewModel.DomainModel.Domains
                                                     where domain.Default == true
                                                     select domain).First();

            foreach (Nester.Models.AppDomain domain in _baseModels.TargetViewModel.DomainModel.Domains)
            {
                if (domain.Default)
                    continue;

                if (!await Dns.IsDomainIPAsync(domain.Name, defaultDomain.IPAddress))
                {
                    await DisplayAlert("Nester", "The domain name " + domain.Name +
                        " currently does not resolve to " + defaultDomain.IPAddress +
                        ". Make sure to update the DNS", "OK");
                    return false;
                }

                string unmatchedAlias = await Dns.GetUnmatchedDomainAliasIPAsync(
                    domain.Aliases, defaultDomain.IPAddress);

                if (unmatchedAlias != null)
                {
                    await DisplayAlert("Nester", "The alias " + unmatchedAlias +
                        " currently does not resolve to " + defaultDomain.IPAddress +
                        ". Make sure to update the DNS", "OK");
                    return false;
                }
            }

            return true;
        }

        private async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                if (_baseModels.TargetViewModel.DeploymentModel.UpgradableAppTiers.Any())
                {
                    await UpgradeAsync();
                }
                else
                {
                    
                    foreach (var version in _baseModels.TargetViewModel.DeploymentModel.DotnetVersions)
                    {
                        if (version.Name == SoftwareVersion.SelectedItem as string)
                        {
                            _selVersion = version;
                            break;
                        }
                    }

                    if (_baseModels.TargetViewModel.DeploymentModel.Deployments.Any())
                    {
                        await UpdateAsync();
                    }
                    else
                    {
                        await InstallAsync();
                    }
                }

                await _baseModels.TargetViewModel.QueryStatusAsync();

                AppView appView = MainSideView.GetAppView(_baseModels.TargetViewModel.EditApp.Id);
                if (appView != null)
                {
                    appView.UpdateState();
                }

                MainSideView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private async void OnCancelButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                // Head back to homepage if the 
                // page was called from here
                MainSideView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        private async Task UpgradeAsync()
        {
            IsServiceActive = true;

            AppService appService = _baseModels
                .TargetViewModel
                .ServicesViewModel
                .Services.FirstOrDefault(
                x => x.Tag == "nest-oak");

            // 'users/{uid}/apps/{aid}/deployments/{dep}/app_services/{sid}/app_service_tiers/{tierid}'
            await _baseModels.TargetViewModel
                .DeploymentModel
                .UpdateAppUpgradeServiceTiersAsync(appService);

            IsServiceActive = false;
        }

        private async Task UpdateAsync()
        {
            if (!await IsDnsOkAsync())
                return;

            IsServiceActive = true;

            Deployment deployment =
                _baseModels.TargetViewModel.DeploymentModel.Deployments.First();
            deployment.FrameworkVersionId = _selVersion.Id;
            deployment.MakeBackup = false;

            await _baseModels.TargetViewModel.DeploymentModel.UpdateDeploymentAsync(deployment);

            IsServiceActive = false;
        }

        private async Task InstallAsync()
        {
            var customDomains = _baseModels.TargetViewModel.DomainModel.Domains.Where(domain => !domain.Default).ToList();

            if (customDomains.Any())
            {
                string domainList = "\n";
                foreach (var domain in customDomains)
                {
                    domainList += domain.Name + "\n";
                }

                await DisplayAlert("Nester", "The following custom domains will be removed. " +
                    "Add the domains and re-deploy when the App IP is known.\n" + domainList, "OK");

                foreach (var domain in customDomains)
                {
                    await _baseModels.TargetViewModel.DomainModel.RemoveDomainAsync(domain);
                }
            }

            IsServiceActive = true;

            _baseModels.TargetViewModel.DeploymentModel.EditDeployment.FrameworkVersionId = _selVersion.Id;

            await _baseModels.TargetViewModel.DeploymentModel.CreateDeployment(
                _baseModels.TargetViewModel.DeploymentModel.EditDeployment);

            _baseModels.TargetViewModel.EditApp.Deployment =
                _baseModels.TargetViewModel.DeploymentModel.EditDeployment;

            IsServiceActive = false;
        }
    }
}
