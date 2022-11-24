namespace MonkeyPaste.Common.Avalonia.Utils.Extensions {
    public static class MpLiteralKeyStringExtensions {
        public static string LITERAL_SHIFT => "Shift";
        public static string LITERAL_CONTROL => "Control";
        public static string LITERAL_ALT => "Alt";
        public static string LITERAL_ESCAPE => "Escape";
        public static string LITERAL_META => "Meta";
        public static bool IsShift(this string litKeyStr) {
            return litKeyStr == LITERAL_SHIFT;
        }

        public static bool IsAlt(this string litKeyStr) {
            return litKeyStr == LITERAL_ALT;
        }
        public static bool IsCtrl(this string litKeyStr) {
            return litKeyStr == LITERAL_CONTROL;
        }

        public static bool IsEscape(this string litKeyStr) {
            return litKeyStr == LITERAL_ESCAPE;
        }

        public static bool IsMeta(this string litKeyStr) {
            return litKeyStr == LITERAL_META;
        }
    }
}
