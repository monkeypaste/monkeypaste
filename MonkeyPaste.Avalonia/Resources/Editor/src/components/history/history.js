// #region Life Cycle

function initHistory() {
}

// #endregion Life Cycle

// #region Getters

function getLastTextChangedDataTransferMessage(label = 'Edited') {
	if (!hasTextChangedDelta()) {
		return null;
	}
	// output 'MpQuillDataTransferCompletedNotification'
	let dti_msg = {
		dataItems: [
			{
				format: globals.URI_LIST_FORMAT,
				data: JSON.stringify([`${globals.LOCAL_HOST_URL}/?type=CopyItem&handle=${globals.ContentHandle}`])
			}
		]
	};
	let edit_dt_msg = {
		changeDeltaJsonStr: toBase64FromJsonObj(JSON.stringify(globals.LastTextChangedDelta)),
		sourceDataItemsJsonStr: toBase64FromJsonObj(dti_msg),
		transferLabel: label
	};
	return edit_dt_msg;
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function hasTextChangedDelta() {
	return globals.LastTextChangedDelta != null;
}

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
	if (hasTextChangedDelta()) {
		globals.LastTextChangedDelta = globals.LastTextChangedDelta.compose(delta);
	} else {
		globals.LastTextChangedDelta = delta;
	}
}
function clearLastDelta() {
	log('Delta log cleared. It was: ');
	log(globals.LastTextChangedDelta);
	globals.LastTextChangedDelta = null;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers