using Avalonia.Input;
using DynamicData;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Avalonia {

    public class MpKeyGestureHelper<T> where T : struct {

        #region Private Variables

        private List<Tuple<T, DateTime>> _downChecker = new List<Tuple<T, DateTime>>();
        private List<T> _downs = new List<T>();
        private string _finalGesture = string.Empty;

        #endregion

        #region Properties
        public List<T> Downs =>
            _downs;
        public bool IsKeyUnificationEnabled { get; set; } = true;

        public bool IsKeyRepeatIgnored { get; set; } = true;

        public bool ResetAfterGesture { get; set; }
        #endregion

        #region Constructors
        #endregion

        #region Public Methods

        public void AddKeyDown(T key) {
            key = ResolveKey(key);

            if (_downs.Contains(key) &&
                IsKeyRepeatIgnored) {
                // ignore repeat
                return;
            }

            if (!string.IsNullOrEmpty(_finalGesture) && ResetAfterGesture) {
                Reset();
            }
            ValidateDown(key);

            _downs.Add(key);
        }

        public void RemoveKeyDown(T key) {
            key = ResolveKey(key);

            ValidateUp(key);

            string cur_gesture = ToLiteral(_downs);

            _downs.Remove(key);

            if (!IsModifierKey(key)) {
                // when modifier goes up or nothing else is down that's the end of the sequence no matter what
                _finalGesture = cur_gesture;
                return;
            }
        }

        public string GetCurrentGesture() {
            if (ResetAfterGesture && !string.IsNullOrEmpty(_finalGesture)) {
                return _finalGesture;
            }
            return ToLiteral(_downs);
        }

        public void ClearCurrentGesture() {
            Reset();
        }

        #endregion

        #region Private Methods

        private T ResolveKey(T key) {
            if (IsKeyUnificationEnabled) {
                if (key is Key avkey) {
                    key = (T)Convert.ChangeType(avkey.GetUnifiedKey(), Enum.GetUnderlyingType(typeof(T)));
                } else if (key is KeyCode shkey) {
                    key = (T)Convert.ChangeType(shkey.GetUnifiedKey(), Enum.GetUnderlyingType(typeof(T)));
                } else {
                    MpConsole.WriteTraceLine($"Unsupported key type '{key.GetType()}'", null);
                }
            }
            return key;
        }
        private int GetPriority(T key) {
            if (key is Key avkey) {
                return avkey.GesturePriority();
            }
            if (key is KeyCode shkey) {
                return shkey.GesturePriority();
            }
            return int.MaxValue;
        }

        private bool IsModifierKey(T key) {
            if (key is Key avkey) {
                return avkey.IsModKey();
            }
            if (key is KeyCode shkey) {
                return shkey.IsModKey();
            }
            return false;
        }

        private void Reset() {
            _finalGesture = string.Empty;
            _downs.Clear();
            _downChecker.Clear();
        }

        private void ValidateDown(T key) {
            var sb = new StringBuilder();
            if (_downChecker.Where(x => DateTime.Now - x.Item2 > TimeSpan.FromSeconds(15)) is IEnumerable<Tuple<T, DateTime>> dttl &&
                dttl.Any()) {
                //assumes won't be holding key down longer than 30 seconds
                //this may give false positives if breakpoint hit & resumed with key down
                sb.AppendLine($"Orphan downs detected by time delay. Removing: {string.Join(",", dttl.Select(x => ToLiteral(x.Item1)))}");
                dttl.ToList().ForEach(x => _downs.Remove(x.Item1));
                dttl.ToList().ForEach(x => _downChecker.Remove(x));
                ClearCurrentGesture();
            }
            if (_downChecker.Count != _downs.Count &&
                _downs.Cast<T>() is IEnumerable<T> dkcl &&
                _downChecker.Select(x => x.Item1) is IEnumerable<T> dckcl) {

                var diff = dkcl.Difference(dckcl);
                sb.AppendLine($"Orphan downs detected by count mismatch. Removing: {string.Join(",", diff.Select(x => ToLiteral(x)))}");
                diff.ToList().ForEach(x => _downs.Remove(x));
                diff.ToList().ForEach(x => _downChecker.Remove(_downChecker.FirstOrDefault(y => y.Item1.Equals(x))));
                ClearCurrentGesture();
            }
#if DEBUG
            if (sb.ToString() is string orphan_msg &&
                !string.IsNullOrEmpty(orphan_msg)) {
                sb.AppendLine($"Result: Downs: {_downs.Count} DownCheckers: {_downChecker.Count} Gesture: '{GetCurrentGesture()}'");
                orphan_msg = sb.ToString();
                MpConsole.WriteLine(orphan_msg);
                Mp.Services.NotificationBuilder.ShowMessageAsync(
                           title: $"Orphans Detected",
                           body: orphan_msg,
                           iconSourceObj: "KeyboardImage",
                           maxShowTimeMs: 10_000).FireAndForgetSafeAsync();
            }
#endif
            _downChecker.Add(new Tuple<T, DateTime>(key, DateTime.Now));
        }

        private void ValidateUp(T key) {
            if (_downChecker.FirstOrDefault(x => x.Item1.Equals(key)) is Tuple<T, DateTime> dt) {
                _downChecker.Remove(dt);
            }
        }

        private string ToLiteral(IEnumerable<T> keys) {
            return Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { keys });
        }
        private string ToLiteral(T key) {
            if (key is Key avkey) {
                return Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { avkey } });
            }
            if (key is KeyCode shkey) {
                return Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { shkey } });
            }
            return string.Empty;
        }
        #endregion
    }
}
