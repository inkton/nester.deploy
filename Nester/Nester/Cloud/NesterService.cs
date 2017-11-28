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
using System.Threading.Tasks;

namespace Inkton.Nester.Cloud
{
    public struct BasicAuth
    {
        public BasicAuth(bool enabled = false,
            string username = null,
            string password = null)
        {
            Enabled = enabled;
            Username = username;
            Password = password;
        }

        public bool Enabled;
        public string Username;
        public string Password;
    }

    public delegate Task<Cloud.ServerStatus> HttpRequest<T>(T seed,
        IDictionary<string, string> data, string subPath = null) where T : Cloud.ManagedEntity, new();
    public delegate Task<Cloud.ServerStatus> CachedHttpRequest<T>(T seed,
        IDictionary<string, string> data, string subPath = null, bool doCache = true) where T : Cloud.ManagedEntity, new();

    public interface INesterService
    {
        string Endpoint { get; set; }

        BasicAuth BasicAuth { get; set; }

        void ClearSession();

        Task<string> GetIPAsync(string host);

        Task<Cloud.ServerStatus> SignupAsync(Auth.Permit permit);

        Task<Cloud.ServerStatus> RecoverPasswordAsync(Auth.Permit permit);

        Task<Cloud.ServerStatus> QueryTokenAsync(Auth.Permit permit = null);

        Task<Cloud.ServerStatus> ResetTokenAsync(Auth.Permit permit = null);

        Task<Cloud.ServerStatus> CreateAsync<T>(T seed,
            IDictionary<string, string> data = null, string subPath = null, bool doCache = true) where T : Cloud.ManagedEntity, new();

        Task<Cloud.ServerStatus> QueryAsync<T>(T seed,
            IDictionary<string, string> data = null, string subPath = null, bool doCache = true) where T : Cloud.ManagedEntity, new();

        Task<Cloud.ServerStatus> QueryAsyncList<T>(T seed,
            IDictionary<string, string> data = null, string subPath = null, bool doCache = true) where T : Cloud.ManagedEntity, new();

        Task<Cloud.ServerStatus> UpdateAsync<T>(T seed,
            IDictionary<string, string> data = null, string subPath = null, bool doCache = true) where T : Cloud.ManagedEntity, new();

        Task<Cloud.ServerStatus> RemoveAsync<T>(T seed,
                IDictionary<string, string> data = null, string subPath = null, bool doCache = false) where T : Cloud.ManagedEntity, new();
    }
}
