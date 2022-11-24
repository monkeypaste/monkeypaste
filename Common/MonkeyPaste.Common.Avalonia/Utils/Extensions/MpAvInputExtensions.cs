using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvInputExtensions {
        #region Keyboard

        public static KeyModifiers ToAvKeyModifiers(this MpKeyModifierFlags kmf) {
            return (KeyModifiers)kmf;
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
