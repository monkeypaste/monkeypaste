using Android.Content;
using Android.Webkit;
using Android.Widget;
using Avalonia.Threading;
using Java.Interop;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia.Android {
    class MpAvAdJsInterface : Java.Lang.Object {
        MpIWebViewHost _host;
        Context context;

        public MpAvAdJsInterface(Context context, MpIWebViewHost host) {
            _host = host;
            this.context = context;
        }

        [Export]
        [JavascriptInterface]
        public void InvokeMethod(string fn, string msg) {
            MpConsole.WriteLine($"Fn: '{fn}' Data: '{msg}'");
            if (_host == null || _host.BindingHandler == null) {
                return;
            }
            Dispatcher.UIThread.Post(() => {
                _host.BindingHandler.HandleBindingNotification(fn.ToEnum<MpAvEditorBindingFunctionType>(), msg);
            });

        }
    }
}
