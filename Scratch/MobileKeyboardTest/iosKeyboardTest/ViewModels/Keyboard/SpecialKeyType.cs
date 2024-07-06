using System;

namespace iosKeyboardTest
{
    public enum SpecialKeyType
    {
        None = 0,
        Shift,
        Backspace,
        SymbolToggle,
        Enter,
        Tab,
        CapsLock,
        Emoji,
        ArrowLeft,
        ArrowRight,
        NextKeyboard
    }
    public enum ShiftStateType
    {
        None = 0,
        Shift,
        ShiftLock
    }
    public enum CharSetType
    {
        Letters = 0,
        Symbols1,
        Symbols2
    }

    [Flags]
    public enum KeyboardFlags : long {
        None = 0,

        // PLATFORM
        Android = 1L << 1,
        iOS = 1L << 2,

        // ORIENTATION
        Portrait = 1L << 3,
        Landscape = 1L << 4,

        // LAYOUT
        Float = 1L << 5,
        Full = 1L << 6,

        // MODE
        FreeText = 1L << 7,
        Numbers = 1L << 8,
        Url = 1L << 9,
        Email = 1L << 10,
        Search = 1L << 11,

        // THEME
        Light = 1L << 12,
        Dark = 1L << 13,

        // DEVICE
        Phone = 1L << 14,
        Tablet = 1L << 15,
    }
}
