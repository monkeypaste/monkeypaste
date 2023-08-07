using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private static readonly object _GestureLock = new object();
        private static readonly object _RestoreGestureLock = new object();
        #endregion

        #region Interfaces

        #region MpIKeyStrokeSimulator Implementation

        public async Task<bool> SimulateKeyStrokeSequenceAsync(
            string keystr, bool restoreDownState = true) {
            var seq = Mp.Services.KeyConverter.ConvertStringToKeySequence<KeyCode>(keystr);
            bool success = await SimulateKeyStrokeSequenceAsync(seq, restoreDownState);
            return success;
        }

        public async Task<bool> SimulateKeyStrokeSequenceAsync<T>(
            IReadOnlyList<IReadOnlyList<T>> gesture,
            bool restoreDownState = true) {
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

            try {
                await MpFifoAsyncQueue.WaitByConditionAsync(
                    lockObj: restoreDownState ? _RestoreGestureLock : _GestureLock,
                    waitWhenTrueFunc: () => {
                        // when hotkey pasting (or some shortcut driven keyboard macro), state needs to be restored so only wait for a prev gesture then clear/restore downs

                        // when just automating keys wait for no current downs to proceed
                        return restoreDownState ? false : Mp.Services.KeyDownHelper.Downs.Any();
                    },
                    wait_step: 100,
                    locked_step: 200,
                    enter_step: 300,
                    debug_label: gesture_label);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Key Gesture '{gesture_label}' simulation FAILED.", ex);
                return false;
            }

            // NOTE this is simplifying gesture to non-sequence since seq currently invalid
            KeyCode[] to_press = filtered_gesture.SelectMany(x => x).Distinct().ToArray();
            KeyCode[] to_hide = null;
            if (restoreDownState) {
                to_hide = Mp.Services.KeyDownHelper.Downs.Cast<KeyCode>().ToArray();
            }

            // temporarily release cur downs (Hide) not in this gesture
            SimulateKeys(to_hide, false);

            // press needed downs for this gesture
            SimulateKeys(to_press, true);

            // release needed downs for this gesture
            SimulateKeys(to_press, false);

            //restore hidden downs before this gesture
            SimulateKeys(to_hide, true);
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
        private void SimulateKeys(KeyCode[] keys, bool isDown) {
            if (keys == null || keys.Length == 0) {
                return;
            }
            for (int i = 0; i < keys.Length; i++) {
                UioHookResult result = SimulateKey(keys[i], isDown);
                MpDebug.Assert(result == UioHookResult.Success, "check log for key sim error");
            }
        }
        private UioHookResult SimulateKey(KeyCode key, bool isDown) {
            MpConsole.WriteLine($"SIM {(isDown ? "DOWN" : "UP")}: {key}");

            UioHookResult result =
                isDown ?
                _eventSimulator.SimulateKeyPress(key) :
                _eventSimulator.SimulateKeyRelease(key);

            if (result != UioHookResult.Success) {
                MpConsole.WriteLine($"Error {(isDown ? "pressing" : "releasing")} key: '{key}' in seq: '{Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { key } })}' error: '{result}'");
            }

            return result;
        }

        private IEnumerable<KeyCode> ClearDownState(IEnumerable<KeyCode> ignoredKeys) {
            // don't clear downs that are in gesture,
            // ex: like 'Control+Enter' simulates 'Control+V'
            // 'Control' will be returned to down when physical maybe upped, either way it shouldn't be simulated
            // or phys/sim will create mismatch until its pressed again

            // find down keys not ignored (ignored is what needs to be simulated)
            List<KeyCode> downs_to_clear = Mp.Services.KeyDownHelper.Downs.Cast<KeyCode>().Where(x => !ignoredKeys.Contains(x)).ToList();
            var false_downs = downs_to_clear.Where(x => SimulateKey(x, false) != UioHookResult.Success).ToList();
            // when up can't be simulated (pretty sure) that means the keyboard doesn't think its down so:
            // 1. Remove failures from downs since key hook is out of whack
            // 2. omit failures from result to be restored after gesture

            // 1
            //false_downs.ForEach(x => Mp.Services.KeyDownHelper.Remove(x));
            // 2
            //false_downs.ForEach(x => downs_to_clear.Remove(x));
            if (false_downs.Any()) {

            }
            return downs_to_clear;
        }

        private void RestoreDownState(IEnumerable<KeyCode> downs) {
            downs.ForEach(x => SimulateKey(x, true));
        }

        private async Task WaitAndStartSimulateAsync(string gesture_label) {
            if (Mp.Services.KeyDownHelper.Downs.IsNullOrEmpty()) {
                return;
            }

            int this_sim_id = ++_waitCount;
            MpConsole.WriteLine($"Sim gesture '{gesture_label}' waiting at queue: {this_sim_id}...");
            while (_waitCount >= this_sim_id) {
                if (Mp.Services.KeyDownHelper.Downs.Any()) {
                    await Task.Delay(100);
                }
                if (_waitCount > this_sim_id) {
                    // not next (wait extra 100 for next to start)
                    await Task.Delay(200);
                    continue;
                }
                // add wait cause repetetive pasting is leaking last gesture for some reason
                await Task.Delay(300);
                _waitCount--;
                MpConsole.WriteLine($"Sim gesture '{gesture_label}' waiting DONE");
            }
        }


        #endregion

        #region Commands
        #endregion
    }
}
