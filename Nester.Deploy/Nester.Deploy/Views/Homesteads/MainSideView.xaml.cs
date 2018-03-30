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
using Xamarin.Forms;
using Inkton.Nester.ViewModels;
using System.Collections.Generic;

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

        public void ResetView(AppViewModel appModel = null)
        {
            if (appModel == null)
            {
                BannerView view = new BannerView();
                view.ShowProgress = false;
                CreateRootView(view);
            }
            else
            {
                bool isAppViewCurrent = (
                    _currentView != null && 
                    _currentView is AppView );

                bool createView = false;

                if (isAppViewCurrent)
                {
                    if (_currentView.BaseModels.TargetViewModel.EditApp.Id
                            != appModel.EditApp.Id)
                    {
                        createView = true;
                    }
                }
                else
                {
                    createView = true;
                }

                if (createView)
                {
                    INesterControl nesterControl = 
                        (Application.Current as INesterControl);                        
                    nesterControl.Target = appModel;

                    AppView appView = GetAppView(appModel.EditApp.Id); 

                    if (appView == null)
                    {
                        appView = new AppView(
                             nesterControl.BaseModels);
                        appView.UpdateBindings();

                        if (appModel.EditApp.IsActive)
                        {
                            appView.ReloadAnalytics();
                        }

                        _viewCache[appModel.EditApp.Id] = appView;
                    }

                    CreateRootView(appView);
                }
            }
        }

        public void Reload()
        {
            if (_currentView != null && _currentView is AppView)
            {
                (_currentView as AppView)
                    .BaseModels.TargetViewModel.Reload();
            }
        }
    }
}
    