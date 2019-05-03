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

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Inkton.Nester;
using Inkton.Nester.ViewModels;
using DeployApp = Nester.Deploy.App;

namespace Inkton.Nester.Views
{
    public partial class MainSideView : MasterDetailPage
    {
        private View _currentView;
        private Dictionary<long, AppView> _viewCache;

        public MainSideView()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.UWP)
            {
                MasterBehavior = MasterBehavior.Popover;
            }

            _viewCache = new Dictionary<long, AppView>();
        }

        public AppView GetAppView(long appId)
        {
            AppView appView = null;

            if (_viewCache.ContainsKey(appId))
            {
                appView = _viewCache[appId];
            }

            return appView;
        }

        public View CurrentView
        {
            get
            {
                return _currentView;
            }
        }

        public void ShowEntry()
        {
            (Master as HomeView).MainSideView = this;

            ((Detail as NavigationPage).CurrentPage as BannerView).ShowProgress = false;
            ((Detail as NavigationPage).CurrentPage as BannerView).MainSideView = this;

            EntryView entry = new EntryView();
            entry.MainSideView = this;
            Detail.Navigation.PushAsync(entry);
        }

        public void CreateRootView(View view)
        {
            Detail = new NavigationPage(view);
            _currentView = view;
            _currentView.MainSideView = this;
        }

        public async void StackViewAsync(View view)
        {
            await (Detail as NavigationPage).PushAsync(view);
            _currentView = view;
            _currentView.MainSideView = this;
        }

        public async void CurrentLevelViewAsync(View view)
        {
            Detail.Navigation.InsertPageBefore(view, _currentView);
            await (Detail as NavigationPage).PopAsync();
            _currentView = view;
            _currentView.MainSideView = this;
        }

        public async void UnstackViewAsync()
        {
            await (Detail as NavigationPage).PopAsync();
            _currentView = (Detail as NavigationPage).CurrentPage as View;
        }

        public void UpdateView(AppViewModel loadAppViewModel = null)
        {
            BaseViewModels baseModels = ((DeployApp)Application.Current)
                .BaseViewModels;

            if (loadAppViewModel == null)
            {
                loadAppViewModel = baseModels.AppCollectionViewModel
                    .AppModels.FirstOrDefault();

                if (loadAppViewModel == null)
                {
                    BannerView view = new BannerView();
                    view.ShowProgress = false;
                    CreateRootView(view);
                    return;
                }                
            }

            bool isAppViewCurrent = (
                _currentView != null && 
                _currentView is AppView );

            bool changeView = false;

            if (isAppViewCurrent)
            {
                if (_currentView.AppViewModel.EditApp.Id
                        != loadAppViewModel.EditApp.Id)
                {
                    changeView = true;
                }
            }
            else
            {
                changeView = true;
            }

            if (changeView)
            {
                AppView appView = GetAppView(loadAppViewModel.EditApp.Id); 

                if (appView == null)
                {                  
                    appView = new AppView(loadAppViewModel);

                    System.Diagnostics.Debug.WriteLine(
                        string.Format("Created a view for", loadAppViewModel.EditApp.Tag));

                    if (loadAppViewModel.EditApp.Id > 0)
                    {
                        Task.Run(async () => {
                            await appView.AppViewModel.InitAsync();
                        });
                    }

                    _viewCache[loadAppViewModel.EditApp.Id] = appView;
                }

                CreateRootView(appView);
            }
        }

        async public Task ReloadAsync()
        {
            if (_currentView != null && _currentView is AppView)
            {
                await (_currentView as AppView)
                    .AppViewModel.ReloadAsync();
            }
        }
    }
}
    