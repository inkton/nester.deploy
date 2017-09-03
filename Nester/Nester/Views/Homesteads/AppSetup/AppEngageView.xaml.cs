using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Nester.Views
{
    public partial class AppEngageView : Nester.Views.View
    {
        public AppEngageView(AppViewModel appViewModel)
        {
            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                                ButtonCreate,
                                ButtonJoin
                });

            _appViewModel = appViewModel;
            BindingContext = _appViewModel;
        }

        async private void OnCreateAppClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            Navigation.InsertPageBefore(new AppBasicDetailView(_appViewModel), this);
            await Navigation.PopAsync();

            IsServiceActive = false;
        }

        async private void OnJoinAppClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            await _appViewModel.ContactModel.QueryInvitationsAsync();

            Navigation.InsertPageBefore(new AppJoinDetailView(_appViewModel), this);
            await Navigation.PopAsync();

            IsServiceActive = false;
        }
    }
}
