﻿using System;
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
    public partial class AppLocationView : Nester.Views.View
    {
        struct ForestButton
        {
            public ForestButton(AppLocationView view, string forestTag)
            {
                TapGestureRecognizer tap = new TapGestureRecognizer
                {
                    Command = new Command<Admin.Forest>(async (forest) => await view.OnSelectLocation(forest, true)),
                    CommandParameter = view._appViewModel.DeploymentModel.ForestsByTag[forestTag.Replace('_', '-')]
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

        public AppLocationView(AppViewModel appViewModel, 
            ObservableCollection<Admin.Forest> validForests)
        {
            InitializeComponent();

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

            _appViewModel = appViewModel;

            BindingContext = _appViewModel.DeploymentModel;
            _forestButtons = new Dictionary<string, ForestButton>();

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
                Admin.Forest found = validForests.FirstOrDefault(x => x.Tag == tagUnderscores);
                if (found != null)
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

            ProcessMessage<Admin.Forest>("select",
                    new Func<Admin.Forest, bool, bool, Task<Cloud.ServerStatus>>(OnSelectLocation));
        }

        protected override void UnsubscribeFromMessages()
        {
            base.UnsubscribeFromMessages();
            MessagingCenter.Unsubscribe<ManagedObjectMessage<Admin.Forest>>(this, "select");
        }

        private async Task<Cloud.ServerStatus> OnSelectLocation(Admin.Forest forest, bool doCache, bool throwIfError = true)
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

                _appViewModel.DeploymentModel.EditDeployment.ForestId = forest.Id;
                Navigation.InsertPageBefore(new AppSummaryView(_appViewModel), this);
                await Navigation.PopAsync();

            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
            return status;
        }
    }
}

