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
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Xamarin.Forms;
using Plugin.DeviceInfo;
using Newtonsoft.Json;
using Inkton.Nest.Model;
using Inkton.Nester;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Cloud;
using Inkton.Nester.Storage;
using Inkton.Nester.Views;
using Inkton.Nester.Helpers;

namespace Nester.Deploy
{
    public partial class App : Application, IKeeper, INesterControl
    {
        private User _user;
        private const int ServiceVersion = 2;
        private LogService _log;
        private NesterService _platform, _backend;
        private BaseViewModels _baseModels;
        private MainSideView _mainSideView;

        public App()
        {
            InitializeComponent();

            _user = new User();

            StorageService cache = new StorageService(Path.Combine(
                    Path.GetTempPath(), "NesterCache"));
            cache.Clear();

            Dictionary<string, string> clientSignature = new Dictionary<string, string>();
            clientSignature["device"] = JsonConvert.SerializeObject(CrossDeviceInfo.Current);
            clientSignature["app_version"] = typeof(EntryView).GetTypeInfo()
                    .Assembly.GetName().Version.ToString();

            string clientSignatureJSON =
                JsonConvert.SerializeObject(clientSignature);

            _log = new LogService(Path.Combine(
                    Path.GetTempPath(), "NesterLog"));
            _platform = new NesterService(
                ServiceVersion, clientSignatureJSON, cache);
            _backend = new NesterService(
                ServiceVersion, clientSignatureJSON, cache);

            _baseModels = new BaseViewModels(
                new AuthViewModel(), 
                new PaymentViewModel(),
                new AppViewModel(),
                new AppCollectionViewModel());

            _mainSideView = new MainSideView();

            MainPage = _mainSideView;

            _mainSideView.ShowEntry();
        }

        public BaseViewModels ViewModels
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
            get { return _baseModels.AppViewModel; }
        }

        public NesterService Service
        {
            get { return _platform; }
        }

        public NesterService Backend
        {
            get { return _backend; }
        }

        public LogService Log
        {
            get { return _log; }
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
            if (appModel == null)
            {
                _baseModels.AppViewModel = _baseModels
                    .AppCollectionViewModel
                    .AppModels.FirstOrDefault();
            }
            else
            {
                _baseModels.AppViewModel = appModel;
            }

            _mainSideView.UpdateView();
        }
    }
}
