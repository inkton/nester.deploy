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
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Inkton.Nester;
using Inkton.Nester.ViewModels;
using DeployApp = Nester.Deploy.App;

namespace Inkton.Nester.Views
{
    public partial class MainView : MasterDetailPage
    {
        private Dictionary<long, AppView> _viewCache;

        public MainView()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.UWP)
            {
                MasterBehavior = MasterBehavior.Popover;
            }

            _viewCache = new Dictionary<long, AppView>();

            StackViewAsync(new LoginView()).Wait();
        }
        
        public async Task GoHomeAsync()
        {
            NavigationPage detail = Detail as NavigationPage;

            // Home (BannerView/AppView) -> Dialogs
            var homeView = detail.Navigation.NavigationStack.FirstOrDefault();

            if (homeView == null)
            {
                BannerView noAppsSign = new BannerView();
                StackAppView(noAppsSign);
            }
            else
            {
                bool isAppView = homeView is AppView;

                if (!isAppView)
                {
                    // An appView is needed. 
                    BaseViewModels baseModels = ((DeployApp)Application.Current)
                        .BaseViewModels;
                    AppViewModel loadAppViewModel = baseModels.AppCollectionViewModel
                         .AppModels.FirstOrDefault();
                    if (loadAppViewModel != null)
                    {
                        detail.Navigation.InsertPageBefore(
                            await GetAppViewAsync(loadAppViewModel), homeView);
                    }
                }

                while (detail.Navigation.NavigationStack.Count > 1)
                {
                    await Detail.Navigation.PopAsync();
                }
            }
        }
            
        public async Task LogoutAsync()
        {
            NavigationPage detail = Detail as NavigationPage;

            // Home (BannerView/AppView) -> Dialogs
            var homeView = detail.Navigation.NavigationStack.FirstOrDefault();

            if (homeView != null)
            {
                detail.Navigation.InsertPageBefore(
                    new LoginView(), homeView);
            }
            else
            {
                StackAppView(new LoginView());
            }

            while (!(detail.CurrentPage is LoginView))
            {
                await Detail.Navigation.PopAsync();
            }

            BaseViewModels baseModels = ((DeployApp)Application.Current)
                .BaseViewModels;

            if (baseModels.AuthViewModel.IsAuthenticated)
            {
                baseModels.AuthViewModel.Platform.Permit.Invalid();
            }
        }

        public async Task StackViewAsync(View view)
        {
            await (Detail as NavigationPage).PushAsync(view);
        }

        public async Task StackViewSkipBackAsync(View view)
        {
            Detail.Navigation.InsertPageBefore(view, (Detail as NavigationPage).CurrentPage);
            await (Detail as NavigationPage).PopAsync();
        }

        public async Task UnstackViewAsync()
        {
            await (Detail as NavigationPage).PopAsync();
        }

        async public Task<AppView> GetAppViewAsync(AppViewModel appModel)
        {
            // this creates a cache of app views
            // the appview takes time to init
            // caching seems to help - revisit
            AppView appView = null;

            if (_viewCache.ContainsKey(appModel.EditApp.Id))
            {
                appView = _viewCache[appModel.EditApp.Id];
            }

            if (appView == null)
            {
                appView = new AppView(appModel);

                System.Diagnostics.Debug.WriteLine(
                    string.Format("Created a view for", appModel.EditApp.Tag));

                if (appModel.EditApp.Id > 0)
                {
                    await appView.AppViewModel.InitAsync();
                }

                _viewCache[appModel.EditApp.Id] = appView;
            }

            return appView;
        }

        public async Task UpdateViewAsync(AppViewModel loadAppViewModel = null)
        {
            BaseViewModels baseModels = ((DeployApp)Application.Current)
                .BaseViewModels;

            if (loadAppViewModel == null)
            {
                // No appmodel provided - get one from the collection
                loadAppViewModel = baseModels.AppCollectionViewModel
                    .AppModels.FirstOrDefault();

                if (loadAppViewModel == null)
                {
                    BannerView noAppsSign = new BannerView();
                    StackAppView(noAppsSign);
                    return;
                }                
            }

            bool isAppViewCurrent = 
                ((Detail as NavigationPage).CurrentPage is AppView);

            // if the the app is currently displayed - ignore load.
            if (isAppViewCurrent)
            {
                if (((Detail as NavigationPage).CurrentPage as AppView).AppViewModel.EditApp.Id
                        != loadAppViewModel.EditApp.Id)
                {
                    StackAppView(await GetAppViewAsync(loadAppViewModel));
                }
            }
            else
            {
                StackAppView(await GetAppViewAsync(loadAppViewModel));
            }
        }
        
        async public Task ReloadAsync()
        {
            await ((Detail as NavigationPage).CurrentPage as AppView)?
                .AppViewModel.ReloadAsync();
        }

        private void StackAppView(View newView)
        {
            NavigationPage detail = Detail as NavigationPage;
            View oldView = detail.Navigation.NavigationStack.FirstOrDefault() as View;
            Debug.Assert(oldView != null);
            Debug.Assert(oldView is BannerView || oldView is AppView);

            if (oldView == newView)
            {
                return;
            }

            detail.Navigation.InsertPageBefore(newView, oldView);
            detail.Navigation.RemovePage(oldView);
        }
    }
}
    