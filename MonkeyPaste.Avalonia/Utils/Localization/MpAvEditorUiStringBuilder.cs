using MonkeyPaste.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;

namespace MonkeyPaste.Avalonia {
    public static class MpAvEditorUiStringBuilder {
        #region Private Variables
        const string EDITOR_STR_INSERT_MARKER = "<INSERT KVP HERE>";

        const string EDITOR_KEY_PREFIX = "Editor";
        const string COMMON_UI_STR_FILE_NAME = "UiStrings.resx";

        const string EDITOR_UI_STR_FILE_NAME = "UiStrings.js";

        static string EditorUiStringJsContentTemplate => string.Format(
@"var UiStrings = {{
{0}
}};", EDITOR_STR_INSERT_MARKER);

        static string CommonUiStrPath =>
            Path.Combine(
                MpCommonHelpers.GetSolutionDir(),
                typeof(MpAvEditorUiStringBuilder).Assembly.GetName().Name,
                "Resources",
                "UiStrings",
                COMMON_UI_STR_FILE_NAME);

        static string EditorUiStrPath =>
            Path.Combine(
                MpCommonHelpers.GetSolutionDir(),
                typeof(MpAvEditorUiStringBuilder).Assembly.GetName().Name,
                "Resources",
                "Editor",
                "src",
                "components",
                "localizer",
                EDITOR_UI_STR_FILE_NAME);

        #endregion

        #region Public Methods

        public static void Init() {
#if DEBUG
            CreateJsUiStrings();
#endif
        }
        #endregion

        #region Private Methods

        private static void CreateJsUiStrings() {
            using ResXResourceReader resx_reader = new ResXResourceReader(CommonUiStrPath);
            // find all Editor* keys in uistring.resx 
            var editor_res_lookup = new Dictionary<string, string>();
            foreach (DictionaryEntry de in resx_reader) {
                if (!IsEditorResource(de)) {
                    continue;
                }
                editor_res_lookup.Add(de.Key.ToString(), de.Value.ToString());
            }
            resx_reader.Close();

            string inner_content = string.Join(string.Empty, editor_res_lookup.Select(x => GetEntryJs(x)));
            string total_content = EditorUiStringJsContentTemplate.Replace(EDITOR_STR_INSERT_MARKER, inner_content);

            string existing_content = EditorUiStrPath.IsFile() ?
                MpFileIo.ReadTextFromFile(EditorUiStrPath) :
                string.Empty;

            if (total_content == existing_content) {
                MpConsole.WriteLine("Js Ui strings match. All appears well");
                return;
            }
            MpDebug.Break($"CAUTION! Js uistrings changed. App will shutdown and changes will be reflected on restart...");

            string result = MpFileIo.WriteTextToFile(EditorUiStrPath, total_content, false);
            bool success = result == EditorUiStrPath;
            MpConsole.WriteLine($"Localizer: {EditorUiStrPath} create {(success ? "SUCCESS" : "FAIL")}");
            // NOTE! Clean and rebuild before re-running
            Mp.Services.ShutdownHelper.ShutdownApp($"Js UI strings updated at path '{EditorUiStrPath}'");
        }
        private static bool IsEditorResource(DictionaryEntry de) {
            return de.Key.ToStringOrEmpty().StartsWith(EDITOR_KEY_PREFIX);
        }
        private static string GetEntryJs(KeyValuePair<string, string> kvp) {
            return $"\t{kvp.Key}: `{kvp.Value}`,{Environment.NewLine}";
        }
        #endregion
    }
}
