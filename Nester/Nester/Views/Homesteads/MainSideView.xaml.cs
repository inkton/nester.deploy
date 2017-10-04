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

            Detail = new BannerView(BannerView.Status.Initializing);

            _viewLoader = new Func<Views.View, bool>(LoadView);
            Home.Init(_viewLoader);
        }

        public AuthViewModel AuthViewModel
        {
            get { return Home.AuthViewModel; }
            set { Home.AuthViewModel = value; }
        }

        public AppViewModel AppViewModel
        {
            get { return Home.AppViewModel; }
            set { Home.AppViewModel = value; }
        }

        protected bool LoadView(Views.View view)
        {
            view.LoadView = _viewLoader;
            view.MasterDetailPage = this;

            if (view is AppView)
            {
                (view as AppView).GetAnalyticsAsync();
            }

            Detail = view;
            
            return true;
        }
    }
}
    