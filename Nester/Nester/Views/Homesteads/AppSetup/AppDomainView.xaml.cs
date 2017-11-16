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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Nester.Views
{
    public partial class AppDomainView : Nester.Views.View
    {
        private Regex _domainVerifier;

        public AppDomainView(AppViewModel appViewModel)
        {
            InitializeComponent();

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

            _appViewModel = appViewModel;

            BindingContext = _appViewModel.DomainModel;

            Aliases.Unfocused += Aliases_Unfocused;
            Name.Unfocused += Name_Unfocused;
            AppDomainsList.SelectionChanged += AppDomainsList_SelectionChanged;
            ButtonCert.Clicked += ButtonCert_ClickedAsync;

            // Thanks - http://stackoverflow.com/questions/10306690/domain-name-validation-with-regex
            _domainVerifier = new Regex(
                @"^((?!-)[A-Za-z0-9-]{1,63}(?<!-)\.)+[A-Za-z]{2,6}$"
             , RegexOptions.Singleline | RegexOptions.IgnoreCase);

            Clear();
        }

        async private void OnButtonBasicDetailsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                LoadView(new AppBasicDetailView(_appViewModel));
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
                await _appViewModel.ContactModel.InitAsync();

                LoadView(new ContactsView(_appViewModel));
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
                await _appViewModel.NestModel.InitAsync();

                LoadView(new AppNestsView(_appViewModel));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonCert_ClickedAsync(object sender, EventArgs e)
        {
            if (AppDomainsList.SelectedItem != null)
            {
                Admin.AppDomain browseDomain = AppDomainsList.SelectedItem as Admin.AppDomain;
                _appViewModel.DomainModel.EditDomain = browseDomain;
                await Navigation.PushAsync(new AppDomainCertView(_appViewModel));
            }
        }

        private void Clear()
        {
            if (AppDomainsList.SelectedItems.Any())
            {
                AppDomainsList.SelectedItems.RemoveAt(0);
            }

            _appViewModel.DomainModel.EditDomain = new Admin.AppDomain();
            _appViewModel.DomainModel.EditDomain.App = _appViewModel.EditApp;

            SetDefaults();

            Validate();
        }

        private void SetDefaults()
        {
            bool enableEdits = true;

            if (AppDomainsList.SelectedItem != null)
            {
                Admin.AppDomain browseDomain = AppDomainsList.SelectedItem as Admin.AppDomain;
                Utils.Object.CopyPropertiesTo(browseDomain,
                    _appViewModel.DomainModel.EditDomain);
                enableEdits = !browseDomain.Default;
            }

            Tag.IsEnabled = enableEdits;
            Name.IsEnabled = enableEdits;
            Aliases.IsEnabled = enableEdits;
            IsPrimary.IsEnabled = enableEdits;
        }

        private Admin.AppDomain CopyUpdate(Admin.AppDomain browsDomain)
        {
            browsDomain.App = _appViewModel.EditApp;
            browsDomain.Tag = Tag.Text;
            browsDomain.Name = Name.Text;
            if (Aliases.Text == null || Aliases.Text.Length == 0)
                Aliases.Text = null;
            browsDomain.Aliases = Aliases.Text;
            browsDomain.Primary = IsPrimary.IsToggled;

            return browsDomain;
        }

        async private void SetPrimaryDomain(Admin.AppDomain priaryDomain)
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
                _appViewModel.EditApp.PrimaryDomainId = priaryDomain.Id;
                await _appViewModel.UpdateAppAsync(_appViewModel.EditApp);
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
                if (alias != null && alias.Length >0 && 
                    _domainVerifier.Match(alias).Success)
                {
                    if (verifiedList.Length > 0)
                    {
                        verifiedList.Append(" ");
                    }

                    verifiedList.Append(alias);
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

        private void Validate()
        {
            _appViewModel.DomainModel.Validated = false;
            _appViewModel.DomainModel.CanUpdate = false;

            if (_appViewModel != null)
            {
                /* used to enable the add function. a domain can
                 * be added only if valid fields and no list item 
                 * has been selected and currenly receivng focus.
                 */
                _appViewModel.DomainModel.Validated = (
                    TagValidator.IsValid &&
                    NameValidator.IsValid &&
                    AliasesValidator.IsValid                      
                );

                /* used to enable the update function. a domain can
                 * be updaed only if valid fields has been selected 
                 * and an item from a list is selected.
                 */
                _appViewModel.DomainModel.CanUpdate =
                    _appViewModel.DomainModel.Validated &&
                   AppDomainsList.SelectedItem != null &&
                    !(AppDomainsList.SelectedItem as Admin.AppDomain).Default;
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
                await Process(AppDomainsList.SelectedItem as Admin.AppDomain, true,
                    _appViewModel.DomainModel.QueryDomainAsync
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
                var existDomains = from domain in _appViewModel.DomainModel.Domains
                                 where domain.Tag == Tag.Text
                                 select domain;
                if (existDomains.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The domain with this tag already exist", "OK");
                    return;
                }

                existDomains = from domain in _appViewModel.DomainModel.Domains
                                   where domain.Name == Name.Text
                               select domain;
                if (existDomains.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The domain already exist", "OK");
                    return;
                }

                Admin.AppDomain defaultDomain = (from domain in _appViewModel.DomainModel.Domains
                                                 where domain.Default == true
                                                 select domain).First();
                string wildcardStripped = Name.Text;

                if (wildcardStripped.StartsWith("*."))
                {
                    wildcardStripped = Name.Text.Remove(0, 2);
                }

                string ip = await ThisUI.NesterService.GetIPAsync(wildcardStripped);

                if (ip == null || ip != defaultDomain.Ip)
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The domain name "+ wildcardStripped + 
                        " currently does not resolve to " + defaultDomain.Ip + 
                        ". Make sure to update the DNS", "OK");
                    return;
                }

                if (Aliases.Text != null)
                {
                    foreach (string alias in Aliases.Text.Split(' '))
                    {
                        ip = await ThisUI.NesterService.GetIPAsync(alias);

                        if (ip == null || ip != defaultDomain.Ip)
                        {
                            IsServiceActive = false;
                            await DisplayAlert("Nester", "The alias " + alias +
                                " currently does not resolve to " + defaultDomain.Ip +
                                ". Make sure to update the DNS", "OK");
                            return;
                        }
                    }
                }

                Admin.AppDomain newDomain = CopyUpdate(new Admin.AppDomain());
                if (newDomain != null)
                {
                    await _appViewModel.DomainModel.CreateDomainAsync(newDomain);
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
                Admin.AppDomain updatingDomain = AppDomainsList.SelectedItem as Admin.AppDomain;

                if (updatingDomain.Default)
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "Cannot make changes to the default domain", "OK");
                    return;
                }

                var existDomains = from domain in _appViewModel.DomainModel.Domains
                                   where domain.Tag == Tag.Text && domain.Id != updatingDomain.Id 
                                   select domain;
                if (existDomains.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The domain with this tag already exist", "OK");
                    return;
                }

                existDomains = from domain in _appViewModel.DomainModel.Domains
                               where domain.Name == Name.Text && domain.Id != updatingDomain.Id
                               select domain;
                if (existDomains.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The domain already exist", "OK");
                    return;
                }

                Admin.AppDomain updateDomain = CopyUpdate(updatingDomain);
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
                            _appViewModel.DomainModel.UpdateDomainAsync
                        );
                        SetDefaults();

                        if (!_appViewModel.DomainModel.Domains.Where(x => x.Primary == true).Any())
                        {
                            SetPrimaryDomain(_appViewModel.DomainModel.Domains.First());
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
                await Process(AppDomainsList.SelectedItem as Admin.AppDomain, true,
                    _appViewModel.DomainModel.RemoveDomainAsync,
                       new Func<Admin.AppDomain, Task<bool>>(
                            async (obj) =>
                            {
                                return await DisplayAlert("Nester", "Would you like to remove this domain", "Yes", "No");
                            }
                        )
                );

                Clear();
                SetPrimaryDomain(_appViewModel.DomainModel.Domains.First());
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
                if (_appViewModel.WizardMode)
                {
                    // if currently trvelling back and forth on the 
                    // app wizard - close the wizard
                    this.Navigation.RemovePage(
                        this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);

                    // Pop contact view
                    this.Navigation.RemovePage(
                        this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);

                    // Pop this to go to Homeview <->
                    await this.Navigation.PopAsync();
                }
                else
                {
                    if (_appViewModel.EditApp.IsDeployed)
                    {
                        await DisplayAlert("Nester", "Make sure to re-deploy the app for changes to take effect", "OK");
                    }

                    // Head back to homepage if the 
                    // page was called from here
                    LoadHomeView();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
    }
}


