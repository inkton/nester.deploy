using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nester.Admin;
using Xamarin.Forms;

namespace Nester.Views
{
    public partial class HomeView : TabbedPage
    {
        Views.AuthViewModel _authViewModel = null;
        Views.AppViewModel _appViewModel = null;
        MainSideView _mainSideView = null;

        public HomeView()
        {
            InitializeComponent();

            Children.All(child =>
            {
                if (child is MainSideView)
                {
                    _mainSideView = (child as MainSideView);

                    _authViewModel = _mainSideView.AuthViewModel;
                    _appViewModel = _mainSideView.AppViewModel;
                }
                return true;
            });

            Children.All(child =>
            {
                if (child is SettingsView)
                {
                    (child as SettingsView).AuthViewModel = _authViewModel;
                    (child as SettingsView).AppViewModel = _appViewModel;
                }
                return true;
            });

        }

        public AuthViewModel AuthViewModel
        {
            get
            {
                return _authViewModel;
            }
            set
            {
                _authViewModel = value;

                Children.All(child =>
                {
                    if (child is MainSideView)
                    {
                        (child as MainSideView).AuthViewModel = value;
                    }
                                        
                    if (child is SettingsView)
                    {
                        (child as SettingsView).AuthViewModel = value;
                    }

                    return true;
                });
            }
        }

        public AppViewModel AppViewModel
        {
            get {
                return _appViewModel;
            }
            set {
                _appViewModel = value;

                Children.All(child =>
                    {
                        if (child is MainSideView)
                        {
                            (child as MainSideView).AppViewModel = value;
                        }

                        if (child is SettingsView)
                        {
                            (child as SettingsView).AppViewModel = value;
                        }

                        return true;
                    }
                );
            }
        }
    }
}
