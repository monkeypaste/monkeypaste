using MonoMac.AppKit;
using System;
using System.IO;

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

