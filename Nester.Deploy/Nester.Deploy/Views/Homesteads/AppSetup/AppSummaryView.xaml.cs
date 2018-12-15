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
using Inkton.Nest.Model;
using Inkton.Nester.ViewModels;
using Nester.Deploy.Helpers;
using Inkton.Nest.Cloud;
using Inkton.Nester.Cloud;

namespace Inkton.Nester.Views
{
    public partial class AppSummaryView : View
    {
        SoftwareFramework.Version _selVersion = null;

        public AppSummaryView(BaseViewModels baseModels)
        {
            InitializeComponent();

            ViewModels = baseModels;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {                   
                    ButtonDone,
                    ButtonCancel
                });

            ButtonDoDiscount.Clicked += ButtonDoDiscount_ClickedAsync;
            CreditCode.TextChanged += CreditCode_TextChanged;
            ButtonClearDiscount.Clicked += ButtonClearDiscount_Clicked;

            _baseViewModels.AppViewModel.DeploymentViewModel.ApplyCredit = null;
        }

        public override void UpdateBindings()
        {
            if (App != null)
            {
                Title = App.Name;
            }

            BindingContext = _baseViewModels.AppViewModel;

            if (_baseViewModels.AppViewModel.ServicesViewModel.UpgradableAppTiers.Any())
            {
                // The app is being created and deployed. Only selected services are applicable

                SoftwareVersion.IsVisible = false;

                DeployWarning.Text = "• Read and understand the terms for deployed apps.\n";
                DeployWarning.Text += "• The upgrade progress will be reported on Slack if integrated.\n";

                ButtonDone.Text = "Upgrade";

                ButtonDoDiscount.Text = "Show";
            }
            else
            {
                // The app is being created or updated. All services are applicable

                _baseViewModels.AppViewModel.DeploymentViewModel.DotnetVersions.Reverse().All(version =>
                {
                    SoftwareVersion.Items.Add(version.Name);
                    return true;
                });

                SoftwareVersion.SelectedIndex = 0;

                DeployWarning.Text = "• Read and understand the terms for deployed apps.\n";
                DeployWarning.Text += "• The deployment progress will be reported on Slack if integrated.\n";

                if (App.IsDeployed)
                {
                    DeployWarning.Text += "• New DevKits will be rebuilt with new access keys.\n";
                    DeployWarning.Text += "• Download the Devkits again once the deployment is complete.\n";
                    DeployWarning.Text += "• Please ensure the following things are inplace:\n";
                    DeployWarning.Text += "    ‣ The custom domains and aliases point to the IP address " + App.IPAddress + ".\n";
                    DeployWarning.Text += "    ‣ The custom domain IP has been fully propagted.\n";
                    DeployWarning.Text += "    ‣ The app has a backup to restore state.\n";

                    ButtonDoDiscount.Text = "Show";
                }
                else
                {
                    ButtonDoDiscount.Text = "Apply";
                }
            }

            DisplayTotals();
        }

        private async Task<bool> IsDnsOkAsync()
        {
            Nest.Model.AppDomain defaultDomain = (from domain in _baseViewModels.AppViewModel.DomainViewModel.Domains
                                                     where domain.Default == true
                                                     select domain).First();

            foreach (Nest.Model.AppDomain domain in _baseViewModels.AppViewModel.DomainViewModel.Domains)
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

        private async void ButtonDoDiscount_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                _baseViewModels.AppViewModel.ServicesViewModel.CreateServicesTables();
                _baseViewModels.AppViewModel.DeploymentViewModel.ApplyCredit = null;

                if (CreditCode.Text.Length > 0)
                {
                    Credit credit = _baseViewModels.PaymentViewModel.EditCredit;
                    credit.Code = CreditCode.Text.Trim();

                    ResultSingle<Credit> result = await _baseViewModels
                        .PaymentViewModel.QueryCreditAsync();

                    if (result.Code == 0)
                    {
                        credit = _baseViewModels.PaymentViewModel.EditCredit;
                        DisplayTotals(credit);
                        _baseViewModels.AppViewModel
                            .DeploymentViewModel
                            .ApplyCredit = credit;
                    }
                }   
                else
                {
                    DisplayTotals();
                }

                await PricesGrid.ScrollToAsync(0, PricesGrid.Content.Height, true);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;

        }

