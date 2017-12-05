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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inkton.Nester.Admin;
using Xamarin.Forms;
using Syncfusion.SfBusyIndicator.XForms;

namespace Inkton.Nester.Views
{
    public partial class MainSideView : MasterDetailPage
    {
        private Views.View _currentView;
        
        public MainSideView()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.UWP)
            {
                MasterBehavior = MasterBehavior.Popover;
            }
        }

        public Views.View CurrentView
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

        public void ResetView(Views.BaseModels baseModels = null)
        {             
            if (baseModels == null)
            {
                BannerView view = new BannerView();
                view.ShowProgress = false;
                LoadView(view);
            }
            else
            {
                CreateAppView(baseModels);
            }
        }

        public bool LoadView(Views.View view)
        {
            Detail = new NavigationPage(view);
            _currentView = view;
            _currentView.MainSideView = this;
            return true;
        }

        public bool CreateAppView(Views.BaseModels baseModels)
        {
            AppView.Status newState;
            bool viewLoadNeeded = true;

            if (baseModels.AppViewModel.EditApp.IsBusy)
            {
                newState = AppView.Status.Updating;
            }
            else if (!baseModels.AppViewModel.EditApp.IsDeployed)
            {
                newState = AppView.Status.WaitingDeployment;
            }
            else
            {
                newState = AppView.Status.Deployed;
            }

            if (_currentView != null && _currentView is AppView && _currentView.BaseModels != null &&
                _currentView.BaseModels.AppViewModel.EditApp.Id == baseModels.AppViewModel.EditApp.Id)
            {
                viewLoadNeeded = ((_currentView as AppView).State != newState);
            }

            if (viewLoadNeeded)
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    Views.AppView appView = new Views.AppView(baseModels);
                    appView.State = AppView.Status.Refreshing;
                    appView.BaseModels = baseModels;
                    if (newState == AppView.Status.Deployed)
                    {
                        appView.GetAnalyticsAsync();
                    }
                    LoadView(appView);
                    appView.State = newState;

                });
            }

            return true;
        }

        public void Reload(AppViewModel appModel)
        {
            if (_currentView != null && _currentView is AppView && _currentView.BaseModels != null &&
                _currentView.BaseModels.AppViewModel.EditApp.Id == appModel.EditApp.Id)
            {
                (_currentView as AppView).ReloadAsync();
            }
            else
            {
                appModel.Reload();
            }
        }
    }
}
    