#if MAC
using MonoMac.AppKit;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvMacHelpers {
        private static bool _isInitialized;

        public static void EnsureInitialized() {
            if (_isInitialized) {
                return;
            }
            _isInitialized = true;
            NSApplication.Init();
        }
    }
}
#endif
