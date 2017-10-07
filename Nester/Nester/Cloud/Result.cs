using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;
using System.Resources;
using System.Reflection;
using System.Dynamic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace Nester.Cloud
{
    public class Result
    {
        public const int NEST_RESULT_SUCCESS = 0;
        public const int NEST_RESULT_WARNING = 100;
        public const int NEST_RESULT_WARNING_UPDATING = 102;
        public const int NEST_RESULT_ERROR = -200;
        public const int NEST_RESULT_ERROR_FATAL = -202;
        public const int NEST_RESULT_ERROR_NAUTH = -204;
        public const int NEST_RESULT_ERROR_FIELDS = -206;
        public const int NEST_RESULT_ERROR_USER_NFOUND = -208;
        public const int NEST_RESULT_ERROR_APP_NFOUND = -210;
        public const int NEST_RESULT_ERROR_APP_TAG_EXIST = -212;
        public const int NEST_RESULT_ERROR_APP_EXISTS = -214;
        public const int NEST_RESULT_ERROR_USER_NAUTH = -216;
        public const int NEST_RESULT_ERROR_LOGIN_FAILED = -218;
        public const int NEST_RESULT_ERROR_PMETHOD_NFOUND = -220;
        public const int NEST_RESULT_ERROR_CONTACT_NFOUND = -222;
        public const int NEST_RESULT_ERROR_INVITATION_NFOUND = -224;
        public const int NEST_RESULT_ERROR_SERVICE_NFOUND = -226;
        public const int NEST_RESULT_ERROR_SERVICE_TYPE_EXISTS = -228;
        public const int NEST_RESULT_ERROR_APP_SERVICE_TIER_NFOUND = -230;
        public const int NEST_RESULT_ERROR_APP_SERVICE_TIER_EXIST = -232;
        public const int NEST_RESULT_ERROR_APP_SERVICE_FTIER_EXIST = -234;
        // Cannot cancel because serivice is essential                
        public const int NEST_RESULT_ERROR_APP_SERVICE_ESSENTIAL = -236;// Cann
        public const int NEST_RESULT_ERROR_APP_SERVICE_ALWAYS_ON = -238;
        public const int NEST_RESULT_ERROR_ADD_PAYMENT_METHOD = -240;
        public const int NEST_RESULT_ERROR_SUBSCRIPTION_NFOUND = -242;
        public const int NEST_RESULT_ERROR_SUBSCRIPTION_INVALID = -244;
        public const int NEST_RESULT_ERROR_FOREST_NOT_PLANTABLE = -246;
        public const int NEST_RESULT_ERROR_FOREST_NOT_FOUND = -248;
        // The project must have an app service before adding tree 
        public const int NEST_RESULT_ERROR_FOREST_APP_NFOUND = -250;
        // Must assign the project to tree before adding tree servi
        public const int NEST_RESULT_ERROR_APP_ASSIGN_TREE = -252;
        // Do not assign a tree now. Cannot assign a tree before su
        public const int NEST_RESULT_ERROR_APP_SERVICE_TIER_NEEDED = -254;
        // Project must be deployed to assign app services         
        public const int NEST_RESULT_ERROR_APP_NDEPLOYED = -256;
        // app cannot be deployed                                  
        public const int NEST_RESULT_ERROR_APP_DEPLOYED = -258;
        // The app cannot be changed after deployment              
        public const int NEST_RESULT_ERROR_APP_NUPDATES = -260;
        // Not a shared service tier                               
        public const int NEST_RESULT_ERROR_APP_SERVICE_TIER_NSHARED = -262;
        // Not a dedicated service tier                            
        public const int NEST_RESULT_ERROR_APP_SERVICE_TIER_NDEDICATED = -26;
        // A worker must specify the dependent handler.            
        public const int NEST_RESULT_ERROR_APP_HANDLER_NEEDED = -266;
        // Invalid service level.                                  
        public const int NEST_RESULT_ERROR_INVALID_SERVICE_LEVEL = -268;
        // Nest not found.                                         
        public const int NEST_RESULT_ERROR_NEST_NFOUND = -270;
        // Forest invalid for the  app tier                        
        public const int NEST_RESULT_ERROR_FOREST_INVALID = -272;
        // web handler nest already exist                          
        public const int NEST_RESULT_ERROR_NEST_HANDLER_EXIST = -274;
        // Domain not registered                                   
        public const int NEST_RESULT_ERROR_DOMAIN_UNREGISTERED = -276;
        // Domain not found                                        
        public const int NEST_RESULT_ERROR_DOMAIN_NFOUND = -278;
        // Domain ssl not found                                    
        public const int NEST_RESULT_ERROR_DOMAIN_CERT_NFOUND = -280;
        // Domain only one certificate can be assigned
        public const int NEST_RESULT_ERROR_DOMAIN_CERT_ASSIGNED = -282;
        // App not ready for the operation
        public const int NEST_RESULT_ERROR_APP_NREADY = -284;
        public const int NEST_RESULT_ERROR_SUBSCRIPTION_STATE = -286;
        public const int NEST_RESULT_ERROR_TREE_NFOUND = -288;
        public const int NEST_RESULT_NEST_HANDLER_NFOUND = -290;
        public const int NEST_RESULT_DOMAIN_DEFAULT = -292;
        public const int NEST_RESULT_DOMAIN_ALIAS_MALFORMED = -294;
        public const int NEST_RESULT_ERROR_DEPLOYMENT_NFOUND = -296;
        public const int NEST_RESULT_ERROR_DEPLOYMENT_EXIST = -298;
        public const int NEST_RESULT_ERROR_DEPLOYMENT_WRONGID = -300;
        // Errors produced locally whilst handing queries
        public const int NEST_RESULT_ERROR_LOCAL = -999;
        public const int NEST_RESULT_ERROR_PAYMENT_DECLINED = -302;
        public const int NEST_RESULT_ERROR_PAYMENT_BUSY = -304;
        public const int NEST_RESULT_ERROR_PERM_NFOUND = -306;
        public const int NEST_RESULT_ERROR_PERM_FOUND = -308;
        public const int NEST_RESULT_ERROR_NPLATFORM_NFOUND = -310;
        public const int NEST_RESULT_ERROR_AUTH_SECCODE = -312;
        public const int NEST_RESULT_ERROR_CONTACT_EXCEEDED = -320;

        public Result()
        {   
            ResultCode = -1;
            ResultText = "Uknown Error";
        }

        [JsonProperty("notes")]
        public string Notes { get; set; }
        [JsonProperty("result_code")]
        public int ResultCode { get; set; }
        [JsonProperty("result_text")]
        public string ResultText { get; set; }
        [JsonProperty("data")]
        public ExpandoObject Data { get; set; }

        public static ServerStatus ConvertObject<T>(string json, T seed) where T : ManagedEntity, new()
        {
            Result result = JsonConvert.DeserializeObject<Result>(json);
            T obj = default(T);

            if (result.ResultCode == 0 && result.Data != null)
            {
                var sourceDict = result.Data as IDictionary<string, object>;
                if (sourceDict.ContainsKey(seed.Entity))
                {
                    ExpandoObject entity = sourceDict[seed.Entity] as ExpandoObject;
                    obj = new T();
                    // this will copy default ref objects
                    Utils.Object.CopyPropertiesTo(seed, obj);
                    // this will copy the properties in the JSON
                    Utils.Object.CopyExpandoPropertiesTo(entity, obj);
                }
            }

            // forward to marshall the object back to the ui thread
            return ServerStatus.FromServerResult(obj, result);
        }

        public static ServerStatus ConvertObjectList<T>(string json, T seed) where T : ManagedEntity, new()
        {
            Result result = JsonConvert.DeserializeObject<Result>(json);
            ObservableCollection<T> objectList = null;

            if (result.ResultCode == 0 && result.Data != null)  
            {
                T obj = new T();
                var sourceDict = result.Data as IDictionary<string, object>;
                List<object> list = sourceDict[obj.Collection.TrimEnd('/')] as List<object>;
                objectList = new ObservableCollection<T>();

                if (list != null && list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        Utils.Object.CopyPropertiesTo(seed, obj);
                        Utils.Object.CopyExpandoPropertiesTo(item as ExpandoObject, obj);
                        objectList.Add(obj);
                        obj = new T();
                    }
                }
            }

            return ServerStatus.FromServerResult(objectList, result);
        }

        public static void ThrowError(Cloud.ServerStatus status)
        {
            string message = "Failed to connect to Nest server - Please check Internet connectiviy";

            if (status.Code != NEST_RESULT_ERROR_LOCAL)
            {
                if (status.Description != null && status.Description.Length > 0)
                {
                    message = status.LocalDescription;
                }
                if (status.Notes != null && status.Notes.Length > 0)
                {
                    message += " - " + status.Notes;
                }
            }

            throw new Exception(message);
        }

        public static async Task<ServerStatus> WaitForObjectAsync<T>(bool throwIfError, T seed, 
            CachedHttpRequest<T> request, bool doCache = true, IDictionary < string, string> data = null, 
            string subPath = null) where T : Cloud.ManagedEntity, new()
        {
            Cloud.ServerStatus status = await 
                request(seed, data, subPath, doCache);

            if (status.Code < 0 && throwIfError)
            {
                ThrowError(status);
            }

            return status;
        }

        public static async Task<ServerStatus> WaitForObjectListAsync<T>(
            bool throwIfError, T seed, bool doCache = true, IDictionary < string, string> data = null, 
            string subPath = null) where T : Cloud.ManagedEntity, new()
        {
            Cloud.ServerStatus status = await ((NesterUI)NesterUI.Current).
                NesterService.QueryAsyncList(seed, data, subPath, doCache);

            if (status.Code < 0 && throwIfError)
            {
                ThrowError(status);
            }

            return status;
        }

        public static async Task<ServerStatus> WaitForObjectListAsync<T>(
            INesterService nesterService, bool throwIfError, T seed, bool doCache = true, IDictionary<string, string> data = null,
            string subPath = null) where T : Cloud.ManagedEntity, new()
        {
            Cloud.ServerStatus status = await 
                nesterService.QueryAsyncList(seed, data, subPath, doCache);

            if (status.Code < 0 && throwIfError)
            {
                ThrowError(status);
            }

            return status;
        }
    }
}
