using Avalonia.WebView.iOS.Core;
using AvaloniaWebView;
using MonkeyPaste.Avalonia;
using WebKit;

namespace iosTest.iOS {
    public class MpAvIosWebViewHelper : MpAvIDeviceWebViewHelper {
        public void ConfigureWebview(object arg) {
            if(arg is not object[] argParts ||
                argParts[0] is not WebView iwv ||
                iwv.PlatformWebView is not IosWebViewCore iwvc ||
                iwvc.WebView is not WKWebView wv ||
                argParts[1] is not MpAvIWebViewBindingResponseHandler mpwv) {
                return;
            }
#pragma warning disable CA1416 // Validate platform compatibility
            wv.Inspectable = true;
#pragma warning restore CA1416 // Validate platform compatibility
            WKUserContentController cc = wv.Configuration.UserContentController;
            if (cc == null) {
                // BUG have to use apples cc or it won't register
                // from https://stackoverflow.com/a/34491757/105028
                cc = new WKUserContentController();
            }
            cc.AddScriptMessageHandler(new MpAvIosWebViewMessageHandler(mpwv), "webview");
            if(wv.Configuration.UserContentController == null) {
                wv.Configuration.UserContentController = cc;
            }            
        }
    }
}
