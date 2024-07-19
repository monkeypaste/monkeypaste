using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
    }
}
