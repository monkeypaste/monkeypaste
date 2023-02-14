using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvKeyGestureHelper<T> where T : class {
        #region Private Variables

        private List<KeyValuePair<T, DateTime>> _downs = new List<KeyValuePair<T, DateTime>>();
        private List<KeyValuePair<T, DateTime>> _ups = new List<KeyValuePair<T, DateTime>>();

        private DateTime _lastUpTime;

        private int _downCount = 0;

        private Func<T, int> _getPriority;
        #endregion

        public string CurrentGesture { get; set; }

        public MpAvKeyGestureHelper(Func<T, int> getPriority) {
            _getPriority = getPriority;
        }

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
                                                d.Value < ups[i - 1].Value &&
                                                //d.Key.Co(ups[i - 1].Key) == 0).ToList();
                                                d.Key == ups[i - 1].Key).ToList();
                    for (int j = 0; j < downsToRemove.Count; j++) {
                        downs.Remove(downsToRemove[j]);
                    }
                }

                DateTime upTime = ups[i].Value;

                var combo = downs.Where(d => d.Value < upTime)
                                  .Select(d => d.Key)
                                  .ToList().OrderBy(x => _getPriority.Invoke(x));
                combos.Add(string.Join("+", combo.Select(x => x.ToString())));
                if (downs.All(d => d.Value < upTime)) {
                    break;
                }
            }

            if (combos.Count == 0 && downs.Count > 0 && _ups.Count == 0) {
                //mainly for realtime feedback
                combos.Add(string.Join("+", downs.Select(x => x.Key).OrderBy(x => _getPriority.Invoke(x))));
            }
            //if(typeof(T) == typeof(Key)) {
            //    return MpAvKeyboardInputHelpers.Convert(string.Join(", ", combos));
            //}
            //return MpSharpHookKeyboardInputHelpers.ConvertKeyStringToSendKeysString(string.Join(", ", combos));
            return string.Join(", ", combos);
        }

        public void AddKeyDown(T key, bool isRepeat = false) {
            if (isRepeat) {
                return;
            }
            if (_lastUpTime != DateTime.MaxValue && DateTime.Now - _lastUpTime > TimeSpan.FromSeconds(2)) {
                Reset();
            }

            _downs.Add(new KeyValuePair<T, DateTime>(key, DateTime.Now));
            _downCount++;

            CurrentGesture = GetGesture();
        }

        public void AddKeyUp(T key) {
            _ups.Add(new KeyValuePair<T, DateTime>(key, DateTime.Now));
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
    }
}
