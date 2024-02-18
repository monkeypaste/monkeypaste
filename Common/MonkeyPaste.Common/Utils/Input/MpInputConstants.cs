namespace MonkeyPaste.Common {
    public static class MpInputConstants {
        public const string COMBO_SEPARATOR = "+";
        public const string SEQUENCE_SEPARATOR = "|";


        public const string AV_CONTROL_KEY_LITERAL = "Ctrl";
        public const string AV_CAPS_LOCK_KEY_LITERAL = "Caps";

        public const string SH_CAPS_LOCK_KEY_LITERAL = "CapsLock";

        public const string CONTROL_KEY_LITERAL = "Control";
        public const string META_KEY_LITERAL =
#if WINDOWS
            WIN_META_KEY_LITERAL;
#else
            MAC_META_KEY_LITERAL;
#endif
        public const string ALT_KEY_LITERAL = "Alt";
        public const string SHIFT_KEY_LITERAL = "Shift";

        public const string ESCAPE_KEY_LITERAL = "Esc";
        public const string ENTER_KEY_LITERAL = "Enter";
        public const string CAPS_LOCK_KEY_LITERAL = "Caps Lock";

        public const string BACKSPACE_KEY_LITERAL = "Backspace";

        public const string WIN_META_KEY_LITERAL = "Win";
        public const string LINUX_META_KEY_LITERAL = "Meta";
        public const string MAC_META_KEY_LITERAL = "Meta";

        //  0001FA9F
        public const string WIN_META_KEY_DISPLAY_VALUE = "\U0001FA9F";

        public const string MAC_META_KEY_DISPLAY_VALUE = "⌘";
        public const string MAC_SHIFT_KEY_DISPLAY_VALUE = "⇧";
        public const string MAC_ALT_KEY_DISPLAY_VALUE = "⌥";
        public const string MAC_CTRL_KEY_DISPLAY_VALUE = "⌃";
        public const string MAC_ESC_KEY_DISPLAY_VALUE = "⎋";


        public const string META_KEY_DISPLAY_VALUE =
#if MAC
            MAC_META_KEY_DISPLAY_VALUE;
#else
            WIN_META_KEY_DISPLAY_VALUE;
#endif


        // NOTE literals are ordered by priority (ie sharphook GesturePriority)
        public static string[] MOD_LITERALS =>
            new string[] {
                CONTROL_KEY_LITERAL,
                ALT_KEY_LITERAL,
                META_KEY_LITERAL,
                SHIFT_KEY_LITERAL
            };

    }
}
