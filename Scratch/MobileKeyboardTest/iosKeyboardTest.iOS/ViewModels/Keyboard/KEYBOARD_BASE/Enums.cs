using System;

namespace iosKeyboardTest.iOS {
    [Flags]
    public enum KeyboardFeedbackFlags : long {
        None = 0,
        Vibrate = 1L << 1,
        Click = 1L << 2,
        Return = 1L << 3,
    }
    public enum SpecialKeyType
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
        Search, // PrimarySpecial (ios only)
        Go, // PrimarySpecial
        Enter, // PrimarySpecial
        Next, // PrimarySpecial
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
        Symbols2,
        Numbers1,
        Numbers2,
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
        FloatLayout = 1L << 5,
        FullLayout = 1L << 6,

        // MODE
        FreeText = 1L << 7,
        Numbers = 1L << 8,
        Url = 1L << 9,
        Email = 1L << 10,
        Search = 1L << 11,
        Next = 1L << 12,
        Done = 1L << 13,

        // THEME
        Light = 1L << 14,
        Dark = 1L << 15,

        // DEVICE
        Mobile = 1L << 16,
        Tablet = 1L << 17,
        
        // UI
        PlatformView = 1L << 18,
    }
}
