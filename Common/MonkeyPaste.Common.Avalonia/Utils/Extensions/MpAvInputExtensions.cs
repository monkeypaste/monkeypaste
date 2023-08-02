using Avalonia.Input;
using SharpHook.Native;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvInputExtensions {
        #region Keyboard

        public static KeyModifiers ToAvKeyModifiers(this MpKeyModifierFlags kmf) {
            return (KeyModifiers)kmf;
        }
        public static bool IsModKey(this Key key) {
            return key.ToAvKeyModifiers() != KeyModifiers.None;
        }
        public static int GesturePriority(this Key kc) {
            switch (kc) {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    return 0;
                case Key.LeftAlt:
                case Key.RightAlt:
                    return 1;
                case Key.LWin:
                case Key.RWin:
                    return 2;
                case Key.LeftShift:
                case Key.RightShift:
                    return 3;
                default:
                    return 4;
            }
        }
        public static KeyModifiers ToAvKeyModifiers(this Key key) {
            switch (key) {
                case Key.LeftAlt:
                case Key.RightAlt:
                    return KeyModifiers.Alt;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    return KeyModifiers.Control;
                case Key.LeftShift:
                case Key.RightShift:
                    return KeyModifiers.Shift;
                case Key.LWin:
                case Key.RWin:
                    return KeyModifiers.Meta;
                default:
                    return KeyModifiers.None;
            }
        }
        public static Key GetUnifiedKey(this Key kc) {
            // Default to left for mods and numpad for arrows and numbers
            switch (kc) {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    return Key.LeftCtrl;
                case Key.LeftAlt:
                case Key.RightAlt:
                    return Key.LeftAlt;
                case Key.LWin:
                case Key.RWin:
                    return Key.LWin;
                case Key.LeftShift:
                case Key.RightShift:
                    return Key.LeftShift;
                default:
                    string keyStr = kc.ToString();
                    if (keyStr.StartsWith("NumPad")) {
                        return kc;
                    }
                    if (Enum.TryParse(typeof(Key), $"NumPad{keyStr.Replace("D", string.Empty)}", out object unified_numpad_kc_obj)) {
                        return (Key)unified_numpad_kc_obj;
                    }
                    return kc;
            }
        }
        public static KeyModifiers ToAvKeyModifiers(this IEnumerable<IEnumerable<Key>> seq) {
            KeyModifiers mods = KeyModifiers.None;
            foreach (var combo in seq) {
                foreach (var key in combo) {
                    mods |= key.ToAvKeyModifiers();
                }
            }
            return mods;
        }

        public static MpKeyModifierFlags ToPortableKeyModifierFlags(this KeyModifiers kmf) {
            return (MpKeyModifierFlags)kmf;
        }

        public static KeyStates ToAvKeyStates(this MpKeyStateFlags kmf) {
            return (KeyStates)kmf;
        }

        public static MpKeyStateFlags ToPortableKeyStateFlags(this KeyStates kmf) {
            return (MpKeyStateFlags)kmf;
        }

        public static RawInputModifiers ToAvRawInputModifiers(this MpRawInputModifierFlags kmf) {
            return (RawInputModifiers)kmf;
        }

        public static MpRawInputModifierFlags ToPortableRawInputModifierFlags(this RawInputModifiers kmf) {
            return (MpRawInputModifierFlags)kmf;
        }

        #endregion


        #region Pointer


        #endregion
    }
}
