using System;

namespace MonkeyPaste {
    [Flags]
    public enum MpAppendModeFlags {
        None = 0,
        AppendBlock = 1,
        AppendInline = 2,
        Manual = 4,
        Paused = 8,
        Pre = 16
    }
}
