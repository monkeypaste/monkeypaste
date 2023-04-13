using Avalonia.Input;
using System.Collections;
using System.Collections.Generic;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvInputExtensions {
        #region Keyboard

        public static KeyModifiers ToAvKeyModifiers(this MpKeyModifierFlags kmf) {
            return (KeyModifiers)kmf;
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
