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
using Inkton.Nest.Model;
using Inkton.Nest.Cloud;
using Inkton.Nester.Cloud;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class AppBasicDetailView : View
    {
        private AppViewModel _appSearch = new AppViewModel();

        public AppBasicDetailView(BaseViewModels baseModels)
        {
            InitializeComponent();

            ViewModels = baseModels;

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

            ButtonAppServices.Clicked += ButtonAppServices_ClickedAsync;
            ButtonNests.Clicked += ButtonNests_ClickedAsync;
            ButtonContacts.Clicked += ButtonContacts_ClickedAsync;
            ButtonDomains.Clicked += ButtonDomains_ClickedAsync;
            ButtonUpdate.Clicked += ButtonUpdate_ClickedAsync;

            ButtonDone.IsVisible = _baseViewModels.WizardMode;
            if (_baseViewModels.WizardMode)
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

            UpdateBackupParameters();
        }

        async private void ButtonUpdate_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                GetBackupParameters();

                await _baseViewModels.AppViewModel.UpdateAppAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void ButtonAppServices_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.CurrentLevelViewAsync(new AppTierView(_baseViewModels));
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
                MainSideView.CurrentLevelViewAsync(new AppDomainView(_baseViewModels));
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
                MainSideView.CurrentLevelViewAsync(new ContactsView(_baseViewModels));
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
                MainSideView.CurrentLevelViewAsync(new AppNestsView(_baseViewModels));
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

                foreach (AppViewModel.AppType appType in _baseViewModels.AppViewModel.ApplicationTypes)
                {
                    if (App.Type == appType.Tag)
                    {
                        AppTypeListView.SelectedItems.Add(appType);
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
                        App.Type = appType.Tag;
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
                _appSearch.EditApp.OwnedBy = null;
                _appSearch.EditApp.Tag = tagTrimmed;

                ResultSingle<App> result = await _appSearch
                    .QueryAppAsync(null, true, false);
                if (result.Code == Cloud.ServerStatus.NEST_RESULT_SUCCESS)
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
                _baseViewModels.AppViewModel.Validated = (
                    TagValidator.IsValid &&
                    NameValidator.IsValid &&
                    PasswordValidator.IsValid &&
                    App.Type != null
                    );
            }
        }

        void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }

        private void UpdateBackupParameters()
        {
            BackupHour.SelectedItem = App.BackupHour.ToString();
        }

        private void GetBackupParameters()
        {
            App.BackupHour = int.Parse(BackupHour.SelectedItem as string);
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                GetBackupParameters();

                App.Type = "uniflow";

                if (AppTypeListView.SelectedItem != null)
                {
                    App.Type = (AppTypeListView.SelectedItem as AppViewModel.AppType).Tag;
                }

                if (_baseViewModels.WizardMode)
                {
                    AppTierView tierView = new AppTierView(_baseViewModels);
                    tierView.MainSideView = MainSideView;
                    MainSideView.Detail.Navigation.InsertPageBefore(tierView, this);
                    await MainSideView.Detail.Navigation.PopAsync();
                }
                else
                {
                    // Head back to homepage if the 
                    // page was called from here
                    MainSideView.UnstackViewAsync();
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
