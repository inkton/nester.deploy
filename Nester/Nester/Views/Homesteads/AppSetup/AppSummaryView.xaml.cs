using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Nester.Views
{
    public partial class AppSummaryView : Nester.Views.View
    {
        public AppSummaryView(AppViewModel appViewModel)
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {                   
                    ButtonDone
                });

            _appViewModel = appViewModel;
            BindingContext = _appViewModel;
            


            _appViewModel.DeploymentModel.DotnetVersions.All(version =>
            {
                SoftwareVersion.Items.Add(version.Name);
                return true;
            });

            SoftwareVersion.SelectedIndex = 0;
            DeployWarning.Text = "The deployment will take some time to complete. ";

            if (appViewModel.EditApp.IsDeployed)
            {
                DeployWarning.Text += "New DevKits will be rebuilt with new access keys. ";
                DeployWarning.Text += "Download the Devkits again once the deployment is complete. ";
                DeployWarning.Text += "The project files should be updated to reflect the runtime framework version. ";
            }
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                Admin.SoftwareFramework.Version selVersion = null;
                foreach (var version in _appViewModel.DeploymentModel.DotnetVersions)
                {
                    if (version.Name == SoftwareVersion.SelectedItem as string)
                    {
                        selVersion = version;
                        break;
                    }
                }

                await _appViewModel.DeploymentModel.InitAsync();

                if (_appViewModel.DeploymentModel.Deployments.Any())
                {
                    Admin.Deployment deployment =
                        _appViewModel.DeploymentModel.Deployments.First();
                    deployment.FrameworkVersionId = selVersion.Id;

                    await _appViewModel.DeploymentModel.UpdateDeploymentAsync(deployment);
                }
                else
                {
                    _appViewModel.DeploymentModel.EditDeployment.FrameworkVersionId = selVersion.Id;

                    await _appViewModel.DeploymentModel.CreateDeployment(
                        _appViewModel.DeploymentModel.EditDeployment);

                    _appViewModel.EditApp.Deployment = 
                        _appViewModel.DeploymentModel.EditDeployment;
                }

                await _appViewModel.InitAsync();
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
                IsServiceActive = false;
            }
        }
    }
}
