using System;

namespace MonkeyPaste {
    [Flags]
    public enum MpAppendModeFlags {
        None = 0,
        AppendLine = 1,
        Append = 2,
        Manual = 4
    }
}
