using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources.NetStandard;

namespace MonkeyPaste.Avalonia {
    public class MpUiStringToEnumConverter : MpIUiStringToEnumConverter {
        public object UiStringToEnum(string uiStr, Type enumType = null) {
            return uiStr.UiStringToEnum(enumType);
        }
    }
    public static class MpAvEnumToUiStringResourceConverter {
        #region Private Variables
        #endregion

        #region Properties
        // enums: (all enum to labels, maybe missing some ToStrings() in places...._
        public static Type[] UiEnums => new Type[] {
                // enum to label
                typeof(MpBillingCycleType),
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
                typeof(MpTagType),
                typeof(MpReadOnlyTagType),
                typeof(WatcherChangeTypes), //external
            };

        private static Dictionary<Type, string> _UniqueNones => new Dictionary<Type, string>() {
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
            { typeof(MpTimeSpanWithinUnitType), UiStrings.SearchCriteriaDefaultOptionLabel },
            { typeof(MpBillingCycleType), UiStrings.AccountBillingCycleNoneLabel },
        };

        static string EnumUiResxFileName =>
            "EnumUiStrings.resx";

        static string ActualEnumUiResxResourcePath =>
            Path.Combine(
                MpCommonHelpers.GetSolutionDir(),
                typeof(MpAvEnumToUiStringResourceConverter).Assembly.GetName().Name,
                "Resources",
                "Localization",
                "Enums",
                EnumUiResxFileName);

        #endregion

        #region Public Methods

        public static bool CheckEnumUiStrings() {
            // returns true if needs restart
#if !DEBUG || !WINDOWS
            return false;
#endif
            if (!string.IsNullOrWhiteSpace(EnumUiStrings.Culture.Name)) {
                // non-invariant don't update
                MpConsole.WriteLine($"Enum UI Strings ignoring culture '{EnumUiStrings.Culture}' its non-invariant '{EnumUiStrings.Culture.Name}'");
                return false;
            }
            var diffs = GetCodeAndResxEnumDiffs(ActualEnumUiResxResourcePath);
            if (!diffs.Any()) {
                MpConsole.WriteLine($"Enum Ui strings match. All appears well");
                return false;
            }

            // either new/missing entries or values changed
            MpConsole.WriteLine("Enums Changed! Here are diffs: ", true);
            diffs.ForEach(x => MpConsole.WriteLine($"'{x.Key}'=>'{x.Value}'", stampless: true));
            string target_path = ActualEnumUiResxResourcePath;
            target_path = CreateEnumResx(target_path);

            // NOTE! EnumUiStrings should NOT have designer.cs after shutdown.
            // Add empty row and save to generate, then clean and rebuild before re-running
            MpDebug.Break($"CAUTION! Enum uistrings changed. If not planned terminate but note diffs before and fix because its about to be overwriten...", true);

            //Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.ResourceUpdate, $"Enum UI strings updated at path '{target_path}'");
            return true;
        }

        #region Extensions


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
            foreach (var enumType in UiEnums) {
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
            // NOTE using reflection not .ResourceManager returns invariant value only but this 
            // should only run with default culture
            foreach (var pi in typeof(EnumUiStrings).GetProperties()) {
                if (pi.GetValue(null) is string val) {
                    elu.Add(pi.Name, val);
                }
            }
            return elu;
        }


        private static string CreateEnumResx(string resx_path) {
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
        #endregion

    }
}
