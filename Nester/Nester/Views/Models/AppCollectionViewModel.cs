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

namespace Inkton.Nester.Views
{
    public class AppCollectionViewModel : AppViewModel
    {
        private ObservableCollection<AppViewModel> _appModels;

        public AppCollectionViewModel()
        {
            _appModels = new ObservableCollection<AppViewModel>();
        }

        public ObservableCollection<AppViewModel> AppModels
        {
            get
            {
                return _appModels;
            }
            set
            {
                SetProperty(ref _appModels, value);
            }
        }

        public async Task<Cloud.ServerStatus> LoadApps(
            bool doCache = true, bool throwIfError = true)
        {
            Cloud.ServerStatus status = await Cloud.Result.WaitForObjectListAsync(
                throwIfError, _editApp, doCache);

            if (status.Code == 0)
            {
                ObservableCollection<Admin.App> apps = status.PayloadToList<Admin.App>();

                if (apps.Any())
                {
                    foreach (Admin.App app in apps)
                    {
                        AppViewModel appModel = new AppViewModel();
                        appModel.EditApp = app;
                        await appModel.InitAsync();
                        _appModels.Add(appModel);
                    }

                    NesterControl.CreateAppView(
                        new AppModelPair(NesterControl.AppModelPair.AuthViewModel, _appModels.First()));
                    OnPropertyChanged("Apps");
                }
                else
                {
                    NesterControl.ResetView();
                }
            }

            return status;
        }

        public void AddApp(Admin.App app)
        {
            AppViewModel appModel = new AppViewModel();
            appModel.EditApp = app;
            AddModel(appModel);
        }

        public void AddModel(AppViewModel appModel)
        {
            _appModels.Add(appModel);

            Task.Run(() => appModel.InitAsync());
            NesterControl.CreateAppView(
                new AppModelPair(NesterControl.AppModelPair.AuthViewModel, appModel));
        }

        public void RemoveApp(AppViewModel appModel)
        {
            _appModels.Remove(appModel);
            
            if (_appModels.Any())
            {
                NesterControl.CreateAppView(
                    new AppModelPair(NesterControl.AppModelPair.AuthViewModel, _appModels.First()));
            }
            else
            {
                NesterControl.ResetView();
            }
        }
    }
}
