// #region Globals

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
	if (isInputFocused != globals.IsHostFocused) {
		log('host selection changed. globals.IsHostFocused: ' + isInputFocused);
	}
	globals.IsHostFocused = isInputFocused;
	if (isInputFocused && !document.hasFocus()) {
		forceWindowFocus();
	}
	if (!globals.IsHostFocused) {
		hideAllPopups();

	}
}

// #endregion Setters

// #region State

function isWindowFocused() {
	if (!isRunningOnHost()) {
		return true;
	}
	if (document.hasFocus()) {
		if (globals.IsHostFocused == null) {
			return true;
		}
		return globals.IsHostFocused;
	}
	return false;
}
// #endregion State

// #region Actions

function forceWindowFocus() {
	window.focus();
	log('document focus attempted: ' + (document.hasFocus() ? "SUCCESS" : "FAILED"));
	if (!document.hasFocus()) {
		window.blur();
		window.focus();

		log('document focus (after window focus toggle): ' + (document.hasFocus() ? "SUCCESS" : "FAILED"));
		if (!document.hasFocus()) {
			let test = document.activeElement;
			console.table(test);
			if (!document.hasFocus()) {
				window.open('', globals.EDITOR_WINDOW_NAME).focus();
				log('document focus (after window.open): ' + (document.hasFocus() ? "SUCCESS" : "FAILED"));
			}
		}
	}
}

function ensureWindowFocus() {
	if (document.hasFocus()) {
		return;
	}
	forceWindowFocus();
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers