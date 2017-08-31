using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Nester.Cache
{
    public interface IStorageService
    {
        string Path
        {
            get;
        }

        void Clear();

        void Save<T>(T obj) where T : Cloud.ManagedEntity;

        bool Load<T>(T obj) where T : Cloud.ManagedEntity;

        void Remove<T>(T obj) where T : Cloud.ManagedEntity;
    }
}
