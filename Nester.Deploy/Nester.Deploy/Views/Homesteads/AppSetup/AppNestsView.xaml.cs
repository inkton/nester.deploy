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
using Inkton.Nest.Model;
using Inkton.Nester.ViewModels;
using Inkton.Nester.Helpers;

namespace Inkton.Nester.Views
{
    public partial class AppNestsView : View
    {
        public AppNestsView(AppViewModel appViewModel, bool wizardMode = false)
            :base(wizardMode)
        {
            InitializeComponent();

            AppViewModel = appViewModel;

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

            AppNestsList.ItemSelected += AppNestsList_ItemSelected;
            Memory.SelectedIndexChanged += Memory_SelectedIndexChanged;
            Tag.Unfocused += Tag_Unfocused;

            ButtonDone.IsVisible = _wizardMode;
            if (_wizardMode)
            {
                // hide but do not collapse
                TopButtonPanel.Opacity = 0;

                NavigationPage.SetHasNavigationBar(this, true);
            }

            if (AppViewModel.EditApp.Type == "biflow")
            {
                Type.Items.Remove("API Handler");
            }
        }

        public override void UpdateBindings()
        {
            base.UpdateBindings();

            BindingContext = AppViewModel.NestViewModel;
        }

        async private void OnButtonBasicDetailsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await MainView.StackViewSkipBackAsync(
                    new AppBasicDetailView(AppViewModel));
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async private void OnButtonServiceClickedAsync(object sender, EventArgs e)
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

        async private void OnButtonDomainsClickedAsync(object sender, EventArgs e)
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

        async private void OnButtonContactsClickedAsync(object sender, EventArgs e)
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

        private void Clear()
        {
            AppNestsList.SelectedItem = null;
            AppViewModel.NestViewModel.EditNest = new Inkton.Nest.Model.Nest();
            AppViewModel.NestViewModel.EditNest.OwnedBy = AppViewModel.EditApp;

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

            Inkton.Nest.Model.Nest browseNest = AppNestsList.SelectedItem as Inkton.Nest.Model.Nest;
            Inkton.Nest.Model.Nest copy = new Inkton.Nest.Model.Nest();
            browseNest.CopyTo(copy);
            AppViewModel.NestViewModel.EditNest = copy;

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
            NestPlatform platform = AppViewModel.NestViewModel.Platforms.First(
                x => x.Id == browseNest.PlatformId);

            if (AppViewModel.EditApp.Type == "uniflow")
            {
                /*
                 * The standard webserver can be a mvc or api type 
                 * + worker makes 3 nest types
                 */
                System.Diagnostics.Debug.Assert(Type.Items.Count == 3);

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
            else if (AppViewModel.EditApp.Type == "biflow")
            {
                /*
                 * The websocket can be a mvc type only
                 * + worker makes 2 nest types
                 */
                System.Diagnostics.Debug.Assert(Type.Items.Count == 2);

                if (platform.Tag == "mvc")
                {
                    Type.SelectedIndex = 0;
                }
                else
                {
                    Type.SelectedIndex = 1;
                }
            }
        }

        private Inkton.Nest.Model.Nest CopyUpdate(Inkton.Nest.Model.Nest browseNest)
        {           
            browseNest.OwnedBy = AppViewModel.EditApp;
            browseNest.Tag = Inkton.Nester.Helpers.Tag.Clean(Tag.Text);
            browseNest.Name = Name.Text;
            browseNest.NestStatus = "active";

            NestPlatform platform = null;

            if (AppViewModel.EditApp.Type == "uniflow")
            {
                /*
                 * The standard webserver can be a mvc or api type 
                 * + worker makes 3 nest types
                 */
                System.Diagnostics.Debug.Assert(Type.Items.Count == 3);

                if (Type.SelectedIndex == 0)
                {
                    platform = AppViewModel.NestViewModel.Platforms.First(
                            x => x.Tag == "mvc");
                }
                else if (Type.SelectedIndex == 1)
                {
                    platform = AppViewModel.NestViewModel.Platforms.First(
                            x => x.Tag == "api");
                }
                else if (Type.SelectedIndex == 2)
                {
                    platform = AppViewModel.NestViewModel.Platforms.First(
                            x => x.Tag == "worker");
                }
            }
            else if (AppViewModel.EditApp.Type == "biflow")
            {
                /*
                 * The websocket can be a mvc type only
                 * + worker makes 2 nest types
                 */
                System.Diagnostics.Debug.Assert(Type.Items.Count == 2);

                if (Type.SelectedIndex == 0)
                {
                    platform = AppViewModel.NestViewModel.Platforms.First(
                            x => x.Tag == "mvc");
                }
                else if (Type.SelectedIndex == 1)
                {
                    platform = AppViewModel.NestViewModel.Platforms.First(
                            x => x.Tag == "worker");
                }
            }

            browseNest.Platform = platform;
            browseNest.Scale = (int)Scaling.Value;
            browseNest.ScaleSize = Memory.Items[Memory.SelectedIndex];

            return browseNest;
        }

        private void AppNestsList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            SetDefaults();
            DisplayMemory();
            Validate();
        }

