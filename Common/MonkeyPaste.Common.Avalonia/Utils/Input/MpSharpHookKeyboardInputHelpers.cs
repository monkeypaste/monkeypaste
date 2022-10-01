using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpHook.Native;
using MonkeyPaste.Common;
using System.Diagnostics;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpSharpHookKeyboardInputHelpers {
        public static List<List<KeyCode>> ConvertStringToKeySequence(string keyStr) {
            var keyList = new List<List<KeyCode>>();
            if (string.IsNullOrEmpty(keyStr)) {
                return keyList;
            }

            var combos = keyStr.Split(new string[] { MpAvKeyGestureHelper2.SEQUENCE_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var c in combos) {
                var keys = c.Split(new string[] { MpAvKeyGestureHelper2.COMBO_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
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
                    outStr += MpAvKeyGestureHelper2.SEQUENCE_SEPARATOR;
                }
                foreach (var k in kl) {
                    outStr += GetKeyLiteral(k) + MpAvKeyGestureHelper2.COMBO_SEPARATOR;
                }
                outStr = outStr.Remove(outStr.Length - MpAvKeyGestureHelper2.COMBO_SEPARATOR.Length, MpAvKeyGestureHelper2.COMBO_SEPARATOR.Length);
            }
            if (!string.IsNullOrEmpty(outStr)) {
                if (outStr.EndsWith(MpAvKeyGestureHelper2.SEQUENCE_SEPARATOR)) {
                    outStr = outStr.Remove(outStr.Length - MpAvKeyGestureHelper2.SEQUENCE_SEPARATOR.Length, MpAvKeyGestureHelper2.SEQUENCE_SEPARATOR.Length);
                }
            }
            return outStr;
        }

        public static KeyCode GetKeyValue(string keyStr) {
           
            string lks = keyStr.ToLower();
            if (lks == "control") {
                return KeyCode.VcLeftControl;//.LeftCtrl;
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
                return "Shift";
            }
            if (key == KeyCode.VcLeftAlt || key == KeyCode.VcRightAlt) {
                return "Alt";
            }
            if (key == KeyCode.VcLeftControl || key == KeyCode.VcRightControl) {
                return "Control";
            }
            if(key == KeyCode.VcNumPadDivide) {
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
            if (key == KeyCode.VcNumPadEnter) {
                return @"Enter";
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
            if(keyStr.StartsWith("VcNumPad")) {
                return key.ToString().Replace("VcNumPad", String.Empty);
            }
            return key.ToString().Replace("Vc", String.Empty);
        }
    }
}
