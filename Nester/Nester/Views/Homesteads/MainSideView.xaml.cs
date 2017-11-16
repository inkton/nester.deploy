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
using Nester.Admin;
using Xamarin.Forms;

namespace Nester.Views
{
    public partial class MainSideView : MasterDetailPage
    {
        protected Func<Views.View, bool> _viewLoader;

        public MainSideView()
        {
            InitializeComponent();

            Detail = new BannerView(BannerView.Status.Initializing);

            _viewLoader = new Func<Views.View, bool>(LoadView);
            Home.Init(_viewLoader);
        }

        public AuthViewModel AuthViewModel
        {
            get { return Home.AuthViewModel; }
            set { Home.AuthViewModel = value; }
        }

        public AppViewModel AppViewModel
        {
            get { return Home.AppViewModel; }
            set { Home.AppViewModel = value; }
        }

        protected bool LoadView(Views.View view)
        {
            view.LoadView = _viewLoader;
            view.MasterDetailPage = this;

            if (view is AppView)
            {
                (view as AppView).GetAnalyticsAsync();
            }

            Detail = view;
            
            return true;
        }
    }
}
    