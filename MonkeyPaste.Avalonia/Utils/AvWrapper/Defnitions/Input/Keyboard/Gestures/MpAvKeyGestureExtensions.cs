using Avalonia.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvKeyGestureExtensions {
        public static string ToKeyLiteral(this KeyGesture kg) {
            if (kg == null) {
                return string.Empty;
            }
            return Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] {
                kg.KeyModifiers.ToKeys().Union(new[]{kg.Key})
            });
        }

        public static IEnumerable<Key> ToKeys(this KeyModifiers km) {
            if (km.HasFlag(KeyModifiers.Alt)) {
                yield return Key.LeftAlt;
            }
            if (km.HasFlag(KeyModifiers.Control)) {
                yield return Key.LeftCtrl;
            }
            if (km.HasFlag(KeyModifiers.Shift)) {
                yield return Key.LeftShift;
            }
            if (km.HasFlag(KeyModifiers.Meta)) {
                yield return Key.LWin;
            }
            yield break;
        }
    }
}
