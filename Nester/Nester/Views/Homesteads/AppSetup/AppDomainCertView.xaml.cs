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
        public AppDomainCertView(AppViewModel appViewModel)
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                                Type,
                                ButtonRefresh,
                                ButtonUpdate,
                                ButtonDone
                });

            _appViewModel = appViewModel;

            BindingContext = _appViewModel.DomainModel;

            //Name.Unfocused += Name_Unfocused;
            Type.SelectedIndexChanged += Type_SelectedIndexChanged;

            Chain.TextChanged += Chain_TextChanged;
            PrivateKey.TextChanged += PrivateKey_TextChanged;

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
            if (_appViewModel.DomainModel.EditDomain.Certificate != null)
            {
                if (_appViewModel.DomainModel.EditDomain.Certificate.Type == "free")
                {
                    Type.SelectedIndex = 0;
                }
                else
                {
                    Type.SelectedIndex = 1;
                }
            }
            else
            {
                Type.SelectedIndex = -1;
            }
        }

        private void Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_appViewModel.DomainModel.EditDomain.Certificate != null)
            {
                if (Type.SelectedIndex == 0)
                {
                    PrivateKey.IsEnabled = false;
                    Chain.IsEnabled = false;

                    _appViewModel.DomainModel.EditDomain.Certificate.Type = "free";
                }
                else
                {
                    PrivateKey.IsEnabled = true;
                    Chain.IsEnabled = true;

                    _appViewModel.DomainModel.EditDomain.Certificate.Type = "custom";
                }
            }

            Validate();
        }

        private void Validate()
        {
            _appViewModel.DomainModel.CanUpdate = false;
            _appViewModel.DomainModel.Validated = false;

            if (_appViewModel != null &&
                _appViewModel.DomainModel.EditDomain != null &&
                _appViewModel.DomainModel.EditDomain.Certificate != null)
            {
                if (_appViewModel.DomainModel.EditDomain.Certificate.Type == "free")
                {
                    _appViewModel.DomainModel.Validated = true;
                }
                else
                {
                    _appViewModel.DomainModel.Validated = (
                           _appViewModel.DomainModel.EditDomain.Certificate.PrivateKey != null &&
                            _appViewModel.DomainModel.EditDomain.Certificate.PrivateKey.Length > 0 &&
                           _appViewModel.DomainModel.EditDomain.Certificate.CertificateChain != null &&
                            _appViewModel.DomainModel.EditDomain.Certificate.CertificateChain.Length > 0
                    );
                }

                /* used to enable the update function. a certificate can
                 * be updaed only if valid fields has been selected 
                 * and an item from a list is selected.
                 */
                _appViewModel.DomainModel.CanUpdate = _appViewModel.DomainModel.Validated;
            }
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

        async private void OnRefreshDomainButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            await Process(_appViewModel.DomainModel.EditDomain, true,
                _appViewModel.DomainModel.QueryDomainAsync
            );

            UpdateTypeSelection();

            IsServiceActive = false;
        }

        async private void OnUpdateDomainButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            await Process(_appViewModel.DomainModel.EditDomain, true,
                _appViewModel.DomainModel.UpdateDomainAsync
            );

            IsServiceActive = false;
        }

        async private void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                await this.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}


