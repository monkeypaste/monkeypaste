using MonkeyPaste.Common;
using System;
using System.Collections.Specialized;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDocusaurusHelpers {
        public static string GetCustomUrl(string url, bool hideNav, bool isDark) {
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
            if (isDark) {
                queryString.Add("docusaurus-theme", "dark");
            }
            string custom_url = url + "?" + queryString.ToString() + anchor_suffix;
            return custom_url;
        }
    }
}
