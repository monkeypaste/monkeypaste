using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;

namespace MonkeyPaste.Avalonia {
    public class MpAvInternalKeyConverter : MpIKeyConverter<Key> {
        public int GetKeyPriority(Key key) {
            switch (key) {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    return 0;
                case Key.LeftAlt:
                case Key.RightAlt:
                    return 1;
                //case Key.Left:
                //case Key.VcRightMeta:
                //    return 2;
                case Key.LeftShift:
                case Key.RightShift:
                    return 3;
                default:
                    return 4;
            }
        }
        public Key ConvertStringToKey(string keyStr) {
            if (keyStr.IsNullOrEmpty()) {
                return Key.None;
            }

            string lks = keyStr.ToLower();
            if (lks == MpInputConstants.ESCAPE_KEY_LITERAL.ToLower()) {
                return Key.Escape;
            }
            if (lks == MpInputConstants.AV_CONTROL_KEY_LITERAL.ToLower() ||
                lks == MpInputConstants.CONTROL_KEY_LITERAL.ToLower()) {
                return Key.LeftCtrl;//.LeftCtrl;
            }
            if (lks == MpInputConstants.AV_CAPS_LOCK_KEY_LITERAL.ToLower() ||
                lks == MpInputConstants.SH_CAPS_LOCK_KEY_LITERAL.ToLower()) {
                return Key.CapsLock;
            }
            if (lks == MpInputConstants.BACKSPACE_KEY_LITERAL.ToLower()) {
                return Key.Back;
            }
            if (lks == MpInputConstants.META_KEY_LITERAL.ToLower() ||
                lks == MpInputConstants.WIN_META_KEY_LITERAL.ToLower() ||
                lks == MpInputConstants.MAC_META_KEY_LITERAL.ToLower() ||
                lks == MpInputConstants.WIN_META_KEY_DISPLAY_VALUE.ToLower() ||
                lks == MpInputConstants.MAC_META_KEY_DISPLAY_VALUE.ToLower() ||
                lks == MpInputConstants.LINUX_META_KEY_LITERAL.ToLower()) {
                return Key.LWin;
            }

            if (lks == "alt") {
                return Key.LeftAlt;//.LeftAlt;
            }
            if (lks == "shift") {
                return Key.LeftShift;
            }
            if (lks == ";") {
                return Key.OemSemicolon;
            }
            if (lks == "`") {
                return Key.OemTilde;
            }
            if (lks == "'") {
                return Key.OemQuotes;
            }
            if (lks == "-") {
                return Key.OemMinus;
            }
            if (lks == "=") {
                return Key.OemPlus;
            }
            if (lks == ",") {
                return Key.OemComma;
            }
            if (lks == @"/") {
                return Key.OemQuestion;
            }
            if (lks == ".") {
                return Key.OemPeriod;
            }
            if (lks == "[") {
                return Key.OemOpenBrackets;
            }
            if (lks == "]") {
                return Key.OemCloseBrackets;
            }
            if (lks == "|") {
                return Key.OemPipe;
            }
            if (lks == "pagedown") {
                return Key.PageDown;
            }
            if (lks == "caps lock") {
                return Key.CapsLock;
            }
            if (lks == "backspace") {
                return Key.Back;
            }
            if (lks.Length == 1 && lks.ToCharArray()[0] is char lks_char &&
                lks_char >= '0' && lks_char <= '9') {
                // avalonia numbers are prefixed w/ 'D' and parsing as-is 
                // returns the key with the number's keycode so prefix before parsing
                lks = "d" + lks;
            }

            if (Enum.TryParse(typeof(Key), lks, true, out object? keyObj) &&
               keyObj is Key key) {
                return key;
            }

            MpConsole.WriteLine($"Error parsing key literal '{lks}'");
            return Key.None;
        }

        public string GetKeyLiteral(Key key) {
            if (key == Key.LeftShift || key == Key.RightShift) {
                return MpInputConstants.SHIFT_KEY_LITERAL;
            }
            if (key == Key.LeftAlt || key == Key.RightAlt) {
                return MpInputConstants.ALT_KEY_LITERAL;
            }
            if (key == Key.LeftCtrl || key == Key.RightCtrl) {
                return MpInputConstants.AV_CONTROL_KEY_LITERAL;
            }
            if (key == Key.Escape) {
                return MpInputConstants.ESCAPE_KEY_LITERAL;
            }
            if (key == Key.Enter || key == Key.Return) {
                return MpInputConstants.ENTER_KEY_LITERAL;
            }
            if (key == Key.Back) {
                return MpInputConstants.BACKSPACE_KEY_LITERAL;
            }
            if (key == Key.LWin || key == Key.RWin) {
#if WINDOWS
                return MpInputConstants.WIN_META_KEY_LITERAL;
#elif LINUX
                return MpInputConstants.LINUX_META_KEY_LITERAL;
#elif MAC
                return MpInputConstants.MAC_META_KEY_LITERAL;
#endif
            }
            if (key == Key.OemSemicolon) {
                return ";";
            }
            if (key == Key.OemTilde) {
                return "`";
            }
            if (key == Key.OemQuotes) {
                return "'";
            }
            if (key == Key.OemMinus) {
                return "-";
            }
            if (key == Key.OemPlus) {
                return "=";
            }
            if (key == Key.OemComma) {
                return ",";
            }
            if (key == Key.OemQuestion) {
                return @"/";
            }
            if (key == Key.OemPeriod) {
                return ".";
            }
            if (key == Key.OemOpenBrackets) {
                return "[";
            }
            if (key == Key.OemCloseBrackets) {
                return "]";
            }
            if (key == Key.OemPipe) {
                return "|";
            }
            if (key == Key.PageDown) {
                return "PageDown";
            }
            return key.ToString();
        }
    }
}
