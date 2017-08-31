using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Nester.Views
{
    public partial class SettingsView : TabbedPage
    {
        protected Views.AuthViewModel _authViewModel;
        protected Views.AppViewModel _appViewModel;

        public SettingsView()
        {
            InitializeComponent();
        }

        public AuthViewModel AuthViewModel
        {
            get { return _authViewModel; }
            set
            {
                _authViewModel = value;
                Children.All(child =>
                {
                    if (child is UserView)
                    {
                        (child as UserView).AuthViewModel = _authViewModel;
                    }

                    if (child is AuthView)
                    {
                        (child as AuthView).AuthViewModel = _authViewModel;
                    }

                    if (child is PaymentView)
                    {
                        (child as PaymentView).AuthViewModel = _authViewModel;
                    }

                    if (child is UserHistoryView)
                    {
                        (child as UserHistoryView).AuthViewModel = _authViewModel;
                    }

                    return true;
                }
                );
            }
        }

        public AppViewModel AppViewModel
        {
            get { return _appViewModel; }
            set
            {
                _appViewModel = value;
                Children.All(child =>
                {
                    if (child is UserView)
                    {
                        (child as UserView).AppViewModel = _appViewModel;
                    }

                    if (child is AuthView)
                    {
                        (child as AuthView).AppViewModel = _appViewModel;
                    }

                    if (child is PaymentView)
                    {
                        (child as PaymentView).AppViewModel = _appViewModel;
                    }

                    if (child is UserHistoryView)
                    {
                        (child as UserHistoryView).AppViewModel = _appViewModel;
                    }

                    return true;
                }
                );
            }
        }

        protected override void OnAppearing()
        {
            Children.All(x =>
            {
                if (x is Views.UserView)
                {
                    (x as Views.UserView).AuthViewModel.WizardMode = false;
                }
                return true;
            });

            base.OnAppearing();
        }
    }
}
