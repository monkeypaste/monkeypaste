using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using static Xamarin.Forms.Internals.Profile;
using MonkeyPaste.Common;
using CsvHelper.Configuration;
using CsvHelper;
using Newtonsoft.Json;
using System.IO;

namespace MonkeyPaste.Common {

    public static class MpExtensions {

        #region Collections

        public static bool AddOrReplace<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key, TValue value) {
            //returns true if kvp was added
            //returns false if kvp was replaced
            if (d.ContainsKey(key)) {
                d[key] = value;
                return false;
            }
            d.Add(key, value);
            return true;
        }
        public static T[] ToArray<T>(this object obj) where T:class {
            return new T[] { obj as T };
        }

        public static List<T> ToList<T>(this object obj) where T:class {
            return new List<T> { obj as T };
        }
        public static int FastIndexOf<T>(this IList<T> list, T value) {
            //this is 15x faster according to: https://stackoverflow.com/a/8266937/105028
            for (int index = 0; index < list.Count; index++) {
                if (list[index].Equals(value)) {
                    return index;
                }
            }
            return -1;
        }
        public static bool IsNullOrEmpty<T>(this IList<T> list) {
            return list == null || list.Count == 0;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list) {
            return list == null || list.Count() == 0;
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> list) {
            return list == null || list.Count == 0;
        }

        public static T PeekOrDefault<T>(this Stack<T> stack) {
            if(stack.Count == 0) {
                return default(T);
            }
            return stack.Peek();
        }

        public static T PeekOrDefault<T>(this Queue<T> queue) {
            if (queue.Count == 0) {
                return default(T);
            }
            return queue.Peek();
        }

        public static T DequeueOrDefault<T>(this Queue<T> queue) {
            if (queue.Count == 0) {
                return default(T);
            }
            return queue.Dequeue();
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (T item in source)
                action(item);
        }

        public static List<T> GetRange<T>(this ObservableCollection<T> collection, int startIdx, int count) {
            if (count == 0 && startIdx + count > 0) {
                throw new Exception("Collection empty");
            }
            if (startIdx < 0 || startIdx >= collection.Count()) { 
                throw new Exception($"startIdx {startIdx} is greater than collection {collection.Count()}");
            }
            if (startIdx + count >= collection.Count()) {
                count = collection.Count() - startIdx;
            }
            int i = 0;
            var outList = new List<T>();
            while (count > 0) {
                outList.Add(collection.ElementAt(startIdx + i));
                i++;
                count--;
            }
            return outList;
        }

        public static IOrderedEnumerable<TSource> OrderByDynamic<TSource, TKey>(this IEnumerable<TSource> source, bool isDescending, Func<TSource, TKey> keySelector) {
            if (isDescending) {
                return source.OrderByDescending<TSource, TKey>(keySelector);
            } else {
                return source.OrderBy<TSource, TKey>(keySelector);
            }
        }

        public static IOrderedEnumerable<TSource> OrderByDynamic<TSource, TKey>(this IEnumerable<TSource> source, bool isDescending, Func<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (isDescending) {
                return source.OrderByDescending<TSource, TKey>(keySelector,comparer);
            } else {
                return source.OrderBy<TSource, TKey>(keySelector,comparer);
            }
        }

