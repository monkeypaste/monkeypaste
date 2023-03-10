using System;
using System.Text;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpWin32InputHelpers {
        public static string ConvertKeyStringToSendKeysString(string keyString) {
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
