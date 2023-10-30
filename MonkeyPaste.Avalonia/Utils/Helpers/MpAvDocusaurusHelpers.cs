using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDocusaurusHelpers {
        public static string GetThemeUrlAttrb(bool isDark) {
            return $"?docusaurus-theme={(isDark ? "dark" : "light")}";
        }
        private static string GetMainOnlyJs() {
            // NOTE this just hides the header and footer
            string help_css = @"
nav.navbar.navbar--fixed-top, footer {display: none !important;}";

            string help_js = string.Format(@"
var style_elm = document.createElement('style');
document.head.appendChild(style_elm);
style_elm.innerText = '{0}';
", help_css);

            return help_js;
        }

        public static async Task LoadMainOnlyAsync(MpAvWebView wv) {
            if (wv.DataContext is not MpIAsyncObject ao) {
                return;
            }
            await Task.Delay(300);
            // should only happen when loading whats new
            while (ao.IsBusy) {
                await Task.Delay(100);
            }
            await Task.Delay(300);

            wv.ExecuteJavascript(GetMainOnlyJs());
        }
    }
}
