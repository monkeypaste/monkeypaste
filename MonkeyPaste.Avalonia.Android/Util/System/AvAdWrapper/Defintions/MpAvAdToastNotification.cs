using Android.Widget;
using Avalonia.WebView.Android.Core;
using AvaloniaWebView;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdToastNotification : MpIPlatformToastNotification {
        public void ShowToast(string title, string text, object icon, string accentHexColor) {
            // TODO create custom toast like this https://stackoverflow.com/a/76724589/105028

            ToastLength tl = text.Length < 30 ? ToastLength.Short : ToastLength.Long;
            Toast.MakeText(MainActivity.Instance, text, tl).Show();
        }
    }
    public class MpAvAdWebViewHelper : MpAvIDeviceWebViewHelper {
        public void EnableFileAccess(WebView wv) {
            if(wv.PlatformWebView is not AndroidWebViewCore wvc ||
                wvc.WebView is not { } wkwv) {
                return;                
            }
            wkwv.Settings.AllowFileAccessFromFileURLs = true;
            wkwv.Settings.AllowUniversalAccessFromFileURLs = true;
            wkwv.Settings.AllowFileAccess = true;
            wkwv.Settings.AllowContentAccess = true;
        }
    }
}
