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
using Inkton.Nester.Helpers;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class AppBasicDetailView : View
    {
        private AppViewModel _appSearch;

        public AppBasicDetailView(AppViewModel appViewModel, bool wizardMode = false)
            :base(wizardMode)
        {
            InitializeComponent();

            AppViewModel = appViewModel;

            _appSearch = new AppViewModel(BaseViewModels.Platform);

            Tag.Unfocused += Tag_UnfocusedAsync;

            AppTypeListView.ItemSelected += AppTypeListView_ItemSelected;
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

            ButtonDone.IsVisible = _wizardMode;
            if (_wizardMode)
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

            foreach (AppViewModel.AppType appType in AppViewModel.ApplicationTypes)
            {
                if (AppViewModel.EditApp.Type == appType.Tag)
                {
                    AppTypeListView.SelectedItem = appType;
                    break;
                }
            }

            Validate();
        }

        async private void ButtonUpdate_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                GetBackupParameters();

                await AppViewModel.UpdateAppAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void ButtonAppServices_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await MainView.StackViewSkipBackAsync(new AppTierView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void ButtonDomains_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
               await MainView.StackViewSkipBackAsync(new AppDomainView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void ButtonContacts_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await MainView.StackViewSkipBackAsync(new ContactsView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void ButtonNests_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await MainView.StackViewSkipBackAsync(new AppNestsView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        private void AppTypeListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            AppViewModel.EditApp.Type = (e.SelectedItem as AppViewModel.AppType).Tag;
        }

        private async void Tag_UnfocusedAsync(object sender, FocusEventArgs e)
        {
            if (Tag.Text == null)
                return;

            Tag.Text = Inkton.Nester.Helpers.Tag.Clean(Tag.Text);

            if (Tag.Text.Length > 0)
            {
                _appSearch.EditApp.OwnedBy = null;
                _appSearch.EditApp.Tag = Tag.Text;

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
                AppViewModel.Validated = (
                    TagValidator.IsValid &&
                    NameValidator.IsValid &&
                    PasswordValidator.IsValid &&
                    AppViewModel.EditApp.Type != null
                    );
            }
        }

        void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }

        private void UpdateBackupParameters()
        {
            BackupHour.SelectedItem = AppViewModel.EditApp.BackupHour.ToString();
        }

        private void GetBackupParameters()
        {
            AppViewModel.EditApp.BackupHour = int.Parse(BackupHour.SelectedItem as string);
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                GetBackupParameters();

                AppViewModel.EditApp.Type = "uniflow";

                if (AppTypeListView.SelectedItem != null)
                {
                    AppViewModel.EditApp.Type = (AppTypeListView.SelectedItem as AppViewModel.AppType).Tag;
                }

                if (_wizardMode)
                {
                    await MainView.StackViewAsync(
                        new AppTierView(AppViewModel, _wizardMode));
                }
                else
                {
                    // Head back to homepage if the 
                    // page was called from here
                    await MainView.UnstackViewAsync();
                }
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }
    }
}
