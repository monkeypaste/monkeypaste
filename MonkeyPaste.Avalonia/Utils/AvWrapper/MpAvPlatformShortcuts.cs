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
                MpKeyLiteralStringHelpers.META_KEY_LITERAL :
                MpKeyLiteralStringHelpers.CONTROL_KEY_LITERAL;

            CutKeys = $"{opKey}+X";
            CopyKeys = $"{opKey}+C";
            PasteKeys = $"{opKey}+V";
        }
    }
}
