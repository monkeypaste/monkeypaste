
using System;

#if DESKTOP
using CefNet.Avalonia;
using PropertyChanged;
#endif

namespace MonkeyPaste.Avalonia {
#if DESKTOP
    [DoNotNotify]
#endif
    public abstract class MpAvWebViewBase :
#if DESKTOP
        WebView
#else
        Object
#endif
        , MpIWebView, MpIHasDataContext {

#if !DESKTOP
        public object DataContext { get; set; }
#endif

        public void ExecuteJavascript(string script) {
#if DESKTOP
            this.GetMainFrame().ExecuteJavaScript(script, this.GetMainFrame().Url, 0);
#endif
        }
    }
}
