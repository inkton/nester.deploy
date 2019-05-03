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
using System.Text.RegularExpressions;
using Xamarin.Forms;
using Inkton.Nest.Model;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Helpers;

namespace Inkton.Nester.Views
{
    public partial class AppDomainCertView : View
    {
        private Regex _domainVerifier;

        public AppDomainCertView(AppViewModel appViewModel)
        {
            InitializeComponent();

            AppViewModel = appViewModel;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                                Type,
                                ButtonUpdate,
                                ButtonDone
                });

            //Name.Unfocused += Name_Unfocused;
            Type.SelectedIndexChanged += Type_SelectedIndexChanged;

            Chain.TextChanged += Chain_TextChanged;
            PrivateKey.TextChanged += PrivateKey_TextChanged;

            _domainVerifier = new Regex(
                 @"^((?!-)[A-Za-z0-9-]{1,63}(?<!-)\.)+[A-Za-z]{2,6}$"
              , RegexOptions.Singleline | RegexOptions.IgnoreCase);

            UpdateTypeSelection();
        }

        public override void UpdateBindings()
        {
            base.UpdateBindings();

            BindingContext = AppViewModel.DomainViewModel;
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

            if (AppViewModel.DomainViewModel.EditDomain.Name != null &&
                !_domainVerifier.Match(AppViewModel.DomainViewModel.EditDomain.Name).Success)
            {
                canAddFreeCert = false;
            }
            else
            {
                if (AppViewModel.DomainViewModel.EditDomain.Aliases != null &&
                    AppViewModel.DomainViewModel.EditDomain.Aliases.Length > 0)
                {
                    string[] aliasArray = AppViewModel.DomainViewModel.EditDomain.Aliases.Split(new char[] { ',', ' ' });

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

            if (AppViewModel.DomainViewModel.EditDomain.Certificate != null)
            {
                string type = AppViewModel.DomainViewModel.EditDomain.Certificate.Type;
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
                PrivateKey.Text = AppViewModel.DomainViewModel.EditDomain.Certificate.PrivateKey;
                Chain.Text = AppViewModel.DomainViewModel.EditDomain.Certificate.CertificateChain;
            }

            Validate();
        }

        private void Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            Validate();
        }

        private void Validate()
        {
            AppViewModel.DomainViewModel.CanUpdate = false;
            AppViewModel.DomainViewModel.Validated = false;

            string type = Type.SelectedItem as string;

            if (type == "Custom")
            {
                PrivateKey.IsEnabled = true;
                Chain.IsEnabled = true;

                AppViewModel.DomainViewModel.Validated = (
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

                AppViewModel.DomainViewModel.Validated = true;
            }

            /* used to enable the update function. a certificate can
                * be updaed only if valid fields has been selected 
                * and an item from a list is selected.
                */
            AppViewModel.DomainViewModel.CanUpdate = AppViewModel.DomainViewModel.Validated;
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
                await ErrorHandler.ExceptionAsync(this, ex);
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
                if (AppViewModel.DomainViewModel.EditDomain.Certificate != null)
                {
                    await Process(AppViewModel.DomainViewModel.EditDomain.Certificate, true,
                         AppViewModel.DomainViewModel.RemoveDomainCertificateAsync
                    );
                }

                string type = Type.SelectedItem as string;

                if (type != "None")
                {
                    AppDomainCertificate cert = new AppDomainCertificate();
                    cert.OwnedBy = AppViewModel.DomainViewModel.EditDomain;
                    cert.Tag = AppViewModel.DomainViewModel.EditDomain.Tag;
                    cert.Type = type.ToLower();
                    cert.PrivateKey = PrivateKey.Text;
                    cert.CertificateChain = Chain.Text;

                    await Process(cert, true,
                        AppViewModel.DomainViewModel.CreateDomainCertificateAsync
                    );
                }
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                MainSideView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }
        }
    }
}


