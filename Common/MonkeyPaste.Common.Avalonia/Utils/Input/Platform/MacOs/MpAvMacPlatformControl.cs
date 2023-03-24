#if MAC
using Avalonia.Platform;
using Avalonia.Threading;
using MonoMac.Foundation;
using MonoMac.WebKit;
using System;

namespace MonkeyPaste.Common.Avalonia {

    public class MpAvMacPlatformControl : MpAvIPlatformControl {
        public IPlatformHandle CreateControl(bool isSecond, IPlatformHandle parent, Func<IPlatformHandle> createDefault) {
            // Note: We are using MonoMac for example purposes
            // It shouldn't be used in production apps
            MpAvMacHelpers.EnsureInitialized();

            var webView = new WebView();
            Dispatcher.UIThread.Post(() => {
                webView.MainFrame.LoadRequest(new NSUrlRequest(new NSUrl(
                    isSecond ? "https://bing.com" : "https://google.com/")));
            });
            return new MpAvMacViewHandle(webView);
        }
    }
}
#endif