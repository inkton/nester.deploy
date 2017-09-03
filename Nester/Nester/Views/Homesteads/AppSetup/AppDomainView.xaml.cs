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

            if (domain != null && !_domainVerifier.Match(domain).Success)
            {
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
                    await Process(updateDomain, true,
                        _appViewModel.DomainModel.UpdateDomainAsync
                    );

                    SetDefaults();
                    //SetPrimaryDomain();
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


