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

using System.Resources;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using Inkton.Nester.Models;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Cloud;
using Inkton.Nester.Cache;
using Inkton.Nester.Views;
using Inkton.Nester;

namespace Nester.Deploy
{
    public partial class App : Application, INesterControl
    {
        private User _user;
        private const int ServiceVersion = 1;
        private NesterService _service, _target;
        private StorageService _storage;
        private BaseModels _baseModels;
        private MainSideView _mainSideView;

        public App()
        {
            InitializeComponent();

            _user = new User();

            _service = new NesterService();
            _service.Version = ServiceVersion;

            _target = new NesterService();
            _storage = new StorageService();
            _storage.Clear();

            _baseModels = new BaseModels(
                new AuthViewModel(), 
                new PaymentViewModel(),
                new AppViewModel(),
                new AppCollectionViewModel());

            _mainSideView = new MainSideView();

            MainPage = _mainSideView;

            _mainSideView.ShowEntry();
        }

        public BaseModels BaseModels
        {
            get { return _baseModels; }
        }

        public User User
        {
            get { return _user; }
            set { _user = value; }
        }

        public AppViewModel Target
        {
            get { return _baseModels.TargetViewModel; }
            set {
                _baseModels.TargetViewModel = value;

                if (value != null)
                {
                    _target.Version = ServiceVersion;
                    _target.Endpoint = string.Format(
                        "https://{0}/", value.EditApp.Hostname);
                    _target.BasicAuth = new Inkton.Nester.Cloud.BasicAuth(
                        true, value.EditApp.Tag, value.EditApp.NetworkPassword);
                }
            }
        }

        public Inkton.Nester.Cloud.NesterService Service
        {
            get { return _service; }
        }

        public Inkton.Nester.Cloud.NesterService DeployedApp
        {
            get { return _target; }
        }

        public Inkton.Nester.Cache.StorageService StorageService
        {
            get { return _storage; }
        }

        public string StoragePath
        {
            get {
                return System.IO.Path.GetTempPath();
            }
        }

        public ResourceManager GetResourceManager()
        {
            ResourceManager resmgr = new ResourceManager(
                "Inkton.Nester.Resources",
                typeof(INesterControl).GetTypeInfo().Assembly);
            return resmgr;
        }

        public void ResetView(AppViewModel appModel = null)
        {
            _mainSideView.ResetView(appModel);
        }
    }
}
