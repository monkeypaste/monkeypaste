using System;

namespace iosKeyboardTest.iOS {

    public enum MenuPageType {
        None = 0,
        TabSelector,
        Completions,
        OtherTab
    }
    public enum MenuItemType {
        None = 0,
        BackButton,
        OptionsButton,
        CompletionItem,
        OtherTabItem
    }
    [Flags]
    public enum WordBreakTypes : long {
        None = 0,
        Grammatical = 1L << 1,
        UpperToLowerCase = 1L << 2, // camel or pascal case
        UnderScore = 1L << 3, // snake case
        Hyphen = 1L << 4, // kebob case
    }
    
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


    public enum MyPrefKeys {
        None = 0,
        DO_NUM_ROW,
        DO_EMOJI_KEY,
        DO_SOUND,
        SOUND_LEVEL, // 0-100, 15
        DO_VIBRATE,
        VIBRATE_LEVEL, // 0-5, 1
        DO_POPUP,
        DO_LONG_POPUP,
        LONG_POPUP_DELAY, // 0-1000, 500
        DO_NIGHT_MODE,
        DO_KEY_BOARDERS,
        BG_OPACITY, // 0-255, 255
        FG_OPACITY, // 0-255, 255
        DO_SUGGESTION_STRIP,
        DO_NEXT_WORD_COMPLETION,
        MAX_COMPLETION_COUNT, //0-20, 8
        DO_AUTO_CORRECT,
        DO_BACKSPACE_UNDOS_LAST_AUTO_CORRECT,
        DO_AUTO_CAPITALIZATION,
        DO_DOUBLE_SPACE_PERIOD,
        DO_CURSOR_CONTROL,
        CURSOR_CONTROL_SENSITIVITY_X, //0-100, 50
        CURSOR_CONTROL_SENSITIVITY_Y, //0-100, 50
        DO_CASE_COMPLETION
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
        Normal = 1L << 7,
        Numbers = 1L << 8,
        Digits = 1L << 9,
        Pin = 1L << 10,
        Url = 1L << 11,
        Email = 1L << 12,
        Search = 1L << 13,
        Next = 1L << 14,
        Done = 1L << 15,

        // THEME
        Light = 1L << 16,
        Dark = 1L << 17,

        // DEVICE
        Mobile = 1L << 18,
        Tablet = 1L << 19,
        
        // LOOK & FEEL
        PlatformView = 1L << 20,
        //NumberRow = 1L << 21,
        //KeyBorders = 1L << 22,
        //EmojiKey = 1L << 23,

        
        //Vibrate = 1L << 24,
        //Sound = 1L << 25,
        //ShowPopups = 1L << 26,
        //ShowLongPress = 1L << 27,

        OneHanded = 1L << 28,

        //AutoCap = 1L << 29,
        //DoubleTapSpace = 1L << 30,
        //CursorControl = 1L << 31,
        
    }
}
