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

namespace Inkton.Nester.Views
{
    public partial class AppSummaryView : View
    {
        public AppSummaryView(BaseModels baseModels)
        {
            _baseModels = baseModels;

            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {                   
                    ButtonDone
                });

            BindingContext = _baseModels.TargetViewModel;
            
            _baseModels.TargetViewModel.DeploymentModel.DotnetVersions.All(version =>
            {
                SoftwareVersion.Items.Add(version.Name);
                return true;
            });

            SoftwareVersion.SelectedIndex = 0;
            DeployWarning.Text = "The deployment will take some time to complete. ";

            if (_baseModels.TargetViewModel.EditApp.IsDeployed)
            {
                DeployWarning.Text += "New DevKits will be rebuilt with new access keys. ";
                DeployWarning.Text += "Download the Devkits again once the deployment is complete. ";
                DeployWarning.Text += "The project files should be updated to reflect the runtime framework version. ";
            }
        }
        
        private async Task<bool> IsDnsOkAsync()
        {
            // The custom domains and aliases must point to 
            // the App IP in the DNS.

            AppDomain defaultDomain = (from domain in _baseModels.TargetViewModel.DomainModel.Domains
                                             where domain.Default == true
                                             select domain).First();

            foreach (AppDomain domain in _baseModels.TargetViewModel.DomainModel.Domains)
            {
                if (domain.Default)
                    continue;

                string wildcardStripped = domain.Name;

                if (wildcardStripped.StartsWith("*."))
                {
                    wildcardStripped = domain.Name.Remove(0, 2);
                }

                string ip = await NesterControl.Service.GetIPAsync(wildcardStripped);

                if (ip == null || ip != defaultDomain.Ip)
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The domain name " + wildcardStripped +
                        " currently does not resolve to " + defaultDomain.Ip +
                        ". Make sure to update the DNS", "OK");
                    return false;
                }

                if (domain.Aliases != null && domain.Aliases.Length > 0)
                {
                    foreach (string alias in domain.Aliases.Split(' '))
                    {
                        ip = await NesterControl.Service.GetIPAsync(alias);

                        if (ip == null || ip != defaultDomain.Ip)
                        {
                            IsServiceActive = false;
                            await DisplayAlert("Nester", "The alias " + alias +
                                " currently does not resolve to " + defaultDomain.Ip +
                                ". Make sure to update the DNS", "OK");
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                SoftwareFramework.Version selVersion = null;
                foreach (var version in _baseModels.TargetViewModel.DeploymentModel.DotnetVersions)
                {
                    if (version.Name == SoftwareVersion.SelectedItem as string)
                    {
                        selVersion = version;
                        break;
                    }
                }

                if (_baseModels.TargetViewModel.DeploymentModel.Deployments.Any())
                {
                    if (_baseModels.TargetViewModel.EditApp.IsDeployed)
                    {
                        if (!await IsDnsOkAsync())
                            return;
                    }

                    Deployment deployment =
                        _baseModels.TargetViewModel.DeploymentModel.Deployments.First();
                    deployment.FrameworkVersionId = selVersion.Id;

                    await _baseModels.TargetViewModel.DeploymentModel.UpdateDeploymentAsync(deployment);
                }
                else
                {
                    if (!_baseModels.TargetViewModel.EditApp.IsDeployed)
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
                    }

                    _baseModels.TargetViewModel.DeploymentModel.EditDeployment.FrameworkVersionId = selVersion.Id;

                    await _baseModels.TargetViewModel.DeploymentModel.CreateDeployment(
                        _baseModels.TargetViewModel.DeploymentModel.EditDeployment);

                    _baseModels.TargetViewModel.EditApp.Deployment = 
                        _baseModels.TargetViewModel.DeploymentModel.EditDeployment;
                }

                await _baseModels.TargetViewModel.InitAsync();

                await NesterControl.ResetViewAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
                IsServiceActive = false;
            }
        }

        private async void OnCancelButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                // Head back to homepage if the 
                // page was called from here
                await NesterControl.ResetViewAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
    }
}
