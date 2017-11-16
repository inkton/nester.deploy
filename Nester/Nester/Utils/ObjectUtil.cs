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
using Xamarin.Forms;
using System.Reflection;
using System.Dynamic;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Nester.Utils
{
    public static class Object
    {

        public static void Serialize(object value, System.IO.Stream s)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(s))
            using (Newtonsoft.Json.JsonTextWriter jsonWriter = new Newtonsoft.Json.JsonTextWriter(writer))
            {
                Newtonsoft.Json.JsonSerializer ser = new Newtonsoft.Json.JsonSerializer();
                ser.Serialize(jsonWriter, value);
                jsonWriter.Flush();
            }
        }

        public static T Deserialize<T>(System.IO.Stream s)
        {
            using (System.IO.StreamReader reader = new System.IO.StreamReader(s))
            using (Newtonsoft.Json.JsonTextReader jsonReader = new Newtonsoft.Json.JsonTextReader(reader))
            {
                Newtonsoft.Json.JsonSerializer ser = new Newtonsoft.Json.JsonSerializer();
                return ser.Deserialize<T>(jsonReader);
            }
        }

        public static void CopyPropertiesTo<T, TU>(this T source, TU dest)
        {
            var sourceProps = source.GetType().GetRuntimeProperties()
                   .Where(x => x.CanWrite).ToList();
            var destProps = dest.GetType().GetRuntimeProperties()
                   .Where(x => x.CanWrite).ToList();

            foreach (var sourceProp in sourceProps)
            {
                var value = sourceProp.GetValue(source, null);
                var p = destProps.FirstOrDefault(x => x.Name == sourceProp.Name);
                
                if (p != null)
                {
                    if (value == null)
                    {
                        p.SetValue(dest, null, null);
                    }
                    else
                    {
                        p.SetValue(dest, value, null);
                    }
                }
            }
        }

        public static void PourPropertiesTo<T, TU>(this T source, TU dest)
        {
            /* 
             * This version is similar to copy but does not 
             * copy if the source is null.
             * 
             */
            var sourceProps = source.GetType().GetRuntimeProperties()
                   .Where(x => x.CanWrite).ToList();
            var destProps = dest.GetType().GetRuntimeProperties()
                   .Where(x => x.CanWrite).ToList();

            foreach (var sourceProp in sourceProps)
            {
                var value = sourceProp.GetValue(source, null);
                var p = destProps.FirstOrDefault(x => x.Name == sourceProp.Name);

                if (value != null)
                {
                    p.SetValue(dest, value, null);
                }
            }
        }

        public static void CopyJsonPropertiesTo<T, TU>(this T source, TU dest)
        {
            var sourceProps = source.GetType().GetRuntimeProperties()
                   .Where(x => x.CanWrite).ToList();
            var destProps = dest.GetType().GetRuntimeProperties()
                   .Where(x => x.CanWrite).ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    var customAtr = sourceProp.CustomAttributes.ToArray();
                    if (customAtr != null && customAtr.Length > 0)
                    {
                        if (customAtr.First().AttributeType.Name == "JsonPropertyAttribute")
                        {
                            p.SetValue(dest, sourceProp.GetValue(source, null), null);
                        }
                    }
                }
            }
        }
       
        public static void CopyExpandoPropertiesTo<T>(ExpandoObject source, T dest)
        {
            var sourceProps = source.GetType().GetRuntimeProperties()
                   .Where(x => x.CanWrite).ToList();
            var destProps = dest.GetType().GetRuntimeProperties()
                   .Where(x => x.CanWrite).ToList();

            foreach (var destProp in destProps)
            {
                if (destProp.CustomAttributes.Any())
                {
                    var attribute = destProp.CustomAttributes.First();

                    if (attribute.AttributeType == typeof(Newtonsoft.Json.JsonPropertyAttribute))
                    {
                        string propName = (string)destProp.CustomAttributes.ElementAt(0).ConstructorArguments.ElementAt(0).Value;

                        var sourceDict = source as IDictionary<string, object>;
                        bool ignore = false;

                        foreach (var arg in attribute.NamedArguments)  
                        {
                            if (arg.MemberName == "NullValueHandling")
                            {
                                if (!sourceDict.ContainsKey(propName))
                                {
                                    ignore = true;
                                }
                                break;
                            }
                        }

                        if (ignore)
                        {
                            continue;
                        }

                        var value = sourceDict[propName];

                        object setObj = destProp.GetValue(dest);

                        if (setObj == null)
                        {
                            // process reference types

                            if (destProp.PropertyType == typeof(string))
                            {
                                destProp.SetValue(dest, value);
                                continue;
                            }
                            else if ((destProp.PropertyType == typeof(string[])))
                            {
                                IList objList = value as IList;
                                string [] array = new string[objList.Count];

                                int index = 0;
                                foreach (object item in (value as IList))
                                {
                                    array[index] = item as string;
                                    ++index;
                                }

                                destProp.SetValue(dest, array, null);
                                continue;
                            }
                            else if ((destProp.PropertyType == typeof(ObservableCollection<ExpandoObject>)))
                            {
                                IEnumerable<object> objEnum = value as IEnumerable<object>;

                                if (objEnum.Count() == 0)
                                {
                                    continue;
                                }

                                IEnumerable<object> objSubEnum = objEnum.FirstOrDefault() as IEnumerable<object>;

                                if (objSubEnum.Count() == 0)
                                {
                                    continue;
                                }

                                var data = new ObservableCollection<ExpandoObject>();
                                int index = 0, subIndex = 0;

                                foreach (IEnumerable<object> item in objEnum)
                                {
                                    var subObject = new ExpandoObject() as IDictionary<string, object>;

                                    foreach (object subItem in item)
                                    {
                                        subObject.Add("Value" + subIndex.ToString(), subItem);
                                        ++subIndex;
                                    }

                                    data.Add(subObject as ExpandoObject);

                                    ++index;
                                    subIndex = 0;
                                }

                                destProp.SetValue(dest, data, null);
                                continue;
                            }
                            else if ((destProp.PropertyType == typeof(object[,])))
                            {
                                IEnumerable<object> objEnum = value as IEnumerable<object>;

                                if (objEnum.Count() == 0)
                                {
                                    continue;
                                }

                                IEnumerable<object> objSubEnum = objEnum.FirstOrDefault() as IEnumerable<object>;

                                if (objSubEnum.Count() == 0)
                                {
                                    continue;
                                }

                                object[,] array = new object[objEnum.Count(), objSubEnum.Count()];
                                int index = 0, subIndex = 0;

                                foreach (IEnumerable<object> item in objEnum)
                                {
                                    foreach (object subItem in item)
                                    {
                                        array[index, subIndex] = subItem;
                                        ++subIndex;
                                    }
                                    ++index;
                                    subIndex = 0;
                                }

                                destProp.SetValue(dest, array, null);
                                continue;
                            }

                            setObj = Activator.CreateInstance(
                                destProp.PropertyType);
                        }

                        if (setObj is Cloud.ManagedEntity)
                        {
                            CopyExpandoPropertiesTo(value as ExpandoObject, setObj);
                            destProp.SetValue(dest, setObj);
                        }
                        else if (destProp.PropertyType == typeof(decimal))
                        {
                            destProp.SetValue(dest, new Decimal((long)value));
                        }
                        else if (destProp.PropertyType == typeof(bool))
                        {
                            if (value is Int64)
                            {
                                destProp.SetValue(dest, ((Int64)value) != 0);
                            }
                            else if (value is string)
                            {
                                destProp.SetValue(dest, ((string)value) != "False");
                            }
                        }
                        else
                        {
                            destProp.SetValue(dest, value);
                        }
                    }
                }
            }
        }
    }
}
