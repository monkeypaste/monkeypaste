using MonkeyPaste.Common;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvSharpHookKeyConverter : MpIKeyConverter<KeyCode> {
        public int GetKeyPriority(KeyCode key) {
            return key.GesturePriority();
        }
        public KeyCode ConvertStringToKey(string keyStr) {
            return keyStr.ToSharpHookKeyCode();
        }

        public string GetKeyLiteral(KeyCode key) {
            return key.GetKeyLiteral();
        }

    }
}
