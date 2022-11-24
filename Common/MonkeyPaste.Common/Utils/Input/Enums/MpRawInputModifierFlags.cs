using System;

namespace MonkeyPaste.Common {
    [Flags]
    public enum MpRawInputModifierFlags {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Meta = 8,
        LeftMouseButton = 16,
        RightMouseButton = 32,
        MiddleMouseButton = 64,
        XButton1MouseButton = 128,
        XButton2MouseButton = 256,
        KeyboardMask = Alt | Control | Shift | Meta
    }
}
