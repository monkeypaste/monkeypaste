﻿// #region Globals

var LastTextChangedDelta = null;

// #endregion Globals

// #region Life Cycle

function initHistory() {
}

// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function historyUndo() {
	const was_enabled = globals.quill.isEnabled();
	if (!was_enabled) {
		globals.quill.enable(true);
	}
	globals.quill.history.undo();
	if (!was_enabled) {
		globals.quill.enable(false);
	}
}

function historyRedo() {
	const was_enabled = globals.quill.isEnabled();
	if (!was_enabled) {
		globals.quill.enable(true);
	}
	globals.quill.history.redo();
	if (!was_enabled) {
		globals.quill.enable(false);
	}
}

function addHistoryItem(delta) {
	if (LastTextChangedDelta == null) {
		LastTextChangedDelta = delta;
	} else {
		LastTextChangedDelta = LastTextChangedDelta.compose(delta);
	}
}
function clearLastDelta() {
	log('Delta log cleared. It was: ');
	log(LastTextChangedDelta);
	LastTextChangedDelta = null;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers