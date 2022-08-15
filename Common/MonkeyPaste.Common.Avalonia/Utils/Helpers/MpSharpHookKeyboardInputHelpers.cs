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
                    keyList[keyList.Count - 1].Add(ConvertStringToKey(k));
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

        public static KeyCode ConvertStringToKey(string keyStr) {
            if(Enum.TryParse(typeof(KeyCode), keyStr.StartsWith("Vc") ? keyStr : "Vc" + keyStr, out object? keyCodeObj) &&
                keyCodeObj is KeyCode keyCode) {
                return keyCode;
            }
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
            throw new Exception("unkown key: " + lks);
        }

        public static string ConvertKeyToString(KeyCode key) {
            if (key == KeyCode.VcLeftControl || key == KeyCode.VcRightControl) {
                return "Control";
            }
            if (key == KeyCode.VcLeftAlt || key == KeyCode.VcRightAlt) { //} || key == KeyCode.System) {
                return "Alt";
            }
            if (key == KeyCode.VcLeftShift || key == KeyCode.VcRightShift) {
                return "Shift";
            }

            return key.ToString();
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

        public static string ConvertKeyStringToSendKeysString(string keyString) {
            // NOTE keyString should NOT be a sequence
            //if(keyString.Contains(",")) {
            //    throw new Exception($"keyString '{keyString}' is a sequence and SendKeys only handles one gesture, if seq necessary call multiple times w/ delay in between");
            //}

            var sb = new StringBuilder();
            string[] keySequences = keyString.Split(new string[] { MpAvKeyGestureHelper2.SEQUENCE_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < keySequences.Length; i++) {
                string seq = keySequences[i].Trim();
                //string outStr = string.Empty;
                var keys = seq.Split(new string[] { MpAvKeyGestureHelper2.COMBO_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var key in keys) {
                    switch (key) {
                        case "Control":
                            //outStr += "^";
                            sb.Append("^");
                            break;
                        case "Shift":
                            //outStr += "+";
                            sb.Append("+");
                            break;
                        case "Alt":
                            //outStr += "%";
                            sb.Append("%");
                            break;
                        case "Space":
                            //outStr += " ";
                            sb.Append(" ");
                            break;
                        case "Escape":
                            sb.Append("{ESC}");
                            break;
                        case "Back":
                            sb.Append("{BACKSPACE}");
                            break;
                        case "PageUp":
                            sb.Append("{PGUP}");
                            break;
                        case "PageDown":
                        case "Next":
                            sb.Append("{PGDOWN}");
                            break;
                        case "Capital":
                            sb.Append("{CAPSLOCK}");
                            break;
                        case "Return":
                            sb.Append("{ENTER}");
                            break;
                        case "Home":
                        case "End":
                        case "Del":
                        case "Delete":
                        case "Enter":
                        case "Tab":
                        case "Left":
                        case "Right":
                        case "Up":
                        case "Down":
                            sb.Append("{" + key.ToUpper() + "}");
                            //outStr += "{" + key.ToUpper() + "}";
                            break;
                        default:
                            if (key.ToUpper().StartsWith(@"F") && key.Length > 1) {
                                string fVal = key.Substring(1, key.Length - 1);
                                try {
                                    int val = Convert.ToInt32(fVal);
                                    //outStr += "{F" + val + "}";
                                    sb.Append("{F" + val + "}");
                                }
                                catch (Exception ex) {
                                    MpConsole.WriteLine(@"ShortcutViewModel.SendKeys exception creating key: " + key + " with exception: " + ex);
                                    //outStr += key.ToUpper();
                                    sb.Append(key.ToUpper());
                                    break;
                                }
                            } else {
                                //outStr += key.ToUpper();
                                sb.Append(key.ToUpper());
                            }
                            break;
                    }
                }
            }
            return sb.ToString();
        }

        //public static System.Windows.Input.KeyCode WinformsToWPFKey(System.Windows.Forms.Keys formsKey) {

        //    // Put special case logic here if there's a key you need but doesn't map...  
        //    try {
        //        return KeyInterop.KeyFromVirtualKey((int)formsKey);
        //    }
        //    catch {
        //        // There wasn't a direct mapping...    
        //        return System.Windows.Input.KeyCode.None;
        //    }
        //}

        //public static System.Windows.Forms.Keys WpfKeyToWinformsKey(KeyCode wpfKey) {

        //    // Put special case logic here if there's a key you need but doesn't map...  
        //    try {
        //        return (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(wpfKey);
        //    }
        //    catch {
        //        // There wasn't a direct mapping...    
        //        return System.Windows.Forms.Keys.None;
        //    }
        //}
    }
}
