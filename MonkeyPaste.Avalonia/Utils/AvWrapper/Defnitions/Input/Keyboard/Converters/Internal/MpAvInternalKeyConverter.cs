﻿using Avalonia.Input;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Text;

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
            if (lks == MpInputConstants.AV_CONTROL_KEY_LITERAL.ToLower() ||
                lks == MpInputConstants.CONTROL_KEY_LITERAL.ToLower()) {
                return Key.LeftCtrl;//.LeftCtrl;
            }
            if (lks == MpInputConstants.AV_CAPS_LOCK_KEY_LITERAL.ToLower() ||
                lks == MpInputConstants.SH_CAPS_LOCK_KEY_LITERAL.ToLower()) {
                return Key.CapsLock;
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

        public string ConvertKeyStringToSendKeysString(string keyString) {
            // NOTE keyString should NOT be a sequence
            //if(keyString.Contains(",")) {
            //    throw new Exception($"keyString '{keyString}' is a sequence and SendKeys only handles one gesture, if seq necessary call multiple times w/ delay in between");
            //}

            var sb = new StringBuilder();
            string[] keySequences = keyString.Split(new string[] { MpInputConstants.SEQUENCE_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < keySequences.Length; i++) {
                string seq = keySequences[i].Trim();
                //string outStr = string.Empty;
                var keys = seq.Split(new string[] { MpInputConstants.COMBO_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
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

        //public System.Windows.Input.Key WinformsToWPFKey(System.Windows.Forms.Keys formsKey) {

        //    // Put special case logic here if there's a key you need but doesn't map...  
        //    try {
        //        return KeyInterop.KeyFromVirtualKey((int)formsKey);
        //    }
        //    catch {
        //        // There wasn't a direct mapping...    
        //        return System.Windows.Input.Key.None;
        //    }
        //}

        //public System.Windows.Forms.Keys WpfKeyToWinformsKey(Key wpfKey) {

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