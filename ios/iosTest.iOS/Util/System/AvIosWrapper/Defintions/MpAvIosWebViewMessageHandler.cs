using Foundation;
using MonkeyPaste;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Common.Plugin;
using WebKit;

namespace iosTest.iOS {
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
