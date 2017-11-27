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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Inkton.Nester.Views
{
    public partial class AppNestsView : Inkton.Nester.Views.View
    {
        public AppNestsView(Views.AppModelPair modelPair)
        {
            _modelPair = modelPair;

            InitializeComponent();

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonHome,
                    ButtonBasicDetails,
                    ButtonContacts,
                    ButtonDomains,
                    Memory,
                    Type,
                    Scaling,
                    ButtonClear,
                    ButtonAdd,
                    ButtonRefresh,
                    ButtonUpdate,
                    AppNestsList,
                    ButtonDone
                });

            BindingContext = _modelPair.AppViewModel.NestModel;

            AppNestsList.SelectionChanged += AppNestsList_SelectionChanged;
            Memory.SelectedIndexChanged += Memory_SelectedIndexChanged;

            ButtonDone.IsVisible = _modelPair.WizardMode;
            if (_modelPair.WizardMode)
            {
                // hide but do not collapse
                TopButtonPanel.Opacity = 0;

                NavigationPage.SetHasNavigationBar(this, true);
            }
        }

        async private void OnButtonBasicDetailsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.LoadView(new AppBasicDetailView(_modelPair));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnButtonDomainsClickedAsync(object sender, EventArgs e)
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

        async private void OnButtonContactsClickedAsync(object sender, EventArgs e)
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

        private void Clear()
        {
            if (AppNestsList.SelectedItems.Any())
            {
                AppNestsList.SelectedItems.RemoveAt(0);
            }

            _modelPair.AppViewModel.NestModel.EditNest = new Admin.Nest();
            _modelPair.AppViewModel.NestModel.EditNest.App = _modelPair.AppViewModel.EditApp;

            SetDefaults();

            Validate();
        }

        private void SetDefaults()
        {
            Memory.SelectedIndex = 0;
            Type.SelectedIndex = 0;

            if (AppNestsList.SelectedItem == null)
            {
                Type.IsEnabled = true;
                return;
            }
            else
            {
                Type.IsEnabled = false;
            }

            Admin.Nest browseNest = AppNestsList.SelectedItem as Admin.Nest;
            Admin.Nest copy = new Admin.Nest();
            Utils.Object.CopyPropertiesTo(browseNest, copy);
            _modelPair.AppViewModel.NestModel.EditNest = copy;

            int index = 0;

            foreach (string memory in Memory.Items)
            {
                if (memory == browseNest.ScaleSize)
                {
                    Memory.SelectedIndex = index;
                }
                ++index;
            }

            Scaling.Value = browseNest.Scale;

            Admin.NestPlatform platform = _modelPair.AppViewModel.NestModel.Platforms.First(
                x => x.Id == browseNest.PlatformId);

            if (platform.Tag == "mvc")
            {
                Type.SelectedIndex = 0;
            }
            else if (platform.Tag == "api")
            {
                Type.SelectedIndex = 1;
            }
            else
            {
                Type.SelectedIndex = 2;
            }
        }

        private Admin.Nest CopyUpdate(Admin.Nest browseNest)
        {
            browseNest.App = _modelPair.AppViewModel.EditApp;
            browseNest.Tag = Tag.Text;
            browseNest.Name = Name.Text;
            browseNest.NestStatus = "active";

            Admin.NestPlatform platform = null;

            if (Type.SelectedIndex == 0)
            {
                platform = _modelPair.AppViewModel.NestModel.Platforms.First(
                        x => x.Tag == "mvc");
            }
            else if (Type.SelectedIndex == 1)
            {
                platform = _modelPair.AppViewModel.NestModel.Platforms.First(
                        x => x.Tag == "api");
            }
            else if (Type.SelectedIndex == 2)
            {
                platform = _modelPair.AppViewModel.NestModel.Platforms.First(
                        x => x.Tag == "worker");
            }

            browseNest.Platform = platform;
            browseNest.Scale = (int)Scaling.Value;
            browseNest.ScaleSize = Memory.Items[Memory.SelectedIndex];

            return browseNest;
        }

        private void AppNestsList_SelectionChanged(object sender, Syncfusion.ListView.XForms.ItemSelectionChangedEventArgs e)
        {
            SetDefaults();
            DisplayMemory();
            Validate();
        }

        private void Memory_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayMemory();
        }

        private void DisplayMemory()
        {
            string memoryString = Memory.Items[Memory.SelectedIndex];

            MatchCollection matches = Regex.Matches(memoryString, @"(\d+)(\w+)");

            int value = int.Parse(matches[0].Groups[1].Value);
            int multiplier = 0;

            switch (matches[0].Groups[2].Value)
            {
                case "m" : multiplier = 1; break;
                case "g" : multiplier = 1024 ; break;
            }

            int scale = (int)Scaling.Value;
            int totalMemoryMbytes = value * multiplier * scale;
            UsedMemory.Text = totalMemoryMbytes.ToString() + "m";

            if (_modelPair.AppViewModel.NestModel.EditNest != null)
            {
                _modelPair.AppViewModel.NestModel.EditNest.ScaleSize = memoryString;
                _modelPair.AppViewModel.NestModel.EditNest.Scale = scale;
            }
        }

        private void Validate()
        {
            _modelPair.AppViewModel.NestModel.Validated = false;
            _modelPair.AppViewModel.NestModel.CanUpdate = false;

            if (TagValidator != null)
            {
                /* used to enable the add function. a domain can
                 * be added only if valid fields and no list item 
                 * has been selected and currenly receivng focus.
                 */
                _modelPair.AppViewModel.NestModel.Validated = (
                    TagValidator.IsValid &&
                    NameValidator.IsValid 
                );

                /* used to enable the update function. a domain can
                 * be updaed only if valid fields has been selected 
                 * and an item from a list is selected.
                 */
                _modelPair.AppViewModel.NestModel.CanUpdate =
                    _modelPair.AppViewModel.NestModel.Validated &&
                    AppNestsList.SelectedItem != null;
            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                if (_modelPair.AppViewModel.NestModel.Nests.Any())
                {
                    Admin.Nest nest = _modelPair.AppViewModel.NestModel.Nests.First();
                    AppNestsList.SelectedItem = nest;
                }

                SetDefaults();
                DisplayMemory();
                Validate();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        private void OnFieldValidation(object sender, EventArgs e)
        {
            Validate();
        }

        private void OnScaleSizeValueChanged(object sender, ValueChangedEventArgs e)
        {
            DisplayMemory();
        }

        async void OnClearButtonClicked(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                Clear();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async void OnAddButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                var existNests = from nest in _modelPair.AppViewModel.NestModel.Nests
                                    where nest.Tag == Tag.Text
                                    select nest;
                if (existNests.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The nest with this tag already exist", "OK");
                    return;
                }

                Admin.Nest newNest = CopyUpdate(new Admin.Nest());
                Admin.NestPlatform workerPlatform = _modelPair.AppViewModel.NestModel.Platforms.First(
                    x => x.Tag == "worker");

                // the new nest is a handler
                var handlerNests = from nest in _modelPair.AppViewModel.NestModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (newNest.PlatformId != workerPlatform.Id && 
                    handlerNests.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "Only one handler type nest can exist.", "OK");
                    return;
                }

                await _modelPair.AppViewModel.NestModel.CreateNestAsync(newNest);

                Clear();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async void OnRefreshButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await Process(AppNestsList.SelectedItem as Admin.Nest, true,
                    _modelPair.AppViewModel.NestModel.QueryNestAsync
                );

                SetDefaults();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async void OnUpdateButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                Admin.Nest updatingNest = AppNestsList.SelectedItem as Admin.Nest;

                var existNests = from nest in _modelPair.AppViewModel.NestModel.Nests
                                 where nest.Tag == Tag.Text && nest.Id != updatingNest.Id
                                 select nest;
                if (existNests.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The nest with this tag already exist", "OK");
                    return;
                }

                Admin.Nest updateNest = CopyUpdate(updatingNest);
                if (updateNest != null)
                {
                    await Process(updateNest, true,
                        _modelPair.AppViewModel.NestModel.UpdateNestAsync
                    );

                    SetDefaults();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async void OnRemoveButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                Admin.Nest nest = AppNestsList.SelectedItem as Admin.Nest;

                if (_modelPair.AppViewModel.EditApp.IsDeployed)
                {
                    if (nest.Platform.Tag != "worker")
                    {
                        await DisplayAlert("Nester", "Cannot remove the hander nest while deplyed", "Yes", "No");
                        return;
                    }
                }

                await Process(nest, true,
                    _modelPair.AppViewModel.NestModel.RemoveNestAsync,
                       new Func<Admin.Nest, Task<bool>>(
                            async (obj) =>
                            {
                                return await DisplayAlert("Nester", "Would you like to remove this nest", "Yes", "No");
                            }
                        )
                );

                Clear();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                Admin.NestPlatform workerPlatform = _modelPair.AppViewModel.NestModel.Platforms.First(
                    x => x.Tag == "worker");

                var handlerNests = from nest in _modelPair.AppViewModel.NestModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (handlerNests.ToArray().Length == 0)
                {
                    await DisplayAlert("Nester", "Please add a handler nest to process queries", "OK");
                    return;
                }

                if (_modelPair.WizardMode)
                {
                    await _modelPair.AppViewModel.ContactModel.InitAsync();

                    // if currently trvelling back and forth on the 
                    // app wizard - move to the next

                    ContactsView contactsView = new ContactsView(_modelPair);
                    contactsView.MainSideView = MainSideView;
                    await MainSideView.Detail.Navigation.PushAsync(contactsView);
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
        }
    }
}

