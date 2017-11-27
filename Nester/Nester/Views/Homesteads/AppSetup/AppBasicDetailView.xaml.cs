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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Inkton.Nester.Views
{
    public partial class AppBasicDetailView : Inkton.Nester.Views.View
    {
        private AppViewModel _appSearch = new AppViewModel();

        public AppBasicDetailView(Views.AppModelPair modelPair)
        {
            _modelPair = modelPair;

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

            BindingContext = _modelPair.AppViewModel;

            ButtonNests.Clicked += ButtonNests_ClickedAsync;
            ButtonContacts.Clicked += ButtonContacts_ClickedAsync;
            ButtonDomains.Clicked += ButtonDomains_ClickedAsync;
            ButtonUpdate.Clicked += ButtonUpdate_ClickedAsync;

            ButtonDone.IsVisible = _modelPair.WizardMode;
            if (_modelPair.WizardMode)
            {
                // hide but do not collapse
                TopButtonPanel.Opacity = 0;
                BottomButtonPanel.Opacity = 1;
            }
            else
            {
                TopButtonPanel.Opacity = 1;
                BottomButtonPanel.Opacity = 0;
            }
        }

        async private void ButtonUpdate_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _modelPair.AppViewModel.UpdateAppAsync();
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
                await _modelPair.AppViewModel.DomainModel.InitAsync();

                MainSideView.LoadView(new AppDomainView(_modelPair));
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
                await _modelPair.AppViewModel.ContactModel.InitAsync();

                MainSideView.LoadView(new ContactsView(_modelPair));
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
                await _modelPair.AppViewModel.NestModel.InitAsync();

                MainSideView.LoadView(new AppNestsView(_modelPair));
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

                foreach (AppViewModel.AppType appType in _modelPair.AppViewModel.ApplicationTypes)
                {
                    if (_modelPair.AppViewModel.EditApp.Type == appType.Tag)
                    {
                        AppTypeListView.SelectedItems.Add(appType);
                        _modelPair.AppViewModel.EditApplicationType = appType;
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
                        _modelPair.AppViewModel.EditApplicationType = appType;
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
            if (TagValidator != null)
            {
                _modelPair.AppViewModel.Validated = (
                    TagValidator.IsValid &&
                    NameValidator.IsValid &&
                    PasswordValidator.IsValid &&
                    _modelPair.AppViewModel.EditApplicationType != null
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
                if (_modelPair.WizardMode)
                {
                    AppTierView tierView = new AppTierView(_modelPair);
                    tierView.MainSideView = MainSideView;
                    await MainSideView.Detail.Navigation.PushAsync(tierView);
                }
                else
                {
                    // Head back to homepage if the 
                    // page was called from here

                    ResetView();
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
