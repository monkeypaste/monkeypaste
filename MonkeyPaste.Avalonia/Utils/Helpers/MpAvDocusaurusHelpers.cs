using System.Collections.Specialized;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDocusaurusHelpers {
        public static string GetCustomUrl(string url, bool hideNav, bool isDark) {
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            if (hideNav) {
                queryString.Add(null, "docusaurus-data-help-view");
            }
            if (isDark) {
                queryString.Add("docusaurus-theme", "dark");
            }
            return url + "?" + queryString.ToString();
        }
    }
}
