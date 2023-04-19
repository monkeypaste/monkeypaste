// #region Globals

var IsAppendInsertMode = false;
var IsAppendLineMode = false;

var IsAppendManualMode = false;
var IsAppendPreMode = false;
var IsAppendPaused = false;

var FixedAppendIdx = -1;

// #endregion Globals

// #region Life Cycle

function initAppend() {
	if (!isAppendNotifier()) {
		return;
	}

	document.addEventListener('selectionchange', onAppendDocumentSelectionChange, true);
	disablePauseAppend();
}

// #endregion Life Cycle

// #region Getters

function getAppendDocRange() {

	if (IsAppendManualMode) {
		if (IsAppendPreMode) {
			if (IsAppendInsertMode) {
				return { index: FixedAppendIdx, length: 0, mode:'inline' };
			} else {
				// NOTE need to make block range in terms of the leading block unless its the first
				// for dt to handle blocks since pre is just for before first block
				let line_start_idx = getLineStartDocIdx(FixedAppendIdx);
				//line_start_idx = Math.max(0, line_start_idx - 1);
				return { index: FixedAppendIdx, length: 0, mode: line_start_idx == 0 ?'pre': 'post' };
			}
		} else {
			let sel = cleanDocRange(getDocSelection());
			if (IsAppendInsertMode) {
				sel.mode = 'inline';
				return sel;
			} else {
				//let line_end_idx = getLineEndDocIdx(FixedAppendIdx);
				//line_end_idx = Math.max(0, line_start_idx - 1);
				return { index: getLineEndDocIdx(sel.index + sel.length), length: 0, mode:'post' };
			}
		}		
	} else {
		if (IsAppendPreMode) {
			if (IsAppendInsertMode) {
				return { index: 0, length: 0, mode: 'inline' };
			} else {
				return { index: 0, length: 0, mode: 'pre' };
			}
		} else {
			if (IsAppendInsertMode) {
				return { index: Math.max(0, getDocLength() - 1), length: 1, mode: 'inline' };
			} else {
				return { index: getDocLength(), length: 1, mode: 'post' };
			}
		}
	}
}
// #endregion Getters

// #region Setters
// #endregion Setters

// #region State


function isAppendNotifier() {
	//return window.location.search.toLowerCase().endsWith(APPEND_NOTIFIER_PARAMS.toLowerCase());
	let result = getEditorElement().classList.contains('append');
	return result;
}


function isAnyAppendEnabled() {
	return IsAppendInsertMode || IsAppendLineMode;
}

function enableAppendMode(isAppendLine, isAppendManual, fromHost = false) {
	isAppendManual = isAppendManual == null ? IsAppendManualMode : isAppendManual;
	let did_append_mode_change = IsAppendLineMode != isAppendLine;
	let did_manual_mode_change = IsAppendManualMode != isAppendManual;

	if (isAppendLine) {
		IsAppendLineMode = true;
		IsAppendInsertMode = false;
	} else {
		IsAppendLineMode = false;
		IsAppendInsertMode = true;
	}
	IsAppendPreMode = false;

	// handle all msgs here not in manual 
	if (isAppendManual) {
		enableAppendManualMode(false);
	} else {
		disableAppendManualMode(false);
	}

	getEditorElement().classList.add('append');
	enableSubSelection();

	updatePasteAppendToolbarLabel();

	scrollToAppendIdx();

	if (!fromHost && (did_append_mode_change || did_manual_mode_change)) {
		onAppendStateChanged_ntf();
	}

	drawOverlay();
	log('append mode enabled. IsAppendNotifier: ' + isAppendNotifier());
}

function disableAppendMode(fromHost = false) {
	let did_append_mode_change = isAnyAppendEnabled();
	let did_manual_mode_change = IsAppendManualMode;

	IsAppendLineMode = false;
	IsAppendInsertMode = false;
	// NOTE dont allow manual to notify here (regardless of source) to avoid double messages
	disableAppendManualMode(false);
	disablePauseAppend(false);

	getEditorElement().classList.remove('append');
	updatePasteAppendToolbarLabel();

	if (isReadOnly()) {
		disableSubSelection();
	}

	if (!fromHost && (did_append_mode_change || did_manual_mode_change)) {
		onAppendStateChanged_ntf();
	}
	drawOverlay();
	log('append mode disabled. IsAppendNotifier: ' + isAppendNotifier());
}

function enableAppendManualMode(fromHost = false) {
	let did_manual_mode_change = !IsAppendManualMode;

	IsAppendManualMode = true;
	updatePasteAppendToolbarLabel();
	scrollToAppendIdx();

	drawOverlay();
	log('append manual mode enabled. IsAppendNotifier: ' + isAppendNotifier());
	if (!fromHost && did_manual_mode_change) {
		onAppendStateChanged_ntf();
	}
}

