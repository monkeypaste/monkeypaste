using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
//using Avalonia.Win32;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvKeyStrokeSimulator : MpIKeyStrokeSimulator {

        #region Private Variable
        private EventSimulator _eventSimulator;
        private int _waitCount = 0;
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIKeyStrokeSimulator Implementation
        public bool IsSimulating { get; private set; }

        public async Task<bool> SimulateKeyStrokeSequenceAsync(string keystr, int holdDelay = 0, int releaseDelay = 0) {
            var seq = Mp.Services.KeyConverter.ConvertStringToKeySequence<KeyCode>(keystr);
            bool success = await SimulateKeyStrokeSequenceAsync(seq, null, null, holdDelay, releaseDelay);
            return success;
        }

        public async Task<bool> SimulateKeyStrokeSequenceAsync<T>(IReadOnlyList<IReadOnlyList<T>> gesture, IEnumerable<T> physical_downs = null, IEnumerable<T> sup_downs = null, int holdDelay = 0, int releaseDelay = 0) {
            //MpDebug.Assert(!IsSimulating, "Only 1 at a time");

            if (typeof(T) != typeof(KeyCode)) {
                throw new NotSupportedException("Must be sharphook keycode");
            }
            if (gesture == null ||
                !gesture.Any() ||
                !gesture.First().Any()) {
                // avoid setting simulate if nothing happens
                //MpConsole.WriteLine("No gesture to simulate");
                return true;
            }
            if (IsSimulating) {
                int this_sim_id = ++_waitCount;
                MpConsole.WriteLine($"Sim gesture '{Mp.Services.KeyConverter.ConvertKeySequenceToString(gesture)}' waiting at queue: {this_sim_id}...");
                while (_waitCount >= this_sim_id) {
                    if (IsSimulating) {
                        await Task.Delay(100);
                    }
                    if (_waitCount > this_sim_id) {
                        // not next
                        await Task.Delay(100);
                        continue;
                    }
                    IsSimulating = true;
                    _waitCount--;
                    MpConsole.WriteLine($"Sim gesture '{Mp.Services.KeyConverter.ConvertKeySequenceToString(gesture)}' waiting DONE");
                }
            }
            IsSimulating = true;

            foreach (var combo in gesture) {
                // DOWN (forward order)
                for (int i = 0; i < combo.Count; i++) {
                    if (combo[i] is KeyCode kc) {
                        if (physical_downs != null &&
                            physical_downs.Cast<KeyCode>().Contains(kc) &&
                            (sup_downs == null || (sup_downs != null && !sup_downs.Cast<KeyCode>().Contains(kc)))) {
                            continue;
                        }
                        SimulateKey(kc, true);
                    } else {

                    }
                }
                await Task.Delay(holdDelay);

                // UP (reverse order)
                for (int i = combo.Count - 1; i >= 0; i--) {
                    if (combo[i] is KeyCode kc) {
                        if (physical_downs != null &&
                            physical_downs.Cast<KeyCode>().Contains(kc)) {
                            continue;
                        }
                        if (sup_downs != null && sup_downs.Cast<KeyCode>().Contains(kc)) {
                            continue;
                        }
                        SimulateKey(kc, false);
                    }
                }

                await Task.Delay(releaseDelay);
            }
            IsSimulating = false;
            MpConsole.WriteLine($"Key Gesture '{Mp.Services.KeyConverter.ConvertKeySequenceToString(gesture)}' successfully simulated. Ignored '{Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { physical_downs })}' keys. Only pressed '{Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { sup_downs })}' keys");
            return true;

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

        #region Protected Methods
        #endregion

        #region Private Methods
        private void SimulateKey(KeyCode key, bool isDown) {
            //MpConsole.WriteLine($"SIM {(isDown ? "DOWN" : "UP")}: {key}");
            UioHookResult result =
                isDown ?
                _eventSimulator.SimulateKeyPress(key) :
                _eventSimulator.SimulateKeyRelease(key);

            if (result != UioHookResult.Success) {
                MpDebug.Break($"Error {(isDown ? "pressing" : "releasing")} key: '{key}' in seq: '{Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { key } })}' error: '{result}'");
                //return false;
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}
