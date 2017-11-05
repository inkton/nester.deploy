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
    public partial class AppDomainCertView : Nester.Views.View
    {
        private Regex _domainVerifier;

        public AppDomainCertView(AppViewModel appViewModel)
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                                Type,
                                ButtonUpdate,
                                ButtonDone
                });

            _appViewModel = appViewModel;

            BindingContext = _appViewModel.DomainModel;

            //Name.Unfocused += Name_Unfocused;
            Type.SelectedIndexChanged += Type_SelectedIndexChanged;

            Chain.TextChanged += Chain_TextChanged;
            PrivateKey.TextChanged += PrivateKey_TextChanged;

            _domainVerifier = new Regex(
                 @"^((?!-)[A-Za-z0-9-]{1,63}(?<!-)\.)+[A-Za-z]{2,6}$"
              , RegexOptions.Singleline | RegexOptions.IgnoreCase);

            UpdateTypeSelection();
        }

        private void PrivateKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            Validate();
        }

        private void Chain_TextChanged(object sender, TextChangedEventArgs e)
        {
            Validate();
        }

        private void UpdateTypeSelection()
        {
            bool enable = false;

            Type.Items.Clear();
            Type.Items.Add("None");

            /* free certs can only be added to non-wildcard, 
             * no regex containing, properly formed domain 
             * names */
            bool canAddFreeCert = true;

            if (_appViewModel.DomainModel.EditDomain.Name != null &&
                !_domainVerifier.Match(_appViewModel.DomainModel.EditDomain.Name).Success)
            {
                canAddFreeCert = false;
            }
            else
            {
                if (_appViewModel.DomainModel.EditDomain.Aliases != null &&
                    _appViewModel.DomainModel.EditDomain.Aliases.Length > 0)
                {
                    string[] aliasArray = _appViewModel.DomainModel.EditDomain.Aliases.Split(new char[] { ',', ' ' });

                    foreach (string alias in aliasArray)
                    {
                        if (alias != null &&
                            !_domainVerifier.Match(alias).Success)
                        {
                            canAddFreeCert = false;
                        }
                    }
                }
            }

            if (canAddFreeCert)
            {
                // wildcard domains do not support free certs.
                Type.Items.Add("Free");
            }

            Type.Items.Add("Custom");

            if (_appViewModel.DomainModel.EditDomain.Certificate != null)
            {
                string type = _appViewModel.DomainModel.EditDomain.Certificate.Type;
                type = char.ToUpper(type[0]) + type.Substring(1);

                int index = Type.Items.IndexOf(type);

                if (index >= 0)
                {
                    Type.SelectedIndex = index;
                    enable = (type == "Custom");
                }
                else
                {
                    Type.SelectedIndex = Type.Items.IndexOf("None");
                }
            }
            else
            {
                Type.SelectedIndex = Type.Items.IndexOf("None");
            }

            PrivateKey.IsEnabled = enable;
            Chain.IsEnabled = enable;

            if (!enable)
            {
                PrivateKey.Text = "";
                Chain.Text = "";
            }
            else
            {
                PrivateKey.Text = _appViewModel.DomainModel.EditDomain.Certificate.PrivateKey;
                Chain.Text = _appViewModel.DomainModel.EditDomain.Certificate.CertificateChain;
            }

            Validate();
        }

        private void Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            Validate();
        }

        private void Validate()
        {
            _appViewModel.DomainModel.CanUpdate = false;
            _appViewModel.DomainModel.Validated = false;

            string type = Type.SelectedItem as string;

            if (type == "Custom")
            {
                PrivateKey.IsEnabled = true;
                Chain.IsEnabled = true;

                _appViewModel.DomainModel.Validated = (
                        PrivateKey.Text != null &&
                        PrivateKey.Text.Length > 0 &&
                        Chain.Text != null &&
                        Chain.Text.Length > 0
                );
            }
            else
            {
                PrivateKey.IsEnabled = false;
                Chain.IsEnabled = false;

                _appViewModel.DomainModel.Validated = true;
            }

            /* used to enable the update function. a certificate can
                * be updaed only if valid fields has been selected 
                * and an item from a list is selected.
                */
            _appViewModel.DomainModel.CanUpdate = _appViewModel.DomainModel.Validated;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
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

        async private void OnUpdateDomainButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                if (_appViewModel.DomainModel.EditDomain.Certificate != null)
                {
                    await Process(_appViewModel.DomainModel.EditDomain.Certificate, true,
                        _appViewModel.DomainModel.RemoveDomainCertificateAsync
                    );
                }

                string type = Type.SelectedItem as string;

                if (type != "None")
                {
                    Admin.AppDomainCertificate cert = new Admin.AppDomainCertificate();
                    cert.AppDomain = _appViewModel.DomainModel.EditDomain;
                    cert.Tag = _appViewModel.DomainModel.EditDomain.Tag;
                    cert.Type = type.ToLower();
                    cert.PrivateKey = PrivateKey.Text;
                    cert.CertificateChain = Chain.Text;

                    await Process(cert, true,
                        _appViewModel.DomainModel.CreateDomainCertificateAsync
                    );
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}