        private void Memory_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayMemory();
        }

        private void Tag_Unfocused(object sender, FocusEventArgs e)
        {
            if (Tag.Text != null)
            {
                Tag.Text = Inkton.Nester.Helpers.Tag.Clean(Tag.Text);
            }
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

            if (AppViewModel.NestViewModel.EditNest != null)
            {
                AppViewModel.NestViewModel.EditNest.ScaleSize = memoryString;
                AppViewModel.NestViewModel.EditNest.Scale = scale;
            }
        }

        private void Validate()
        {
            AppViewModel.NestViewModel.Validated = false;
            AppViewModel.NestViewModel.CanUpdate = false;

            if (TagValidator != null)
            {
                /* used to enable the add function. a domain can
                 * be added only if valid fields and no list item 
                 * has been selected and currenly receivng focus.
                 */
                AppViewModel.NestViewModel.Validated = (
                    TagValidator.IsValid &&
                    NameValidator.IsValid 
                );

                /* used to enable the update function. a domain can
                 * be updaed only if valid fields has been selected 
                 * and an item from a list is selected.
                 */
                AppViewModel.NestViewModel.CanUpdate =
                    AppViewModel.NestViewModel.Validated &&
                    AppNestsList.SelectedItem != null;
            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                if (AppViewModel.NestViewModel.Nests.Any())
                {
                    Inkton.Nest.Model.Nest nest = AppViewModel.NestViewModel.Nests.First();
                    AppNestsList.SelectedItem = nest;
                }

                SetDefaults();
                DisplayMemory();
                Validate();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
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
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async void OnAddButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                var existNests = from nest in AppViewModel.NestViewModel.Nests
                                    where nest.Tag == Tag.Text
                                    select nest;
                if (existNests.Any())
                {
                    IsServiceActive = false;
                    await ErrorHandler.ExceptionAsync(this, "The nest with this tag already exist");
                    return;
                }

                Inkton.Nest.Model.Nest newNest = CopyUpdate(new Inkton.Nest.Model.Nest());
                NestPlatform workerPlatform = AppViewModel.NestViewModel.Platforms.First(
                    x => x.Tag == "worker");

                // the new nest is a handler
                var handlerNests = from nest in AppViewModel.NestViewModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (newNest.PlatformId != workerPlatform.Id && 
                    handlerNests.Any())
                {
                    IsServiceActive = false;
                    await ErrorHandler.ExceptionAsync(this, "Only one handler type nest can exist.");
                    return;
                }

                await AppViewModel.NestViewModel.CreateNestAsync(newNest);

                Clear();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async void OnRefreshButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                await Process(AppNestsList.SelectedItem as Inkton.Nest.Model.Nest, true,
                    AppViewModel.NestViewModel.QueryNestAsync
                );

                SetDefaults();
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async void OnUpdateButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                Inkton.Nest.Model.Nest updatingNest = AppNestsList.SelectedItem as Inkton.Nest.Model.Nest;

                var existNests = from nest in AppViewModel.NestViewModel.Nests
                                 where nest.Tag == Tag.Text && nest.Id != updatingNest.Id
                                 select nest;
                if (existNests.Any())
                {
                    IsServiceActive = false;
                    await ErrorHandler.ExceptionAsync(this, "The nest with this tag already exist");
                    return;
                }

                Inkton.Nest.Model.Nest updateNest = CopyUpdate(updatingNest);
                if (updateNest != null)
                {
                    await Process(updateNest, true,
                        AppViewModel.NestViewModel.UpdateNestAsync
                    );

                    SetDefaults();
                }
            }
            catch (Exception ex)
            {
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async void OnRemoveButtonClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                Inkton.Nest.Model.Nest nest = AppNestsList.SelectedItem as Inkton.Nest.Model.Nest;

                if (AppViewModel.EditApp.IsDeployed)
                {
                    if (nest.Platform.Tag != "worker")
                    {
                        await DisplayAlert("Nester", "Cannot remove the hander nest while deplyed", "Yes", "No");
                        return;
                    }
                }

                await Process(nest, true,
                    AppViewModel.NestViewModel.RemoveNestAsync,
                       new Func<Inkton.Nest.Model.Nest, Task<bool>>(
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
                await ErrorHandler.ExceptionAsync(this, ex);
            }

            IsServiceActive = false;
        }

        async void OnDoneButtonClickedAsync(object sender, EventArgs e)
        {
            try
            {
                NestPlatform workerPlatform = AppViewModel.NestViewModel.Platforms.First(
                    x => x.Tag == "worker");

                var handlerNests = from nest in AppViewModel.NestViewModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (handlerNests.ToArray().Length == 0)
                {
                    await ErrorHandler.ExceptionAsync(this, "Please add a handler nest to process queries");
                    return;
                }

                if (_wizardMode)
                {
                    await AppViewModel.ContactViewModel.InitAsync();

                    // if currently trvelling back and forth on the 
                    // app wizard - move to the next
                    await MainView.StackViewAsync(
                        new ContactsView(AppViewModel, _wizardMode));
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
        }
    }
}

