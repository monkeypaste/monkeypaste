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
        public static Encoding DetectTextEncoding(this byte[] b, out String text, int taster = 1000) {
            // from https://stackoverflow.com/a/12853721/105028

            // byte[] b = File.ReadAllBytes(filename);

            //////////////// First check the low hanging fruit by checking if a
            //////////////// BOM/signature exists (sourced from http://www.unicode.org/faq/utf_bom.html#bom4)
            if (b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0xFE && b[3] == 0xFF) { text = Encoding.GetEncoding("utf-32BE").GetString(b, 4, b.Length - 4); return Encoding.GetEncoding("utf-32BE"); }  // UTF-32, big-endian 
            else if (b.Length >= 4 && b[0] == 0xFF && b[1] == 0xFE && b[2] == 0x00 && b[3] == 0x00) { text = Encoding.UTF32.GetString(b, 4, b.Length - 4); return Encoding.UTF32; }    // UTF-32, little-endian
            else if (b.Length >= 2 && b[0] == 0xFE && b[1] == 0xFF) { text = Encoding.BigEndianUnicode.GetString(b, 2, b.Length - 2); return Encoding.BigEndianUnicode; }     // UTF-16, big-endian
            else if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xFE) { text = Encoding.Unicode.GetString(b, 2, b.Length - 2); return Encoding.Unicode; }              // UTF-16, little-endian
            else if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) { text = Encoding.UTF8.GetString(b, 3, b.Length - 3); return Encoding.UTF8; } // UTF-8
            else if (b.Length >= 3 && b[0] == 0x2b && b[1] == 0x2f && b[2] == 0x76) { text = Encoding.UTF7.GetString(b, 3, b.Length - 3); return Encoding.UTF7; } // UTF-7


            //////////// If the code reaches here, no BOM/signature was found, so now
            //////////// we need to 'taste' the file to see if can manually discover
            //////////// the encoding. A high taster paramValue is desired for UTF-8
            if (taster == 0 || taster > b.Length) taster = b.Length;    // Taster size can't be bigger than the filesize obviously.


            // Some text files are encoded in UTF8, but have no BOM/signature. Hence
            // the below manually checks for a UTF8 pattern. This code is based off
            // the top answer at: https://stackoverflow.com/questions/6555015/check-for-invalid-utf8
            // For our purposes, an unnecessarily strict (and terser/slower)
            // implementation is shown at: https://stackoverflow.com/questions/1031645/how-to-detect-utf-8-in-plain-c
            // For the below, false positives should be exceedingly rare (and would
            // be either slightly malformed UTF-8 (which would suit our purposes
            // anyway) or 8-bit extended ASCII/UTF-16/32 at a vanishingly long shot).
            int i = 0;
            bool utf8 = false;
            while (i < taster - 4) {
                if (b[i] <= 0x7F) { i += 1; continue; }     // If all characters are below 0x80, then it is valid UTF8, but UTF8 is not 'required' (and therefore the text is more desirable to be treated as the default codepage of the computer). Hence, there's no "utf8 = true;" code unlike the next three checks.
                if (b[i] >= 0xC2 && b[i] < 0xE0 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0) { i += 2; utf8 = true; continue; }
                if (b[i] >= 0xE0 && b[i] < 0xF0 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0) { i += 3; utf8 = true; continue; }
                if (b[i] >= 0xF0 && b[i] < 0xF5 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0 && b[i + 3] >= 0x80 && b[i + 3] < 0xC0) { i += 4; utf8 = true; continue; }
                utf8 = false; break;
            }
            if (utf8 == true) {
                text = Encoding.UTF8.GetString(b);
                return Encoding.UTF8;
            }


            // The next check is a heuristic attempt to detect UTF-16 without a BOM.
            // We simply look for zeroes in odd or even byte places, and if a certain
            // threshold is reached, the code is 'probably' UF-16.          
            double threshold = 0.1; // proportion of chars step 2 which must be zeroed to be diagnosed as utf-16. 0.1 = 10%
            int count = 0;
            for (int n = 0; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double)count) / taster > threshold) { text = Encoding.BigEndianUnicode.GetString(b); return Encoding.BigEndianUnicode; }
            count = 0;
            for (int n = 1; n < taster; n += 2) if (b[n] == 0) count++;
            if (((double)count) / taster > threshold) { text = Encoding.Unicode.GetString(b); return Encoding.Unicode; } // (little-endian)


            // Finally, a long shot - let's see if we can find "charset=xyz" or
            // "encoding=xyz" to identify the encoding:
            for (int n = 0; n < taster - 9; n++) {
                if (
                    ((b[n + 0] == 'c' || b[n + 0] == 'C') && (b[n + 1] == 'h' || b[n + 1] == 'H') && (b[n + 2] == 'a' || b[n + 2] == 'A') && (b[n + 3] == 'r' || b[n + 3] == 'R') && (b[n + 4] == 's' || b[n + 4] == 'S') && (b[n + 5] == 'e' || b[n + 5] == 'E') && (b[n + 6] == 't' || b[n + 6] == 'T') && (b[n + 7] == '=')) ||
                    ((b[n + 0] == 'e' || b[n + 0] == 'E') && (b[n + 1] == 'n' || b[n + 1] == 'N') && (b[n + 2] == 'c' || b[n + 2] == 'C') && (b[n + 3] == 'o' || b[n + 3] == 'O') && (b[n + 4] == 'd' || b[n + 4] == 'D') && (b[n + 5] == 'i' || b[n + 5] == 'I') && (b[n + 6] == 'n' || b[n + 6] == 'N') && (b[n + 7] == 'g' || b[n + 7] == 'G') && (b[n + 8] == '='))
                    ) {
                    if (b[n + 0] == 'c' || b[n + 0] == 'C') n += 8; else n += 9;
                    if (b[n] == '"' || b[n] == '\'') n++;
                    int oldn = n;
                    while (n < taster && (b[n] == '_' || b[n] == '-' || (b[n] >= '0' && b[n] <= '9') || (b[n] >= 'a' && b[n] <= 'z') || (b[n] >= 'A' && b[n] <= 'Z'))) { n++; }
                    byte[] nb = new byte[n - oldn];
                    Array.Copy(b, oldn, nb, 0, n - oldn);
                    try {
                        string internalEnc = Encoding.ASCII.GetString(nb);
                        if (internalEnc == "UTF8" || string.IsNullOrEmpty(internalEnc)) {
                            // BUG
                            // System.ArgumentException: ''UTF8' is not a supported encoding name.
                            // For information on defining a custom encoding,
                            // see the documentation for the Encoding.RegisterProvider method. (Parameter 'name')'
                            // workaround from https://stackoverflow.com/a/58174713/105028
                            internalEnc = "UTF-8";
                        }
                        text = Encoding.GetEncoding(internalEnc).GetString(b);
                        return Encoding.GetEncoding(internalEnc);
                    }
                    catch { break; }    // If C# doesn't recognize the name of the encoding, break.
                }
            }


            // If all else fails, the encoding is probably (though certainly not
            // definitely) the user's local codepage! One might present to the user a
            // list of alternative encodings as shown here: https://stackoverflow.com/questions/8509339/what-is-the-most-common-encoding-of-each-language
            // A full list can be found using Encoding.GetEncodings();
            text = Encoding.Default.GetString(b);
            return Encoding.Default;
        }
        public static bool HasValue(this double value) {
            return !Double.IsNaN(value) && !Double.IsInfinity(value);
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
            return prop_info.GetValue(obj);
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
            object safeValue;
            try {
                //if (t.IsEnum && newValue is string newEnumKey) {
                //    // from https://stackoverflow.com/a/15855966/105028
                //    //newValue = MpCommonTools.Services.UiStrEnumConverter.UiStringToEnum(newEnumKey, t);
                //    try {
                //        object enumVal = Enum.Parse(t, newEnumKey, true);
                //        newValue = enumVal;
                //    }
                //    catch (Exception ex) {
                //        MpConsole.WriteTraceLine($"Error converting string '{newEnumKey}' to enum type '{t}'");
                //    }
                //}
                safeValue = (newValue == null) ? null : Convert.ChangeType(newValue, t);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"SetPropertyValue conversion error. ", ex);
                safeValue = newValue;
            }
            try {
                propertyInfo.SetValue(obj, safeValue, null);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"SetPropertyValue set paramValue error. ", ex);
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
