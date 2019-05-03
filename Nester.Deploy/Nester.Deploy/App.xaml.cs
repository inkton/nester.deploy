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

        public App()
        {
            InitializeComponent();

            // This is helps to trace issues with 
            // the API calls on the server 

            _log = new LogService(Path.Combine(
                    Path.GetTempPath(), "NesterLog"));
            _baseModels = new BaseViewModels();

            _mainSideView = new MainSideView();
            MainPage = _mainSideView;
            _mainSideView.ShowEntry();
        }

        public BaseViewModels BaseViewModels
        {
            get { return _baseModels; }
        }

        public LogService Log
        {
            get { return _log; }
        }

        public ResourceManager GetResourceManager()
        {
            ResourceManager resmgr = new ResourceManager(
                "Inkton.Nester.Text",
                typeof(INesterClient).GetTypeInfo().Assembly);
            return resmgr;
        }

        public void RefreshView(AppViewModel appModel = null)
        {
            _mainSideView.UpdateView(appModel);
        }
    }
}
