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
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Inkton.Nester.Views
{
    public class View : ContentPage
    {
        protected ActivityIndicator _activityIndicator;
        protected List<Xamarin.Forms.View> _blockWhenActive;
        protected List<Xamarin.Forms.View> _activeBlockViews;
        protected Views.AppModelPair _modelPair;
        protected MainSideView _mainSideView;

        public View()
        {
            SubscribeToMessages();
        }

        public virtual AppModelPair AppModelPair
        {
            get { return _modelPair; }
            set { _modelPair = value; }
        }

        public virtual MainSideView MainSideView
        {
            get { return _mainSideView; }
            set { _mainSideView = value; }
        }

        public Admin.INesterControl NesterControl
        {
            get
            {
                return Application.Current as Admin.INesterControl;
            }
        }

        public Admin.App App
        {
            get
            {
                return _modelPair.AppViewModel.EditApp;
            }
        }

        protected void ResetView()
        {
            if (_modelPair == null || _modelPair.AppViewModel == null)
            {
                // models has not been set or there is no
                // apps to begin with. the dashboard is set 
                // to a blank view.
                NesterControl.ResetView();
            }
            else
            {
                NesterControl.CreateAppView(_modelPair);
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
