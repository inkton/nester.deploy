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
using System.Threading.Tasks;
using Xamarin.Forms;
using Inkton.Nest.Model;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class AppLocationView : View
    {
        struct ForestButton
        {
            public ForestButton(AppLocationView view, string forestTag)
            {
                TapGestureRecognizer tap = new TapGestureRecognizer
                {
                    Command = new Command<Forest>(async (forest) => await view.OnSelectLocation(forest, true)),
                    CommandParameter = view.ViewModels.AppViewModel.DeploymentViewModel.ForestsByTag[forestTag.Replace('_', '-')]
                };

                FlagLabel = view.FindByName<Label>("FlagLabel_" + forestTag);
                FlagLabel.GestureRecognizers.Add(tap);

                FlagImage = view.FindByName<Image>("FlagImage_" + forestTag);
                FlagImage.GestureRecognizers.Add(tap);

                FlagHolder = view.FindByName<StackLayout>("FlagHolder_" + forestTag);
                FlagHolder.GestureRecognizers.Add(tap);
            }

            public Label FlagLabel;
            public Image FlagImage;
            public StackLayout FlagHolder;
        }

        private Dictionary<string, ForestButton> _forestButtons;

        public AppLocationView(BaseViewModels baseModels, 
            ObservableCollection<Forest> validForests)
        {
            InitializeComponent();

            ViewModels = baseModels;

            _forestButtons = new Dictionary<string, ForestButton>();
            
            if (baseModels.AppViewModel.ServicesViewModel.SelectedAppServiceTag == "nest-oak")
            {
                SetupOakLocations(validForests);
            }
            else
            {
                SetupRedbudLocations(validForests);
            }

            ButtonCancel.Clicked += ButtonCancel_ClickedAsync;
        }

        public override void UpdateBindings()
        {
            base.UpdateBindings();

            BindingContext = _baseViewModels.AppViewModel.DeploymentViewModel;
        }

        private void SetupOakLocations(ObservableCollection<Forest> validForests)
        {
            OakLocations.IsVisible = true;
            RedbudLocations.IsVisible = false;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    FlagImage_blue_mountain,
                    FlagImage_stadtwald,
                    FlagImage_bois_de_boulogne,
                    FlagImage_sherwood,
                    FlagImage_aokigahara,
                    FlagImage_vondelpark,
                    FlagImage_sentosa,
                    FlagImage_great_trinity,
                    FlagImage_fernbank,
                    FlagImage_redwood,
                    FlagImage_schiller,
                    FlagImage_angeles,
                    FlagImage_whitewater,
                    FlagImage_altmar,
                    FlagImage_hoh
                });

            foreach (string forestTag in new string[]
                {
                    "blue_mountain", "stadtwald", "bois_de_boulogne",
                    "sherwood","aokigahara", "vondelpark",
                    "sentosa", "great_trinity", "fernbank",
                    "redwood", "schiller", "angeles",
                    "whitewater", "altmar", "hoh"
                })
            {
                var tagUnderscores = forestTag.Replace('_', '-');
                Forest found = validForests.FirstOrDefault(x => x.Tag == tagUnderscores);
                if (found == null)
                {
                    AppLocationView view = this;
                    StackLayout flagHolder = view.FindByName<StackLayout>("FlagHolder_" + forestTag);
                    flagHolder.Opacity = 0.5;
                }
                else
                {
                    _forestButtons.Add(forestTag, new ForestButton(this, forestTag));
                }
            }
        }

        private void SetupRedbudLocations(ObservableCollection<Forest> validForests)
        {
            OakLocations.IsVisible = false;
            RedbudLocations.IsVisible = true;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    FlagHolder_cumberland,
                    FlagHolder_bourassa,
                    FlagHolder_gifford,
                    FlagHolder_elliott,
                    FlagHolder_glendalough,
                    FlagHolder_epping,
                    FlagHolder_vincennes_woods,
                    FlagHolder_waldspielpark_scheerwald,
                    FlagHolder_bukit_timah,
                    FlagHolder_hinohara,
                    FlagHolder_bukhansan,
                    FlagHolder_sanjay_gandhi,
                    FlagHolder_mont_royal 
                });

            foreach (string forestTag in new string[]
                {
                    "cumberland", "bourassa", "gifford",
                    "elliott","glendalough", "epping",
                    "vincennes_woods", "waldspielpark_scheerwald", "bukit_timah",
                    "hinohara", "bukhansan", "sanjay_gandhi",
                    "mont_royal"
                })
            {
                var tagUnderscores = forestTag.Replace('_', '-');
                Forest found = validForests.FirstOrDefault(x => x.Tag == tagUnderscores);
                if (found == null)
                {
                    AppLocationView view = this;
                    StackLayout flagHolder = view.FindByName<StackLayout>("FlagHolder_" + forestTag);
                    flagHolder.Opacity = 0.5;
                }
                else
                {
                    _forestButtons.Add(forestTag, new ForestButton(this, forestTag));
                }
            }
        }

        private void AnimateButtonTouched(Xamarin.Forms.View view, uint duration, string hexColorInitial, string hexColorFinal, int repeatCountMax)
        {
            var repeatCount = 0;
            view.Animate("changedBG", new Animation((val) => {
                if (repeatCount == 0)
                {
                    view.BackgroundColor = Color.FromHex(hexColorInitial);
                }
                else
                {
                    view.BackgroundColor = Color.FromHex(hexColorFinal);
                }
            }), duration, finished: (val, b) => {
                repeatCount++;
            }, repeat: () => {
                return repeatCount < repeatCountMax;
            });
        }

        protected override void SubscribeToMessages()
        {
            base.SubscribeToMessages();

            ProcessMessage<Forest>("select",
                    new Func<Forest, bool, bool, Task<Cloud.ServerStatus>>(OnSelectLocation));
        }

        protected override void UnsubscribeFromMessages()
        {
            base.UnsubscribeFromMessages();
            MessagingCenter.Unsubscribe<ManagedObjectMessage<Forest>>(this, "select");
        }

        private async Task<Cloud.ServerStatus> OnSelectLocation(Forest forest, bool doCache, bool throwIfError = true)
        {
            IsServiceActive = true;
            Cloud.ServerStatus status = new Cloud.ServerStatus();

            try
            {
                ForestButton button = _forestButtons[forest.Tag.Replace('-', '_')];

                await button.FlagImage.ScaleTo(0.9, 50, Easing.Linear);
                await Task.Delay(100);
                await button.FlagImage.ScaleTo(1, 50, Easing.Linear);

                AnimateButtonTouched(button.FlagHolder, 1500, "#66b9f1", "#E4F1FE", 1);
                AnimateButtonTouched(button.FlagHolder, 1500, "#66b9f1", "#E4F1FE", 1);
                AnimateButtonTouched(button.FlagHolder, 1500, "#66b9f1", "#E4F1FE", 1);

                _baseViewModels.AppViewModel.DeploymentViewModel.EditDeployment.ForestId = forest.Id;
                MainSideView.CurrentLevelViewAsync(new AppSummaryView(_baseViewModels));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
            return status;
        }

        private async void ButtonCancel_ClickedAsync(object sender, EventArgs e)
        {
            IsServiceActive = true;

            try
            {
                // Head back to homepage if the 
                // page was called from here
                MainSideView.UnstackViewAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }
    }
}


