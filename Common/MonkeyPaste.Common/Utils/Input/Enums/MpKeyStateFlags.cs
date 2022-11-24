using System;

namespace MonkeyPaste.Common {
    [Flags]
    public enum MpKeyStateFlags {
        None = 0,
        Down = 1,
        Toggled = 2,
    }
}
