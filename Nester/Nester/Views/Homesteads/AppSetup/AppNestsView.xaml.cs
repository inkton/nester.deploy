using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Nester.Views
{
    public partial class AppNestsView : Nester.Views.View
    {
        public AppNestsView(AppViewModel appViewModel)
        {
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

            _appViewModel = appViewModel;
            BindingContext = _appViewModel.NestModel;

            AppNestsList.SelectionChanged += AppNestsList_SelectionChanged;
            Memory.SelectedIndexChanged += Memory_SelectedIndexChanged;
        }

        async private void OnButtonBasicDetailsClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                LoadView(new AppBasicDetailView(_appViewModel));
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
                await _appViewModel.DomainModel.InitAsync();

                LoadView(new AppDomainView(_appViewModel));
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
                await _appViewModel.ContactModel.InitAsync();

                LoadView(new ContactsView(_appViewModel));
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

            _appViewModel.NestModel.EditNest = new Admin.Nest();
            _appViewModel.NestModel.EditNest.App = _appViewModel.EditApp;

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
            _appViewModel.NestModel.EditNest = copy;

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

            Admin.NestPlatform platform = _appViewModel.NestModel.Platforms.First(
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
            browseNest.App = _appViewModel.EditApp;
            browseNest.Tag = Tag.Text;
            browseNest.Name = Name.Text;
            browseNest.NestStatus = "active";

            Admin.NestPlatform platform = null;

            if (Type.SelectedIndex == 0)
            {
                platform = _appViewModel.NestModel.Platforms.First(
                        x => x.Tag == "mvc");
            }
            else if (Type.SelectedIndex == 1)
            {
                platform = _appViewModel.NestModel.Platforms.First(
                        x => x.Tag == "api");
            }
            else if (Type.SelectedIndex == 2)
            {
                platform = _appViewModel.NestModel.Platforms.First(
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

            if (_appViewModel.NestModel.EditNest != null)
            {
                _appViewModel.NestModel.EditNest.ScaleSize = memoryString;
                _appViewModel.NestModel.EditNest.Scale = scale;
            }
        }

        private void Validate()
        {
            _appViewModel.NestModel.Validated = false;
            _appViewModel.NestModel.CanUpdate = false;

            if (_appViewModel != null &&
                _appViewModel.NestModel != null)
            {
                /* used to enable the add function. a domain can
                 * be added only if valid fields and no list item 
                 * has been selected and currenly receivng focus.
                 */
                _appViewModel.NestModel.Validated = (
                    TagValidator.IsValid &&
                    NameValidator.IsValid 
                );

                /* used to enable the update function. a domain can
                 * be updaed only if valid fields has been selected 
                 * and an item from a list is selected.
                 */
                _appViewModel.NestModel.CanUpdate =
                    _appViewModel.NestModel.Validated &&
                    AppNestsList.SelectedItem != null;
            }
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                if (_appViewModel.NestModel.Nests.Any())
                {
                    Admin.Nest nest = _appViewModel.NestModel.Nests.First();
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
                var existNests = from nest in _appViewModel.NestModel.Nests
                                    where nest.Tag == Tag.Text
                                    select nest;
                if (existNests.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "The nest with this tag already exist", "OK");
                    return;
                }

                Admin.Nest newNest = CopyUpdate(new Admin.Nest());
                Admin.NestPlatform workerPlatform = _appViewModel.NestModel.Platforms.First(
                    x => x.Tag == "worker");

                // the new nest is a handler
                var handlerNests = from nest in _appViewModel.NestModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (newNest.PlatformId != workerPlatform.Id && 
                    handlerNests.Any())
                {
                    IsServiceActive = false;
                    await DisplayAlert("Nester", "Only one handler type nest can exist.", "OK");
                    return;
                }

                await _appViewModel.NestModel.CreateNestAsync(newNest);

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
                    _appViewModel.NestModel.QueryNestAsync
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

                var existNests = from nest in _appViewModel.NestModel.Nests
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
                        _appViewModel.NestModel.UpdateNestAsync
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

                if (_appViewModel.EditApp.IsDeployed)
                {
                    if (nest.Platform.Tag != "worker")
                    {
                        await DisplayAlert("Nester", "Cannot remove the hander nest while deplyed", "Yes", "No");
                        return;
                    }
                }

                await Process(nest, true,
                    _appViewModel.NestModel.RemoveNestAsync,
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
                Admin.NestPlatform workerPlatform = _appViewModel.NestModel.Platforms.First(
                    x => x.Tag == "worker");

                var handlerNests = from nest in _appViewModel.NestModel.Nests
                                   where nest.PlatformId != workerPlatform.Id
                                   select nest;

                if (handlerNests.ToArray().Length == 0)
                {
                    await DisplayAlert("Nester", "Please add a handler nest to process queries", "OK");
                    return;
                }

                if (_appViewModel.WizardMode)
                {
                    await _appViewModel.ContactModel.InitAsync();

                    // if currently trvelling back and forth on the 
                    // app wizard - move to the next
                    await Navigation.PushAsync(new ContactsView(_appViewModel));
                }
                else
                {
                    // Head back to homepage if the 
                    // page was called from here
                    LoadHomeView();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }
    }
}

