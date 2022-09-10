using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvKeyboardInputHelpers {

        public static List<List<Key>> ConvertStringToKeySequence(string keyStr) {
            var keyList = new List<List<Key>>();
            if (string.IsNullOrEmpty(keyStr)) {
                return keyList;
            }

            var combos = keyStr.Split(new string[] { MpAvKeyGestureHelper2.SEQUENCE_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var c in combos) {
                var keys = c.Split(new string[] { MpAvKeyGestureHelper2.COMBO_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
                keyList.Add(new List<Key>());
                foreach (var k in keys) {
                    keyList[keyList.Count - 1].Add(ConvertStringToKey(k));
                }
            }
            return keyList;
        }

        public static List<List<string>> ConvertStringToKeyLiteralSequence(string keyStr) {
            var kseq = ConvertStringToKeySequence(keyStr);
            var lseq = new List<List<string>>();
            foreach(var kcombo in kseq) {
                var lcombo = new List<string>();
                foreach(var k in kcombo) {
                    lcombo.Add(GetKeyLiteral(k));
                }
                lseq.Add(lcombo);
            }
            return lseq;
        }

        public static string ConvertKeySequenceToString(List<List<Key>> keyList) {
            var outStr = string.Empty;
            foreach (var kl in keyList) {
                if (!string.IsNullOrEmpty(outStr)) {
                    outStr += MpAvKeyGestureHelper2.SEQUENCE_SEPARATOR;
                }
                foreach (var k in kl) {
                    outStr += GetKeyLiteral(k) + MpAvKeyGestureHelper2.COMBO_SEPARATOR;
                }
                outStr = outStr.Remove(outStr.Length - 1, 1);
            }
            if (!string.IsNullOrEmpty(outStr)) {
                if (outStr.EndsWith("|")) {
                    outStr = outStr.Remove(outStr.Length - 2, 2);
                }
            }
            return outStr;
        }

        public static Key ConvertStringToKey(string keyStr) {
            if(keyStr.IsNullOrEmpty()) {
                return Key.None;
            }

            string lks = keyStr.ToLower();
            if (lks == "ctrl") {
                return Key.LeftCtrl;//.LeftCtrl;
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
            if(lks == "control") {
                return Key.LeftCtrl;
            }

            var kg = KeyGesture.Parse(lks);

            return (Key)Enum.Parse(typeof(Key), keyStr, true);
        }

        public static string ConvertKeyToString(Key key) {
            if (key == Key.LeftCtrl || key == Key.RightCtrl) {
                return "Ctrl";
            }
            if (key == Key.LeftAlt || key == Key.RightAlt) { //} || key == Key.System) {
                return "Alt";
            }
            if (key == Key.LeftShift || key == Key.RightShift) {
                return "Shift";
            }

            return key.ToString();
        }

        public static string GetKeyLiteral(Key key) {
            if (key == Key.LeftShift || key == Key.RightShift) {
                return MpKeyLiteralStringHelpers.SHIFT_KEY_LITERAL;
            }
            if (key == Key.LeftAlt || key == Key.RightAlt) {
                return MpKeyLiteralStringHelpers.ALT_KEY_LITERAL;
            }
            if (key == Key.LeftCtrl || key == Key.RightCtrl) {
                return MpKeyLiteralStringHelpers.CONTROL_KEY_LITERAL;
            }
            if(key == Key.Escape) {
                return MpKeyLiteralStringHelpers.ESCAPE_KEY_LITERAL;
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
                        case "Ctrl":
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

        //public static System.Windows.Input.Key WinformsToWPFKey(System.Windows.Forms.Keys formsKey) {

        //    // Put special case logic here if there's a key you need but doesn't map...  
        //    try {
        //        return KeyInterop.KeyFromVirtualKey((int)formsKey);
        //    }
        //    catch {
        //        // There wasn't a direct mapping...    
        //        return System.Windows.Input.Key.None;
        //    }
        //}

        //public static System.Windows.Forms.Keys WpfKeyToWinformsKey(Key wpfKey) {

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
