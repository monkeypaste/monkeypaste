using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Common.Avalonia;
using SharpHook;
using SharpHook.Native;

namespace MonkeyPaste.Avalonia {
    public class MpKeyGestureHelper {
        #region Private Variables

        private List<KeyValuePair<KeyCode, DateTime>> _downs = new List<KeyValuePair<KeyCode, DateTime>>();
        private List<KeyValuePair<KeyCode, DateTime>> _ups = new List<KeyValuePair<KeyCode, DateTime>>();

        private DateTime _lastUpTime;

        private int _downCount = 0;

        #endregion

        public string CurrentGesture { get; set; }

        public string GetGesture() {
            if (_ups.Count == 0 && _downs.Count == 0) {
                return CurrentGesture;
            }
            var ups = _ups.Select(x => x).OrderBy(x => x.Value).ToList();
            var downs = _downs.Select(x => x).OrderBy(x => x.Value).ToList();

            List<string> combos = new List<string>();

            for (int i = 0; i < ups.Count(); i++) {
                if (i > 0) {
                    //remove last up from downs
                    var downsToRemove = downs.Where(d =>
                                                d.Value < ups[i - 1].Value
                                                && d.Key == ups[i - 1].Key
                                                ).ToList();
                    for (int j = 0; j < downsToRemove.Count; j++) {
                        downs.Remove(downsToRemove[j]);
                    }
                }

                DateTime upTime = ups[i].Value;

                var combo = downs.Where(d => d.Value < upTime)
                                  .Select(d => d.Key)
                                  .ToList().OrderBy(x => GetPriority(x));
                combos.Add(string.Join("+", combo.Select(x => x.ToString())));
                if (downs.All(d => d.Value < upTime)) {
                    break;
                }
            }

            if (combos.Count == 0 && downs.Count > 0 && _ups.Count == 0) {
                //mainly for realtime feedback
                combos.Add(string.Join("+", downs.Select(x => x.Key).OrderBy(x => GetPriority(x))));
            }

            return MpAvKeyboardInputHelpers.ConvertKeyStringToSendKeysString(string.Join(", ", combos));
        }

        public void AddKeyDown(KeyCode key, bool isRepeat = false) {
            if (isRepeat) {
                return;
            }
            if (_lastUpTime != DateTime.MaxValue && DateTime.Now - _lastUpTime > TimeSpan.FromSeconds(2)) {
                Reset();
            }

            _downs.Add(new KeyValuePair<KeyCode, DateTime>(key, DateTime.Now));
            _downCount++;

            CurrentGesture = GetGesture();
        }

        public void AddKeyUp(KeyCode key) {
            _ups.Add(new KeyValuePair<KeyCode, DateTime>(key, DateTime.Now));
            _downCount--;

            if (_downCount == 0) {
                CurrentGesture = GetGesture();
            }
            _lastUpTime = DateTime.Now;
        }

        public void Reset() {
            CurrentGesture = string.Empty;
            _ups.Clear();
            _downs.Clear();
            _lastUpTime = DateTime.MaxValue;
        }

        private int GetPriority(KeyCode key) {
            switch (key) {
                case KeyCode.VcLeftControl:
                    return 0;
                case KeyCode.VcRightControl:
                    return 1;
                //case KeyCode.System:
                case KeyCode.VcLeftAlt:
                    return 2;
                case KeyCode.VcRightAlt:
                    return 3;
                case KeyCode.VcLeftShift:
                    return 4;
                case KeyCode.VcRightShift:
                    return 5;
                default:
                    return 6;
            }
        }
    }
}
