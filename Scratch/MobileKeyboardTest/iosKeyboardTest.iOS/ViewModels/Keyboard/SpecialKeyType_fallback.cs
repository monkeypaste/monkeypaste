using System;

namespace iosKeyboardTest.iOS
{
    public enum SpecialKeyType_fallback
    {
        None = 0,
        Shift,
        Backspace,
        SymbolToggle,
        NumberSymbolsToggle,
        Tab,
        CapsLock,
        Emoji,
        ArrowLeft,
        ArrowRight,
        NextKeyboard,
        Done, // PrimarySpecial (default)
        Go, // PrimarySpecial
        Search, // PrimarySpecial
        Enter, // PrimarySpecial
        Next, // PrimarySpecial
    }
    public enum ShiftStateType_fallback
    {
        None = 0,
        Shift,
        ShiftLock
    }
    public enum CharSetType_fallback
    {
        Letters = 0,
        Symbols1,
        Symbols2,
        Numbers1,
        Numbers2,
    }

    [Flags]
    public enum KeyboardFlags_fallback : long {
        None = 0,

        // PLATFORM
        Android = 1L << 1,
        iOS = 1L << 2,

        // ORIENTATION
        Portrait = 1L << 3,
        Landscape = 1L << 4,

        // LAYOUT
        FloatLayout = 1L << 5,
        FullLayout = 1L << 6,

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
        Mobile = 1L << 14,
        Tablet = 1L << 15,
    }
}
