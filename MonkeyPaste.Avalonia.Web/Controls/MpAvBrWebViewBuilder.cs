using Avalonia.Browser;
using Avalonia.Platform;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia.Web {
    public class MpAvBrWebViewBuilder :
        MpAvINativeControlBuilder,
        MpAvIWebViewInterop {

        #region Interfaces

        #region MpAvINativeControlBuilder Implementation
        public IPlatformHandle Build(IPlatformHandle parent, Func<IPlatformHandle> createDefault, MpIWebViewHost host) {
            var iframe = EmbedInterop.CreateElement("iframe");
            iframe.SetProperty("id", host.HostGuid);
            iframe.SetProperty("class", "editor-iframe");
            iframe.SetProperty("src", "Editor/index.html?auto_test");
            iframe.SetProperty("crossorigin", true);
            iframe.SetProperty("credentialless", true);

            return new JSObjectControlHandle(iframe);
        }
        #endregion

        #region MpAvIWebViewInterop Implementation

        public void SendMessage(MpAvIPlatformHandleHost nwvh, string msg) {
            if (nwvh is MpIWebViewHost wvh) {
                EmbedInterop.SendMessage(wvh.HostGuid, msg);
                return;
            }
            //MpDebug.Break("look at props to find web view");
            // EmbedInterop.
        }

        public async Task<string> SendMessageAsync(MpAvIPlatformHandleHost nwvh, string msg) {
            //if (nwvh.PlatformHandle is AndroidViewControlHandle avch &&
            //    avch.View is WebView wv) {

            //    MpAvAdMessageCallback mc = new MpAvAdMessageCallback();

            //    wv.EvaluateJavascript(msg, mc);
            //    if (nwvh is MpIAsyncJsEvalTracker jset) {
            //        jset.PendingEvals++;
            //    }
            //    var sw = Stopwatch.StartNew();

            //    while (mc.Result == null) {
            //        await Task.Delay(100);
            //        if (sw.ElapsedMilliseconds > 10_000) {
            //            MpDebug.Break($"Async js timeout for msg '{msg}'");
            //        }
            //    }
            //    if (nwvh is MpIAsyncJsEvalTracker jset2) {
            //        jset2.PendingEvals--;
            //    }
            //    return mc.Result;
            //}
            //MpDebug.Break("look at props to find web view");
            await Task.Delay(1);
            return string.Empty;
        }


        public void ReceiveMessage(string bindingName, string msg) {

            MpConsole.WriteLine($"Received '{bindingName}' w/ data: '{msg}'");
        }

        public void Bind(MpIWebViewBindable handler) {
            //if (handler is MpAvIPlatformHandleHost phh &&
            //    phh.PlatformHandle is AndroidViewControlHandle avch &&
            //    avch.View is MpAvAdWebView wv &&
            //    wv is MpIWebViewNavigator wvn &&
            //    wv is MpIOffscreenRenderSource osrs) {

            //    EventHandler<string> navReg = (s, e) => {
            //        wvn.Navigate(e);
            //    };
            //    EventHandler<string> navResp = (s, e) => {
            //        handler.OnNavigated(e);
            //    };

            //    handler.OnNavigateRequest += navReg;

            //    wv.Navigated += navResp;

            //    // TODO add detach when unload here?
            //}
        }

        #endregion

        #endregion
    }

    internal static partial class EmbedInterop {
        [JSImport("globalThis.document.createElement")]
        public static partial JSObject CreateElement(string tagName);


        [JSImport("globalThis.sendMessage")]
        public static partial JSObject SendMessage(string hostGuid, string msg);
    }
}
