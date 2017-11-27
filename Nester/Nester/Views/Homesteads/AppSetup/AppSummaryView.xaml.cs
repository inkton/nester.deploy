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

using Xamarin.Forms;

namespace Inkton.Nester.Views
{
    public partial class AppSummaryView : Inkton.Nester.Views.View
    {
        public AppSummaryView(Views.AppModelPair modelPair)
        {
            _modelPair = modelPair;

            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {                   
                    ButtonDone
                });

            BindingContext = _modelPair.AppViewModel;
            
            _modelPair.AppViewModel.DeploymentModel.DotnetVersions.All(version =>
            {
                SoftwareVersion.Items.Add(version.Name);
                return true;
            });

            SoftwareVersion.SelectedIndex = 0;
            DeployWarning.Text = "The deployment will take some time to complete. ";

            if (_modelPair.AppViewModel.EditApp.IsDeployed)
            {
                DeployWarning.Text += "New DevKits will be rebuilt with new access keys. ";
                DeployWarning.Text += "Download the Devkits again once the deployment is complete. ";
                DeployWarning.Text += "The project files should be updated to reflect the runtime framework version. ";
            }
        }
        
        private async Task<bool> IsDnsOkAsync()
        {
            Admin.AppDomain defaultDomain = (from domain in _modelPair.AppViewModel.DomainModel.Domains
                                             where domain.Default == true
                                             select domain).First();

            foreach (Admin.AppDomain domain in _modelPair.AppViewModel.DomainModel.Domains)
            {
                if (domain.Default)
                    continue;

                string wildcardStripped = domain.Name;

                if (wildcardStripped.StartsWith("*."))
                {
                    wildcardStripped = domain.Name.Remove(0, 2);
                }

                string ip = await NesterControl.NesterService.GetIPAsync(wildcardStripped);

                if (ip == null || ip != defaultDomain.Ip)
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The domain name " + wildcardStripped +
                        " currently does not resolve to " + defaultDomain.Ip +
                        ". Make sure to update the DNS", "OK");
                    return false;
                }

                if (domain.Aliases != null)
                {
                    foreach (string alias in domain.Aliases.Split(' '))
                    {
                        ip = await NesterControl.NesterService.GetIPAsync(alias);

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
                if (!await IsDnsOkAsync())
                    return;

                Admin.SoftwareFramework.Version selVersion = null;
                foreach (var version in _modelPair.AppViewModel.DeploymentModel.DotnetVersions)
                {
                    if (version.Name == SoftwareVersion.SelectedItem as string)
                    {
                        selVersion = version;
                        break;
                    }
                }

                if (_modelPair.AppViewModel.DeploymentModel.Deployments.Any())
                {
                    Admin.Deployment deployment =
                        _modelPair.AppViewModel.DeploymentModel.Deployments.First();
                    deployment.FrameworkVersionId = selVersion.Id;

                    await _modelPair.AppViewModel.DeploymentModel.UpdateDeploymentAsync(deployment);
                }
                else
                {
                    _modelPair.AppViewModel.DeploymentModel.EditDeployment.FrameworkVersionId = selVersion.Id;

                    await _modelPair.AppViewModel.DeploymentModel.CreateDeployment(
                        _modelPair.AppViewModel.DeploymentModel.EditDeployment);

                    _modelPair.AppViewModel.EditApp.Deployment = 
                        _modelPair.AppViewModel.DeploymentModel.EditDeployment;
                }

                await _modelPair.AppViewModel.InitAsync();

                ResetView();
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
                ResetView();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
    }
}
