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
    public partial class MainSideView : MasterDetailPage
    {
        protected Func<Views.View, bool> _viewLoader;

        public MainSideView()
        {
            InitializeComponent();

            _viewLoader = new Func<Views.View, bool>(LoadView);
            AppsView.Init(_viewLoader);
        }

        public AuthViewModel AuthViewModel
        {
            get { return AppsView.AuthViewModel; }
            set { AppsView.AuthViewModel = value; }
        }

        public AppViewModel AppViewModel
        {
            get { return AppsView.AppViewModel; }
            set { AppsView.AppViewModel = value; }
        }

        protected bool LoadView(Views.View view)
        {
            view.LoadView = _viewLoader;
            view.MasterDetailPage = this;

            if (view is AppView)
            {
                (view as AppView).GetAnalyticsAsync();
            }

            Detail = new NavigationPage(view);  

            return true;
        }
    }
}
    