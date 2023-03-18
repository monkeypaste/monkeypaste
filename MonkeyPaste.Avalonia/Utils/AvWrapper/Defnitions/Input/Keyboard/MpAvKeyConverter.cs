using Avalonia.Input;
using MonkeyPaste.Common;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Avalonia {
    public class MpAvKeyConverter : MpIKeyConverterHub {

        private MpIKeyConverter<KeyCode> _globalConverter = new MpGlobalKeyConverter();
        private MpIKeyConverter<Key> _internalConverter = new MpAvInternalKeyConverter();

        public string ConvertKeySequenceToString<T>(IEnumerable<IEnumerable<T>> keyList) {
            var outStr = string.Empty;
            foreach (var kl in keyList) {
                if (!string.IsNullOrEmpty(outStr)) {
                    outStr += MpInputConstants.SEQUENCE_SEPARATOR;
                }
                foreach (var k in kl) {
                    if (k is KeyCode keyCode) {
                        outStr += _globalConverter.GetKeyLiteral(keyCode) + MpInputConstants.COMBO_SEPARATOR;
                    } else if (k is Key key) {
                        outStr += _internalConverter.GetKeyLiteral(key) + MpInputConstants.COMBO_SEPARATOR;
                    } else {
                        throw new NotImplementedException($"Unknown key type '{typeof(T)}'");
                    }
                }
                outStr = outStr.Remove(outStr.Length - MpInputConstants.COMBO_SEPARATOR.Length, MpInputConstants.COMBO_SEPARATOR.Length);
            }
            if (!string.IsNullOrEmpty(outStr)) {
                if (outStr.EndsWith(MpInputConstants.SEQUENCE_SEPARATOR)) {
                    outStr = outStr.Remove(outStr.Length - MpInputConstants.SEQUENCE_SEPARATOR.Length, MpInputConstants.SEQUENCE_SEPARATOR.Length);
                }
            }
            return outStr;
        }
        public IEnumerable<IEnumerable<T>> ConvertStringToKeySequence<T>(string keyStr) where T : Enum {
            var keyList = new List<List<T>>();
            if (string.IsNullOrEmpty(keyStr)) {
                return keyList;
            }

            var combos = keyStr.SplitNoEmpty(MpInputConstants.SEQUENCE_SEPARATOR);
            foreach (var c in combos) {
                var kl = c.SplitNoEmpty(MpInputConstants.COMBO_SEPARATOR);
                keyList.Add(new List<T>());
                foreach (var k in kl) {
                    T t_key = default(T);
                    if (typeof(T) == typeof(KeyCode) &&
                        _globalConverter.ConvertStringToKey(k) is KeyCode gk &&
                        gk != KeyCode.CharUndefined) {
                        t_key = (T)Convert.ChangeType(gk, Enum.GetUnderlyingType(typeof(T)));
                    } else if (typeof(T) == typeof(Key) &&
                                _internalConverter.ConvertStringToKey(k) is Key ik &&
                                ik != Key.None) {
                        t_key = (T)Convert.ChangeType(ik, Enum.GetUnderlyingType(typeof(T)));
                    }
                    keyList[keyList.Count - 1].Add(t_key);
                }
            }
            return keyList;
        }

        public IEnumerable<IEnumerable<string>> ConvertStringToKeyLiteralSequence(string keyStr) {
            // NOTE arbitrarily using avalonia keys as intermediary here

            var kseq = ConvertStringToKeySequence<Key>(keyStr);
            var lseq = new List<List<string>>();
            foreach (var kcombo in kseq) {
                var lcombo = new List<string>();
                foreach (var k in kcombo) {
                    lcombo.Add(_internalConverter.GetKeyLiteral(k));
                }
                lseq.Add(lcombo);
            }
            return lseq;
        }

        //public static string ConvertKeyStringToSendKeysString(string keyString) {
        //    // NOTE keyString should NOT be a sequence
        //    //if(keyString.Contains(",")) {
        //    //    throw new Exception($"keyString '{keyString}' is a sequence and SendKeys only handles one gesture, if seq necessary call multiple times w/ delay in between");
        //    //}

        //    var sb = new StringBuilder();
        //    string[] keySequences = keyString.Split(new string[] { MpKeyGestureHelper2.SEQUENCE_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
        //    for (int i = 0; i < keySequences.Length; i++) {
        //        string seq = keySequences[i].Trim();
        //        //string outStr = string.Empty;
        //        var keys = seq.Split(new string[] { MpKeyGestureHelper2.COMBO_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
        //        foreach (var key in keys) {
        //            switch (key) {
        //                case "Ctrl":
        //                    //outStr += "^";
        //                    sb.Append("^");
        //                    break;
        //                case "Shift":
        //                    //outStr += "+";
        //                    sb.Append("+");
        //                    break;
        //                case "Alt":
        //                    //outStr += "%";
        //                    sb.Append("%");
        //                    break;
        //                case "Space":
        //                    //outStr += " ";
        //                    sb.Append(" ");
        //                    break;
        //                case "Escape":
        //                    sb.Append("{ESC}");
        //                    break;
        //                case "Back":
        //                    sb.Append("{BACKSPACE}");
        //                    break;
        //                case "PageUp":
        //                    sb.Append("{PGUP}");
        //                    break;
        //                case "PageDown":
        //                case "Next":
        //                    sb.Append("{PGDOWN}");
        //                    break;
        //                case "Capital":
        //                    sb.Append("{CAPSLOCK}");
        //                    break;
        //                case "Return":
        //                    sb.Append("{ENTER}");
        //                    break;
        //                case "Home":
        //                case "End":
        //                case "Del":
        //                case "Delete":
        //                case "Enter":
        //                case "Tab":
        //                case "Left":
        //                case "Right":
        //                case "Up":
        //                case "Down":
        //                    sb.Append("{" + key.ToUpper() + "}");
        //                    //outStr += "{" + key.ToUpper() + "}";
        //                    break;
        //                default:
        //                    if (key.ToUpper().StartsWith(@"F") && key.Length > 1) {
        //                        string fVal = key.Substring(1, key.Length - 1);
        //                        try {
        //                            int val = Convert.ToInt32(fVal);
        //                            //outStr += "{F" + val + "}";
        //                            sb.Append("{F" + val + "}");
        //                        }
        //                        catch (Exception ex) {
        //                            MpConsole.WriteLine(@"ShortcutViewModel.SendKeys exception creating key: " + key + " with exception: " + ex);
        //                            //outStr += key.ToUpper();
        //                            sb.Append(key.ToUpper());
        //                            break;
        //                        }
        //                    } else {
        //                        //outStr += key.ToUpper();
        //                        sb.Append(key.ToUpper());
        //                    }
        //                    break;
        //            }
        //        }
        //    }
        //    return sb.ToString();
        //}

    }
}
