using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Nester.Views
{
    public class View : ContentPage
    {
        protected Views.AuthViewModel _authViewModel;
        protected Views.AppViewModel _appViewModel;

        protected ActivityIndicator _activityIndicator;
        protected List<Xamarin.Forms.View> _blockWhenActive = null;
        protected List<Xamarin.Forms.View> _activeBlockViews = null;

        protected Func<Views.View, bool> _viewLoader;
        protected MasterDetailPage _masterDetailPage;

        public View()
        {
            SubscribeToMessages();
        }

        public void SetModels(
            AuthViewModel authViewModel, 
            AppViewModel appViewModel)
        {
            _authViewModel = authViewModel;
            _appViewModel = appViewModel;
        }

        public virtual AuthViewModel AuthViewModel
        {
            get { return _authViewModel; }
            set { _authViewModel = value; }
        }

        public virtual AppViewModel AppViewModel
        {
            get { return _appViewModel; }
            set {  _appViewModel = value; }
        }

        public NesterUI ThisUI
        {
            get
            {
                return ((NesterUI)NesterUI.Current);
            }
        }

        public Admin.App App
        {
            get
            {
                return _appViewModel.EditApp;
            }
        }

        public MasterDetailPage MasterDetailPage
        {
            get
            {
                return _masterDetailPage;
            }
            set
            {
                _masterDetailPage = value;
            }
        }

        public Func<Views.View, bool> LoadView
        {
            get
            {
                return _viewLoader;
            }
            set
            {
                _viewLoader = value;
            }
        }

        protected void LoadHomeView()
        {
            if (_viewLoader != null)
            {
                Views.View view = null;

                if (_appViewModel.EditApp.IsBusy)
                {
                    view = new BannerView(BannerView.Status.Updating);
                }
                else if (!_appViewModel.EditApp.IsDeployed)
                {
                    view = new BannerView(BannerView.Status.WaitingDeployment);
                }
                else
                {
                    view = new Views.AppView(_appViewModel);
                }

                view.AppViewModel = _appViewModel;
                view.LoadView = _viewLoader;
                _viewLoader(view);
            }

        }

        protected void SetActivityMonotoring(ActivityIndicator activityIndicator,
            List<Xamarin.Forms.View> blockWhenActive = null)
        {
            _activityIndicator = activityIndicator;
            _blockWhenActive = blockWhenActive;
            _activeBlockViews = new List<Xamarin.Forms.View>();

            _activityIndicator.IsEnabled = false;
            _activityIndicator.IsRunning = false;
        }

        public bool IsServiceActive
        {
            get { return _activityIndicator.IsRunning; }
            set
            {
                _activityIndicator.IsVisible = value;
                _activityIndicator.IsRunning = value;

                if (_blockWhenActive != null)
                {
                    if (value)
                    {
                        _blockWhenActive.All(control =>
                        {
                            if (control.IsEnabled)
                            {
                                control.IsEnabled = false;
                                _activeBlockViews.Add(control);
                            }
                            return true;
                        });
                    }
                    else
                    {
                        _activeBlockViews.All(control =>
                        {
                            control.IsEnabled = true;
                            return true;
                        });

                        _activeBlockViews.Clear();
                    }
                }
            }
        }

        virtual protected void SubscribeToMessages()
        {
            MessagingCenter.Subscribe<Nester.Views.AlertMessage>(this, "popup", (alertMessage) =>
            {
                DisplayAlert("Nester", alertMessage.Message, "OK");
            });
        }

        virtual protected void UnsubscribeFromMessages()
        {
            MessagingCenter.Unsubscribe<Nester.Views.AlertMessage>(this, "popup");
        }

        async protected Task Process<T>(T obj, bool doCache,
            Func<T, bool, bool, Task<Cloud.ServerStatus>> processAsync,
            Func<T, Task<bool>> confirmAsync = null)
        {
            try
            {
                if (confirmAsync != null)
                {
                    if (!await confirmAsync(obj))
                    {
                        return;
                    }
                }

                await processAsync(obj, true, doCache);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }
        }

        protected void ProcessMessage<T>(string message, 
            Func<T, bool, bool, Task<Cloud.ServerStatus>> processAsync,
            Func<T, Task<bool>> confirmAsync = null)
        {
            MessagingCenter.Subscribe<ManagedObjectMessage<T>>(this, message, async (objMessage) =>
            {
                await Process(objMessage.Object, true,
                    processAsync, confirmAsync);
            });
        }

        protected override void OnAppearing()
        {
            IsServiceActive = false;

            base.OnAppearing();

            if (_blockWhenActive != null)
            {
                _blockWhenActive.All(control =>
                {
                    control.FadeTo(1, 1000, Easing.Linear);
                    return true;
                });
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (_blockWhenActive != null)
            {
                _blockWhenActive.All(control =>
                {
                    control.FadeTo(0, 1000, Easing.Linear);
                    return true;
                });
            }
        }
    }
}