function disableAppendManualMode(fromHost = false) {
	let did_manual_mode_change = IsAppendManualMode;
	IsAppendManualMode = false;

	updatePasteAppendToolbarLabel();

	if (isAnyAppendEnabled()) {
		setDocSelection(getAppendDocRange());
		scrollToAppendIdx();
	}

	drawOverlay();
	log('append manual mode disabled. IsAppendNotifier: ' + isAppendNotifier());

	if (!fromHost && did_manual_mode_change) {
		onAppendStateChanged_ntf();
	}
}

function enablePauseAppend(fromHost = false) {
	const did_pause_append_change = !IsAppendPaused;
	IsAppendPaused = true;
	getPasteAppendPauseAppendButtonElement().innerHTML = getSvgHtml('play', null,false);

	if (!fromHost && did_pause_append_change) {
		onAppendStateChanged_ntf();
	}
}

function disablePauseAppend(fromHost = false) {
	const did_pause_append_change = IsAppendPaused;
	IsAppendPaused = false;
	getPasteAppendPauseAppendButtonElement().innerHTML = getSvgHtml('pause', null, false);

	if (!fromHost && did_pause_append_change) {
		onAppendStateChanged_ntf();
		// trigger content change to refresh clipboard to append buffer
		onContentChanged_ntf();
	}
}

function enablePreAppend(fromHost = false) {
	const did_pre_append_change = !IsAppendPreMode;
	IsAppendPreMode = true;
	FixedAppendIdx = getDocSelection().index;

	if (!fromHost && did_pre_append_change) {
		onAppendStateChanged_ntf();
	}
}

function disablePreAppend(fromHost = false) {
	const did_pre_append_change = IsAppendPreMode;
	IsAppendPreMode = false;
	FixedAppendIdx = -1;

	if (!fromHost && did_pre_append_change) {
		onAppendStateChanged_ntf();
	}
}
// #endregion State

// #region Actions

function updateAppendModeState(
	isAppendLine,
	isAppend,
	isAppendManual,
	isAppendStatePaused,
	isAppendPre,
	appendDocIdx,
	appendDocLength,
	appendData,
	fromHost) {
	let new_append_range = cleanDocRange({ index: appendDocIdx, length: appendDocLength });

	let is_enabling = !isAnyAppendEnabled() && (isAppendLine || isAppend);
	let is_disabling = isAnyAppendEnabled() && !isAppendLine && !isAppend;
	let is_resuming = IsAppendPaused && !isAppendStatePaused;
	let is_pausing = !IsAppendPaused && isAppendStatePaused;
	let is_pre_changing = IsAppendPreMode != isAppendPre;

	let is_updating_state =
		IsAppendLineMode != isAppendLine ||
		IsAppendInsertMode != isAppend ||
		IsAppendManualMode != isAppendManual ||
		IsAppendPaused != isAppendStatePaused ||
		IsAppendPreMode != isAppendPre;

	let is_appending_data = !isNullOrEmpty(appendData);

	let is_append_range_changed =
		isAppendManual &&
		didSelectionChange(getAppendDocRange(), new_append_range);

	log(' ');
	log(`updateAppendFromHost changes:`);
	log('is_enabling: ' + is_enabling);
	log('is_disabling: ' + is_disabling);
	log('is_updating_state: ' + is_updating_state);
	log('is_append_range_changed: ' + is_append_range_changed);
	log('cur append range: ', getAppendDocRange());
	log('new append range: ', new_append_range);
	log('is_appending_data: ' + is_appending_data);
	log('is_resuming: ' + is_resuming);
	log('is_pausing: ' + is_pausing);
	log('append_data: ' + appendData);
	log(' ');

	if (is_pre_changing) {
		if (isAppendPre) {
			enablePreAppend(fromHost);
		} else {
			disablePreAppend(fromHost);
		}
	}
	if (is_pausing) {
		enablePauseAppend(fromHost);
	}
	if (is_resuming) {
		disablePauseAppend(fromHost);
	}
	if (is_append_range_changed) {
		setDocSelection(new_append_range);
	}

	if (is_enabling || is_updating_state) {
		enableAppendMode(isAppendLine, isAppendManual, fromHost);
	}
	if (is_disabling) {
		disableAppendMode(fromHost);
	}

	if (is_appending_data) {
		appendContentData(appendData);
	}
}

function scrollToAppendIdx() {
	//
	if (IsAppendManualMode || IsAppendInsertMode) {
		scrollDocRangeIntoView(getAppendDocRange());
	}
	scrollToEnd();
}

// #endregion Actions

// #region Event Handlers


// #endregion Event Handlers