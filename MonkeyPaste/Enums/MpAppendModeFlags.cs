using System;

namespace MonkeyPaste {
    [Flags]
    public enum MpAppendModeFlags {
        None = 0,
        AppendLine = 1,
        AppendInsert = 2,
        Manual = 4,
        Paused = 8
    }
}
