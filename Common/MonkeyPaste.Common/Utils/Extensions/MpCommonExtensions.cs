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

        public static HtmlDocument ToHtmlDocument(this string html) {
            try {
                var doc = new HtmlDocument();
                doc.LoadHtml(html.ToStringOrEmpty());
                return doc;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error creating html doc.", ex);
            }
            return new HtmlDocument();
        }
        public static HtmlNodeCollection SelectNodesSafe(this HtmlNode node, string xpath) {
            if (node.SelectNodes(xpath) is not { } hnc) {
                return new(node);
            }
            return hnc;
        }

        public static bool IsBlockElement(this HtmlNode node) {
            return Regex.IsMatch(node.Name, "^(address|blockquote|body|center|dir|div|dl|fieldset|form|h[1-6]|hr|isindex|menu|noframes|noscript|ol|p|pre|table|ul|dd|dt|frameset|li|tbody|td|tfoot|th|thead|tr)");
        }
        public static HtmlNode CloneEmpty(this HtmlNode node) {
            var empty_clone = node.Clone();
            empty_clone.RemoveAllChildren();
            return empty_clone;
        }
        public static HtmlNode CreateElement(this HtmlDocument doc, string name, HtmlNode firstChild) {
            HtmlNode elm = doc.CreateElement(name);
            elm.AppendChild(firstChild);
            return elm;
        }

        public static IEnumerable<HtmlNode> SplitTextRanges(
            this HtmlDocument doc,
            (int idx, int len)[] ranges,
            string split_class = "",
            string[] assert_match_texts = default) {
            // NOTE idx,len should be based on plain text, no special entities or line breaks

            var text_nodes = doc.DocumentNode.SelectNodesSafe("//text()");

            if (!text_nodes.Any()) {
                return [];
            }
            string assert_total_text = string.Empty;
            if (assert_match_texts != default) {
                assert_total_text = doc.DocumentNode.InnerText.DecodeSpecialHtmlEntities();
            }

            List<HtmlNode> splitNodes = [];
            int cur_text_node_idx = 0;
            int cur_range_idx = -1;
            int cur_pt_idx = 0;
            int match_start_idx = 0;
            int match_end_idx = 0;
            int len = 0;
            HtmlNode splitNode = null;
            StringBuilder sb = new StringBuilder();

            bool SelectNextRange() {
                // returns false if no more ranges
                if (splitNode != null) {
                    splitNode.AppendChild(doc.CreateTextNode(sb.ToString().EncodeSpecialHtmlEntities()));
                    splitNode.AddClass(split_class);
                    splitNodes.Add(splitNode);

                    if (cur_range_idx < assert_match_texts.Length) {
                        string assert_match_text = assert_match_texts[cur_range_idx];
                        MpDebug.Assert(assert_match_text != null, $"Error assert_text/range count mismatch", true);
                        MpDebug.Assert(splitNode != null, $"Error '{assert_match_text.ToStringOrEmpty()}' not found", true);
                        MpDebug.Assert(splitNode.InnerText.DecodeSpecialHtmlEntities() == assert_match_text.ToStringOrEmpty(), $"Error split text '{splitNode.InnerText.DecodeSpecialHtmlEntities()}' does not equal assert text '{assert_match_text.ToStringOrEmpty()}'", true);
                    }
                }
                cur_range_idx++;
                if (cur_range_idx >= ranges.Length) {
                    return false;
                }
                match_start_idx = ranges[cur_range_idx].idx;
                len = ranges[cur_range_idx].len;
                match_end_idx = match_start_idx + len;
                splitNode = null;
                sb.Clear();
                return true;
            }
            if (!SelectNextRange()) {
                // must be no ranges
                return [];
            }
            while (true) {
                int last_text_node_idx = cur_text_node_idx;
                if (cur_text_node_idx >= text_nodes.Count) {
                    break;
                }
                var n = text_nodes[cur_text_node_idx];
                if (n is not HtmlTextNode tn) {
                    cur_text_node_idx++;
                    continue;
                }

                string tn_text = tn.Text.DecodeSpecialHtmlEntities();
                int next_idx = cur_pt_idx + tn_text.Length;

                if (splitNode == null) {
                    // looking for start
                    if (match_start_idx >= cur_pt_idx && match_start_idx < next_idx) {
                        // match starts in this text node
                        int rel_split_start_idx = match_start_idx - cur_pt_idx;
                        int rel_split_start_len = tn_text.Length - rel_split_start_idx;
                        int start_split_len = Math.Min(len, rel_split_start_len);
                        string start_split_text = tn_text.Substring(rel_split_start_idx, start_split_len);
                        // update text for pre split node
                        tn.Text = tn_text.Substring(0, rel_split_start_idx).EncodeSpecialHtmlEntities();
                        // create split node with pre match text
                        sb.Append(start_split_text);
                        splitNode = doc.CreateElement("span");
                        tn.ParentNode.InsertAfter(splitNode, tn);

                        int post_split_idx = rel_split_start_idx + start_split_len;
                        if (post_split_idx < tn_text.Length) {
                            // range is entirely within start node and start node has more text after match
                            // create post match node
                            string post_match_text = tn_text.Substring(post_split_idx, tn_text.Length - post_split_idx);
                            tn.ParentNode.InsertAfter(doc.CreateTextNode(post_match_text.EncodeSpecialHtmlEntities()), splitNode);
                            // all done
                            if (!SelectNextRange()) {
                                break;
                            }
                        } else if (start_split_text.Length == len) {
                            // was whole node so all done
                            if (!SelectNextRange()) {
                                break;
                            }
                            cur_text_node_idx++;
                        } else {
                            // need to continue appending text nodes to split node
                            cur_text_node_idx++;
                        }
                    } else {
                        // no start in this node
                        cur_text_node_idx++;
                    }
                } else {
                    if (match_end_idx < next_idx) {
                        // match ends in this text node
                        int rel_split_end_idx = len - sb.ToString().Length;
                        sb.Append(tn_text.Substring(0, rel_split_end_idx));
                        tn.Text = tn_text.Substring(rel_split_end_idx, tn_text.Length - rel_split_end_idx);
                        // all done
                        if (!SelectNextRange()) {
                            break;
                        }
                    } else {
                        // match is across this entire text node
                        sb.Append(tn_text);
                        // clear nodes text
                        tn.Text = string.Empty;
                        if (!SelectNextRange()) {
                            break;
                        }
                        cur_text_node_idx++;
                    }
                }

                if (cur_text_node_idx != last_text_node_idx) {
                    // shifting to next text node
                    cur_pt_idx += tn_text.Length;
                }
            }

            if (assert_match_texts != default) {
                MpDebug.Assert(doc.DocumentNode.InnerText.DecodeSpecialHtmlEntities() == assert_total_text, $"Error total text mismatch. original '{assert_total_text.DecodeSpecialHtmlEntities()}' after split '{doc.DocumentNode.InnerText.DecodeSpecialHtmlEntities()}'", true);
            }
            return splitNodes;
        }

        public static HtmlNode SplitTextRange(this HtmlDocument doc, int idx, int len, HtmlNodeCollection text_nodes = default, string assert_match_text = default) {
            // NOTE idx,len should be based on plain text, no special entities or line breaks
            text_nodes = text_nodes == default ?
                doc.DocumentNode.SelectNodesSafe("//text()") :
                text_nodes;

            if (!text_nodes.Any()) {
                return default;
            }
#if DEBUG
            string assert_total_text = doc.DocumentNode.InnerText.DecodeSpecialHtmlEntities();
#endif
            int cur_idx = 0;
            int match_start_idx = idx;
            int match_end_idx = idx + len;
            HtmlNode splitNode = null;
            var sb = new StringBuilder();
            // split start
            foreach (var n in text_nodes) {
                if (n is not HtmlTextNode tn) {
                    continue;
                }
                string tn_raw_text = tn.Text;
                string tn_text = tn.Text.DecodeSpecialHtmlEntities();
                int next_idx = cur_idx + tn_text.Length;

                if (splitNode == null) {
                    // looking for start
                    if (match_start_idx >= cur_idx && match_start_idx < next_idx) {
                        // match starts in this text node
                        int rel_split_start_idx = match_start_idx - cur_idx;
                        int rel_split_start_len = tn_text.Length - rel_split_start_idx;
                        int start_split_len = Math.Min(len, rel_split_start_len);
                        string start_split_text = tn_text.Substring(rel_split_start_idx, start_split_len);
                        // update text for pre split node
                        tn.Text = tn_text.Substring(0, rel_split_start_idx).EncodeSpecialHtmlEntities();
                        // create split node with pre match text
                        sb.Append(start_split_text);
                        splitNode = doc.CreateElement("span");
                        tn.ParentNode.InsertAfter(splitNode, tn);

                        int post_split_idx = rel_split_start_idx + start_split_len;
                        if (post_split_idx < tn_text.Length) {
                            // range is entirely within start node and start node has more text after match
                            // create post match node
                            string post_match_text = tn_text.Substring(post_split_idx, tn_text.Length - post_split_idx);
                            tn.ParentNode.InsertAfter(doc.CreateTextNode(post_match_text.EncodeSpecialHtmlEntities()), splitNode);
                            // all done
                            break;
                        }
                        if (start_split_text.Length == len) {
                            // was whole node so all done
                            break;
                        }

                        // need to continue appending text nodes to split node
                    }
                } else {
                    HtmlTextNode split_text_node = splitNode.FirstChild as HtmlTextNode;
                    if (match_end_idx < next_idx) {
                        // match ends in this text node
                        int rel_split_end_idx = len - sb.ToString().Length;
                        sb.Append(tn_text.Substring(0, rel_split_end_idx));
                        tn.Text = tn_text.Substring(rel_split_end_idx, tn_text.Length - rel_split_end_idx);
                        // all done
                        break;
                    } else {
                        // match is across this entire text node
                        sb.Append(tn_text);
                        // clear nodes text
                        tn.Text = string.Empty;
                    }
                }

                cur_idx += tn_text.Length;
            }

            if (splitNode != null) {
                splitNode.AppendChild(doc.CreateTextNode(sb.ToString().EncodeSpecialHtmlEntities()));
            }
            //            if (assert_match_text != default) {
            //                MpDebug.Assert(splitNode != null, $"Error '{assert_match_text}' not found", true);
            //                MpDebug.Assert(splitNode.InnerText.DecodeSpecialHtmlEntities() == assert_match_text, $"Error split text '{splitNode.InnerText.DecodeSpecialHtmlEntities()}' does not equal assert text '{assert_match_text}'", true);
            //            }
            //#if DEBUG
            //            MpDebug.Assert(doc.DocumentNode.InnerText.DecodeSpecialHtmlEntities() == assert_total_text, $"Error total text mismatch. original '{assert_total_text.DecodeSpecialHtmlEntities()}' after split '{doc.DocumentNode.InnerText.DecodeSpecialHtmlEntities()}'", true);
            //#endif
            return splitNode;
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
