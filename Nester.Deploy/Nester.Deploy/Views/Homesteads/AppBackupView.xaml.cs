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
using Xamarin.Forms.Xaml;
using Inkton.Nest.Model;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Helpers;

namespace Inkton.Nester.Views
{
    public partial class AppBackupView : View
    {
        public AppBackupView(BaseViewModels baseModels)
        {
            InitializeComponent();

            ViewModels = baseModels;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonCancel,
                    ButtonRestore,
                    ButtonBackup,
                    ButtonRefresh
                });

            _activityIndicator = ServiceActive;

            ButtonCancel.Clicked += ButtonCancel_Clicked;
            ButtonRestore.Clicked += ButtonRestore_ClickedAsync;
            ButtonBackup.Clicked += ButtonBackup_ClickedAsync;
            ButtonRefresh.Clicked += ButtonRefresh_ClickedAsync;
        }

        public override void UpdateBindings()
        {
            base.UpdateBindings();

            BindingContext = _baseViewModels.AppViewModel.DeploymentViewModel;

            UpdateDescriptions();
        }

        private void UpdateDescriptions()
        {
            foreach (AppBackup backup in _baseViewModels.AppViewModel.DeploymentViewModel.AppBackups)
            {
                switch (backup.Status)
                {
                    case "initialized":
                        backup.Description = "A Backup pending";
                        break;
                    case "definition-complete":
                        backup.Description = "The App definition";
                        break;
                    case "deployment-progressing":
                        backup.Description = "In progress ..";
                        break;
                    case "deployment-complete":
                        backup.Description = "The App backup complete";
                        break;
                }
            }
        }


        private void ButtonCancel_Clicked(object sender, EventArgs e)
        {
            IsServiceActive = true;
            MainSideView.UnstackViewAsync();
            IsServiceActive = false;
        }

        private async void ButtonRestore_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                if (AppBackups.SelectedItem != null)
                {
                    bool proceed = await DisplayAlert("Nester",
                        "This will restore from a backup and replace the current App. Proceed?", "Yes", "No");

                    if (!proceed)
                    {
                        IsServiceActive = false;
                        return;
                    }

                    await _baseViewModels.AppViewModel.DeploymentViewModel.RestoreAppAsync(
                        AppBackups.SelectedItem as Nest.Model.AppBackup);

                    // Reload everything
                    await _baseViewModels.AppViewModel.InitAsync();

                    AppView appView = MainSideView.GetAppView(App.Id);
                    if (appView != null)
                    {
                        appView.UpdateStatus();
                    }
                }

                MainSideView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        private async void ButtonBackup_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                bool proceed = await DisplayAlert("Nester",
                    "This will create a new backup. Proceed?", "Yes", "No");

                if (!proceed)
                {
                    IsServiceActive = false;
                    return;
                }

                await _baseViewModels.AppViewModel
                    .DeploymentViewModel.BackupAppAsync();

                UpdateDescriptions();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        private async void ButtonRefresh_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await _baseViewModels.AppViewModel
                    .DeploymentViewModel.QueryAppBackupsAsync();

                UpdateDescriptions();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }
    }
}