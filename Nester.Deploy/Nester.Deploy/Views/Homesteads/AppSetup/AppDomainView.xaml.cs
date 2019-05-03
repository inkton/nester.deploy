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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using Inkton.Nest;
using Inkton.Nest.Model;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Helpers;

namespace Inkton.Nester.Views
{
    public partial class AppDomainView : View
    {
        private Regex _domainVerifier;

        public AppDomainView(AppViewModel appViewModel)
        {
            InitializeComponent();

            AppViewModel = appViewModel;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                                ButtonHome,
                                ButtonBasicDetails,
                                ButtonNests,
                                ButtonContacts,
                                ButtonClear,
                                ButtonAdd,
                                ButtonRefresh,
                                ButtonUpdate,
                                ButtonRemove,
                                ButtonCert,
                                ButtonDone
                });

            Aliases.Unfocused += Aliases_Unfocused;
            Name.Unfocused += Name_Unfocused;

            AppDomainsList.ItemSelected += AppDomainsList_ItemSelected;

            ButtonCert.Clicked += ButtonCert_Clicked;

            ButtonDone.IsVisible = _baseViewModels.WizardMode;
            if (_baseViewModels.WizardMode)
            {
                // hide but do not collapse
                TopButtonPanel.Opacity = 0;
            }

            // Thanks - http://stackoverflow.com/questions/10306690/domain-name-validation-with-regex
            _domainVerifier = new Regex(
                @"^((?!-)[A-Za-z0-9-]{1,63}(?<!-)\.)+[A-Za-z]{2,6}$"
             , RegexOptions.Singleline | RegexOptions.IgnoreCase);

            Clear();

