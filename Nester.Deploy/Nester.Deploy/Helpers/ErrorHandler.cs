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
using System.Threading.Tasks;
using Xamarin.Forms;
using NesterDeploy = Nester.Deploy.App;

namespace Inkton.Nester.Helpers
{
    public static class ErrorHandler
    {
        public static async Task ExceptionAsync(Page page, Exception e)
        {
            await page.DisplayAlert("Nester Deploy", e.Message, "OK");
            (Application.Current as NesterDeploy).Log
                .Trace(e.Message, e.Source, LogSeverity.LogSeverityCritical);
        }

        public static async Task ExceptionAsync(Page page, string error)
        {
            await page.DisplayAlert("Nester Deploy", error, "OK");
            (Application.Current as NesterDeploy).Log
                .Trace(error, "--", LogSeverity.LogSeverityCritical);
        }
    }
}
