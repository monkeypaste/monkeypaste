using Android.Views;
using Android.Webkit;
using Avalonia.Android;
using Avalonia.Platform;
using Avalonia.Threading;
using Java.Interop;
using MonkeyPaste;
using MonkeyPaste.Avalonia;
using MonkeyPaste.Avalonia.Android;
using MonkeyPaste.Common;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ControlCatalog.Android;

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

    public async Task<string> SendMessageAsync(MpAvIPlatformHandleHost nwvh, string msg) {
        if (nwvh.PlatformHandle is AndroidViewControlHandle avch &&
            avch.View is WebView wv) {

            MpAvAdMessageCallback mc = new MpAvAdMessageCallback();

            wv.EvaluateJavascript(msg, mc);
            if (nwvh is MpIAsyncJsEvalTracker jset) {
                jset.PendingEvals++;
            }
            var sw = Stopwatch.StartNew();

            while (mc.Result == null) {
                await Task.Delay(100);
                if (sw.ElapsedMilliseconds > 10_000) {
                    MpDebug.Break($"Async js timeout for msg '{msg}'");
                }
            }
            if (nwvh is MpIAsyncJsEvalTracker jset2) {
                jset2.PendingEvals--;
            }
            return mc.Result;
        }
        MpDebug.Break("look at props to find web view");
        return string.Empty;
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
    internal class MpAvAdMessageCallback : Java.Lang.Object, IValueCallback {
        public string Result { get; private set; }
        public void OnReceiveValue(Java.Lang.Object value) {
            Result = value.ToString();
        }
    }


    #endregion

    #endregion
}