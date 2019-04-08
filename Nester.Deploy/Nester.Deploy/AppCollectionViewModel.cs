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

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Inkton.Nest.Model;
using Inkton.Nest.Cloud;
using Inkton.Nester.Cloud;

namespace Inkton.Nester.ViewModels
{
    public class AppCollectionViewModel : ViewModel
    {
        private ObservableCollection<AppViewModel> _appModels;

        public AppCollectionViewModel(NesterService service)
            :base(service)
        {
            _editApp = new App();
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

        public void ResetOwner(User owner)
        {
            _editApp.OwnedBy = owner;

            foreach (AppViewModel appModel in _appModels)
            {
                appModel.EditApp.OwnedBy = owner;
            }
        }

        public async Task<ResultMultiple<App>> LoadApps(
            bool doCache = true, bool throwIfError = true)
        {
            ResultMultiple<App> result = await ResultMultipleUI<App>.WaitForObjectAsync(
                Platform, throwIfError, _editApp, doCache);

            if (result.Code == 0)
            {
                ObservableCollection<App> apps = result.Data.Payload;

                if (apps.Any())
                {
                    foreach (App app in apps)
                    {
                        AddApp(app);
                    }
                }
            }

            return result;
        }

        public AppViewModel AddApp(App app)
        {
            AppViewModel appModel = new AppViewModel(
                Client.ApiVersion, Client.Signature, _platform);
            appModel.EditApp = app;
            AddModel(appModel);
            return appModel;
        }

        public void AddModel(AppViewModel appModel)
        {
            _appModels.Add(appModel);
            OnPropertyChanged("Apps");
        }

        public void RemoveApp(AppViewModel appModel)
        {
            _appModels.Remove(appModel);
            OnPropertyChanged("Apps");
        }
    }
}