            ButtonAppServices.Clicked += ButtonAppServices_ClickedAsync;
        }

        public override void UpdateBindings()
        {
            base.UpdateBindings();

            BindingContext = AppViewModel.DomainViewModel;
        }

        async private void OnButtonServiceClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.CurrentLevelViewAsync(new AppTierView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnButtonBasicDetailsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                _baseViewModels.WizardMode = false;
                MainSideView.CurrentLevelViewAsync(new AppBasicDetailView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void ButtonAppServices_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.CurrentLevelViewAsync(new AppTierView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnButtonContactsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.CurrentLevelViewAsync(new ContactsView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnButtonNestsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.CurrentLevelViewAsync(new AppNestsView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        private void ButtonCert_Clicked(object sender, EventArgs e)
        {
            if (AppDomainsList.SelectedItem != null)
            {
                Nest.Model.AppDomain browseDomain = AppDomainsList.SelectedItem as Nest.Model.AppDomain;
                AppViewModel.DomainViewModel.EditDomain = browseDomain;
                AppDomainCertView certView = new AppDomainCertView(AppViewModel);
                MainSideView.StackViewAsync(certView);
            }
        }

        private void Clear()
        {
            AppDomainsList.SelectedItem = null;
            AppViewModel.DomainViewModel.EditDomain = new Nest.Model.AppDomain();
            AppViewModel.DomainViewModel.EditDomain.OwnedBy = AppViewModel.EditApp;

            SetDefaults();

            Validate();
        }

        private void SetDefaults()
        {
            bool enableEdits = true;

            if (AppDomainsList.SelectedItem != null)
            {
                Nest.Model.AppDomain browseDomain = AppDomainsList.SelectedItem as Nest.Model.AppDomain;
                browseDomain.CopyTo(AppViewModel.DomainViewModel.EditDomain);
                enableEdits = !browseDomain.Default;
                IsPrimary.IsToggled = AppViewModel
                    .EditApp.PrimaryDomainId == browseDomain.Id;
            }

            Tag.IsEnabled = enableEdits;
            Name.IsEnabled = enableEdits;
            Aliases.IsEnabled = enableEdits;
            IsPrimary.IsEnabled = enableEdits;
        }

        private Nest.Model.AppDomain CopyUpdate(Nest.Model.AppDomain browsDomain)
        {
            browsDomain.OwnedBy = AppViewModel.EditApp;
            browsDomain.Tag = Tag.Text;
            browsDomain.Name = Name.Text;
            if (Aliases.Text == null || Aliases.Text.Length == 0)
                Aliases.Text = null;
            browsDomain.Aliases = Aliases.Text;
            browsDomain.Primary = IsPrimary.IsToggled;

            return browsDomain;
        }

        async private void SetPrimaryDomain(Nest.Model.AppDomain priaryDomain)
        {
            /*
             * The default domain is the <apptag>.nestapp.yt
             * this never changes.  The primary domain is the
             * webserver primary domain.  This is needed for
             * SSL certificates.
             * 
             */
            try
            {
                priaryDomain.Primary = true;
                AppViewModel.EditApp.PrimaryDomainId = priaryDomain.Id;
                await AppViewModel.UpdateAppAsync(AppViewModel.EditApp);
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }
        }

        private void AppDomainsList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            SetDefaults();
            Validate();
        }

        private bool VerifyAliases(string aliases, out string cleanedAliases)
        {
            bool ok = true;

            string[] aliasArray = aliases.Split(new char[] {',', ' '});
            StringBuilder verifiedList = new StringBuilder();

            foreach (string alias in aliasArray)
            {
                string aliasAdd = alias.Trim();

                if (aliasAdd != null && aliasAdd.Length >0 && 
                    _domainVerifier.Match(aliasAdd).Success)
                {
                    if (verifiedList.Length > 0)
                    {
                        verifiedList.Append(",");
                    }

                    verifiedList.Append(aliasAdd);
                }
                else
                {
                    ok = false;
                }
            }

            cleanedAliases = verifiedList.ToString();
            return ok;
        }

        private void Name_Unfocused(object sender, FocusEventArgs e)
        {
            string domain = (sender as Xamarin.Forms.Entry).Text;

            if (domain != null && domain.Length < 3)
            {
                /*
                 * the domain name can contain wildcards i.e. *.example.com
                 */

                NameValidator.IsValid = false;
                NameValidator.Message = "Enter a valid domain name";
            }
        }

        private void Aliases_Unfocused(object sender, FocusEventArgs e)
        {
            string aliases = (sender as Xamarin.Forms.Entry).Text;

            if (aliases != null && aliases.Length > 0)
            {
                try
                {
                    string cleanedAliases;
                    AliasesValidator.IsValid = VerifyAliases(
                        aliases, out cleanedAliases);
                    Aliases.Text = cleanedAliases;
                }
                catch (Exception)
                {
                    AliasesValidator.IsValid = false;
                }

                if (!AliasesValidator.IsValid)
                {
                    AliasesValidator.Message = "Enter a list of valid domains";
                }
            }
        }
        
        private async Task<bool> DoesIPMatchAppIPAsync()
        {
            Nest.Model.AppDomain defaultDomain = (from domain in AppViewModel.DomainViewModel.Domains
                                                     where domain.Default == true
                                                     select domain).First();

            if (!await Dns.IsDomainIPAsync(Name.Text, defaultDomain.IPAddress))
            {
                IsServiceActive = false;
                await ErrorHandler.ExceptionAsync(this, "The domain name " + Name.Text +
                    " currently does not resolve to " + defaultDomain.IPAddress +
                    ". Make sure to update the DNS");
                return false;
            }

            string unmatchedAlias = await Dns.GetUnmatchedDomainAliasIPAsync(
                Aliases.Text, defaultDomain.IPAddress);

            if (unmatchedAlias != null)
            {
                IsServiceActive = false;
                await ErrorHandler.ExceptionAsync(this, "The alias " + unmatchedAlias +
                    " currently does not resolve to " + defaultDomain.IPAddress +
                    ". Make sure to update the DNS");
                return false;
            }

            return true;
        }

        private void Validate()
        {
            AppViewModel.DomainViewModel.Validated = false;
            AppViewModel.DomainViewModel.CanUpdate = false;

            if (TagValidator != null)
            {
                /* used to enable the add function. a domain can
                 * be added only if valid fields and no list item 
                 * has been selected and currenly receivng focus.
                 */
                AppViewModel.DomainViewModel.Validated = (
                    TagValidator.IsValid &&
                    NameValidator.IsValid &&
                    AliasesValidator.IsValid                      
                );

                /* used to enable the update function. a domain can
                 * be updaed only if valid fields has been selected 
                 * and an item from a list is selected.
                 */
                AppViewModel.DomainViewModel.CanUpdate =
                    AppViewModel.DomainViewModel.Validated &&
                   AppDomainsList.SelectedItem != null &&
                    !(AppDomainsList.SelectedItem as Nest.Model.AppDomain).Default;
            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                AppDomainsList.SelectedItem = null;

                SetDefaults();
                Validate();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }
        }

        private void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }

        private void OnClearButtonClicked(object sender, EventArgs e)
        {
            Clear();
        }

        async private void OnRefreshButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await Process(AppDomainsList.SelectedItem as Nest.Model.AppDomain, true,
                    AppViewModel.DomainViewModel.QueryDomainAsync
                );

                SetDefaults();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnAddButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                var existDomains = from domain in AppViewModel.DomainViewModel.Domains
                                 where domain.Tag == Tag.Text
                                 select domain;
                if (existDomains.Any())
                {
                    IsServiceActive = false;
                    await ErrorHandler.ExceptionAsync(this, "The domain with this tag already exist");
                    return;
                }

                existDomains = from domain in AppViewModel.DomainViewModel.Domains
                                   where domain.Name == Name.Text
                               select domain;
                if (existDomains.Any())
                {
                    IsServiceActive = false;
                    await ErrorHandler.ExceptionAsync(this, "The domain already exist");
                    return;
                }

                if (!await DoesIPMatchAppIPAsync())
                {
                    return;
                }

                Nest.Model.AppDomain newDomain = CopyUpdate(new Nest.Model.AppDomain());
                if (newDomain != null)
                {
                    await AppViewModel.DomainViewModel.CreateDomainAsync(newDomain);
                    SetPrimaryDomain(newDomain);
                }

                Clear();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnUpdateButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                Nest.Model.AppDomain updatingDomain = AppDomainsList.SelectedItem as Nest.Model.AppDomain;

                if (updatingDomain.Default)
                {
                    IsServiceActive = false;
                    await ErrorHandler.ExceptionAsync(this, "Cannot make changes to the default domain");
                    return;
                }

                var existDomains = from domain in AppViewModel.DomainViewModel.Domains
                                   where domain.Tag == Tag.Text && domain.Id != updatingDomain.Id 
                                   select domain;
                if (existDomains.Any())
                {
                    IsServiceActive = false;
                    await ErrorHandler.ExceptionAsync(this, "The domain with this tag already exist");
                    return;
                }

                existDomains = from domain in AppViewModel.DomainViewModel.Domains
                               where domain.Name == Name.Text && domain.Id != updatingDomain.Id
                               select domain;
                if (existDomains.Any())
                {
                    IsServiceActive = false;
                    await ErrorHandler.ExceptionAsync(this, "The domain already exist");
                    return;
                }

                if (!await DoesIPMatchAppIPAsync())
                {
                    return;
                }

                Nest.Model.AppDomain updateDomain = CopyUpdate(updatingDomain);
                if (updateDomain != null)
                {
                    bool proceed = true;

                    if (updateDomain.Certificate != null)
                    {
                        proceed = await DisplayAlert("Nester", "This will clear the existing SSL certificate. Proceed?", "Yes", "No");
                    }

                    if (proceed)
                    {
                        updateDomain.Certificate = null;

                        await Process(updateDomain, true,
                            AppViewModel.DomainViewModel.UpdateDomainAsync
                        );
                        SetDefaults();

                        if (!AppViewModel.DomainViewModel.Domains.Where(x => x.Primary == true).Any())
                        {
                            SetPrimaryDomain(AppViewModel.DomainViewModel.Domains.First());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnRemoveButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await Process(AppDomainsList.SelectedItem as Nest.Model.AppDomain, true,
                    AppViewModel.DomainViewModel.RemoveDomainAsync,
                       new Func<Nest.Model.AppDomain, Task<bool>>(
                            async (obj) =>
                            {
                                return await DisplayAlert("Nester", "Would you like to remove this domain", "Yes", "No");
                            }
                        )
                );

                Clear();
                SetPrimaryDomain(AppViewModel.DomainViewModel.Domains.First());
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                // Head back to homepage if the 
                // page was called from here
                MainSideView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }
    }
}


