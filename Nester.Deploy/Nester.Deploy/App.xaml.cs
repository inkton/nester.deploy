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
using Inkton.Nester.Storage;
using Inkton.Nester.Views;
using Inkton.Nester.Helpers;

namespace Nester.Deploy
{
    public partial class App : Application, INesterClient
    {
        private LogService _log;
        private BaseViewModels _baseModels;
        private MainSideView _mainSideView;
        private User _currUser;

        public App()
        {
            InitializeComponent();
                        
            _log = new LogService(Path.Combine(
                    Path.GetTempPath(), "NesterLog"));
            _baseModels = new BaseViewModels();

            _mainSideView = new MainSideView();
            MainPage = _mainSideView;
            _mainSideView.ShowEntry();
        }

        public BaseViewModels ViewModels
        {
            get { return _baseModels; }
        }

        public AppViewModel Target
        {
            get { return _baseModels.AppViewModel; }
        }

        public int ApiVersion
        {
            get { return 2; }
        }

        public string Signature
        {
            get
            {
                // This is helps to trace issues with the API calls on the server 
                Dictionary<string, string> clientSignature = new Dictionary<string, string>();
                clientSignature["device"] = JsonConvert.SerializeObject(CrossDeviceInfo.Current);
                clientSignature["app_version"] = typeof(EntryView).GetTypeInfo()
                        .Assembly.GetName().Version.ToString();
                return JsonConvert.SerializeObject(clientSignature);
            }
        }

        public User User
        {
            get { return _currUser; }
        }

        public LogService Log
        {
            get { return _log; }
        }

        public ResourceManager GetResourceManager()
        {
            ResourceManager resmgr = new ResourceManager(
                "Inkton.Nester.Resources",
                typeof(INesterClient).GetTypeInfo().Assembly);
            return resmgr;
        }

        public void ResetPermit(Permit permit = null)
        {
            _currUser = null;

            if (permit != null)
            {
                _currUser = permit.Owner;
                _baseModels.ResetPermit(permit);
            }
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
