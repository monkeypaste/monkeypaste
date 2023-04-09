// #region Globals

var IsHostFocused = null;
// #endregion Globals

// #region Life Cycle
function initInput() {
	initKeyboard();
	initMouse();
	initSelection();
}
// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

function setInputFocusable(isInputFocused) {
	if (isInputFocused != IsHostFocused) {
		log('host selection changed. IsHostFocused: ' + isInputFocused);
	}
	IsHostFocused = isInputFocused;
	if (isInputFocused && !document.hasFocus()) {
		window.focus();
		log('document focus attempted: ' + (document.hasFocus() ? "SUCCESS" : "FAILED"));
	}
	if (!IsHostFocused) {
		hideAllPopups();
	}
}

// #endregion Setters

// #region State

function isWindowFocused() {
	if (!isRunningInHost()) {
		return true;
	}
	if (document.hasFocus()) {
		if (IsHostFocused == null) {
			return true;
		}
		return IsHostFocused;
	}
	return false;
}
// #endregion State

// #region Actions

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers