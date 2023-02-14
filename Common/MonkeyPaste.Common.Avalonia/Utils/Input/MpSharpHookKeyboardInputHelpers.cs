using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpSharpHookKeyboardInputHelpers {
        public static List<List<KeyCode>> ConvertStringToKeySequence(string keyStr) {
            var keyList = new List<List<KeyCode>>();
            if (string.IsNullOrEmpty(keyStr)) {
                return keyList;
            }

            var combos = keyStr.Split(new string[] { MpKeyGestureHelper2.SEQUENCE_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var c in combos) {
                var keys = c.Split(new string[] { MpKeyGestureHelper2.COMBO_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
                keyList.Add(new List<KeyCode>());
                foreach (var k in keys) {
                    keyList[keyList.Count - 1].Add(GetKeyValue(k));
                }
            }
            return keyList;
        }

        public static string ConvertKeySequenceToString(List<List<KeyCode>> keyList) {
            var outStr = string.Empty;
            foreach (var kl in keyList) {
                if (!string.IsNullOrEmpty(outStr)) {
                    outStr += MpKeyGestureHelper2.SEQUENCE_SEPARATOR;
                }
                foreach (var k in kl) {
                    outStr += GetKeyLiteral(k) + MpKeyGestureHelper2.COMBO_SEPARATOR;
                }
                outStr = outStr.Remove(outStr.Length - MpKeyGestureHelper2.COMBO_SEPARATOR.Length, MpKeyGestureHelper2.COMBO_SEPARATOR.Length);
            }
            if (!string.IsNullOrEmpty(outStr)) {
                if (outStr.EndsWith(MpKeyGestureHelper2.SEQUENCE_SEPARATOR)) {
                    outStr = outStr.Remove(outStr.Length - MpKeyGestureHelper2.SEQUENCE_SEPARATOR.Length, MpKeyGestureHelper2.SEQUENCE_SEPARATOR.Length);
                }
            }
            return outStr;
        }

        public static KeyCode GetKeyValue(string keyStr) {
            string lks = keyStr.ToLower();
            if (lks == MpKeyLiteralStringHelpers.AV_CONTROL_KEY_LITERAL.ToLower() ||
                lks == MpKeyLiteralStringHelpers.CONTROL_KEY_LITERAL.ToLower()) {
                return KeyCode.VcLeftControl;//.LeftCtrl;
            }
            if (lks == MpKeyLiteralStringHelpers.AV_CAPS_LOCK_KEY_LITERAL.ToLower() ||
               lks == MpKeyLiteralStringHelpers.SH_CAPS_LOCK_KEY_LITERAL.ToLower()) {
                return KeyCode.VcCapsLock;
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
                return KeyCode.VcBackquote;
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
                return KeyCode.VcBackSlash;
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
            if (Enum.TryParse(typeof(KeyCode), keyStr.StartsWith("Vc") ? keyStr : "Vc" + keyStr.ToUpper(), out object? keyCodeObj) &&
               keyCodeObj is KeyCode keyCode) {
                return keyCode;
            }
            throw new Exception("unkown key: " + lks);
        }

        public static string GetKeyLiteral(KeyCode key) {
            if (key == KeyCode.VcLeftShift || key == KeyCode.VcRightShift) {
                return MpKeyLiteralStringHelpers.SHIFT_KEY_LITERAL;
            }
            if (key == KeyCode.VcLeftAlt || key == KeyCode.VcRightAlt) {
                return MpKeyLiteralStringHelpers.ALT_KEY_LITERAL;
            }
            if (key == KeyCode.VcLeftControl || key == KeyCode.VcRightControl) {
                return MpKeyLiteralStringHelpers.CONTROL_KEY_LITERAL;
            }
            if (key == KeyCode.VcCapsLock) {
                return MpKeyLiteralStringHelpers.CAPS_LOCK_KEY_LITERAL;
            }
            if (key == KeyCode.VcNumPadEnter) {
                return MpKeyLiteralStringHelpers.ENTER_KEY_LITERAL;
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
                Debugger.Break();
            }
            if (key == KeyCode.VcSemicolon) {
                return ";";
            }
            if (key == KeyCode.VcBackquote) {
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
            if (key == KeyCode.VcBackSlash) {
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
    }
}