        public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> source, IEnumerable<TKey> keys, Func<TSource, TKey> keySelector) => source.Join(keys, keySelector, id => id, (o, id) => o);
        
        public static void RemoveRange<TSource>(this IList<TSource> collection, int startIdx, int count) where TSource: class {
            if(count == 0) {
                return;
            }
            if(startIdx < 0 || startIdx >= collection.Count()) {
                return;
            }
            if(startIdx + count >= collection.Count()) {
                count = collection.Count() - startIdx;
            }
            while(count > 0) {
                collection.RemoveAt(startIdx);
                count--;
            }
        }

        //public static IList<TSource> AddRange<TSource>(this IList<TSource> collection, IList<TSource> itemsToAdd) where TSource : class {
        //    foreach (var item in itemsToAdd) {
        //        collection.Add(item);
        //    }
        //    return collection;
        //}

        //public static IList<TSource> Add<TSource>(this IList<TSource> collection, TSource itemToAdd) where TSource : class {
        //    collection.Add(itemToAdd);
        //    return collection;
        //}

        //public static IList<TSource> InsertRange<TSource>(this IList<TSource> collection, int idx, IList<TSource> itemsToInsert) where TSource : class {
        //    itemsToInsert.Reverse();
        //    foreach (var item in itemsToInsert) {
        //        collection.Insert(idx, item);
        //    }
        //    return collection;
        //}

        //public static IList<TSource> Insert<TSource>(this IList<TSource> collection, int idx, TSource itemToInsert) where TSource : class {
        //    collection.Insert(idx, itemToInsert);
        //    return collection;
        //}

        //public static List<T> ToList<T>(this T obj) where T : class {
        //    return new List<T>() { obj };
        //}

        //public static ObservableCollection<T> ToObservableCollection<T>(this T obj) where T : class {
        //    return new ObservableCollection<T>() { obj };
        //}
        #endregion

        #region Enums

        public static string[] EnumToLabels(this Type e,  string noneText = "", bool hideFirst = false) {
            if(e == null || !e.IsEnum) {
                return new string[] { };
            }

            var names = Enum.GetNames(e);
            for (int i = hideFirst ? 1:0; i < names.Length; i++) {
                names[i] = names[i].ToLabel(noneText);
            }
            return names;
        }

        public static string EnumToName<TValue>(this TValue value, string noneText = "None")
            where TValue : Enum {
            string valStr = value.ToString();
            var valParts = valStr.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

            return valParts[valParts.Length - 1];
        }

        public static string EnumToLabel<TValue>(this TValue value, string noneText = "")
            where TValue : Enum {

            return value.EnumToName(noneText).ToLabel(noneText);
        }

        public static int EnumToInt<TValue>(this TValue value) 
            where TValue : Enum => Convert.ToInt32(value);

        #endregion

        #region Strings
       
        #endregion

        #region Visual
        
        #endregion

        #region Streams

        public static Stream ToStream(this string value) {
            return value.ToStream(Encoding.UTF8);
        }

        public static Stream ToStream(this string value, System.Text.Encoding encoding) {
            var bytes = encoding.GetBytes(value);
            return new MemoryStream(bytes);
        }

        #endregion


        #region Data

        public static bool HasValue(this double value) {
            return !Double.IsNaN(value) && !Double.IsInfinity(value);
        }

        public static int ByteCount(this object obj) {
            if(obj == null) {
                return 0;
            }
            RuntimeTypeHandle th = obj.GetType().TypeHandle;
            //int size = *(*(int**)&th + 1);
            int size = Marshal.ReadInt32(obj.GetType().TypeHandle.Value, 4);
            return size;
        }

        public static bool IsUnsetValue(this object obj) {
            return obj.ToString().Contains("DependencyProperty.UnsetValue");
        }
        public static DateTime ToDateTime(this double unixTimeStamp) {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        #endregion

        #region Reflection

        public static async Task<T> InvokeAsync<T>(this MethodInfo @this, object obj, params object[] parameters) {
            dynamic awaitable = @this.Invoke(obj, parameters);
            await awaitable;
            return (T)awaitable.GetAwaiter().GetResult();
        }

        public static async Task InvokeAsync(this MethodInfo @this, object obj, params object[] parameters) {
            dynamic awaitable = @this.Invoke(obj, parameters);
            await awaitable;
        }

        public static object GetPropertyValue(this object obj, string propertyPath, object[] index = null) {
            object propObj = obj;
            PropertyInfo propInfo = null;
            var propPathParts = propertyPath.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < propPathParts.Length; i++) {
                string propPathPart = propPathParts[i];

                if (propObj == null) {
                    throw new Exception($"Child Object {propPathPart} on path {propertyPath} not found on object: {obj.GetType()}");
                }
                Type objType = propObj.GetType();
                propInfo = objType.GetProperty(propPathPart);
                if (propObj == null) {
                    throw new Exception($"Property {propPathPart} not found on object: {propObj.GetType()}");
                }

                if (propInfo == null) {
                    //this breaks when combining static/dynamic content parameters for http requests
                    //but returning the path is intended flow
                    return propertyPath;
                }
                propInfo.GetValue(propObj);

                if (i < propPathParts.Length - 1) {
                    propObj = propInfo.GetValue(propObj);
                }
            }
            try {
                return propInfo.GetValue(propObj, index);
            }catch(Exception ex) {
                MpConsole.WriteTraceLine(ex);
                return propertyPath;
            }            
        }

        public static T GetPropertyValue<T>(this object obj, string propertyPath, object[] index = null) 
            where T : class {
            return obj.GetPropertyValue(propertyPath, index) as T;
        }

        #endregion
    }
}
