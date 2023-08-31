using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace MonkeyPaste.Avalonia {
    public static class MpAvEnumToUiStringResourceConverter {
        #region Private Variables
        // enums: (all enum to labels, maybe missing some ToStrings() in places...._
        private static Type[] _UiEnums = new Type[] {
                // enum to label
                typeof(MpNotificationType),
                typeof(MpShortcutType),
                typeof(MpContentQueryPropertyPathType),
                typeof(MpActionType),
                typeof(MpTriggerType),
                typeof(MpPluginDependencyType),
                typeof(MpClipboardFormatType),
                typeof(MpSettingsFrameType),
                typeof(MpPluginBrowserTabType),
                typeof(MpContentQueryPropertyGroupType),
                typeof(MpNextJoinOptionType),
                typeof(MpRootOptionType),
                typeof(MpTextOptionType),
                typeof(MpNumberOptionType),
                typeof(MpColorOptionType),
                typeof(MpDimensionOptionType),
                typeof(MpImageOptionType),
                typeof(MpFileOptionType),
                typeof(MpFileContentOptionType),
                typeof(MpContentTypeOptionType),
                typeof(MpAppOptionType),
                typeof(MpWebsiteOptionType),
                typeof(MpSourcesOptionType),
                typeof(MpTransactionType),
                typeof(MpDateTimeOptionType),
                typeof(MpDateBeforeUnitType),
                typeof(MpDateAfterUnitType),
                typeof(MpTimeSpanWithinUnitType),
                typeof(MpContentSortType),
                typeof(MpRoutingType),
                typeof(MpComparisonOperatorType),
                typeof(MpUserAccountType),
            };

        private static Dictionary<Type, string> _UniqueNones = new Dictionary<Type, string>() {
            { typeof(MpRootOptionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpTextOptionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpNumberOptionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpColorOptionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpDimensionOptionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpImageOptionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpFileOptionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpFileContentOptionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpContentTypeOptionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpAppOptionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpWebsiteOptionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpSourcesOptionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpTransactionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpDateTimeOptionType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpDateBeforeUnitType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpDateAfterUnitType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpTimeSpanWithinUnitType), UiStrings.SearchCriteriaDefaultOptionLabel }
        };
        #endregion

        #region Public Methods

        #region Conversion
        public static void CheckForEnumsChanged(string resx_path) {
            var resx_elu = GetEnumResourceLookup(resx_path);
            var code_elu = GetAllEnumsLookup();

            var changed_entries = resx_elu.Difference(code_elu);
            MpDebug.Assert(!changed_entries.Any(), $"Enums ui string change detected");
        }

        public static string CreateAll(string resx_path) {
            try {
                if (resx_path.IsFile()) {
                    MpFileIo.DeleteFile(resx_path);
                }
                using MemoryStream ms = new MemoryStream();
                using ResXResourceWriter oWriter = new ResXResourceWriter(resx_path);

                // NOTES
                // 1. param enums to labels need to KEEP enum as value NOT label

                var code_elu = GetAllEnumsLookup();
                code_elu.ForEach(x => oWriter.AddResource(x.Key, x.Value));
                oWriter.Generate();
                oWriter.Close();
                return resx_path;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error creating enum uistrings at path '{resx_path}'.", ex);
            }
            return null;
        }

        #endregion

        #region Extensions

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
            }
            return enum_strs.ToArray();
        }
        public static string EnumToUiString<TValue>(this TValue value, string noneText = "")
            where TValue : Enum {
            // NOTE noneText is unused now and using secondary lookup instead but leaving it
            // cause not sure what will come up...

            string enum_key = GetEnumKey(value);

            if (typeof(EnumUiStrings).GetProperty(enum_key) is not PropertyInfo pi ||
                pi.GetValue(null) is not string enum_ui_string) {
                MpDebug.Break($"Missing enum key '{enum_key}'");
                return string.Empty;
            }
            return enum_ui_string;
        }
        #endregion

        #endregion

        #region Private Methods
        private static Dictionary<string, string> GetAllEnumsLookup() {
            // enum key format: <EnumType>_<EnumValName>
            var elu = new Dictionary<string, string>();
            foreach (var enumType in _UiEnums) {
                string type_prefix = enumType.ToString().SplitNoEmpty(".").Last();
                foreach (var (enumName, idx) in Enum.GetNames(enumType).WithIndex()) {
                    string name_key = $"{type_prefix}_{enumName}";
                    MpDebug.Assert(!elu.ContainsKey(name_key), $"Name Key '{name_key}' already exists...");
                    string name_val = enumName.ToProperCase();
                    if (idx == 0 && _UniqueNones.TryGetValue(enumType, out string unique_none)) {
                        name_val = unique_none;
                    }
                    elu.Add(name_key, name_val);
                }

            }
            return elu;
        }

        private static Dictionary<string, string> GetEnumResourceLookup(string resx_path) {
            var elu = new Dictionary<string, string>();
            if (!resx_path.IsFile()) {
                return elu;
            }
            using var ms = new FileStream(resx_path, FileMode.Open);
            using var rrr = new ResXResourceReader(ms);
            foreach (var pi in rrr.GetType().GetProperties()) {
                if (pi.GetValue(null) is string val) {
                    elu.Add(pi.Name, val);
                }
            }
            return elu;
        }

        private static string GetEnumKey(Enum enumVal) {
            return $"{enumVal.GetType().ToString().SplitNoEmpty(".").Last()}_{enumVal}";
        }
        #endregion

    }
}
