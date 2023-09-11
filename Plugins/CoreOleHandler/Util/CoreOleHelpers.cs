using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using MonkeyPaste.Common.Avalonia;

namespace CoreOleHandler {
    public static class CoreOleHelpers {

        private static IClipboard _clipboardRef;
        public static IClipboard ClipboardRef {
            get {
                if (_clipboardRef == null) {
                    var mw = Application.Current.GetMainWindow();
                    if (mw is Control c && TopLevel.GetTopLevel(c) is TopLevel tl &&
                        tl.Clipboard is IClipboard cb) {
                        _clipboardRef = cb;
                    }
                }
                return _clipboardRef;
            }
        }
    }
}