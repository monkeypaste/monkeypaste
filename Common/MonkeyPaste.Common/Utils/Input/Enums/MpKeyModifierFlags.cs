using System;

namespace MonkeyPaste.Common {
    [Flags]
    public enum MpKeyModifierFlags {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Meta = 8,
    }
}
