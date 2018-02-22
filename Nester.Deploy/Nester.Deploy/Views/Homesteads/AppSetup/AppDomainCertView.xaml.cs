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
using Inkton.Nester.Models;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class AppDomainCertView : View
    {
        private Regex _domainVerifier;

        public AppDomainCertView(BaseModels baseModels)
        {
            InitializeComponent();

            BaseModels = baseModels;

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

            BindingContext = _baseModels.TargetViewModel.DomainModel;
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

            if (_baseModels.TargetViewModel.DomainModel.EditDomain.Name != null &&
                !_domainVerifier.Match(_baseModels.TargetViewModel.DomainModel.EditDomain.Name).Success)
            {
                canAddFreeCert = false;
            }
            else
            {
                if (_baseModels.TargetViewModel.DomainModel.EditDomain.Aliases != null &&
                    _baseModels.TargetViewModel.DomainModel.EditDomain.Aliases.Length > 0)
                {
                    string[] aliasArray = _baseModels.TargetViewModel.DomainModel.EditDomain.Aliases.Split(new char[] { ',', ' ' });

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

            if (_baseModels.TargetViewModel.DomainModel.EditDomain.Certificate != null)
            {
                string type = _baseModels.TargetViewModel.DomainModel.EditDomain.Certificate.Type;
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
                PrivateKey.Text = _baseModels.TargetViewModel.DomainModel.EditDomain.Certificate.PrivateKey;
                Chain.Text = _baseModels.TargetViewModel.DomainModel.EditDomain.Certificate.CertificateChain;
            }

            Validate();
        }

        private void Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            Validate();
        }

        private void Validate()
        {
            _baseModels.TargetViewModel.DomainModel.CanUpdate = false;
            _baseModels.TargetViewModel.DomainModel.Validated = false;

            string type = Type.SelectedItem as string;

            if (type == "Custom")
            {
                PrivateKey.IsEnabled = true;
                Chain.IsEnabled = true;

                _baseModels.TargetViewModel.DomainModel.Validated = (
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

                _baseModels.TargetViewModel.DomainModel.Validated = true;
            }

            /* used to enable the update function. a certificate can
                * be updaed only if valid fields has been selected 
                * and an item from a list is selected.
                */
            _baseModels.TargetViewModel.DomainModel.CanUpdate = _baseModels.TargetViewModel.DomainModel.Validated;
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
                if (_baseModels.TargetViewModel.DomainModel.EditDomain.Certificate != null)
                {
                    await Process(_baseModels.TargetViewModel.DomainModel.EditDomain.Certificate, true,
                        _baseModels.TargetViewModel.DomainModel.RemoveDomainCertificateAsync
                    );
                }

                string type = Type.SelectedItem as string;

                if (type != "None")
                {
                    AppDomainCertificate cert = new AppDomainCertificate();
                    cert.AppDomain = _baseModels.TargetViewModel.DomainModel.EditDomain;
                    cert.Tag = _baseModels.TargetViewModel.DomainModel.EditDomain.Tag;
                    cert.Type = type.ToLower();
                    cert.PrivateKey = PrivateKey.Text;
                    cert.CertificateChain = Chain.Text;

                    await Process(cert, true,
                        _baseModels.TargetViewModel.DomainModel.CreateDomainCertificateAsync
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
                MainSideView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}


