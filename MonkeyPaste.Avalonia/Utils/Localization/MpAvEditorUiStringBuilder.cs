using MonkeyPaste.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources.NetStandard;

namespace MonkeyPaste.Avalonia {
    public static class MpAvEditorUiStringBuilder {
        #region Private Variables
        const string INDEX_LOCALIZER_CULTURE_MARKER = "<!-- INSERT CULTURE SCRIPT REF AFTER ME -->";
        const string INDEX_LOCALIZER_REPLACE_LINE_PREFIX_CHECK = "<script defer src=\"src/components/localizer/";

        const string SCRIPT_LINE_SWAP_MARKER = "<SWAP HERE>";
        const string SCRIPT_LINE_TEMPLATE = "<script defer src=\"src/components/localizer/<SWAP HERE>\"></script>";

        const string EDITOR_STR_INSERT_MARKER = "<INSERT KVP HERE>";

        const string EDITOR_KEY_PREFIX = "Editor";
        const string COMMON_UI_STR_FILE_NAME = "UiStrings";
        const string COMMON_UI_STR_FILE_EXT = "resx";

        const string EDITOR_UI_STR_FILE_NAME = "UiStrings";
        const string EDITOR_UI_STR_FILE_EXT = "js";

        static string EditorUiStringJsContentTemplate => string.Format(
@"var UiStrings = {{
{0}
}};", EDITOR_STR_INSERT_MARKER);

        static string CommonUiStrPath {
            get {
                string cul_suff = MpAvCultureManager.IsDefaultCulture(UiStrings.Culture) ?
                    string.Empty : "." + UiStrings.Culture.Name;

                string com_ui_str_path = Path.Combine(
                    MpCommonHelpers.GetSolutionDir(),
                    typeof(MpAvEditorUiStringBuilder).Assembly.GetName().Name,
                    "Resources",
                    "UiStrings",
                    COMMON_UI_STR_FILE_NAME + cul_suff + "." + COMMON_UI_STR_FILE_EXT);
                return com_ui_str_path;
            }
        }

        static string EditorUiStrPath {
            get {
                // append culture suffix for non-defaults
                string cul_suff = MpAvCultureManager.IsDefaultCulture(UiStrings.Culture) ?
                    string.Empty : "." + UiStrings.Culture.Name;

                string ed_ui_str_path = Path.Combine(
                    MpCommonHelpers.GetSolutionDir(),
                    typeof(MpAvEditorUiStringBuilder).Assembly.GetName().Name,
                    "Resources",
                    "Editor",
                    "src",
                    "components",
                    "localizer",
                    EDITOR_UI_STR_FILE_NAME + cul_suff + "." + EDITOR_UI_STR_FILE_EXT);

                return ed_ui_str_path;
            }
        }

        static string EditorIndexHtmlPath {
            get {
                return Path.Combine(
                    MpCommonHelpers.GetSolutionDir(),
                    typeof(MpAvEditorUiStringBuilder).Assembly.GetName().Name,
                    "Resources",
                    "Editor",
                    "index.html");
            }
        }

        #endregion

        #region Public Methods

        public static void Init() {
#if DEBUG && WINDOWS
            CreateJsUiStrings();
#endif
        }
        #endregion

        #region Private Methods

        private static bool SetJsUiStringScriptTag() {
            // read index.html
            string index_html_text = MpFileIo.ReadTextFromFile(EditorIndexHtmlPath);

            // get cur uistring file name
            string ed_ui_str_file_name = Path.GetFileName(EditorUiStrPath);
            // create script line
            string script_line = SCRIPT_LINE_TEMPLATE.Replace(SCRIPT_LINE_SWAP_MARKER, ed_ui_str_file_name);
            if (index_html_text.Contains(script_line)) {
                // already set
                return false;
            }
            // split index.html by marker
            var index_html_parts = index_html_text.SplitNoEmpty(INDEX_LOCALIZER_CULTURE_MARKER);
            MpDebug.Assert(index_html_parts.Length == 2, $"Editor uistring error. Index.html missing marker '{INDEX_LOCALIZER_CULTURE_MARKER}' at path '{EditorIndexHtmlPath}'");

            // create str of everything before script tag and add splitted including marker text
            string pre = index_html_parts[0] + Environment.NewLine + INDEX_LOCALIZER_CULTURE_MARKER;

            var post_parts = index_html_parts[1].SplitNoEmpty(Environment.NewLine).ToList();
            MpDebug.Assert(post_parts[0].Trim().StartsWith(INDEX_LOCALIZER_REPLACE_LINE_PREFIX_CHECK), $"Editor uistring error. Insert line supposed to start with '{INDEX_LOCALIZER_REPLACE_LINE_PREFIX_CHECK}' but line is '{post_parts[0]}'");

            // create str of everything after current script tag
            string updated_post = string.Join(Environment.NewLine, post_parts.Skip(1));

            //merge parts
            string updated_index_html_text = string.Format(@"{0}{1}{2}{1}{3}", pre, Environment.NewLine, script_line, updated_post);
            // write it!
            MpFileIo.WriteTextToFile(EditorIndexHtmlPath, updated_index_html_text, false);

            MpConsole.WriteLine($"Localizer script tag '{script_line}' swapped into Index.html at '{EditorIndexHtmlPath}' success");
            return true;
        }

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

            // create js key-values str for Editor* items
            string inner_content = string.Join(string.Empty, editor_res_lookup.Select(x => GetEntryJs(x)));

            // swap placeholder w/ key-values
            string runtime_content = EditorUiStringJsContentTemplate.Replace(EDITOR_STR_INSERT_MARKER, inner_content);

            string existing_content = EditorUiStrPath.IsFile() ?
                MpFileIo.ReadTextFromFile(EditorUiStrPath) :
                string.Empty;

            if (runtime_content == existing_content) {
                MpConsole.WriteLine("Js Ui strings match. ");
                bool was_ref_updated = SetJsUiStringScriptTag();
                if (!was_ref_updated) {
                    return;
                }
                MpDebug.Break($"CAUTION! Js uistrings ref changed. App will shutdown and changes will be reflected on restart...");

                Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.EditorResourceUpdate, $"Js UI strings ref updated");
                return;
            }
            MpDebug.Break($"CAUTION! Js uistrings changed. App will shutdown and changes will be reflected on restart...");
            // create/update uistrings.js file
            string result = MpFileIo.WriteTextToFile(EditorUiStrPath, runtime_content, false);
            bool success = result == EditorUiStrPath;
            MpConsole.WriteLine($"Localizer: {EditorUiStrPath} create {(success ? "SUCCESS" : "FAIL")}");
            if (success) {
                SetJsUiStringScriptTag();
            }
            // NOTE! Clean and rebuild before re-running
            Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.EditorResourceUpdate, $"Js UI strings updated at path '{EditorUiStrPath}'");
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
