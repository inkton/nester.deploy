using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Nester.Views
{
    public partial class AppBasicDetailView : Nester.Views.View
    {
        private AppViewModel _appSearch = new AppViewModel();

        public AppBasicDetailView(AppViewModel appViewModel)
        {
            InitializeComponent();

            Tag.Unfocused += Tag_UnfocusedAsync;

            AppTypeListView.SelectionMode = Syncfusion.ListView.XForms.SelectionMode.Single;
            AppTypeListView.Loaded += AppTypeListView_Loaded;
            AppTypeListView.SelectionChanged += AppTypeListView_SelectionChanged;

            // ButtonServices to be enabled later
            // when upgrading is supported.
            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonHome,
                    ButtonNests,
                    ButtonContacts,
                    ButtonDomains,
                    ButtonDone,
                    ButtonUpdate
                });

            _appViewModel = appViewModel;
            BindingContext = _appViewModel;

            ButtonNests.Clicked += ButtonNests_ClickedAsync;
            ButtonContacts.Clicked += ButtonContacts_ClickedAsync;
            ButtonDomains.Clicked += ButtonDomains_ClickedAsync;
            ButtonUpdate.Clicked += ButtonUpdate_ClickedAsync;
        }

        async private void ButtonUpdate_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _appViewModel.UpdateAppAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonDomains_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _appViewModel.DomainModel.InitAsync();

                LoadView(new AppDomainView(_appViewModel));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonContacts_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _appViewModel.ContactModel.InitAsync();

                LoadView(new ContactsView(_appViewModel));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonNests_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _appViewModel.NestModel.InitAsync();

                LoadView(new AppNestsView(_appViewModel));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void AppTypeListView_Loaded(object sender, Syncfusion.ListView.XForms.ListViewLoadedEventArgs e)
        {
            IsServiceActive = true;

            try
            {
                AppTypeListView.SelectedItems.Clear();

                foreach (AppViewModel.AppType appType in _appViewModel.ApplicationTypes)
                {
                    if (_appViewModel.EditApp.Type == appType.Tag)
                    {
                        AppTypeListView.SelectedItems.Add(appType);
                        _appViewModel.EditApplicationType = appType;
                        break;
                    }
                }

                Validate();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void AppTypeListView_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            IsServiceActive = true;

            try
            {
                foreach (AppViewModel.AppType appType in e.AddedItems)
                {
                    if (appType != null)
                    {
                        _appViewModel.EditApplicationType = appType;
                        break;
                    }
                }

                Validate();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private async void Tag_UnfocusedAsync(object sender, FocusEventArgs e)
        {
            string tag = (sender as Xamarin.Forms.Entry).Text;
            if (tag == null)
                return;

            string tagTrimmed = tag.Trim();

            if (tagTrimmed.Length > 0)
            {
                _appSearch.EditApp.Owner = null;
                _appSearch.EditApp.Tag = tagTrimmed;

                Cloud.ServerStatus status = await _appSearch.QueryAppAsync(
                    null, true, false);
                if (status.Code == Cloud.Result.NEST_RESULT_SUCCESS)
                {
                    TagValidator.IsValid = false;
                    TagValidator.Message = "The tag is taken, try another tag";
                }

                Validate();
            }
        }

        void Validate()
        {
            if (_appViewModel != null)
            {
                _appViewModel.Validated = (
                    TagValidator.IsValid &&
                    NameValidator.IsValid &&
                    PasswordValidator.IsValid &&
                    _appViewModel.EditApplicationType != null
                    );
            }
        }

        void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                if (_appViewModel.WizardMode)
                {
                    Navigation.InsertPageBefore(new AppTierView(_appViewModel), this);
                    await Navigation.PopAsync();
                }
                else
                {
                    // Head back to homepage if the 
                    // page was called from here

                    if (_appViewModel.EditApp.IsDeployed)
                    {
                        await DisplayAlert("Nester", "Make sure to re-deploy the app for changes to take effect", "OK");
                    }

                    LoadHomeView();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
    }
}
