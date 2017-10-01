using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nester.Cloud
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
