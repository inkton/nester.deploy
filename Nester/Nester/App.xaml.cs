using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace Nester
{
    public partial class NesterUI : Application
    {
        private Admin.User _user;
        private Cloud.INesterService _nester;
        private Cache.IStorageService _storage;
        private Views.MainSideView _homeView;

        public NesterUI()
        {
            InitializeComponent();

            _user = new Admin.User();
            _nester = DependencyService.Get<Cloud.INesterService>();
            _storage = DependencyService.Get<Cache.IStorageService>();

            _storage.Clear();

            _homeView = new Views.MainSideView();

            MainPage = new NavigationPage(
                new Views.EntryView());
        }

        public Views.MainSideView HomeView
        {
            get { return _homeView; }
        }

        public Views.AppCollectionViewModel AppCollectionViewModel
        {
            get { return _homeView.AppViewModel as Views.AppCollectionViewModel; }
        }

        public Admin.User User
        {
            get { return _user; }
        }

        public Cloud.INesterService NesterService
        {
            get { return _nester; }
        }

        public Cache.IStorageService StorageService
        {
            get { return _storage; }
        }
    }
}
