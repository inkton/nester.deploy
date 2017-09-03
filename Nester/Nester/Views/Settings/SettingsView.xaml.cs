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
                    (child as Views.View).AuthViewModel = _authViewModel;
                    return true;
                });
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
                    (child as Views.View).AppViewModel = _appViewModel;
                    return true;
                    }
                );
            }
        }

        protected override void OnAppearing()
        {
            Children.All(x =>
            {
                if (x is Views.View)
                {
                    (x as Views.View).AppViewModel.WizardMode = false;
                    (x as Views.View).AuthViewModel.WizardMode = false;
                }       
                return true;
            });

            base.OnAppearing();
        }
    }
}
