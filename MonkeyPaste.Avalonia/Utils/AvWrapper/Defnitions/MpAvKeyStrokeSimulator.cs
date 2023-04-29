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
            bool success = await SimulateKeyStrokeSequenceAsync(seq, holdDelay, releaseDelay);
            return success;
        }

        public async Task<bool> SimulateKeyStrokeSequenceAsync<T>(IEnumerable<IEnumerable<T>> gesture, int holdDelay = 0, int releaseDelay = 0) {
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
            IsSimulating = true;
            foreach (var combo in gesture) {
                foreach (var key in combo.Cast<KeyCode>()) {
                    UioHookResult result = _eventSimulator.SimulateKeyPress(key);
                    if (result != UioHookResult.Success) {
                        MpDebug.Break($"Error pressing key: '{key}' in seq: '{Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { key } })}' error: '{result}'");
                        //return false;
                    }
                }
                await Task.Delay(holdDelay);
                foreach (var key in combo.Cast<KeyCode>()) {
                    UioHookResult result = _eventSimulator.SimulateKeyRelease(key);
                    if (result != UioHookResult.Success) {
                        MpDebug.Break($"Error releasing key: '{key}' in seq: '{Mp.Services.KeyConverter.ConvertKeySequenceToString(new[] { new[] { key } })}' error: '{result}'");
                        //return false;
                    }
                }

                await Task.Delay(releaseDelay);
            }
            IsSimulating = false;
            MpConsole.WriteLine($"Key Gesture '{Mp.Services.KeyConverter.ConvertKeySequenceToString(gesture)}' successfully simulated");
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
        #endregion

        #region Commands
        #endregion
    }
}
