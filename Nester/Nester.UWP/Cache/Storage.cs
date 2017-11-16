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