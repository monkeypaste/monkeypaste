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
        private const int _HOLD_DELAY_MS = 0;
        private const int _RELEASE_DELAY_MS = 0;
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIKeyStrokeSimulator Implementation

        public async Task<bool> SimulateKeyStrokeSequenceAsync(string keystr) {
            var seq = Mp.Services.KeyConverter.ConvertStringToKeySequence<KeyCode>(keystr);
            bool success = await SimulateKeyStrokeSequenceAsync(seq);
            return success;
        }

        public async Task<bool> SimulateKeyStrokeSequenceAsync<T>(IReadOnlyList<IReadOnlyList<T>> gesture) {
            if (typeof(T) != typeof(KeyCode)) {
                throw new NotSupportedException("Must be sharphook keycode");
            }
            if (gesture == null) {
                return true;
            }
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
            string gesture_label = Mp.Services.KeyConverter.ConvertKeySequenceToString(gesture);
            //await WaitAndStartSimulateAsync(gesture_label);
            var to_restore = ClearDownState();

            foreach (var combo in filtered_gesture) {
                combo.ForEach(y => SimulateKey(y, true));
                await Task.Delay(_HOLD_DELAY_MS);
                combo.ForEach(y => SimulateKey(y, false));
                await Task.Delay(_RELEASE_DELAY_MS);
            }

            RestoreDownState(to_restore);
            MpConsole.WriteLine($"Key Gesture '{gesture_label}' successfully simulated. ");
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

        private IEnumerable<KeyCode> ClearDownState() {
            List<KeyCode> downs = Mp.Services.KeyDownHelper.Downs.Cast<KeyCode>().ToList();
            downs.ForEach(x => SimulateKey(x, false));
            return downs;
        }

        private void RestoreDownState(IEnumerable<KeyCode> downs) {
            downs.ForEach(x => SimulateKey(x, true));
        }
        private async Task WaitAndStartSimulateAsync(string gesture_label) {
            if (Mp.Services.KeyDownHelper.DownCount > 0) {
                int this_sim_id = ++_waitCount;
                MpConsole.WriteLine($"Sim gesture '{gesture_label}' waiting at queue: {this_sim_id}...");
                while (_waitCount >= this_sim_id) {
                    if (Mp.Services.KeyDownHelper.DownCount > 0) {
                        await Task.Delay(100);
                    }
                    if (_waitCount > this_sim_id) {
                        // not next (wait extra 100 for next to start)
                        await Task.Delay(200);
                        continue;
                    }
                    _waitCount--;
                    MpConsole.WriteLine($"Sim gesture '{gesture_label}' waiting DONE");
                }
            }
        }


        #endregion

        #region Commands
        #endregion
    }
}
