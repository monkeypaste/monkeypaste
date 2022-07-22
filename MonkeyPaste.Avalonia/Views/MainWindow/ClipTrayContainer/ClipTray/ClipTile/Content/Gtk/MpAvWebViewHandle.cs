using Avalonia.Platform;
using SharpWebview;
using System;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    class MpAvWebViewHandle : IPlatformHandle, IDisposable {
        private Webview _webview;

        public MpAvWebViewHandle(Webview view) {
            _webview = view;
        }

        public IntPtr Handle => _webview?.WebViewWindowHandle ?? IntPtr.Zero;
        public string HandleDescriptor => "Webview";

        public void Dispose() {
            _webview.Dispose();
            _webview = null;
        }
    }
}
