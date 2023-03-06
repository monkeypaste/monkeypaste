using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using SharpHook;
using SharpHook.Native;
using System;
using System.Collections.Generic;
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

        public async Task<bool> SimulateKeyStrokeSequenceAsync(string keystr, int holdDelay = 0, int releaseDelay = 0) {
            var seq = Mp.Services.KeyConverter.ConvertStringToKeySequence<KeyCode>(keystr);
            foreach (var combo in seq) {
                foreach (var key in combo) {
                    UioHookResult result = _eventSimulator.SimulateKeyPress(key);
                    if (result != UioHookResult.Success) {
                        throw new Exception($"Error pressing key: '{key}' in seq: '{keystr}' error: '{result}'");
                        //return false;
                    }
                }
                await Task.Delay(holdDelay);
                foreach (var key in combo) {
                    UioHookResult result = _eventSimulator.SimulateKeyRelease(key);
                    if (result != UioHookResult.Success) {
                        throw new Exception($"Error releasing key: '{key}' in seq: '{keystr}' error: '{result}'");
                        //return false;
                    }
                }

                await Task.Delay(releaseDelay);
            }
            MpConsole.WriteLine($"Key Gesture '{keystr}' successfully simulated");
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
