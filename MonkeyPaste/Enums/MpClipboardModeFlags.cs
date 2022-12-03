using System;

namespace MonkeyPaste {
    [Flags]
    public enum MpClipboardModeFlags {
        None = 0,
        AppendInline = 1,
        AppendBlock = 2,
        ListeningForChanges = 4,
        RightClickPaste = 8,
        AutoCopy = 16
    }
}
