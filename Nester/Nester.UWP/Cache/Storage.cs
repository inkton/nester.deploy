using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;
using Newtonsoft.Json;
using Xamarin.Forms;

[assembly: Dependency(typeof(Nester.Cache.StorageService))]

namespace Nester.Cache
{
    public class StorageService : IStorageService
    {
        public StorageService()
        {
            Directory.CreateDirectory(this.Path);
        }

        public string Path
        {
            get
            {
                return System.IO.Path.Combine(
                    ApplicationData.Current.LocalFolder.Path, "nester");
            }
        }

        public void Clear()
        {
            try
            {
                if (Directory.Exists(this.Path))
                {
                    var files = Directory.GetFiles(this.Path, "*", SearchOption.AllDirectories).ToList();
                    foreach (string filePath in files)
                    {
                        File.Delete(filePath);
                    }
                    Directory.Delete(this.Path, true);
                }
            }
            catch (Exception) { }
        }

        private string GetObjectPath(Cloud.ManagedEntity obj)
        {
            return this.Path + @"\" + obj.CollectionKey.TrimEnd('/') + ".json";
        }

        public void Save<T>(T obj) where T : Cloud.ManagedEntity
        {
            if (!Directory.Exists(this.Path + @"\" + obj.Collection.TrimEnd('/')))
            {
                Directory.CreateDirectory(this.Path + @"\" + obj.Collection.TrimEnd('/'));
            }

            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(GetObjectPath(obj), json);
        }

        public bool Load<T>(T obj) where T : Nester.Cloud.ManagedEntity
        {
            string path = GetObjectPath(obj);
            if (!File.Exists(path))
            {
                return false;
            }

            string json = File.ReadAllText(path);
            T copy = JsonConvert.DeserializeObject<T>(json);
            Nester.Utils.Object.CopyPropertiesTo(copy, obj);
            return true;
        }

        public void Remove<T>(T obj) where T : Cloud.ManagedEntity
        {
            if (File.Exists(GetObjectPath(obj)))
            {
                File.Delete(GetObjectPath(obj));
            }
        }
    }
}