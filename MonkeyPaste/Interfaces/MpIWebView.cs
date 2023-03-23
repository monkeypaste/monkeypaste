#if DESKTOP
#endif

namespace MonkeyPaste {
    public interface MpIWebView : MpIHasDataContext {
        void ExecuteJavascript(string script);
    }
}
