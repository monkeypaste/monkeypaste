//using Avalonia.Win32;
using MonkeyPaste.Common;
using System;

namespace MonkeyPaste.Avalonia {
    public class MpAvPlatformShortcuts : MpIPlatformShorcuts {
        public string CutKeys { get; private set; }
        public string CopyKeys { get; private set; }
        public string PasteKeys { get; private set; }

        public MpAvPlatformShortcuts() {
            string opKey = OperatingSystem.IsMacOS() ?
                MpInputConstants.META_KEY_LITERAL :
                MpInputConstants.CONTROL_KEY_LITERAL;

            CutKeys = $"{opKey}+x";
            CopyKeys = $"{opKey}+c";
            PasteKeys = $"{opKey}+v";
        }
    }
}
