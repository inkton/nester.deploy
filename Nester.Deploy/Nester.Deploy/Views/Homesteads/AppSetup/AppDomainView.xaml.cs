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
using Inkton.Nester.Models;
using Inkton.Nester.ViewModels;
using Nester.Deploy.Helpers;

namespace Inkton.Nester.Views
{
    public partial class AppDomainView : View
    {
        private Regex _domainVerifier;

        public AppDomainView(BaseModels baseModels)
        {
            InitializeComponent();

            BaseModels = baseModels;

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
            AppDomainsList.SelectionChanged += AppDomainsList_SelectionChanged;
            ButtonCert.Clicked += ButtonCert_Clicked;

            ButtonDone.IsVisible = _baseModels.WizardMode;
            if (_baseModels.WizardMode)
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

            BindingContext = _baseModels.TargetViewModel.DomainModel;
        }

        async private void OnButtonServiceClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.CurrentLevelViewAsync(new AppTierView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
        async private void OnButtonBasicDetailsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                _baseModels.WizardMode = false;
                MainSideView.CurrentLevelViewAsync(new AppBasicDetailView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonAppServices_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.CurrentLevelViewAsync(new AppTierView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnButtonContactsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.CurrentLevelViewAsync(new ContactsView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnButtonNestsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.CurrentLevelViewAsync(new AppNestsView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private void ButtonCert_Clicked(object sender, EventArgs e)
        {
            if (AppDomainsList.SelectedItem != null)
            {
                Nester.Models.AppDomain browseDomain = AppDomainsList.SelectedItem as Nester.Models.AppDomain;
                _baseModels.TargetViewModel.DomainModel.EditDomain = browseDomain;
                AppDomainCertView certView = new AppDomainCertView(_baseModels);
                MainSideView.StackViewAsync(certView);
            }
        }

        private void Clear()
        {
            if (AppDomainsList.SelectedItems.Any())
            {
                AppDomainsList.SelectedItems.RemoveAt(0);
            }

            _baseModels.TargetViewModel.DomainModel.EditDomain = new Nester.Models.AppDomain();
            _baseModels.TargetViewModel.DomainModel.EditDomain.App = _baseModels.TargetViewModel.EditApp;

            SetDefaults();

            Validate();
        }

        private void SetDefaults()
        {
            bool enableEdits = true;

            if (AppDomainsList.SelectedItem != null)
            {
                Nester.Models.AppDomain browseDomain = AppDomainsList.SelectedItem as Nester.Models.AppDomain;
                Cloud.Object.CopyPropertiesTo(browseDomain,
                    _baseModels.TargetViewModel.DomainModel.EditDomain);
                enableEdits = !browseDomain.Default;
                IsPrimary.IsToggled = _baseModels.TargetViewModel
                    .EditApp.PrimaryDomainId == browseDomain.Id;
            }

            Tag.IsEnabled = enableEdits;
            Name.IsEnabled = enableEdits;
            Aliases.IsEnabled = enableEdits;
            IsPrimary.IsEnabled = enableEdits;
        }

        private Nester.Models.AppDomain CopyUpdate(Nester.Models.AppDomain browsDomain)
        {
            browsDomain.App = _baseModels.TargetViewModel.EditApp;
            browsDomain.Tag = Tag.Text;
            browsDomain.Name = Name.Text;
            if (Aliases.Text == null || Aliases.Text.Length == 0)
                Aliases.Text = null;
            browsDomain.Aliases = Aliases.Text;
            browsDomain.Primary = IsPrimary.IsToggled;

            return browsDomain;
        }

        async private void SetPrimaryDomain(Nester.Models.AppDomain priaryDomain)
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
                _baseModels.TargetViewModel.EditApp.PrimaryDomainId = priaryDomain.Id;
                await _baseModels.TargetViewModel.UpdateAppAsync(_baseModels.TargetViewModel.EditApp);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        private void AppDomainsList_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
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
            Nester.Models.AppDomain defaultDomain = (from domain in _baseModels.TargetViewModel.DomainModel.Domains
                                                     where domain.Default == true
                                                     select domain).First();

            if (!await Dns.IsDomainIPAsync(Name.Text, defaultDomain.IPAddress))
            {
                IsServiceActive = false;
                await DisplayAlert("Nester", "The domain name " + Name.Text +
                    " currently does not resolve to " + defaultDomain.IPAddress +
                    ". Make sure to update the DNS", "OK");
                return false;
            }

            string unmatchedAlias = await Dns.GetUnmatchedDomainAliasIPAsync(
                Aliases.Text, defaultDomain.IPAddress);

            if (unmatchedAlias != null)
            {
                IsServiceActive = false;
                await DisplayAlert("Nester", "The alias " + unmatchedAlias +
                    " currently does not resolve to " + defaultDomain.IPAddress +
                    ". Make sure to update the DNS", "OK");
                return false;
            }

            return true;
        }

        private void Validate()
        {
            _baseModels.TargetViewModel.DomainModel.Validated = false;
            _baseModels.TargetViewModel.DomainModel.CanUpdate = false;

            if (TagValidator != null)
            {
                /* used to enable the add function. a domain can
                 * be added only if valid fields and no list item 
                 * has been selected and currenly receivng focus.
                 */
                _baseModels.TargetViewModel.DomainModel.Validated = (
                    TagValidator.IsValid &&
                    NameValidator.IsValid &&
                    AliasesValidator.IsValid                      
                );

                /* used to enable the update function. a domain can
                 * be updaed only if valid fields has been selected 
                 * and an item from a list is selected.
                 */
                _baseModels.TargetViewModel.DomainModel.CanUpdate =
                    _baseModels.TargetViewModel.DomainModel.Validated &&
                   AppDomainsList.SelectedItem != null &&
                    !(AppDomainsList.SelectedItem as Nester.Models.AppDomain).Default;
            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                if (AppDomainsList.SelectedItems.Any())
                {
                    AppDomainsList.SelectedItems.RemoveAt(0);
                }

                SetDefaults();
                Validate();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
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
                await Process(AppDomainsList.SelectedItem as Nester.Models.AppDomain, true,
                    _baseModels.TargetViewModel.DomainModel.QueryDomainAsync
                );

                SetDefaults();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnAddButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                var existDomains = from domain in _baseModels.TargetViewModel.DomainModel.Domains
                                 where domain.Tag == Tag.Text
                                 select domain;
                if (existDomains.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The domain with this tag already exist", "OK");
                    return;
                }

                existDomains = from domain in _baseModels.TargetViewModel.DomainModel.Domains
                                   where domain.Name == Name.Text
                               select domain;
                if (existDomains.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The domain already exist", "OK");
                    return;
                }

                if (!await DoesIPMatchAppIPAsync())
                {
                    return;
                }

                Nester.Models.AppDomain newDomain = CopyUpdate(new Nester.Models.AppDomain());
                if (newDomain != null)
                {
                    await _baseModels.TargetViewModel.DomainModel.CreateDomainAsync(newDomain);
                    SetPrimaryDomain(newDomain);
                }

                Clear();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnUpdateButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                Nester.Models.AppDomain updatingDomain = AppDomainsList.SelectedItem as Nester.Models.AppDomain;

                if (updatingDomain.Default)
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "Cannot make changes to the default domain", "OK");
                    return;
                }

                var existDomains = from domain in _baseModels.TargetViewModel.DomainModel.Domains
                                   where domain.Tag == Tag.Text && domain.Id != updatingDomain.Id 
                                   select domain;
                if (existDomains.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The domain with this tag already exist", "OK");
                    return;
                }

                existDomains = from domain in _baseModels.TargetViewModel.DomainModel.Domains
                               where domain.Name == Name.Text && domain.Id != updatingDomain.Id
                               select domain;
                if (existDomains.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The domain already exist", "OK");
                    return;
                }

                if (!await DoesIPMatchAppIPAsync())
                {
                    return;
                }

                Nester.Models.AppDomain updateDomain = CopyUpdate(updatingDomain);
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
                            _baseModels.TargetViewModel.DomainModel.UpdateDomainAsync
                        );
                        SetDefaults();

                        if (!_baseModels.TargetViewModel.DomainModel.Domains.Where(x => x.Primary == true).Any())
                        {
                            SetPrimaryDomain(_baseModels.TargetViewModel.DomainModel.Domains.First());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnRemoveButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await Process(AppDomainsList.SelectedItem as Nester.Models.AppDomain, true,
                    _baseModels.TargetViewModel.DomainModel.RemoveDomainAsync,
                       new Func<Nester.Models.AppDomain, Task<bool>>(
                            async (obj) =>
                            {
                                return await DisplayAlert("Nester", "Would you like to remove this domain", "Yes", "No");
                            }
                        )
                );

                Clear();
                SetPrimaryDomain(_baseModels.TargetViewModel.DomainModel.Domains.First());
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
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
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
    }
}


