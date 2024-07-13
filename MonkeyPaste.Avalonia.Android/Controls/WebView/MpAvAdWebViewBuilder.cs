using Android.Views;
using Android.Webkit;
using Avalonia.Android;
using Avalonia.Platform;
using MonkeyPaste.Common;
using System;

namespace MonkeyPaste.Avalonia.Android;

public class MpAvAdAndroidViewControlHandle :
    AndroidViewControlHandle, MpIOffscreenRenderSourceHost {
    public MpIOffscreenRenderSource RenderSource { get; private set; }
    public MpAvAdAndroidViewControlHandle(View view) : base(view) {
        if (view is MpAvAdWebView wv) {
            RenderSource = wv;
        }
    }
}

public class MpAvAdWebViewBuilder :
    MpAvINativeControlBuilder,
    MpAvIWebViewInterop {
    #region Interfaces

    #region MpAvINativeControlBuilder Implementation
    public IPlatformHandle Build(IPlatformHandle parent, Func<IPlatformHandle> createDefault, MpIWebViewHost host) {
        var parentContext = (parent as AndroidViewControlHandle)?.View.Context
            ?? global::Android.App.Application.Context;

        var webView = new MpAvAdWebView(parentContext, host);

        return new MpAvAdAndroidViewControlHandle(webView);
    }
    #endregion

    #region MpAvIWebViewInterop Implementation

    public void SendMessage(MpAvIPlatformHandleHost nwvh, string msg) {
        if (nwvh.PlatformHandle is AndroidViewControlHandle avch &&
            avch.View is WebView wv) {
            wv.EvaluateJavascript(msg, null);
            return;
        }
        MpDebug.Break("look at props to find web view");
    }

    public static void ReceiveMessage(string bindingName, string msg) {
        MpConsole.WriteLine($"Received '{bindingName}' w/ data: '{msg}'");
    }

    void MpAvIWebViewInterop.ReceiveMessage(string bindingName, string msg) =>
        MpAvAdWebViewBuilder.ReceiveMessage(bindingName, msg);

    public void Bind(MpIWebViewBindable handler) {
        if (handler is MpAvIPlatformHandleHost phh &&
            phh.PlatformHandle is AndroidViewControlHandle avch &&
            avch.View is MpAvAdWebView wv &&
            wv is MpIWebViewNavigator wvn &&
            wv is MpIOffscreenRenderSource osrs) {

            EventHandler<string> navReg = (s, e) => {
                wvn.Navigate(e);
            };
            EventHandler<string> navResp = (s, e) => {
                handler.OnNavigated(e);
            };

            handler.OnNavigateRequest += navReg;

            wv.Navigated += navResp;

            // TODO add detach when unload here?
        }
    }

    #endregion

    #endregion
}