// #region Globals

var IsHostSelected = false;

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
	if (isInputFocused != IsHostSelected) {
		log('host selection changed. IsHostSelected: ' + isInputFocused);
	}
	IsHostSelected = isInputFocused;
	if (IsHostSelected && !document.hasFocus()) {
		window.focus();
		log('document focus attempted: ' + (document.hasFocus() ? "SUCCESS" : "FAILED"));
	}

}

// #endregion Setters

// #region State

function isWindowFocused() {
	return document.hasFocus() && IsHostSelected;
}
// #endregion State

// #region Actions

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers