using System.Collections;

namespace MonkeyPaste.Common {
    public static class MpInputConstants {
        public const string COMBO_SEPARATOR = "+";
        public const string SEQUENCE_SEPARATOR = "|";


        public const string AV_CONTROL_KEY_LITERAL = "Ctrl";
        public const string AV_CAPS_LOCK_KEY_LITERAL = "Caps";

        public const string SH_CAPS_LOCK_KEY_LITERAL = "CapsLock";

        public const string CONTROL_KEY_LITERAL = "Control";
        public const string META_KEY_LITERAL = "Meta";
        public const string ALT_KEY_LITERAL = "Alt";
        public const string SHIFT_KEY_LITERAL = "Shift";

        public const string ESCAPE_KEY_LITERAL = "Esc";
        public const string ENTER_KEY_LITERAL = "Enter";
        public const string CAPS_LOCK_KEY_LITERAL = "Caps Lock";

        public static string[] MOD_LITERALS =>
            new string[] {
                CONTROL_KEY_LITERAL,
                META_KEY_LITERAL,
                ALT_KEY_LITERAL
            };
    }
}