        private void CreditCode_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            if (CreditCode.Text.Length == 0)
            {
                ClearDiscountAsync();
            }
        }

        private void ButtonClearDiscount_Clicked(object sender, EventArgs e)
        {
            ClearDiscountAsync();
        }

        private async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                if (_baseViewModels.AppViewModel.ServicesViewModel.UpgradableAppTiers.Any())
                {
                    await UpgradeAsync();

                    await _baseViewModels.AppViewModel
                        .ServicesViewModel
                        .SetAppUpgradingAsync(false);
                }
                else
                {
                    
                    foreach (var version in _baseViewModels
                        .AppViewModel.DeploymentViewModel
                        .DotnetVersions)
                    {
                        if (version.Name == SoftwareVersion.SelectedItem as string)
                        {
                            _selVersion = version;
                            break;
                        }
                    }

                    if (_baseViewModels.AppViewModel
                        .DeploymentViewModel
                        .Deployments.Any())
                    {
                        await UpdateAsync();
                    }
                    else
                    {
                        await InstallAsync();
                    }
                }

                await _baseViewModels.AppViewModel.QueryStatusAsync();

                AppView appView = MainSideView.GetAppView(App.Id);
                if (appView != null)
                {
                    appView.UpdateStatus();
                }

                MainSideView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
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

            await _baseViewModels.AppViewModel
                .ServicesViewModel
                .UpdateAppUpgradeServiceTierAsync();

            IsServiceActive = false;
        }

        private async Task UpdateAsync()
        {
            if (!await IsDnsOkAsync())
                return;

            IsServiceActive = true;

            Deployment deployment =
                _baseViewModels.AppViewModel.DeploymentViewModel.Deployments.First();
            deployment.FrameworkVersionId = _selVersion.Id;

            await _baseViewModels
                .AppViewModel
                .DeploymentViewModel
                .UpdateDeploymentAsync("reapply", deployment);

            IsServiceActive = false;
        }

        private async Task InstallAsync()
        {
            var customDomains = _baseViewModels.AppViewModel.DomainViewModel.Domains.Where(domain => !domain.Default).ToList();

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
                    await _baseViewModels.AppViewModel.DomainViewModel.RemoveDomainAsync(domain);
                }
            }

            IsServiceActive = true;

            _baseViewModels.AppViewModel.DeploymentViewModel.EditDeployment.FrameworkVersionId = _selVersion.Id;

            await _baseViewModels.AppViewModel.DeploymentViewModel.CreateDeployment(
                _baseViewModels.AppViewModel.DeploymentViewModel.EditDeployment);

            App.Deployment = _baseViewModels.AppViewModel
                .DeploymentViewModel
                .EditDeployment;

            IsServiceActive = false;
        }

        private void DisplayTotals(Credit credit = null)
        {
            decimal total = _baseViewModels
                    .AppViewModel
                    .ServicesViewModel
                    .CalculateServiceCost();

            decimal discount = 0;

            if (credit != null)
            {
                if (credit.Type != "trial")
                {
                    decimal value = Convert.ToDecimal(credit.Value);
                    bool isPercentage = credit.Type == "percentage";

                    if (isPercentage)
                    {
                        discount = total * value;
                    }
                    else
                    {
                        discount = total - value;
                    }
                }

                switch (credit.Type)
                {
                    case "percentage":
                        AdditionalInfo.Text = string.Format("The discount is %{0}", credit.Value);
                        break;
                    case "amount":
                        AdditionalInfo.Text = string.Format("A ${0} deduction apply", credit.Value);
                        break;
                    case "trial":
                        AdditionalInfo.Text = string.Format("Trial the app for {0} hours ", credit.Value);
                        break;
                }
            }

            DiscountTotal.Text = string.Format("{0:C2}", discount);
            Total.Text = string.Format("{0:C2}", total - discount);
        }

        private async void ClearDiscountAsync()
        {
            IsServiceActive = true;

            _baseViewModels.AppViewModel.ServicesViewModel.CreateServicesTables();
            _baseViewModels.AppViewModel.DeploymentViewModel.ApplyCredit = null;

            DisplayTotals();
            await PricesGrid.ScrollToAsync(0, PricesGrid.Content.Height, true);

            IsServiceActive = false;
        }
    }
}
