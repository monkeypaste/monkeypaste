using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Plugin {
    public static class MpPluginExtensions {
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
    }
}
