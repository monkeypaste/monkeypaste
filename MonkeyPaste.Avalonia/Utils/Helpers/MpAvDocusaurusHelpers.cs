using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDocusaurusHelpers {
        public static string GetCustomUrl(string url, bool hideNav, bool hideSidebars, bool isDark) {
            string anchor_suffix = string.Empty;
            if (url.SplitNoEmpty("/#") is { } urlParts &&
                urlParts.Length > 1) {
                // anchor needs to be moved to end of url
                MpDebug.Assert(urlParts.Length == 2, $"Error, presuming only 1 anchor in url");
                url = urlParts[0];
                anchor_suffix = "#" + urlParts[1];
            }
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            if (hideNav) {
                queryString.Add(null, "docusaurus-data-help-view");
            }
            if (hideSidebars) {
                queryString.Add(null, "docusaurus-data-app-update-view");
            }
            if (isDark) {
                queryString.Add("docusaurus-theme", "dark");
            } else {
                queryString.Add("docusaurus-theme", "light");
            }
            string custom_url = url + "?" + queryString.ToString() + anchor_suffix;
            return custom_url;
        }

        public static string GetShortcutsMarkdown() {
            string GetShortcutTable(IEnumerable<MpAvShortcutViewModel> svml, string title) {
                var sb = new StringBuilder();
                sb.AppendLine(title);
                sb.AppendLine("|Name|Shortcut|");
                sb.AppendLine("|---|---|");
                svml
                    .ForEach(x => sb.AppendLine($"|{x.ShortcutDisplayName}|{string.Join(" ", x.KeyString.Split("+").Select(y => $"<kbd>{y}</kbd>"))}|"));
                return sb.ToString();
            }
            var sb = new StringBuilder();

            sb.Append(
                GetShortcutTable(
                    MpAvShortcutCollectionViewModel.Instance.Items
                    .Where(x => !x.IsGlobal && !x.IsCustom)
                    .OrderBy(x => x.ShortcutDisplayName),
                    "## Default Application Shortcuts"));

            sb.Append(
                GetShortcutTable(
                    MpAvShortcutCollectionViewModel.Instance.Items
                    .Where(x => x.IsGlobal && !x.IsCustom && !x.IsEditorShortcut)
                     .OrderBy(x => x.ShortcutDisplayName),
                    "## Default Global Shortcuts"));
            return sb.ToString();

        }
    }
}
