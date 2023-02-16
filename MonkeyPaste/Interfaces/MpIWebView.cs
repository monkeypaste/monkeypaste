#if DESKTOP
#endif

namespace MonkeyPaste.Avalonia {
    public interface MpIWebView : MpIHasDataContext {
        void ExecuteJavascript(string script);
    }
}
