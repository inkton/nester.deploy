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

        public AppView GetAppView(long appId)
        {
            AppView appView = null;

            if (_viewCache.ContainsKey(appId))
            {
                appView = _viewCache[appId];
            }

            return appView;
        }
        
        public async Task GoHomeAsync()
        {
            while (!((Detail as NavigationPage).CurrentPage is BannerView ||
                (Detail as NavigationPage).CurrentPage is AppView))
            {
                await Detail.Navigation.PopAsync();
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
        
        public async Task UpdateViewAsync(AppViewModel loadAppViewModel = null)
        {
            BaseViewModels baseModels = ((DeployApp)Application.Current)
                .BaseViewModels;

            if (loadAppViewModel == null)
            {
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

            bool changeView = false;

            if (isAppViewCurrent)
            {
                if (((Detail as NavigationPage).CurrentPage as AppView).AppViewModel.EditApp.Id
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
                        await appView.AppViewModel.InitAsync();
                    }

                    _viewCache[loadAppViewModel.EditApp.Id] = appView;
                }

                StackAppView(appView);
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
            System.Diagnostics.Debug.Assert(oldView != null);
            System.Diagnostics.Debug.Assert(oldView is BannerView || oldView is AppView);

            if (oldView == newView)
            {
                return;
            }

            detail.Navigation.InsertPageBefore(newView, oldView);
            detail.Navigation.RemovePage(oldView);
        }
    }
}
    