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

    public class MpAvKeyGestureHelper<TKeyStruct> where TKeyStruct : struct {

        #region Private Variables

        private List<Tuple<TKeyStruct, DateTime>> _downChecker = new List<Tuple<TKeyStruct, DateTime>>();
        private List<TKeyStruct> _downsSinceIdle = new List<TKeyStruct>();

        private List<TKeyStruct> _downs = new List<TKeyStruct>();
        private string _finalGesture = string.Empty;

        private static bool LOG_ORPHANS =
#if DEBUG && WINDOWS
            true;
#elif DEBUG
            true;
#else
            false;
#endif


        #endregion

        #region Properties
        public List<TKeyStruct> Downs =>
            _downs;
        public bool IsKeyUnificationEnabled { get; set; } = true;

        public bool IsKeyRepeatIgnored { get; set; } = true;

        public bool ResetAfterGesture { get; set; }
        #endregion

        #region Constructors
        public MpAvKeyGestureHelper() {

        }

        #endregion

        #region Public Methods

        public void AddKeyDown(TKeyStruct key) {
            key = ResolveKey(key);

            if (_downs.Contains(key) &&
                IsKeyRepeatIgnored) {
                // ignore repeat
                return;
            }
            //MpConsole.WriteLine($"SHK DOWN '{key}'");

            if (ResetAfterGesture && !string.IsNullOrEmpty(_finalGesture)) {
                Reset();
            }

            if (!ResetAfterGesture && _downs.Count == 0) {
                _downsSinceIdle.Clear();
            }
            ValidateDown(key);

            _downs.Add(key);
            _downsSinceIdle.Add(key);
        }

        public bool RemoveKeyDown(TKeyStruct key) {
            //MpConsole.WriteLine($"SHK UP '{key}'");
            key = ResolveKey(key);

            ValidateUp(key);

            string cur_gesture = ToLiteral(_downs);

            bool removed = _downs.Remove(key);

            if (!IsModifierKey(key)) {
                // when input key goes up that's the end of the sequence
                _finalGesture = cur_gesture;
            }
            return removed;
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

        private TKeyStruct ResolveKey(TKeyStruct key) {
            if (IsKeyUnificationEnabled) {
                if (key is Key avkey) {
                    key = (TKeyStruct)Convert.ChangeType(avkey.GetUnifiedKey(), Enum.GetUnderlyingType(typeof(TKeyStruct)));
                } else if (key is KeyCode shkey) {
                    key = (TKeyStruct)Convert.ChangeType(shkey.GetUnifiedKey(), Enum.GetUnderlyingType(typeof(TKeyStruct)));
                } else {
                    MpConsole.WriteTraceLine($"Unsupported key type '{key.GetType()}'", null);
                }
            }
            return key;
        }
        private int GetPriority(TKeyStruct key) {
            if (key is Key avkey) {
                return avkey.GesturePriority();
            }
            if (key is KeyCode shkey) {
                return shkey.GesturePriority();
            }
            return int.MaxValue;
        }

        private bool IsModifierKey(TKeyStruct key) {
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
            _downsSinceIdle.Clear();
        }

        private void ValidateDown(TKeyStruct key) {
            var sb = new StringBuilder();
            if (_downChecker.Where(x => DateTime.Now - x.Item2 > TimeSpan.FromSeconds(15))
                    is IEnumerable<Tuple<TKeyStruct, DateTime>> dttl &&
                dttl.Any()) {
                // when some down was at least 15 seconds ago..

                if (dttl.Where(x => string.IsNullOrEmpty(ToLiteral(x.Item1))).Select(x => x.Item1) is IEnumerable<TKeyStruct> emptyLiterals &&
                    emptyLiterals.Any()) {
                    MpDebug.Break($"What keys does it think these are?");
                }
                //assumes won't be holding key down longer than 30 seconds
                //this may give false positives if breakpoint hit & resumed with key down
                sb.AppendLine($"Orphan downs detected by time delay. Removing:");
                dttl.ForEach(x => sb.AppendLine($"'{ToLiteral(x.Item1)}'"));
                sb.AppendLine("Downs since idle:");
                _downsSinceIdle.ForEach(x => sb.AppendLine($"'{ToLiteral(x)}'"));


                dttl.ToList().ForEach(x => _downs.Remove(x.Item1));
                dttl.ToList().ForEach(x => _downChecker.Remove(x));
                ClearCurrentGesture();
            }
            if (_downChecker.Count != _downs.Count &&
                _downs.Cast<TKeyStruct>() is IEnumerable<TKeyStruct> dkcl &&
                _downChecker.Select(x => x.Item1) is IEnumerable<TKeyStruct> dckcl) {

                var diff = dkcl.Difference(dckcl);
                sb.AppendLine($"Orphan downs detected by count mismatch. Removing: {string.Join(",", diff.Select(x => ToLiteral(x)))}");
                diff.ToList().ForEach(x => _downs.Remove(x));
                diff.ToList().ForEach(x => _downChecker.Remove(_downChecker.FirstOrDefault(y => y.Item1.Equals(x))));
                ClearCurrentGesture();
            }

            if (LOG_ORPHANS) {
                // NOTE orphan ntf is annoying but need see if other plats do it too, windows is confirmed
                if (sb.ToString() is string orphan_msg &&
                    !string.IsNullOrEmpty(orphan_msg) &&
                    !orphan_msg.Contains("F12")) {
                    sb.AppendLine($"Result: Downs: {_downs.Count} DownCheckers: {_downChecker.Count} Gesture: '{GetCurrentGesture()}'");
                    orphan_msg = sb.ToString();
                    MpConsole.WriteLine(orphan_msg);
                    Mp.Services.NotificationBuilder.ShowMessageAsync(
                               title: $"Orphans Detected",
                               body: orphan_msg,
                               iconSourceObj: "KeyboardImage",
                               maxShowTimeMs: 10_000).FireAndForgetSafeAsync();
                }
            }
            _downChecker.Add(new Tuple<TKeyStruct, DateTime>(key, DateTime.Now));
        }

        private void ValidateUp(TKeyStruct key) {
            if (_downChecker.FirstOrDefault(x => x.Item1.Equals(key)) is Tuple<TKeyStruct, DateTime> dt) {
                _downChecker.Remove(dt);
            }
        }

        private string ToLiteral(IEnumerable<TKeyStruct> keys) {
            return Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { keys });
        }
        private string ToLiteral(TKeyStruct key) {
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
