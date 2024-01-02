using MonkeyPaste.Common;
using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
//using Avalonia.Win32;

namespace MonkeyPaste.Avalonia {
    public class MpAvKeyStrokeSimulator : MpIKeyStrokeSimulator {
        #region Private Variable
        private object _simLock = new object();
        private EventSimulator _eventSimulator;
        private List<KeyCode> _simualtedDowns = new List<KeyCode>();
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIKeyStrokeSimulator Implementation
        public bool IsSimulatingKey<T>(T key) {
            if (key is not KeyCode kc) {
                throw new NotSupportedException("Must be sharphook keycode");
            }
            return _simualtedDowns.Contains(kc.GetUnifiedKey());
        }
        public bool SimulateKeyStrokeSequence(string keystr) {
            var seq = Mp.Services.KeyConverter.ConvertStringToKeySequence<KeyCode>(keystr);
            bool success = SimulateKeyStrokeSequence(seq);
            return success;
        }

        public bool SimulateKeyStrokeSequence<T>(IReadOnlyList<IReadOnlyList<T>> gesture) {
            if (typeof(T) != typeof(KeyCode)) {
                throw new NotSupportedException("Must be sharphook keycode");
            }
            if (gesture == null) {
                return true;
            }
            if (gesture.Count > 1) {
                throw new NotImplementedException("Only single gestures currently supported");
            }
            lock (_simLock) {
                // NOTE retain combo order and key priority order
                // NOTE2 remove keys that are physically down,
                // the simulate up will interfere w/ the keyboard state

                var filtered_gesture =
                    gesture
                        .Where(x => x != null && x.Any())
                        .OrderBy(x => gesture.IndexOf(x))
                        .Select(x => x
                            .Cast<KeyCode>()
                            .OrderBy(z => z.GesturePriority())
                            .ToList())
                        .ToList();

                if (filtered_gesture == null ||
                    !filtered_gesture.Any() ||
                    !filtered_gesture.First().Any()) {
                    return true;
                }

                // NOTE this is simplifying gesture to non-sequence since seq currently invalid
                // ex. monkey copy from ctrl+shift+c

                // downs will ctrl+shift
                KeyCode[] already_downs = Mp.Services.KeyDownHelper.Downs.Cast<KeyCode>().Select(x => x.GetUnifiedKey()).Distinct().ToArray();
                // gesture will be ctrl+c
                var gesture_keys = filtered_gesture.SelectMany(x => x).Select(x => x.GetUnifiedKey()).Distinct();
                // to release will be shift
                KeyCode[] to_release = already_downs.Where(x => !gesture_keys.Contains(x)).ToArray();
                // to press will be c
                KeyCode[] to_press = gesture_keys.Where(x => !already_downs.Contains(x)).ToArray();
                // to clear will be ctrl+shift+c
                KeyCode[] to_clear = gesture_keys.Union(gesture_keys).Distinct().ToArray();
                bool success = true;
                try {
                    // temporarily release cur downs (Hide) not in this gesture
                    SimulateKeys(to_release, false);

                    // press needed downs for this gesture
                    SimulateKeys(to_press, true);

                    // release ALL keys involved, empty keyboard so nothing gets stuck
                    SimulateKeys(to_clear, false);

                    // restore downs
                    // NOTE!!! restoring physical downs DOES NOT WORK, don't try it!
                    // i think under the hood it doesn't associate the sim down w/
                    // the eventual physical up, the physical up is just a dead event 
                    // like a timestamp mismatch where it knows when the physical down
                    // happened and since that doesn't match (or is older) the simulated
                    // press it just ignores it
                    //SimulateKeys(to_release, true);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine(string.Empty, ex);
                    success = false;
                }

                _simualtedDowns.Clear();
                string gesture_label = Mp.Services.KeyConverter.ConvertKeySequenceToString(gesture);
                MpConsole.WriteLine($"Key Gesture '{gesture_label}' {(success ? "was successful" : "failed")}. Sim Downs: {string.Join(",", _simualtedDowns)} ");
                return success;
            }

        }
        #endregion

        #endregion

        #region Properties

        #endregion

        #region Constructors
        public MpAvKeyStrokeSimulator() {
            _eventSimulator = new EventSimulator();
        }
        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void SimulateKeys(KeyCode[] keys, bool isDown) {
            if (keys == null || keys.Length == 0) {
                return;
            }
            for (int i = 0; i < keys.Length; i++) {
                UioHookResult result = SimulateKey(keys[i], isDown);
                if (result != UioHookResult.Success) {
                    throw new Exception("check log for key sim error");
                }
            }
        }
        private UioHookResult SimulateKey(KeyCode key, bool isDown) {
            MpConsole.WriteLine($"SIM {(isDown ? "DOWN" : "UP")}: {key}");

            UioHookResult result;
            if (isDown) {
                // NOTE adding is down BEFORE simulating

                _simualtedDowns.Add(key);
                result = _eventSimulator.SimulateKeyPress(key);
            } else {
                result = _eventSimulator.SimulateKeyRelease(key);
                // NOTE removing is down AFTER simulating

                _simualtedDowns.Remove(key);
            }

            if (result != UioHookResult.Success) {
                MpConsole.WriteLine($"Error {(isDown ? "pressing" : "releasing")} key: '{key}' in seq: '{Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { key } })}' error: '{result}'");
            }

            return result;
        }
        #endregion
    }
}
