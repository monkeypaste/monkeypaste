using Avalonia.WebView.iOS.Core;
using AvaloniaWebView;
using Foundation;
using MonkeyPaste;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Common.Plugin;
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
            wv.Inspectable = true;
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
    public class MpAvIosWebViewMessageHandler : WKScriptMessageHandler {
        MpAvIWebViewBindingResponseHandler mpwv;
        public MpAvIosWebViewMessageHandler(MpAvIWebViewBindingResponseHandler brh) {
            mpwv = brh;
        }
        public override void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message) {
            if(message.Body is not NSString nsstr ||
                nsstr.ToString() is not { } msg ||
                msg.DeserializeObject<MpQuillPostMessageResponse>() is not { } resp) {
                return;
            }

            mpwv.HandleBindingNotification(resp.msgType, resp.msgData, resp.handle);
        }
    }
}
