using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
namespace MonkeyPaste.Common.Wpf {
    public static class MpWpfKeyboardInputHelpers {

        public static List<List<Key>> ConvertStringToKeySequence(string keyStr) {
            var keyList = new List<List<Key>>();
            if (string.IsNullOrEmpty(keyStr)) {
                return keyList;
            }

            var combos = keyStr.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var c in combos) {
                var keys = c.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                keyList.Add(new List<Key>());
                foreach (var k in keys) {
                    keyList[keyList.Count - 1].Add(ConvertStringToKey(k));
                }
            }
            return keyList;
        }

        public static string ConvertKeySequenceToString(List<List<Key>> keyList) {
            var outStr = string.Empty;
            foreach (var kl in keyList) {
                if (!string.IsNullOrEmpty(outStr)) {
                    outStr += ", ";
                }
                foreach (var k in kl) {
                    outStr += GetKeyLiteral(k) + "+";
                }
                outStr = outStr.Remove(outStr.Length - 1, 1);
            }
            if (!string.IsNullOrEmpty(outStr)) {
                if (outStr.EndsWith(", ")) {
                    outStr = outStr.Remove(outStr.Length - 2, 2);
                }
            }
            return outStr;
        }

        public static Key ConvertStringToKey(string keyStr) {
            string lks = keyStr.ToLower();
            if (lks == "control") {
                return Key.LeftCtrl;
            }
            if (lks == "alt") {
                return Key.LeftAlt;
            }
            if (lks == "shift") {
                return Key.LeftShift;
            }
            if (lks == ";") {
                return Key.Oem1;
            }
            if (lks == "`") {
                return Key.Oem3;
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
                return Key.Oem6;
            }
            if (lks == "|") {
                return Key.Oem5;
            }
            if (lks == "PageDown") {
                return Key.Next;
            }
            return (Key)Enum.Parse(typeof(Key), keyStr, true);
        }

        public static string ConvertKeyToString(Key key) {
            if (key == Key.LeftCtrl || key == Key.RightCtrl) {
                return "Control";
            }
            if (key == Key.LeftAlt || key == Key.RightAlt || key == Key.System) {
                return "Alt";
            }
            if (key == Key.LeftShift || key == Key.RightShift) {
                return "Shift";
            }

            return key.ToString();
        }

        public static string GetKeyLiteral(Key key) {
            if (key == Key.LeftShift) {
                return "Shift";
            }
            if (key == Key.LeftAlt) {
                return "Alt";
            }
            if (key == Key.LeftCtrl) {
                return "Control";
            }
            if (key == Key.Oem1) {
                return ";";
            }
            if (key == Key.Oem3) {
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
            if (key == Key.Oem6) {
                return "]";
            }
            if (key == Key.Oem5) {
                return "|";
            }
            if (key == Key.Next) {
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
            string[] keySequences = keyString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < keySequences.Length; i++) {
                string seq = keySequences[i].Trim();
                //string outStr = string.Empty;
                var keys = seq.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
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
                                    Console.WriteLine(@"ShortcutViewModel.SendKeys exception creating key: " + key + " with exception: " + ex);
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

        public static System.Windows.Input.Key WinformsToWPFKey(System.Windows.Forms.Keys formsKey) {

            // Put special case logic here if there's a key you need but doesn't map...  
            try {
                return KeyInterop.KeyFromVirtualKey((int)formsKey);
            }
            catch {
                // There wasn't a direct mapping...    
                return System.Windows.Input.Key.None;
            }
        }

        public static System.Windows.Forms.Keys WpfKeyToWinformsKey(Key wpfKey) {

            // Put special case logic here if there's a key you need but doesn't map...  
            try {
                return (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(wpfKey);
            }
            catch {
                // There wasn't a direct mapping...    
                return System.Windows.Forms.Keys.None;
            }
        }
    }
}
