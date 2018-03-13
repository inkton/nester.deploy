﻿/*
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using Inkton.Nester.Models;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class AppNestsView : View
    {
        public AppNestsView(BaseModels baseModels)
        {
            InitializeComponent();

            BaseModels = baseModels;

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

            AppNestsList.SelectionChanged += AppNestsList_SelectionChanged;
            Memory.SelectedIndexChanged += Memory_SelectedIndexChanged;

            ButtonDone.IsVisible = _baseModels.WizardMode;
            if (_baseModels.WizardMode)
            {
                // hide but do not collapse
                TopButtonPanel.Opacity = 0;

                NavigationPage.SetHasNavigationBar(this, true);
            }
        }

        public override void UpdateBindings()
        {
            base.UpdateBindings();

            BindingContext = _baseModels.TargetViewModel.NestModel;
        }

        async private void OnButtonBasicDetailsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                _baseModels.WizardMode = false;
                MainSideView.CurrentLevelViewAsync(new AppBasicDetailView(_baseModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        async private void OnButtonServiceClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                MainSideView.CurrentLevelViewAsync(new AppTierView(_baseModels));
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
                MainSideView.CurrentLevelViewAsync(new AppDomainView(_baseModels));
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
                MainSideView.CurrentLevelViewAsync(new ContactsView(_baseModels));
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

            _baseModels.TargetViewModel.NestModel.EditNest = new Nest();
            _baseModels.TargetViewModel.NestModel.EditNest.App = _baseModels.TargetViewModel.EditApp;

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

            Nest browseNest = AppNestsList.SelectedItem as Nest;
            Nest copy = new Nest();
            Cloud.Object.CopyPropertiesTo(browseNest, copy);
            _baseModels.TargetViewModel.NestModel.EditNest = copy;

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

            NestPlatform platform = _baseModels.TargetViewModel.NestModel.Platforms.First(
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

        private Nest CopyUpdate(Nest browseNest)
        {
            browseNest.App = _baseModels.TargetViewModel.EditApp;
            browseNest.Tag = Tag.Text;
            browseNest.Name = Name.Text;
            browseNest.NestStatus = "active";

            NestPlatform platform = null;

            if (Type.SelectedIndex == 0)
            {
                platform = _baseModels.TargetViewModel.NestModel.Platforms.First(
                        x => x.Tag == "mvc");
            }
            else if (Type.SelectedIndex == 1)
            {
                platform = _baseModels.TargetViewModel.NestModel.Platforms.First(
                        x => x.Tag == "api");
            }
            else if (Type.SelectedIndex == 2)
            {
                platform = _baseModels.TargetViewModel.NestModel.Platforms.First(
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

            if (_baseModels.TargetViewModel.NestModel.EditNest != null)
            {
                _baseModels.TargetViewModel.NestModel.EditNest.ScaleSize = memoryString;
                _baseModels.TargetViewModel.NestModel.EditNest.Scale = scale;
            }
        }

        private void Validate()
        {
            _baseModels.TargetViewModel.NestModel.Validated = false;
            _baseModels.TargetViewModel.NestModel.CanUpdate = false;

            if (TagValidator != null)
            {
                /* used to enable the add function. a domain can
                 * be added only if valid fields and no list item 
                 * has been selected and currenly receivng focus.
                 */
                _baseModels.TargetViewModel.NestModel.Validated = (
                    TagValidator.IsValid &&
                    NameValidator.IsValid 
                );

                /* used to enable the update function. a domain can
                 * be updaed only if valid fields has been selected 
                 * and an item from a list is selected.
                 */
                _baseModels.TargetViewModel.NestModel.CanUpdate =
                    _baseModels.TargetViewModel.NestModel.Validated &&
                    AppNestsList.SelectedItem != null;
            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                if (_baseModels.TargetViewModel.NestModel.Nests.Any())
                {
                    Nest nest = _baseModels.TargetViewModel.NestModel.Nests.First();
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
                var existNests = from nest in _baseModels.TargetViewModel.NestModel.Nests
                                    where nest.Tag == Tag.Text
                                    select nest;
                if (existNests.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The nest with this tag already exist", "OK");
                    return;
                }

                Nest newNest = CopyUpdate(new Nest());
                NestPlatform workerPlatform = _baseModels.TargetViewModel.NestModel.Platforms.First(
                    x => x.Tag == "worker");

                // the new nest is a handler
                var handlerNests = from nest in _baseModels.TargetViewModel.NestModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (newNest.PlatformId != workerPlatform.Id && 
                    handlerNests.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "Only one handler type nest can exist.", "OK");
                    return;
                }

                await _baseModels.TargetViewModel.NestModel.CreateNestAsync(newNest);

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
                await Process(AppNestsList.SelectedItem as Nest, true,
                    _baseModels.TargetViewModel.NestModel.QueryNestAsync
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
                Nest updatingNest = AppNestsList.SelectedItem as Nest;

                var existNests = from nest in _baseModels.TargetViewModel.NestModel.Nests
                                 where nest.Tag == Tag.Text && nest.Id != updatingNest.Id
                                 select nest;
                if (existNests.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The nest with this tag already exist", "OK");
                    return;
                }

                Nest updateNest = CopyUpdate(updatingNest);
                if (updateNest != null)
                {
                    await Process(updateNest, true,
                        _baseModels.TargetViewModel.NestModel.UpdateNestAsync
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
                Nest nest = AppNestsList.SelectedItem as Nest;

                if (_baseModels.TargetViewModel.EditApp.IsDeployed)
                {
                    if (nest.Platform.Tag != "worker")
                    {
                        await DisplayAlert("Nester", "Cannot remove the hander nest while deplyed", "Yes", "No");
                        return;
                    }
                }

                await Process(nest, true,
                    _baseModels.TargetViewModel.NestModel.RemoveNestAsync,
                       new Func<Nest, Task<bool>>(
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
                NestPlatform workerPlatform = _baseModels.TargetViewModel.NestModel.Platforms.First(
                    x => x.Tag == "worker");

                var handlerNests = from nest in _baseModels.TargetViewModel.NestModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (handlerNests.ToArray().Length == 0)
                {
                    await DisplayAlert("Nester", "Please add a handler nest to process queries", "OK");
                    return;
                }

                if (_baseModels.WizardMode)
                {
                    await _baseModels.TargetViewModel.ContactModel.InitAsync();

                    // if currently trvelling back and forth on the 
                    // app wizard - move to the next

                    ContactsView contactsView = new ContactsView(_baseModels);
                    contactsView.MainSideView = MainSideView;
                    await MainSideView.Detail.Navigation.PushAsync(contactsView);
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
        }
    }
}

