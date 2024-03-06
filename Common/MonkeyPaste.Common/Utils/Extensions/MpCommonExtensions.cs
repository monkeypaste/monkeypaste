using HtmlAgilityPack;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonkeyPaste.Common {
    public static class MpCommonExtensions {

        #region DateTime

        public static string ToTickChecksum(this DateTime dt) {
            return dt.Ticks.ToString().CheckSum();
        }

        #endregion

        #region Collections
        public static void AddDisposable(this IDisposable disp, IList<IDisposable> list) {
            if (disp == null || list == null) {
                if (list == null) {
                    MpDebug.Break($"List should exist");
                }
                return;
            }
            list.Add(disp);
        }
        public static void AddRangeOrDefault<T>(this IList<T> list, IEnumerable<T> range) {
            if (range == null) {
                return;
            }
            list.AddRange(range);
        }

        public static void RemoveNullsInPlace(this object[] fpArr) {
            if (fpArr == null) {
                return;
            }
            // NOTE removing omitted file paths IN PLACE so ref persists

            for (int i = 0; i < fpArr.Length; i++) {
                // shift all nulls to end of list
                if (fpArr[i] != null) {
                    continue;
                }
                for (int j = i; j < fpArr.Length - 1; j++) {
                    // shift null to end 
                    fpArr[j] = fpArr[j + 1];
                }
            }

            //Resize array, removing empties
            Array.Resize(ref fpArr, fpArr.Where(x => x != null).Count());
        }

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source) {
            // from https://thomaslevesque.com/2019/11/18/using-foreach-with-index-in-c/
            return source.Select((item, index) => (item, index));
        }
        public static void Move<T>(this IList<T> list, int oldIdx, int newIdx) {
            oldIdx = Math.Max(0, Math.Min(list.Count - 1, oldIdx));
            newIdx = Math.Max(0, Math.Min(list.Count - 1, newIdx));

            if (oldIdx == newIdx) {
                return;
            }
            T item = list[oldIdx];
            list.RemoveAt(oldIdx);
            if (oldIdx < newIdx) {
                newIdx -= 1;
            }
            list.Insert(newIdx, item);
        }
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> range) {
            if (list == null || range == null) {
                throw new NullReferenceException($"{(list == null ? "Dest must be initialized" : string.Empty)} {(range == null ? "range must be non-null" : string.Empty)}");
            }
            //range.ForEach(x => list.Add(x));
            foreach (var item in range) {
                list.Add(item);
            }
        }

        public static T AggregateOrDefault<T>(this IEnumerable<T> enumerable, Func<T, T, T> func) {
            if (enumerable == null || !enumerable.Any()) {
                return default;
            }
            return enumerable.Aggregate(func);
        }
        public static IEnumerable<T> Difference<T>(this IEnumerable<T> enumerable, IEnumerable<T> other, IEqualityComparer<T> comparer = default) {
            if (enumerable == null && other == null) {
                return new List<T>();
            }
            if (enumerable == null) {
                return other;
            }
            if (other == null) {
                return enumerable;
            }
            if (comparer == default) {
                return enumerable.Union(other).Except(enumerable.Intersect(other)); ;
            }
            return enumerable.Union(other).Except(enumerable.Intersect(other, comparer)); ;
        }


        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> enumerable) {
            List<T> enumerable_copy = enumerable.ToList();
            List<T> rand_list = new List<T>();

            while (enumerable_copy.Count() > 0) {
                int idx = MpRandom.Rand.Next(enumerable_copy.Count);
                T item = enumerable_copy.ElementAt(idx);
                enumerable_copy.RemoveAt(idx);
                rand_list.Add(item);
            }

            return rand_list;
        }

        public static ObservableCollection<TSource> Sort<TSource, TKey>(
            this ObservableCollection<TSource> source,
            Func<TSource, TKey> keySelector,
            bool desc = false) where TSource : class {
            if (source == null) {
                return null;
            }
            Comparer<TKey> comparer = Comparer<TKey>.Default;

            for (int i = source.Count - 1; i >= 0; i--) {
                for (int j = 1; j <= i; j++) {
                    TSource o1 = source[j - 1];
                    TSource o2 = source[j];
                    int comparison = comparer.Compare(keySelector(o1), keySelector(o2));
                    if (desc && comparison < 0) {
                        source.Move(j, j - 1);
                    } else if (!desc && comparison > 0) {
                        source.Move(j - 1, j);
                    }
                }
            }
            return source;
        }

        public static int IndexOf<T>(this IEnumerable<T> obj, T value) {
            return obj.IndexOf(value, null);
        }

        public static int IndexOf<T>(this IEnumerable<T> obj, T value, IEqualityComparer<T> comparer) {
            comparer = comparer ?? EqualityComparer<T>.Default;
            var found = obj
                .Select((a, i) => new { a, i })
                .FirstOrDefault(x => comparer.Equals(x.a, value));
            return found == null ? -1 : found.i;
        }

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
            return list == null || !list.Any();
        }

        public static bool IsNullOrEmpty<T>(this ICollection<T> list) {
            return list == null || list.Count == 0;
        }

        public static T PeekOrDefault<T>(this Stack<T> stack) {
            if (stack.Count == 0) {
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
        //public static T ElementAtOrDefault<T>(this IEnumerable<T> source, int idx) {
        //    if (source == null || idx < 0 || idx >= source.Count()) {
        //        return default(T);
        //    }
        //    return source.ElementAt(idx);
        //}
        public static void ForEach<T>(this IEnumerable source, Action<T> action) {
            if (source == null) {
                return;
            }
            foreach (T item in source) {
                action(item);
            }
        }

        public static void ForEach<T>(this IEnumerable source, Action<T, int> action) {
            if (source == null) {
                return;
            }
            int idx = 0;
            foreach (T item in source) {
                action(item, idx++);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            if (source == null) {
                return;
            }
            foreach (T item in source) {
                action(item);
            }
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action) {
            if (source == null) {
                return;
            }
            int idx = 0;
            foreach (T item in source) {
                action(item, idx++);
            }
        }
        public static void ReplaceItems<T>(this ObservableCollection<T> collection, IEnumerable<T> source) {
            var to_remove = collection.Where(x => !source.Contains(x)).ToList();
            int to_remove_count = to_remove.Count;
            while (to_remove_count > 0) {
                collection.Remove(source.ElementAt(to_remove_count - 1));
                to_remove_count--;
            }
            foreach (var si in source) {
                //if()
            }
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
                return source.OrderByDescending<TSource, TKey>(keySelector, comparer);
            } else {
                return source.OrderBy<TSource, TKey>(keySelector, comparer);
            }
        }

        public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> source, IEnumerable<TKey> keys, Func<TSource, TKey> keySelector) => source.Join(keys, keySelector, id => id, (o, id) => o);

        public static void RemoveRange<TSource>(this IList<TSource> collection, int startIdx, int count) where TSource : class {
            if (count == 0) {
                return;
            }
            if (startIdx < 0 || startIdx >= collection.Count()) {
                return;
            }
            if (startIdx + count >= collection.Count()) {
                count = collection.Count() - startIdx;
            }
            while (count > 0) {
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

        #region HtmlAgility

        public static HtmlNodeCollection SelectNodesSafe(this HtmlNode node, string xpath) {
            if (node.SelectNodes(xpath) is not { } hnc) {
                return new(node);
            }
            return hnc;
        }

        public static HtmlNode FindParent(this HtmlNode node, string parentTagName) {
            HtmlNode cur_node = node;
            while (cur_node != null) {
                if (cur_node.Name == parentTagName) {
                    return cur_node;
                }
                cur_node = cur_node.ParentNode;
            }
            return null;
        }
        public static bool IsBlockElement(this HtmlNode node) {
            return Regex.IsMatch(node.Name, "^(address|blockquote|body|center|dir|div|dl|fieldset|form|h[1-6]|hr|isindex|menu|noframes|noscript|ol|p|pre|table|ul|dd|dt|frameset|li|tbody|td|tfoot|th|thead|tr|html)");
        }
        public static HtmlNode CloneEmpty(this HtmlNode node) {
            var empty_clone = node.Clone();
            empty_clone.RemoveAllChildren();
            return empty_clone;
        }
        public static (HtmlNode start, HtmlNode end) SplitTextNode(this HtmlTextNode text_node, int index, int length, string match_class_to_add) {
            try {
                if (text_node.ParentNode is not { } parentNode) {
                    return default;
                }
                bool was_parent_block = parentNode.IsBlockElement();
                if (was_parent_block) {
                    // ensure parent isn't block
                    HtmlNode new_parent_span = text_node.OwnerDocument.CreateElement("span");
                    new_parent_span.AppendChild(text_node);
                    text_node.ParentNode.ReplaceChild(new_parent_span, text_node);
                    parentNode = new_parent_span;
                    text_node = parentNode.FirstChild as HtmlTextNode;
                }

                string node_text = text_node.InnerText.DecodeSpecialHtmlEntities();
                string match_text = length < 0 ? node_text.Substring(index) : node_text.Substring(index, length);

                List<HtmlNode> split_results = [];
                //HtmlNode temp_wrapper_span = parentNode.CloneEmpty();
                if (index > 0) {
                    // create lead run (in example "{'>',"")
                    string lead_text = node_text.Substring(0, index);
                    HtmlNode lead_text_node = text_node.OwnerDocument.CreateTextNode(lead_text.EncodeSpecialHtmlEntities());
                    HtmlNode lead_span = parentNode.CloneEmpty();
                    lead_span.AppendChild(lead_text_node);
                    //temp_wrapper_span.AppendChild(lead_span);
                    split_results.Add(lead_span);
                }

                // wrap match in span tag
                HtmlNode match_text_node = text_node.OwnerDocument.CreateTextNode(match_text.EncodeSpecialHtmlEntities());
                HtmlNode match_span = parentNode.CloneEmpty();
                match_span.AppendChild(match_text_node);
                match_span.AddClass(match_class_to_add);
                //temp_wrapper_span.AppendChild(match_span_node);
                split_results.Add(match_span);

                int end_idx = index + match_text.Length;

                if (end_idx < node_text.Length) {
                    // create trailing run after encoded special entities
                    string trailing_text = node_text.Substring(end_idx);
                    HtmlNode trail_text_node = text_node.OwnerDocument.CreateTextNode(trailing_text.EncodeSpecialHtmlEntities());
                    HtmlNode trail_span = parentNode.CloneEmpty();
                    trail_span.AppendChild(trail_text_node);
                    //temp_wrapper_span.AppendChild(trail_span);
                    split_results.Add(trail_span);
                }
                HtmlNode real_parent = was_parent_block ? parentNode.ParentNode : parentNode;
                HtmlNode real_child = was_parent_block ? parentNode : text_node;
                real_parent.ReplaceChild(split_results.First(), real_child);
                foreach (var split_node in split_results.Skip(1)) {
                    real_parent.InsertAfter(split_node, split_results.First());
                }
                return (split_results.First(), split_results.Last());
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error splitting node '{text_node.ToStringOrEmpty()}' idx {index} len {length}", ex);
            }
            return default;
        }
        public static void ExtractRange(this (int sub_idx, HtmlTextNode node) start, (int sub_idx, HtmlTextNode node) end, string match_class_to_add) {
            if (start.node == end.node) {
                int len = end.sub_idx - start.sub_idx + 1;
                start.node.SplitTextNode(start.sub_idx, len, match_class_to_add);
                return;
            }

            var start_result = start.node.SplitTextNode(start.sub_idx, -1, match_class_to_add);
            var end_result = end.node.SplitTextNode(0, end.sub_idx + 1, match_class_to_add);

            void ExtractInner(HtmlNode node, HtmlTextNode source, HtmlTextNode target) {
                if (node == null || node == target) {
                    return;
                }

                if (node.IsBlockElement()) {
                    ExtractInner(node.FirstChild, source, target);
                    return;
                }
                if (node != source && node is HtmlTextNode tn) {
                    var cur_result = tn.SplitTextNode(0, -1, match_class_to_add);
                    node = cur_result.end;
                }
                if (node.NextSibling == null) {
                    if (node.FirstChild == null) {
                        ExtractInner(node.ParentNode.NextSibling, source, target);
                        return;
                    }
                    ExtractInner(node.FirstChild, source, target);
                    return;
                }
                ExtractInner(node.NextSibling, source, target);

            }
            ExtractInner(start_result.end, start_result.end.LastChild as HtmlTextNode, end_result.start.FirstChild as HtmlTextNode);
        }
        #endregion

        #region Enums

        public static int Length(this Type e) {
            if (e == null || !e.IsEnum) {
                // should only pass enum
                MpDebug.Break();
                return 0;
            }
            return Enum.GetNames(e).Length;
        }

        public static string[] EnumerateEnumToUiStrings(this Type e, string noneText = "", bool hideFirst = false, string spaceStr = " ") {
            if (e == null || !e.IsEnum) {
                return new string[] { };
            }

            var names = Enum.GetNames(e);
            for (int i = hideFirst ? 1 : 0; i < names.Length; i++) {
                names[i] = names[i].ToProperCase(noneText, spaceStr);
            }
            return names;
        }

        public static IEnumerable<TEnum> EnumerateEnum<TEnum>(this Type enumType) {
            //where TEnum: Enum {
            if (enumType == null || !enumType.IsEnum) {
                yield break;
            }
            foreach (TEnum val in Enum.GetValues(enumType)) {
                yield return val;
            }
        }


        public static string EnumToProperCase<TValue>(this TValue value, string noneText = "")
            where TValue : Enum {

            return value.ToString().ToProperCase(noneText);
        }

        public static bool HasAllFlags<T>(this T value, T flags) where T : Enum {
            if (Enum.GetUnderlyingType(typeof(T)) == typeof(byte)) {
                var byteValue = Convert.ToByte(value);
                var byteFlags = Convert.ToByte(flags);
                return (byteValue & byteFlags) == byteFlags;
            } else if (Enum.GetUnderlyingType(typeof(T)) == typeof(short)) {
                var shortValue = Convert.ToInt16(value);
                var shortFlags = Convert.ToInt16(flags);
                return (shortValue & shortFlags) == shortFlags;
            } else if (Enum.GetUnderlyingType(typeof(T)) == typeof(int)) {
                var intValue = Convert.ToInt32(value);
                var intFlags = Convert.ToInt32(flags);
                return (intValue & intFlags) == intFlags;
            } else if (Enum.GetUnderlyingType(typeof(T)) == typeof(long)) {
                var longValue = Convert.ToInt64(value);
                var longFlags = Convert.ToInt64(flags);
                return (longValue & longFlags) == longFlags;
            } else {
                throw new NotSupportedException("Enum with size of " + Unsafe.SizeOf<T>() + " are not supported");
            }

        }
        public static bool HasAnyFlag<T>(this T value, T flags) where T : Enum {
            if (Enum.GetUnderlyingType(typeof(T)) == typeof(byte)) {
                var byteValue = Convert.ToByte(value);
                var byteFlags = Convert.ToByte(flags);
                return (byteValue & byteFlags) != 0;
            } else if (Enum.GetUnderlyingType(typeof(T)) == typeof(short)) {
                var shortValue = Convert.ToInt16(value);
                var shortFlags = Convert.ToInt16(flags);
                return (shortValue & shortFlags) != 0;
            } else if (Enum.GetUnderlyingType(typeof(T)) == typeof(int)) {
                var intValue = Convert.ToInt32(value);
                var intFlags = Convert.ToInt32(flags);
                return (intValue & intFlags) != 0;
            } else if (Enum.GetUnderlyingType(typeof(T)) == typeof(long)) {
                var longValue = Convert.ToInt64(value);
                var longFlags = Convert.ToInt64(flags);
                return (longValue & longFlags) != 0;
            } else {
                throw new NotSupportedException("Enum with size of " + Unsafe.SizeOf<T>() + " are not supported");
            }
        }

        public static void AddFlag<T>(ref this T value, T flag) where T : struct, Enum {
            //string result = $"{paramValue}, {flag}".Trim(new[] { ' ', ',' }); ;
            //paramValue = (T)Enum.Parse(typeof(T), result);
            string resultStr =
                string.Join(
                    ", ",
                    value.ToString()
                    .SplitNoEmpty(",")
                    .Select(x => x.Trim())
                    .Where(x => x != flag.ToString())
                    .Union(new[] { flag.ToString() }));

            if (Enum.TryParse(resultStr, out T result)) {
                value = result;
            } else {
                value = (T)(object)0;
            }
        }
        public static void RemoveFlag<T>(ref this T value, T flag) where T : struct, Enum {
            // remove flag from string of paramValue and any leading/trailing commas 
            string resultStr =
                string.Join(
                    ", ",
                    value.ToString()
                    .SplitNoEmpty(",")
                    .Select(x => x.Trim())
                    .Where(x => x != flag.ToString()));

            if (Enum.TryParse(resultStr, out T result)) {
                value = result;
            } else {
                value = (T)(object)0;
            }
        }

        public static IEnumerable<T> All<T>(this T value) where T : struct, Enum {
            // remove flag from string of paramValue and any leading/trailing commas 
            return value.ToString().SplitNoEmpty(",").Select(x => x.ToEnum<T>());
        }

        #endregion

        #region EventHandler

        public static bool HasInvokers(this EventHandler eventHandler) {
            return eventHandler != null && eventHandler.GetInvocationList().Length > 0;
        }

        public static bool HasInvokers<T>(this EventHandler<T> eventHandler) {
            return eventHandler != null && eventHandler.GetInvocationList().Length > 0;
        }

        public static bool HasInvoker(this EventHandler eventHandler, object invoker) {
            return eventHandler.HasInvokers() && eventHandler.GetInvocationList().Any(x => x.Equals(invoker));
        }

        public static bool HasInvoker<T>(this EventHandler<T> eventHandler, object invoker) {
            if (eventHandler == null) {
                return false;
            }
            var invokers = eventHandler.GetInvocationList();
            return eventHandler.HasInvokers() && invokers.Any(x => x.Equals(invoker));
        }

        #endregion

        #region Strings

        #endregion

        #region Visual

        #endregion

        #region Streams
        public static string ReadToEnd(this MemoryStream BASE) {
            BASE.Position = 0;
            StreamReader R = new StreamReader(BASE);
            return R.ReadToEnd();
        }

        public static Stream ToStream(this string value) {
            return value.ToStream(Encoding.UTF8);
        }

        public static Stream ToStream(this string value, System.Text.Encoding encoding) {
            var bytes = encoding.GetBytes(value);
            return new MemoryStream(bytes);
        }

        #endregion

        #region Data
        public static bool HasValue(this double value, bool is_zero_value = true) {
            return !Double.IsNaN(value) && !Double.IsInfinity(value) && (is_zero_value ? true : value != 0);
        }

        public static int ByteCount(this object obj) {
            if (obj == null) {
                return 0;
            }
            RuntimeTypeHandle th = obj.GetType().TypeHandle;
            //int size = *(*(int**)&th + 1);
            int size = Marshal.ReadInt32(obj.GetType().TypeHandle.Value, 4);
            return size;
        }

        public static bool IsUnsetValue(this object obj) {
            return obj == null || obj.ToString().Contains("DependencyProperty.UnsetValue");
        }
        public static DateTime ToDateTime(this double unixTimeStamp) {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        #endregion

        #region Nullables

        public static bool IsTrue(this bool? boolVal) {
            return boolVal != null && boolVal.HasValue && boolVal.Value;
        }

        public static bool IsFalse(this bool? boolVal) {
            return boolVal != null && boolVal.HasValue && !boolVal.Value;
        }

        public static bool IsNull(this bool? boolVal) {
            return boolVal == null;
        }

        public static bool IsTrueOrFalse(this bool? boolVal) {
            return !boolVal.IsNull();
        }

        public static bool IsTrueOrNull(this bool? boolVal) {
            return boolVal.IsTrue() || boolVal.IsNull();
        }

        public static bool IsFalseOrNull(this bool? boolVal) {
            return boolVal.IsFalse() || boolVal.IsNull();
        }

        public static bool? DefaultToggleValue(this bool? boolVal, bool nullToggleValue = false) {
            if (boolVal.IsTrue()) {
                return false;
            }
            if (boolVal.IsFalse()) {
                return true;
            }
            return nullToggleValue;
        }

        #endregion

        #region Reflection

        public static async Task InvokeAsync(this MethodInfo @this, object obj, params object[] parameters) {
            dynamic awaitable = @this.Invoke(obj, parameters);
            await awaitable;
        }

        public static bool HasProperty(this object obj, string propertyPath) {
            return obj.GetType().GetProperties().Any(x => x.Name == propertyPath);
        }

        public static object GetPropertyValue(this object obj, string propertyPath, object[] index = null, bool safe = true) {
            if (obj == null) {
                return null;
            }
            var prop_info = obj.GetType().GetProperties().FirstOrDefault(x => x.Name == propertyPath);
            if (prop_info == null) {
                return null;
            }
            object result = prop_info.GetValue(obj);
            return result;
        }


        public static void SetPropertyValue(this object obj, string propertyPath, object newValue) {
            // obj is Type when static class
            Type objType = obj.GetType() == typeof(Type) ? (Type)obj : obj.GetType();
            PropertyInfo propertyInfo = objType.GetProperty(propertyPath);
            if (propertyInfo == null ||
                propertyInfo.SetMethod == null) {
                return;
            }

            Type t = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
            object safeValue = null;
            try {
                if (t.IsEnum) {
                    safeValue = newValue.ToEnum(t);
                } else if (t == typeof(DateTime) && newValue.ParseOrConvertToDateTime(null) is DateTime newDt) {
                    safeValue = newDt;
                } else if (t == typeof(DateTime?)) {
                    safeValue = newValue.ParseOrConvertToDateTime(null);
                } else {
                    safeValue = (newValue == null) ? null : Convert.ChangeType(newValue, t);
                }
                propertyInfo.SetValue(obj, safeValue, null);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"SetPropertyValue error. Obj: '{obj}' Prop: '{propertyPath}' Safe Val: '{safeValue}' New Val: '{newValue}'", ex);
            }
        }
        #endregion

        #region Object


        #endregion

        #region Generics
        public static bool IsDefault<T>(this T value) where T : struct {
            bool isDefault = value.Equals(default(T));

            return isDefault;
        }
        #endregion

        #region Type

        public static bool IsClassSubclassOfOrImplements(this Type t, Type ot) {
            if (t == ot) {
                return true;
            }
            if (ot.IsInterface && ot.IsAssignableFrom(t)) {
                return true;
            }
            if (t.IsSubclassOf(ot)) {
                return true;
            }
            return false;
        }

        public static string ToAssemblyReferencedString(this Type t) {
            return $"{t}, {t.Assembly}";
        }
        #endregion
    }
}
