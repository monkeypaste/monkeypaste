using System;
using System.Linq;

namespace MonkeyPaste.Common {

    public class MpKeyGestureHelper {
        #region Private Variables
        private int _MAX_COMBOS;

        private int _downCount = 0;

        private string _curKeysDown = string.Empty;

        private string _currentGesture = String.Empty;

        private string _finalGesture = string.Empty;

        #endregion

        #region Constructors

        public MpKeyGestureHelper() : this(1) { }
        public MpKeyGestureHelper(int maxCombos) {
            _MAX_COMBOS = maxCombos;
        }
        #endregion

        #region Public Methods

        public void AddKeyDown(string key, bool isRepeat = false) {
            if (isRepeat) {
                return;
            }

            if (_downCount == 0) {
                // Sequences must be multiple keys where:
                // 1. only 1 key is changed between each combo ie. <Some Modifier Keys> + J, <Same modifier keys> + L

                // so if nothing is already down treat as a new gesture
                Reset();
            }
            _downCount++;

            if (_curKeysDown.Contains(MpInputConstants.SEQUENCE_SEPARATOR)) {
                //shouldn't happen
                //MpDebuggerHelper.Break();
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
            } else if (_currentGesture.IndexListOfAll(MpInputConstants.SEQUENCE_SEPARATOR).Count + 1 < _MAX_COMBOS) {
                _currentGesture += MpInputConstants.SEQUENCE_SEPARATOR + _curKeysDown;
            }
            RemovePressedKey(key);
        }

        public bool IsGestureComplete() {
            return !string.IsNullOrEmpty(_finalGesture);
        }

        public string GetCurrentGesture() {
            if (!string.IsNullOrEmpty(_finalGesture)) {
                return _finalGesture;
            }
            if (!string.IsNullOrEmpty(_currentGesture)) {
                return _currentGesture;
            }
            return _curKeysDown;
        }

        public void ClearCurrentGesture() {
            Reset();
        }

        #endregion

        #region Private Methods

        private void AddPressedKey(string key) {
            var curDownParts = _curKeysDown.Split(new string[] { MpInputConstants.COMBO_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (curDownParts.Any(x => x.ToLower() == key.ToLower())) {
                //shouldn't happen
                //MpDebuggerHelper.Break();
                //MpDebuggerHelper.Break();
                return;
            }
            curDownParts.Add(key);
            _curKeysDown = String.Join(MpInputConstants.COMBO_SEPARATOR, curDownParts.OrderBy(x => GetPriority(x)));
        }

        private void RemovePressedKey(string key) {
            var curDownParts = _curKeysDown.Split(new string[] { MpInputConstants.COMBO_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries).ToList();
            string down_toRemove = curDownParts.FirstOrDefault(x => x.ToLower() == key.ToLower());
            if (string.IsNullOrEmpty(down_toRemove)) {
                //shouldn't happen
                //MpDebuggerHelper.Break();
                return;
            }
            curDownParts.Remove(down_toRemove);
            _curKeysDown = String.Join(MpInputConstants.COMBO_SEPARATOR, curDownParts.OrderBy(x => GetPriority(x)));
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

        private void Reset() {
            _currentGesture = string.Empty;
            _curKeysDown = String.Empty;
            _finalGesture = string.Empty;
            _downCount = 0;
        }

        #endregion
    }
}
