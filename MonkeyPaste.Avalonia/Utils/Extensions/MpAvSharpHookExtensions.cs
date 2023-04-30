using MonkeyPaste.Common;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvSharpHookExtensions {
        public static MpPoint GetScaledScreenPoint(this MouseEventData med) {

            //double scale = MpPlatform.Services.ScreenInfoCollection.PixelScaling;
            double scale = MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling;
            var unscaled_p = new MpPoint((double)med.X, (double)med.Y);
            var scaled_p = new MpPoint(Math.Max(0, (double)med.X / scale), Math.Max(0, (double)med.Y / scale));

            return scaled_p;
        }

        public static int GesturePriority(this KeyCode kc) {
            switch (kc) {
                case KeyCode.VcLeftControl:
                case KeyCode.VcRightControl:
                    return 0;
                case KeyCode.VcLeftAlt:
                case KeyCode.VcRightAlt:
                    return 1;
                case KeyCode.VcLeftMeta:
                case KeyCode.VcRightMeta:
                    return 2;
                case KeyCode.VcLeftShift:
                case KeyCode.VcRightShift:
                    return 3;
                default:
                    return 4;
            }
        }

        public static bool IsModKey(this KeyCode kc) {
            switch (kc) {
                case KeyCode.VcLeftControl:
                case KeyCode.VcRightControl:
                case KeyCode.VcLeftAlt:
                case KeyCode.VcRightAlt:
                case KeyCode.VcLeftMeta:
                case KeyCode.VcRightMeta:
                case KeyCode.VcLeftShift:
                case KeyCode.VcRightShift:
                    return true;
                default:
                    return false;
            }
        }

        public static KeyCode GetUnifiedKey(this KeyCode kc) {
            // Default to left for mods and numpad for arrows and numbers
            switch (kc) {
                case KeyCode.VcLeftControl:
                case KeyCode.VcRightControl:
                    return KeyCode.VcLeftControl;
                case KeyCode.VcLeftAlt:
                case KeyCode.VcRightAlt:
                    return KeyCode.VcLeftAlt;
                case KeyCode.VcLeftMeta:
                case KeyCode.VcRightMeta:
                    return KeyCode.VcLeftMeta;
                case KeyCode.VcLeftShift:
                case KeyCode.VcRightShift:
                    return KeyCode.VcLeftShift;
                default:
                    string keyStr = kc.ToString();
                    if (keyStr.StartsWith("VcNumPad")) {
                        return kc;
                    }
                    if (Enum.TryParse(typeof(KeyCode), $"VcNumPad{keyStr.Replace("Vc", string.Empty)}", out object unified_numpad_kc_obj)) {
                        return (KeyCode)unified_numpad_kc_obj;
                    }
                    return kc;
            }
        }

        public static bool IsSameKey(this KeyCode kc, KeyCode okc, bool unify_mods) {
            if (kc == okc) {
                return true;
            }
            if (!unify_mods) {
                return false;
            }
            return kc.GetUnifiedKey() == okc.GetUnifiedKey();
        }

        public static bool IsMatch(this IReadOnlyList<IReadOnlyList<KeyCode>> gesture, IReadOnlyList<IReadOnlyList<KeyCode>> mv, bool unify_mods = false) {
            if (!gesture.StartsWith(mv, unify_mods) ||
                 gesture.Count != mv.Count) {
                return false;
            }
            foreach (var (mv_combo, mv_combo_idx) in mv.WithIndex()) {
                if (gesture[mv_combo_idx].Count != mv_combo.Count) {
                    return false;
                }
            }
            return true;
        }
        public static bool StartsWith(this IReadOnlyList<IReadOnlyList<KeyCode>> gesture, IReadOnlyList<IReadOnlyList<KeyCode>> mv, bool unify_mods = false) {
            if (gesture == null || mv == null) {
                return false;
            }
            foreach (var (mv_combo, mv_combo_idx) in mv.WithIndex()) {
                if (gesture.Count <= mv_combo_idx) {
                    //mv has more combos that gesture so no
                    return false;
                }
                foreach (var (mv_key, mv_key_idx) in mv_combo.WithIndex()) {
                    if (gesture[mv_combo_idx].Count <= mv_key_idx) {
                        //mv combo is longer than gestures combo at idx
                        return false;
                    }
                    if (!gesture[mv_combo_idx][mv_key_idx].IsSameKey(mv_key, unify_mods)) {
                        return false;
                    }
                }
            }
            return true;
        }

        public static List<List<KeyCode>> CloneGesture(this IReadOnlyList<IReadOnlyList<KeyCode>> gesture) {
            return
                gesture
                .Select(x => x.ToList())
                .ToList();
        }
    }
}
