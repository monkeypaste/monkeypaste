using Avalonia.Android;
using Avalonia.Platform;
using MonkeyPaste.Avalonia;
using System;

namespace ControlCatalog.Android;

public class MpAvAdWebView : MpAvINativeControl {
    public IPlatformHandle CreateControl(IPlatformHandle parent, Func<IPlatformHandle> createDefault, object args) {
        var parentContext = (parent as AndroidViewControlHandle)?.View.Context
            ?? global::Android.App.Application.Context;

        var webView = new global::Android.Webkit.WebView(parentContext);

        string url = args.ToString() ?? "https://www.android.com/";
        webView.LoadUrl(url);
        return new AndroidViewControlHandle(webView);
    }
}
