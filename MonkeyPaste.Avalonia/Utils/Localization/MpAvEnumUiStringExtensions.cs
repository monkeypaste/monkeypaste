using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvEnumUiStringExtensions {
        public static string[] EnumToUiStrings(this Type e, string noneText = "", bool hideFirst = false, string spaceStr = " ") {
            List<string> enum_strs = new List<string>();
            if (e == null || !e.IsEnum) {
                return enum_strs.ToArray();
            }

            int idx = 0;
            foreach (Enum val in Enum.GetValues(e)) {
                if (hideFirst && idx == 0) {
                    continue;
                }
                enum_strs.Add(val.EnumToUiString(noneText));
                idx++;
            }
            return enum_strs.ToArray();
        }
        public static string EnumToUiString<TValue>(this TValue value, string noneText = "")
            where TValue : Enum {
            // NOTE noneText is unused now and using secondary lookup instead but leaving it
            // cause not sure what will come up...

            string enum_key = GetEnumKey(value);

            if (EnumUiStrings.ResourceManager.GetString(enum_key, MpAvCurrentCultureViewModel.Instance.CurrentCulture) is not string enum_ui_string) {
                if (!enum_key.ToLowerInvariant().EndsWith("_none")) {
                    MpDebug.Break($"Missing enum key '{enum_key}'");
                }
                return noneText;
            }
            return enum_ui_string;
        }

        public static string EnumKeyToUiString(string key) {
            foreach (var pi in typeof(EnumUiStrings).GetProperties()) {
                try {
                    if (pi.Name != null &&
                        pi.Name.EndsWith(key)) {
                        return EnumUiStrings.ResourceManager.GetString(pi.Name, MpAvCurrentCultureViewModel.Instance.CurrentCulture);
                    }
                }
                catch {
                    continue;
                }
            }
            return string.Empty;
        }
        public static object UiStringToEnum(this string uiStr, Type enumType = null) {
            var keys = FindEnumKeys(uiStr);
            string key = enumType == null ?
                keys.FirstOrDefault() :
                keys.FirstOrDefault(x => x.StartsWith(enumType.Name));

            if (key.SplitNoEmpty("_") is string[] key_parts &&
                key_parts.Length == 2 &&
                MpAvEnumUiStringResourceConverter.UiEnums.FirstOrDefault(x => x.Name == key_parts[0]) is Type enum_type &&
                    key_parts[1].ToEnum(enum_type) is object enum_val) {
                return enum_val;
            }
            MpDebug.Break($"Can't find key for type ui str '{uiStr}' ");
            return default;
        }

        #region Helpers

        private static string GetEnumKey(Enum enumVal) {
            return $"{enumVal.GetType().ToString().SplitNoEmpty(".").Last()}_{enumVal}";
        }

        private static IEnumerable<string> FindEnumKeys(string uiStr) {
            foreach (var pi in typeof(EnumUiStrings).GetProperties()) {
                string key = null;
                try {
                    if (EnumUiStrings.ResourceManager.GetString(pi.Name, MpAvCurrentCultureViewModel.Instance.CurrentCulture) is string val
                        && val == uiStr) {
                        key = pi.Name;
                    }
                }
                catch {
                    continue;
                }
                if (!string.IsNullOrEmpty(key)) {
                    yield return key;
                }
            }
        }
        #endregion
    }
}
