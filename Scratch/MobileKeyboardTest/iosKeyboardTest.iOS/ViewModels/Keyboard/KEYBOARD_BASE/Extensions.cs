using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iosKeyboardTest.iOS {
    public static class KeyboardExtensions {
#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public static async void FireAndForgetSafeAsync(this Task task)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try {
                await task;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
            }
        }
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
        static Random Rand = new Random();
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> enumerable) {
            List<T> enumerable_copy = enumerable.ToList();
            List<T> rand_list = new List<T>();

            while (enumerable_copy.Count() > 0) {
                int idx = Rand.Next(enumerable_copy.Count);
                T item = enumerable_copy.ElementAt(idx);
                enumerable_copy.RemoveAt(idx);
                rand_list.Add(item);
            }

            return rand_list;
        }
        public static bool IsAllCaps(this string str) {
            if (str == null) {
                return false;
            }
            return str.ToCharArray().All(x => IsCapitalCaseChar(x));
        }
        public static bool StartsWithCapitalCaseChar(this string str) {
            if (string.IsNullOrEmpty(str)) {
                return false;
            }
            char let = str[0];
            if (let == default) {
                return false;
            }
            if (let >= 'A' && let <= 'Z') {
                return true;
            }
            return false;
        }

        public static bool StartsWithLowerCaseChar(this string str) {
            if (string.IsNullOrEmpty(str)) {
                return false;
            }
            char let = str[0];
            if (let == default) {
                return false;
            }
            if (let >= 'a' && let <= 'z') {
                return true;
            }
            return false;
        }
        
        public static bool IsCapitalCaseChar(char let) {
            if (let == default) {
                return false;
            }
            if (let >= 'A' && let <= 'Z') {
                return true;
            }
            return false;
        }

        public static bool IsLowerCaseChar(char let) {
            if (let == default) {
                return false;
            }
            if (let >= 'a' && let <= 'z') {
                return true;
            }
            return false;
        }
        public static string ToTitleCase(this string str) {
            TextInfo textInfo = new CultureInfo(CultureInfo.CurrentCulture.Name, false).TextInfo;
            return textInfo.ToTitleCase(str);
        }
        public static string ToProperCase(this string titleCaseStr, string noneText = "", string spaceStr = " ") {
            // TODO when automating UI language need to parameterize low vs up case logic
            //Converts 'ThisIsALabel" to 'This Is A Label'
            var sb = new StringBuilder();
            for (int i = 0; i < titleCaseStr.Length; i++) {
                if (i > 0 &&
                    (IsLowerCaseChar(titleCaseStr[i - 1]) && IsCapitalCaseChar(titleCaseStr[i]) ||
                    IsCapitalCaseChar(titleCaseStr[i - 1]) && IsCapitalCaseChar(titleCaseStr[i]))) {
                    sb.Append(spaceStr);
                }
                sb.Append(titleCaseStr[i]);
            }
            string result = sb.ToString();
            if (result.ToLowerInvariant() == "none") {
                return noneText;
            }
            return result;
        }
    }
}
