using Avalonia.Input.Platform;
using MonkeyPaste.Common.Avalonia;

namespace CoreOleHandler {
    public static class CoreOleHelpers {

        private static IClipboard _clipboardRef;
        public static IClipboard ClipboardRef {
            get {
                if (_clipboardRef == null) {
                    if (MpAvCommonTools.Services != null) {
                        _clipboardRef = MpAvCommonTools.Services.DeviceClipboard;
                    }
                }
                return _clipboardRef;
            }
        }
    }
}