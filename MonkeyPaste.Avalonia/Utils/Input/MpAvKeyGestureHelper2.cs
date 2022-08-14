using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Input;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using SharpHook;
using SharpHook.Native;

namespace MonkeyPaste.Avalonia {
    public class MpAvKeyGestureHelper2 {
        #region Private Variables
        public const string COMBO_SEPARATOR = "+";
        public const string SEQUENCE_SEPARATOR = "|";
        private const int _MAX_COMBOS = int.MaxValue;

        private int _downCount = 0;

        private string _curKeysDown = string.Empty;

        private string _currentGesture = String.Empty;

        private string _finalGesture = string.Empty;
        #endregion


        public void AddKeyDown(string key, bool isRepeat = false) {
            if (isRepeat) {
                return;
            }

            if(_downCount == 0) {
                // Sequences must be multiple keys where:
                // 1. only 1 key is changed between each combo ie. <Some Modifier Keys> + J, <Same modifier keys> + L

                // so if nothing is already down treat as a new gesture
               // Reset();
            }
            _downCount++;

            if(_curKeysDown.Contains(SEQUENCE_SEPARATOR)) {
                //shouldn't happen
                MpDebuggerHelper.Break();
            }

            AddPressedKey(key);
        }

        public void RemoveKeyDown(string key) {            
            _downCount--;
            if (IsModifierKey(key) || _downCount == 0) {
                // when modifier goes up or nothing else is down that's the end of the sequence no matter what
                _finalGesture = _currentGesture;
                return;
            }
            if (string.IsNullOrEmpty(_currentGesture)) {
                _currentGesture = _curKeysDown;
            } else if(_currentGesture.IndexListOfAll(SEQUENCE_SEPARATOR).Count + 1 < _MAX_COMBOS) {
                _currentGesture += SEQUENCE_SEPARATOR + _curKeysDown;
            }

            

            RemovePressedKey(key);
        }

        public bool IsGestureComplete() {
            return !string.IsNullOrEmpty(_finalGesture);
        }

        public string GetCurrentGesture() {
            if(!string.IsNullOrEmpty(_finalGesture)) {
                return _finalGesture;
            }
            if(!string.IsNullOrEmpty(_currentGesture)) {
                return _currentGesture;
            }
            return _curKeysDown;
        }

        public void Reset() {
            _currentGesture = string.Empty;
            _curKeysDown = String.Empty;
            _finalGesture = string.Empty;
            _downCount = 0;
        }

        private void AddPressedKey(string key) {
            var curDownParts = _curKeysDown.Split(new string[] { COMBO_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if(curDownParts.Any(x=>x.ToLower() == key.ToLower())) {
                //shouldn't happen
                //MpDebuggerHelper.Break();
                //MpDebuggerHelper.Break();
                return;
            }
            curDownParts.Add(key);
            _curKeysDown = String.Join(COMBO_SEPARATOR, curDownParts.OrderBy(x => GetPriority(x)));
        }

        private void RemovePressedKey(string key) {
            var curDownParts = _curKeysDown.Split(new string[] { COMBO_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries).ToList();
            string down_toRemove = curDownParts.FirstOrDefault(x => x.ToLower() == key.ToLower());
            if(string.IsNullOrEmpty(down_toRemove)) {
                //shouldn't happen
                //MpDebuggerHelper.Break();
                return;
            }
            curDownParts.Remove(down_toRemove);
            _curKeysDown = String.Join(COMBO_SEPARATOR, curDownParts.OrderBy(x => GetPriority(x)));
        }

        private int GetPriority(string key) {
            switch (key.ToLower()) {
                case "control":
                    return 0;
                case "alt":
                    return 1;
                case "shift":
                    return 2;
                default:
                    return 3;
            }
        }

        private bool IsModifierKey(string key) {
            switch (key.ToLower()) {
                case "control":
                case "alt":
                case "shift":
                    return true;
                default:
                    return false;
            }
        }
    }
}
