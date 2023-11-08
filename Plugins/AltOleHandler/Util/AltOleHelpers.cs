using Avalonia.Input.Platform;
using MonkeyPaste.Common.Avalonia;

namespace AltOleHandler {
    public static class AltOleHelpers {

        private static IClipboard _clipboardRef;
        public static IClipboard ClipboardRef {
            get {
                if (_clipboardRef == null && MpAvCommonTools.Services != null) {
                    _clipboardRef = MpAvCommonTools.Services.DeviceClipboard;
                }
                return _clipboardRef;
            }
        }
    }
}