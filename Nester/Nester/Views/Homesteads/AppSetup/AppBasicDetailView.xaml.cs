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
using Xamarin.Forms;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class AppBasicDetailView : View
    {
        private AppViewModel _appSearch = new AppViewModel();

        public AppBasicDetailView(BaseModels baseModels)
        {
            _baseModels = baseModels;

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

            BindingContext = _baseModels.TargetViewModel;

            ButtonNests.Clicked += ButtonNests_ClickedAsync;
            ButtonContacts.Clicked += ButtonContacts_ClickedAsync;
            ButtonDomains.Clicked += ButtonDomains_ClickedAsync;
            ButtonUpdate.Clicked += ButtonUpdate_ClickedAsync;

            ButtonDone.IsVisible = _baseModels.WizardMode;
            if (_baseModels.WizardMode)
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
                await _baseModels.TargetViewModel.UpdateAppAsync();
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
                await _baseModels.TargetViewModel.DomainModel.InitAsync();

                MainSideView.LoadView(new AppDomainView(_baseModels));
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
                await _baseModels.TargetViewModel.ContactModel.InitAsync();

                MainSideView.LoadView(new ContactsView(_baseModels));
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
                await _baseModels.TargetViewModel.NestModel.InitAsync();

                MainSideView.LoadView(new AppNestsView(_baseModels));
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

                foreach (AppViewModel.AppType appType in _baseModels.TargetViewModel.ApplicationTypes)
                {
                    if (_baseModels.TargetViewModel.EditApp.Type == appType.Tag)
                    {
                        AppTypeListView.SelectedItems.Add(appType);
                        _baseModels.TargetViewModel.EditApplicationType = appType;
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
                        _baseModels.TargetViewModel.EditApplicationType = appType;
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
                if (status.Code == Cloud.ServerStatus.NEST_RESULT_SUCCESS)
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
                _baseModels.TargetViewModel.Validated = (
                    TagValidator.IsValid &&
                    NameValidator.IsValid &&
                    PasswordValidator.IsValid &&
                    _baseModels.TargetViewModel.EditApplicationType != null
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
                if (_baseModels.WizardMode)
                {
                    AppTierView tierView = new AppTierView(_baseModels);
                    tierView.MainSideView = MainSideView;
                    MainSideView.Detail.Navigation.InsertPageBefore(tierView, this);
                    await MainSideView.Detail.Navigation.PopAsync();
                }
                else
                {
                    // Head back to homepage if the 
                    // page was called from here

                    await NesterControl.ResetViewAsync();
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
