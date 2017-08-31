﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Flurl;
using Flurl.Http;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using PCLAppConfig;

[assembly: Dependency(typeof(Nester.Cloud.NesterService))]

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

    public class NesterService : INesterService
    {
        private string _endpoint = "https://api.nestapp.yt/";
        private BasicAuth _basicAuth;

        private Auth.Permit _permit;
        private Cache.IStorageService _storage;
        private string _environment;

        private delegate Task<Cloud.ServerStatus> HttpRequest<T>(T seed,
            IDictionary<string, string> data, string subPath = null) where T : Cloud.ManagedEntity, new();
        public delegate Task<Cloud.ServerStatus> CachedHttpRequest<T>(T seed,
            IDictionary<string, string> data, string subPath = null, bool doCache = true) where T : Cloud.ManagedEntity, new();

        public NesterService()
        {
            _storage = DependencyService.Get<Cache.IStorageService>();

            _environment = ConfigurationManager.AppSettings["env"];
        }

        public string Endpoint
        {
            get { return _endpoint; }
            set { _endpoint = value; }
        }

        public BasicAuth BasicAuth
        {
            get { return _basicAuth; }
            set { _basicAuth = value; }
        }

        public void ClearSession()
        {
            _permit = null;
        }

        public async Task<Cloud.ServerStatus> SignupAsync(Auth.Permit permit)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("password", permit.Password);
            data.Add("email", permit.Owner.Email);

            if (permit.SecurityCode != null)
            {
                data.Add("security_code", permit.SecurityCode);
                data.Add("nickname", permit.Owner.Nickname);
                data.Add("first_name", permit.Owner.FirstName);
                data.Add("surname", permit.Owner.LastName);
                data.Add("territory_iso_code", permit.Owner.TerritoryISOCode);
            }

            data.Add("env", _environment);

            Cloud.ServerStatus status = await PostAsync(permit, data);

            return status;
        }

        public async Task<Cloud.ServerStatus> RecoverPasswordAsync(Auth.Permit permit)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("email", permit.Owner.Email);
            data.Add("env", _environment);

            Cloud.ServerStatus status = await PutAsync(permit, data);

            if (status.Code == 0)
            {
                _permit = status.PayloadToObject<Auth.Permit>();
            }

            return status;
        }

        #region Utility

        private void LogConnectFailure(
            ref Cloud.ServerStatus result, Exception ex)
        {
            if (ex is FlurlHttpException)
            {
                FlurlHttpException httpEx = ex as FlurlHttpException;   
                if (httpEx.Call.Response != null)
                {
                    result.HttpStatus = httpEx.Call.Response.StatusCode;
                }
            }

            Utils.ErrorHandler.Exception(
                ex.Message, ex.StackTrace);
            result.Notes = "Failed to connect to remote server";
        }

        public async Task<Cloud.ServerStatus> QueryTokenAsync(Auth.Permit permit = null)
        {
            if (permit != null)
            {
                _permit = permit;
            }

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("password", _permit.Password);
            data.Add("env", _environment);

            Cloud.ServerStatus status = await GetAsync(_permit, data);

            if (status.Code == 0)
            {
                _permit = status.PayloadToObject<Auth.Permit>();
            }

            return status;
        }

        public async Task<Cloud.ServerStatus> ResetTokenAsync(Auth.Permit newPermit)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("token", _permit.Token);
            data.Add("password", newPermit.Password);
            data.Add("env", _environment);
                
            Cloud.ServerStatus status = await DeleteAsync(_permit, data);

            if (status.Code == 0)
            {
                newPermit.Token = _permit.Token;
                _permit = newPermit;
            }

            return status;
        }

        private async Task<Cloud.ServerStatus> PostAsync<T>(T seed,
            IDictionary<string, string> data, string subPath = null) where T : Cloud.ManagedEntity, new()
        {
            Cloud.ServerStatus result = new Cloud.ServerStatus(
                Result.NEST_RESULT_ERROR_LOCAL);

            try
            {
                string fullUrl = Endpoint + seed.Collection;
                if (subPath != null)
                {
                    fullUrl = fullUrl + subPath;
                }

                string json = await fullUrl.SetQueryParams(data)
                                .PostJsonAsync(seed)
                                .ReceiveString();   
               
                result = Result.ConvertObject(json, seed);
            }
            catch (Exception ex)
            {
                LogConnectFailure(ref result, ex);
            }

            return result;           
        }

        private async Task<Cloud.ServerStatus> GetAsync<T>(T seed,
            IDictionary<string, string> data, string subPath = null) where T : Cloud.ManagedEntity, new()
        {
            Cloud.ServerStatus result = new Cloud.ServerStatus(
                Result.NEST_RESULT_ERROR_LOCAL);

            try
            {
                string fullUrl = Endpoint + seed.CollectionKey;
                if (subPath != null)
                {
                    fullUrl = fullUrl + subPath;
                }

                string json = string.Empty;

                if (_basicAuth.Enabled)
                {
                    json = await fullUrl.SetQueryParams(data).WithBasicAuth(
                            _basicAuth.Username, _basicAuth.Password).GetAsync().ReceiveString();
                }
                else
                {
                    json = await fullUrl.SetQueryParams(data)
                            .GetAsync().ReceiveString();
                }

                result = Result.ConvertObject(json, seed);
            }
            catch (Exception ex)
            {
                LogConnectFailure(ref result, ex);
            }

            return result;
        }

        private async Task<Cloud.ServerStatus> PutAsync<T>(T seed,
            IDictionary<string, string> data, string subPath = null) where T : Cloud.ManagedEntity, new()
        { 
            Cloud.ServerStatus result = new Cloud.ServerStatus(
                Result.NEST_RESULT_ERROR_LOCAL);

            try
            {
                string fullUrl = Endpoint + seed.CollectionKey; 
                if (subPath != null)
                {
                    fullUrl = fullUrl + subPath;
                }

                string objJson = JsonConvert.SerializeObject(seed);
                var httpContent = new StringContent(objJson, Encoding.UTF8, "application/json");
                string json = await fullUrl.SetQueryParams(data)
                                .PutAsync(httpContent)
                                .ReceiveString();

                result = Result.ConvertObject(json, seed);
            }
            catch (Exception ex)
            {
                LogConnectFailure(ref result, ex);
            }

            return result;
        }

        private async Task<Cloud.ServerStatus> DeleteAsync<T>(T seed,
            IDictionary<string, string> data, string subPath = null) where T : Cloud.ManagedEntity, new()
        {
            Cloud.ServerStatus result = new Cloud.ServerStatus(
                Result.NEST_RESULT_ERROR_LOCAL);

            try
            {
                string fullUrl = Endpoint + seed.CollectionKey;
                if (subPath != null)
                {
                    fullUrl = fullUrl + subPath;
                }

                string objJson = JsonConvert.SerializeObject(seed);
                var httpContent = new StringContent(objJson, Encoding.UTF8, "application/json");
                string json = await fullUrl.SetQueryParams(data)
                                .DeleteAsync()
                                .ReceiveString();

                result = Result.ConvertObject(json, seed);
            }
            catch (Exception ex)
            {
                LogConnectFailure(ref result, ex);
            }

            return result;
        }

        private async Task<Cloud.ServerStatus> RetryWithFreshToken<T>(HttpRequest<T> request,
            T seed, IDictionary<string, string> data, 
            string subPath = null, bool doCache = true) where T : Cloud.ManagedEntity, new()
        {
            int retryCount = 3;
            Cloud.ServerStatus result = new Cloud.ServerStatus(
                Result.NEST_RESULT_ERROR_LOCAL);

            if (data == null)
            {
                data = new Dictionary<string, string>();
            }

            for (int i = 0; i < retryCount; i++)                                            
            {    
                // the service allows some non-secure calls
                // such as browse all apps. these do not
                // require a permit.           
                if (_permit != null)
                {
                    if (data.Keys.Contains("token"))
                    {
                        data.Remove("token");
                        data.Remove("env");
                    }

                    data.Add("token", _permit.Token);
                }

                data.Add("env", _environment);

                result = await request(seed, data, subPath);

                if (result.HttpStatus != System.Net.HttpStatusCode.Unauthorized)
                {
                    if (result.Code == 0)
                    {
                        if (doCache)
                        {
                            _storage.Save(seed);
                        }
                        else
                        {
                            _storage.Remove(seed);
                        }
                    }
                    break;
                }

                // Token expired, get another
                await QueryTokenAsync();
            }

            return result;
        }

        #endregion

        public async Task<Cloud.ServerStatus> CreateAsync<T>(T seed,
            IDictionary<string, string> data = null, string subPath = null, bool doCache = true) where T : Cloud.ManagedEntity, new()
        {
            return await RetryWithFreshToken(new HttpRequest<T>(PostAsync),
                seed, data, subPath, doCache);
        }

        public async Task<Cloud.ServerStatus> QueryAsync<T>(T seed,
            IDictionary<string, string> data = null, string subPath = null, bool doCache = true) where T : Cloud.ManagedEntity, new()
        {
            if (doCache && _storage.Load<T>(seed))
            {
                Cloud.ServerStatus status = new Cloud.ServerStatus(0);
                status.Payload = seed;
                return status;
            }

            return await RetryWithFreshToken(new HttpRequest<T>(GetAsync),
                seed, data, subPath, doCache);
        }

        public async Task<Cloud.ServerStatus> QueryAsyncList<T>(T seed,
            IDictionary<string, string> data = null, string subPath = null, bool doCache = true) where T : Cloud.ManagedEntity, new()
        {
            int retryCount = 3;
            Cloud.ServerStatus result = new Cloud.ServerStatus(
                   Result.NEST_RESULT_ERROR_LOCAL);

            if (data == null)
            {
                data = new Dictionary<string, string>();
            }

            string fullUrl = Endpoint + seed.Collection;
            if (subPath != null)
            {
                fullUrl = fullUrl + subPath;
            }

            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    if (_permit != null)
                    {
                        if (data.Keys.Contains("token"))
                        {
                            data.Remove("token");
                            data.Remove("env");
                        }

                        data.Add("token", _permit.Token);
                    }

                    data.Add("env", _environment);
                    string json = string.Empty;

                    if (_basicAuth.Enabled)
                    {
                        json = await fullUrl.SetQueryParams(data).WithBasicAuth(
                                _basicAuth.Username, _basicAuth.Password)
                                .GetAsync().ReceiveString();
                    }
                    else
                    {
                        json = await fullUrl.SetQueryParams(data)
                                .GetAsync().ReceiveString();
                    }

                    result = Result.ConvertObjectList(json, seed);
                }
                catch (Exception ex)
                {
                    LogConnectFailure(ref result, ex);
                }

                if (result.HttpStatus != System.Net.HttpStatusCode.Unauthorized)
                {
                    if (result.Code == 0)
                    {
                        ObservableCollection<T> list = result.Payload as ObservableCollection<T>;

                        if (doCache)
                        {
                            list.All(obj =>
                            {
                                _storage.Save(obj);
                                return true;
                            });
                        }
                        else
                        {
                            list.All(obj =>
                            {
                                _storage.Remove(obj);
                                return true;
                            });
                        }
                    }
                    break;
                }

                // Token expired, get another
                await QueryTokenAsync();
            }

            return result;
        }

        public async Task<Cloud.ServerStatus> UpdateAsync<T>(T seed,
                IDictionary<string, string> data = null, string subPath = null, bool doCache = true) where T : Cloud.ManagedEntity, new()
        {
            return await RetryWithFreshToken(new HttpRequest<T>(PutAsync),
                seed, data, subPath, doCache);
        }

        public async Task<Cloud.ServerStatus> RemoveAsync<T>(T seed,
                IDictionary<string, string> data = null, string subPath = null, bool doCache = false) where T : Cloud.ManagedEntity, new()
        {
            return await RetryWithFreshToken(new HttpRequest<T>(DeleteAsync),
                seed, data, subPath, doCache);
        }
    }
}
