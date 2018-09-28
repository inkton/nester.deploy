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
using Xamarin.Forms.Xaml;
using Inkton.Nest.Model;
using Inkton.Nester.ViewModels;

namespace Inkton.Nester.Views
{
    public partial class AppAuditView : View
    {
        public AppAuditView(BaseViewModels baseModels)
        {
            InitializeComponent();

            ViewModels = baseModels;

            SetActivityMonotoring(ServiceActive,
                new List<Xamarin.Forms.View> {
                    ButtonCancel,
                    ButtonQuery
                });

            _activityIndicator = ServiceActive;

            ButtonQuery.Clicked += ButtonQuery_Clicked;
            ButtonCancel.Clicked += ButtonCancel_Clicked;

            ResetTimeFilter();

            QueryAsync();
        }

        private void ResetTimeFilter()
        {
            AnalyzeDateUTC.Date = DateTime.Now.ToUniversalTime();
            StartTime.Time = new TimeSpan(0, 0, 0);
            EndTime.Time = new TimeSpan(23, 59, 59);

            _baseViewModels.AppViewModel.DeploymentViewModel.AppAudits.Clear();
        }

        private async void QueryAsync()
        {
            IsServiceActive = true;

            try
            {
                _baseViewModels.AppViewModel.DeploymentViewModel.AppAudits.Clear();

                if (StartTime.Time > EndTime.Time)
                {
                    await DisplayAlert("Nester", "Start time must be earler than the end time", "OK");
                    return;
                }

                DateTime analyzeDateUTC = AnalyzeDateUTC.Date;

                // 2018-04-14T06:27:01+00:00 
                Dictionary<string, string> filter = new Dictionary<string, string>();
                filter.Add("from", string.Format(
                    "{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}",
                        analyzeDateUTC.Year, analyzeDateUTC.Month, analyzeDateUTC.Day,
                        StartTime.Time.Hours, StartTime.Time.Minutes, StartTime.Time.Seconds
                    ));
                filter.Add("to", string.Format(
                    "{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}",
                        analyzeDateUTC.Year, analyzeDateUTC.Month, analyzeDateUTC.Day,
                        EndTime.Time.Hours, EndTime.Time.Minutes, EndTime.Time.Seconds
                    ));

                await _baseViewModels.AppViewModel.DeploymentViewModel.QueryAppAuditsAsync(filter);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Nester", ex.Message, "OK");
            }

            IsServiceActive = false;
        }

        private void ButtonQuery_Clicked(object sender, EventArgs e)
        {
            QueryAsync();
        }

        public override void UpdateBindings()
        {
            base.UpdateBindings();

            BindingContext = _baseViewModels.AppViewModel.DeploymentViewModel;
        }
        
        private void ButtonCancel_Clicked(object sender, EventArgs e)
        {
            IsServiceActive = true;
            MainSideView.UnstackViewAsync();
            IsServiceActive = false;
        }
    }
}
