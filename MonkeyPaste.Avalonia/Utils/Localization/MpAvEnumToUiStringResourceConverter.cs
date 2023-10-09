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
                typeof(MpShortcutRoutingProfileType),
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
                typeof(MpContentOptionType),
                typeof(MpMainWindowShowBehaviorType),
                typeof(MpShortcutAssignmentClearButtonType),
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

        static string EnumUiResxFileName =>
            "EnumUiStrings.resx";
        static string TestEnumUiResxResourcePath =>
            Path.Combine(Mp.Services.PlatformInfo.StorageDir, EnumUiResxFileName);

        static string ActualEnumUiResxResourcePath =>
            Path.Combine(
                MpCommonHelpers.GetSolutionDir(),
                typeof(MpAvEnumToUiStringResourceConverter).Assembly.GetName().Name,
                "Resources",
                "UiStrings",
                EnumUiResxFileName);
        #endregion

        #region Public Methods

        public static void Init() {
#if DEBUG
            var diffs = GetCodeAndResxEnumDiffs(ActualEnumUiResxResourcePath);
            if (diffs.Any()) {
                // either new/missing entries or values changed
                MpConsole.WriteLine("Enums Changed! Here are diffs: ", true);
                diffs.ForEach(x => MpConsole.WriteLine($"'{x.Key}'=>'{x.Value}'", stampless: true));
                MpDebug.Break($"CAUTION! Enum uistrings changed. If not planned terminate but note diffs before and fix because its about to be overwriten...");
                string target_path = ActualEnumUiResxResourcePath;
                target_path = CreateEnumResx(target_path);
                // NOTE! EnumUiStrings should NOT have designer.cs after shutdown.
                // Add empty row and save to generate, then clean and rebuild before re-running
                Mp.Services.ShutdownHelper.ShutdownApp($"Enum UI strings updated at path '{target_path}'");
            } else {
                MpConsole.WriteLine($"Enum Ui strings match. All appears well");
            }
#endif
        }

        public static string CreateEnumResx(string resx_path) {
            try {
                if (resx_path.IsFile()) {
                    MpFileIo.DeleteFile(resx_path);
                    string resx_cs_path = resx_path.Replace(".resx", ".Designer.cs");
                    MpDebug.Assert(resx_cs_path.IsFile(), $"EnumUi str error, cannot find designer file '{resx_cs_path}'");
                    // need to remove code file also
                    MpFileIo.DeleteFile(resx_cs_path);
                }
                using MemoryStream ms = new MemoryStream();
                using ResXResourceWriter oWriter = new ResXResourceWriter(resx_path);

                // NOTES
                // 1. param enums to labels need to KEEP enum as value NOT label

                var code_elu = GetCodeEnumsAsLookup();
                code_elu.ForEach(x => oWriter.AddResource(x.Key, x.Value));
                oWriter.Generate();
                oWriter.Close();
                MpConsole.WriteLine($"EnumUiStrings created successfully at path '{resx_path}'");
                return resx_path;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error creating enum uistrings at path '{resx_path}'.", ex);
            }
            return null;
        }

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
                idx++;
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
        private static IEnumerable<KeyValuePair<string, string>> GetCodeAndResxEnumDiffs(string resx_path) {
            var resx_elu = GetResxEnumsAsLookup(resx_path);
            var code_elu = GetCodeEnumsAsLookup();

            return resx_elu.Difference(code_elu);
        }
        private static Dictionary<string, string> GetCodeEnumsAsLookup() {
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

        private static Dictionary<string, string> GetResxEnumsAsLookup(string resx_path) {
            var elu = new Dictionary<string, string>();
            if (!resx_path.IsFile()) {
                return elu;
            }
            using var ms = new FileStream(resx_path, FileMode.Open);
            using var rrr = new ResXResourceReader(ms);
            foreach (var pi in typeof(EnumUiStrings).GetProperties()) {
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
