using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xamarin.Forms;

using Syncfusion.ListView.XForms.UWP;

namespace Nester.UWP
{
    public sealed partial class MainView
    {
        public MainView()
        {
            try
            {
                this.InitializeComponent();
                SfListViewRenderer.Init();

                LoadApplication(new Nester.NesterUI());
            }
            catch (Exception ex)
            {
                Nester.Utils.ErrorHandler.Exception(
                 ex.Message, ex.StackTrace);
            }
        }
    }
}
