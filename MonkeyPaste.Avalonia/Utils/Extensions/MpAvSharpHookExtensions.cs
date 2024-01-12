using Avalonia;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvSharpHookExtensions {
        public static MpPoint GetScaledScreenPoint(this MouseEventData med, out PixelPoint unscaled_point) {
            double scale = MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling;
            int x = Math.Max(0, (int)med.X);
            int y = Math.Max(0, (int)med.Y);

            unscaled_point = new PixelPoint(x, y);
            var scaled_p = new MpPoint(Math.Max(0, (double)x / scale), Math.Max(0, (double)y / scale));
            return scaled_p;
        }

        public static int GesturePriority(this KeyCode kc) {
            switch (kc) {
                case KeyCode.VcLeftControl:
                case KeyCode.VcRightControl:
                    return 0;
                case KeyCode.VcLeftAlt:
                case KeyCode.VcRightAlt:
                    return 1;
                case KeyCode.VcLeftMeta:
                case KeyCode.VcRightMeta:
                    return 2;
                case KeyCode.VcLeftShift:
                case KeyCode.VcRightShift:
                    return 3;
                default:
                    return 4;
            }
        }

        public static bool IsModKey(this KeyCode kc) {
            switch (kc) {
                case KeyCode.VcLeftControl:
                case KeyCode.VcRightControl:
                case KeyCode.VcLeftAlt:
                case KeyCode.VcRightAlt:
                case KeyCode.VcLeftMeta:
                case KeyCode.VcRightMeta:
                case KeyCode.VcLeftShift:
                case KeyCode.VcRightShift:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsAlphaNumeric(this KeyCode kc) {
            switch (kc) {
                case KeyCode.Vc0:
                case KeyCode.Vc1:
                case KeyCode.Vc2:
                case KeyCode.Vc3:
                case KeyCode.Vc4:
                case KeyCode.Vc5:
                case KeyCode.Vc6:
                case KeyCode.Vc7:
                case KeyCode.Vc8:
                case KeyCode.Vc9:
                case KeyCode.VcA:
                case KeyCode.VcB:
                case KeyCode.VcC:
                case KeyCode.VcD:
                case KeyCode.VcE:
                case KeyCode.VcF:
                case KeyCode.VcG:
                case KeyCode.VcH:
                case KeyCode.VcI:
                case KeyCode.VcJ:
                case KeyCode.VcK:
                case KeyCode.VcL:
                case KeyCode.VcM:
                case KeyCode.VcN:
                case KeyCode.VcO:
                case KeyCode.VcP:
                case KeyCode.VcQ:
                case KeyCode.VcR:
                case KeyCode.VcS:
                case KeyCode.VcT:
                case KeyCode.VcU:
                case KeyCode.VcV:
                case KeyCode.VcW:
                case KeyCode.VcX:
                case KeyCode.VcY:
                case KeyCode.VcZ:
                    return true;
                default:
                    return false;
            }
        }

        public static KeyCode GetUnifiedKey(this KeyCode kc) {
            // Default to left for mods and numpad for arrows and numbers
            switch (kc) {
                case KeyCode.VcLeftControl:
                case KeyCode.VcRightControl:
                    return KeyCode.VcLeftControl;
                case KeyCode.VcLeftAlt:
                case KeyCode.VcRightAlt:
                    return KeyCode.VcLeftAlt;
                case KeyCode.VcLeftMeta:
                case KeyCode.VcRightMeta:
                    return KeyCode.VcLeftMeta;
                case KeyCode.VcLeftShift:
                case KeyCode.VcRightShift:
                    return KeyCode.VcLeftShift;
                default:
                    string keyStr = kc.ToString();
                    if (keyStr.StartsWith("VcNumPad")) {
                        return kc;
                    }
                    if (Enum.TryParse(typeof(KeyCode), $"VcNumPad{keyStr.Replace("Vc", string.Empty)}", out object unified_numpad_kc_obj)) {
                        return (KeyCode)unified_numpad_kc_obj;
                    }
                    return kc;
            }
        }

        public static string GetKeyLiteral(this KeyCode key) {
            if (key == KeyCode.VcUndefined) {
                // BUG this came up during a paste shortly after unlocking hd box closed
                // also explorer process wasn't activating before pasting
                return string.Empty;
            }
            if (key == KeyCode.VcLeftShift || key == KeyCode.VcRightShift) {
                return MpInputConstants.SHIFT_KEY_LITERAL;
            }
            if (key == KeyCode.VcLeftAlt || key == KeyCode.VcRightAlt) {
                return MpInputConstants.ALT_KEY_LITERAL;
            }
            if (key == KeyCode.VcLeftControl || key == KeyCode.VcRightControl) {
                return MpInputConstants.CONTROL_KEY_LITERAL;
            }
            if (key == KeyCode.VcCapsLock) {
                return MpInputConstants.CAPS_LOCK_KEY_LITERAL;
            }
            if (key == KeyCode.VcNumPadEnter) {
                return MpInputConstants.ENTER_KEY_LITERAL;
            }
            if (key == KeyCode.VcEscape) {
                return MpInputConstants.ESCAPE_KEY_LITERAL;
            }
            if (key == KeyCode.VcLeftMeta || key == KeyCode.VcRightMeta) {
#if WINDOWS
                return MpInputConstants.WIN_META_KEY_LITERAL;
#elif LINUX
                return MpInputConstants.LINUX_META_KEY_LITERAL;
#elif MAC
                return MpInputConstants.MAC_META_KEY_LITERAL;
#endif
            }
            if (key == KeyCode.VcNumPadDivide) {
                return @"/";
            }
            if (key == KeyCode.VcNumPadMultiply) {
                return @"*";
            }
            if (key == KeyCode.VcNumPadSubtract) {
                return @"-";
            }
            if (key == KeyCode.VcNumPadEquals) {
                return @"=";
            }
            if (key == KeyCode.VcNumPadAdd) {
                return @"+";
            }
            if (key == KeyCode.VcNumPadSeparator) {
                // what key is this?
                MpDebug.Break();
            }
            if (key == KeyCode.VcSemicolon) {
                return ";";
            }
            //if (key == KeyCode.VcBackquote) {
            if (key == KeyCode.VcBackQuote) {
                return "`";
            }
            if (key == KeyCode.VcQuote) {
                return "'";
            }
            if (key == KeyCode.VcMinus) {
                return "-";
            }
            if (key == KeyCode.VcEquals) {
                return "=";
            }
            if (key == KeyCode.VcComma) {
                return ",";
            }
            //if (key == KeyCode.VcBackSlash) {
            if (key == KeyCode.VcBackslash) {
                return @"/";
            }
            if (key == KeyCode.VcPeriod) {
                return ".";
            }
            if (key == KeyCode.VcOpenBracket) {
                return "[";
            }
            if (key == KeyCode.VcCloseBracket) {
                return "]";
            }
            if (key == KeyCode.VcSlash) {
                return @"\";
            }
            string keyStr = key.ToString();
            if (keyStr.StartsWith("VcNumPad")) {
                return key.ToString().Replace("VcNumPad", String.Empty);
            }
            return key.ToString().Replace("Vc", String.Empty);
        }
        public static KeyCode ToSharpHookKeyCode(this string keyStr) {
            string lks = keyStr.ToLower();
            if (lks == MpInputConstants.AV_CONTROL_KEY_LITERAL.ToLower() ||
                lks == MpInputConstants.CONTROL_KEY_LITERAL.ToLower()) {
                return KeyCode.VcLeftControl;//.LeftCtrl;
            }
            if (lks == MpInputConstants.AV_CAPS_LOCK_KEY_LITERAL.ToLower() ||
               lks == MpInputConstants.SH_CAPS_LOCK_KEY_LITERAL.ToLower()) {
                return KeyCode.VcCapsLock;
            }
            if (lks == MpInputConstants.ENTER_KEY_LITERAL.ToLower()) {
                return KeyCode.VcNumPadEnter;
            }
            if (lks == MpInputConstants.ESCAPE_KEY_LITERAL.ToLower()) {
                return KeyCode.VcEscape;
            }
            if (lks == MpInputConstants.META_KEY_LITERAL.ToLower() ||
                lks == MpInputConstants.WIN_META_KEY_LITERAL.ToLower() ||
                lks == MpInputConstants.MAC_META_KEY_LITERAL.ToLower() ||
                lks == MpInputConstants.LINUX_META_KEY_LITERAL.ToLower()) {
                return KeyCode.VcLeftMeta;
            }
            if (lks == "alt") {
                return KeyCode.VcLeftAlt;//.LeftAlt;
            }
            if (lks == "shift") {
                return KeyCode.VcLeftShift;
            }
            if (lks == ";") {
                return KeyCode.VcSemicolon;
            }
            if (lks == "`") {
                //return KeyCode.VcBackquote;
                return KeyCode.VcBackQuote;
            }
            if (lks == "'") {
                return KeyCode.VcQuote;
            }
            if (lks == "-") {
                return KeyCode.VcMinus;
            }
            if (lks == "=") {
                return KeyCode.VcEquals;
            }
            if (lks == ",") {
                return KeyCode.VcComma;
            }
            if (lks == @"/") {
                //return KeyCode.VcBackSlash;
                return KeyCode.VcBackslash;
            }
            if (lks == ".") {
                return KeyCode.VcPeriod;
            }
            if (lks == "[") {
                return KeyCode.VcOpenBracket;
            }
            if (lks == "]") {
                return KeyCode.VcCloseBracket;
            }
            if (lks == "|") {
                return KeyCode.VcSlash;
            }
            if (lks == "PageDown") {
                return KeyCode.VcPageDown;
            }
            if (lks == "caps lock") {
                return KeyCode.VcCapsLock;
            }

            if (Enum.TryParse(typeof(KeyCode), keyStr.StartsWith("Vc") ? keyStr : "Vc" + keyStr.ToUpper(), true, out object? keyCodeObj) &&
               keyCodeObj is KeyCode keyCode) {
                return keyCode;
            }
            string err_msg = $"Unhandled global key literal '{lks}'";
#if DEBUG
            //throw new MpException(err_msg);
            //#else
            MpConsole.WriteLine(err_msg);
            return KeyCode.VcUndefined;
#else
            return KeyCode.VcUndefined;
#endif

        }
        public static bool IsSameKey(this KeyCode kc, KeyCode okc, bool unify_mods) {
            if (kc == okc) {
                return true;
            }
            if (!unify_mods) {
                return false;
            }
            return kc.GetUnifiedKey() == okc.GetUnifiedKey();
        }

        public static bool IsMatch(this IReadOnlyList<IReadOnlyList<KeyCode>> gesture, IReadOnlyList<IReadOnlyList<KeyCode>> mv, bool unify_mods = false) {
            if (!gesture.StartsWith(mv, unify_mods) ||
                 gesture.Count != mv.Count) {
                return false;
            }
            foreach (var (mv_combo, mv_combo_idx) in mv.WithIndex()) {
                if (gesture[mv_combo_idx].Count != mv_combo.Count) {
                    return false;
                }
            }
            return true;
        }
        public static bool StartsWith(this IReadOnlyList<IReadOnlyList<KeyCode>> gesture, IReadOnlyList<IReadOnlyList<KeyCode>> mv, bool unify_mods = false) {
            if (gesture == null || mv == null) {
                return false;
            }
            foreach (var (mv_combo, mv_combo_idx) in mv.WithIndex()) {
                if (gesture.Count <= mv_combo_idx) {
                    //mv has more combos that gesture so no
                    return false;
                }
                foreach (var (mv_key, mv_key_idx) in mv_combo.WithIndex()) {
                    if (gesture[mv_combo_idx].Count <= mv_key_idx) {
                        //mv combo is longer than gestures combo at idx
                        return false;
                    }
                    if (!gesture[mv_combo_idx][mv_key_idx].IsSameKey(mv_key, unify_mods)) {
                        return false;
                    }
                }
            }
            return true;
        }

        public static List<List<KeyCode>> CloneGesture(this IReadOnlyList<IReadOnlyList<KeyCode>> gesture) {
            return
                gesture
                .Select(x => x.ToList())
                .ToList();
        }
    }
}
