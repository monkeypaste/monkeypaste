// #region Globals



// #endregion Globals

// #region Life Cycle

function initAppend() {
	if (!isAppendNotifier()) {
		return;
	}
	getDisableAppendModeButtonElement().innerHTML = getSvgHtml('stop', null, false);
	document.addEventListener('selectionchange', onAppendDocumentSelectionChange, true);
	disablePauseAppend();
}

// #endregion Life Cycle

// #region Getters

function getAppendDocRange() {
	if (globals.ContentItemType == 'FileList') {
		const sel_rows = getSelectedFileItemIdxs();
		const sel_idx = sel_rows.length > 0 && globals.IsAppendManualMode ? sel_rows[0] : globals.IsAppendPreMode ? 0 : getFileListRowElements().length - 1;
		let sel_doc_range = getFileListItemDocRange(sel_idx);
		if (!sel_doc_range) {
			return { index: 0, length: 0, mode: 'pre' };
		}
		sel_doc_range.mode = (sel_doc_range.index == 0 || globals.IsAppendManualMode) && globals.IsAppendPreMode ? 'pre' : 'post';
		return sel_doc_range;
	}
	if (globals.IsAppendManualMode) {
		if (globals.IsAppendPreMode) {
			if (globals.IsAppendInsertMode) {
				return { index: globals.FixedAppendIdx, length: 0, mode:'inline' };
			} else {
				// NOTE need to make block range in terms of the leading block unless its the first
				// for dt to handle blocks since pre is just for before first block
				let line_start_idx = getLineStartDocIdx(globals.FixedAppendIdx);
				return { index: globals.FixedAppendIdx, length: 0, mode: line_start_idx == 0 ?'pre': 'post' };
			}
		} else {
			let sel = cleanDocRange(getDocSelection());
			if (globals.IsAppendInsertMode) {
				sel.mode = 'inline';
				return sel;
			} else {
				return { index: getLineEndDocIdx(sel.index + sel.length), length: 0, mode:'post' };
			}
		}		
	} else {
		if (globals.IsAppendPreMode) {
			if (globals.IsAppendInsertMode) {
				return { index: 0, length: 0, mode: 'inline' };
			} else {
				return { index: 0, length: 0, mode: 'pre' };
			}
		} else {
			if (globals.IsAppendInsertMode) {
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
	return globals.IsAppendInsertMode || globals.IsAppendLineMode;
}

function enableAppendMode(isAppendLine, fromHost = false) {
	let did_append_mode_change = globals.IsAppendLineMode != isAppendLine;

	if (isAppendLine) {
		globals.IsAppendLineMode = true;
		globals.IsAppendInsertMode = false;
	} else {
		globals.IsAppendLineMode = false;
		globals.IsAppendInsertMode = true;
	}

	getEditorElement().classList.add('append');
	if (!isSubSelectionEnabled()) {
		enableSubSelection();
	}	

	updatePasteAppendToolbarLabel();

	scrollToAppendIdx();

	if (!fromHost && did_append_mode_change) {
		onAppendStateChanged_ntf();
	}

	drawOverlay();
	log('append mode enabled. IsAppendNotifier: ' + isAppendNotifier());
}

function disableAppendMode(fromHost = false) {
	let did_append_mode_change = isAnyAppendEnabled();
	let did_manual_mode_change = globals.IsAppendManualMode;

	globals.IsAppendLineMode = false;
	globals.IsAppendInsertMode = false;
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
	let did_manual_mode_change = !globals.IsAppendManualMode;

	globals.IsAppendManualMode = true;
	updatePasteAppendToolbarLabel();
	scrollToAppendIdx();

	drawOverlay();
	log('append manual mode enabled. IsAppendNotifier: ' + isAppendNotifier());
	if (!fromHost && did_manual_mode_change) {
		onAppendStateChanged_ntf();
	}
}

function disableAppendManualMode(fromHost = false) {
	let did_manual_mode_change = globals.IsAppendManualMode;
	globals.IsAppendManualMode = false;

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
	const did_pause_append_change = !globals.IsAppendPaused;
	globals.IsAppendPaused = true;
	getPasteAppendPauseAppendButtonElement().innerHTML = getSvgHtml('play');

	if (!fromHost && did_pause_append_change) {
		onAppendStateChanged_ntf();
	}
}

function disablePauseAppend(fromHost = false) {
	const did_pause_append_change = globals.IsAppendPaused;
	globals.IsAppendPaused = false;
	getPasteAppendPauseAppendButtonElement().innerHTML = getSvgHtml('pause');

	if (!fromHost && did_pause_append_change) {
		onAppendStateChanged_ntf();
		// trigger content change to refresh clipboard to append buffer
		onContentChanged_ntf();
	}
}

function enablePreAppend(fromHost = false) {
	const did_pre_append_change = !globals.IsAppendPreMode;
	globals.IsAppendPreMode = true;
	globals.FixedAppendIdx = getDocSelection().index;

	if (!fromHost && did_pre_append_change) {
		onAppendStateChanged_ntf();
	}
}

function disablePreAppend(fromHost = false) {
	const did_pre_append_change = globals.IsAppendPreMode;
	globals.IsAppendPreMode = false;
	globals.FixedAppendIdx = -1;

	if (!fromHost && did_pre_append_change) {
		onAppendStateChanged_ntf();
	}
}
// #endregion State

// #region Actions

function updateAppendModeState(req, fromHost) {
	if (req == null) {
		if (isAnyAppendEnabled()) {
			disableAppendMode(fromHost);
		}
		return;
	}
	let new_append_range = cleanDocRange({ index: req.appendDocIdx, length: req.appendDocLength });

	let is_enabling = !isAnyAppendEnabled() && (req.isAppendLineMode || req.isAppendInsertMode);
	let is_disabling = isAnyAppendEnabled() && !req.isAppendLineMode && !req.isAppendInsertMode;
	let is_resuming = globals.IsAppendPaused && !req.isAppendPaused;
	let is_pausing = !globals.IsAppendPaused && req.isAppendPaused;
	let is_pre_changing = globals.IsAppendPreMode != req.isAppendPreMode;
	// NOTE manual mode update from host was disabled cause the state keeps getting enabled somehow
	// I think its an async msg problem, the design is supposed to only have 1 append state flag changed
	// at a time so host/editor stays in sync but something weird happens w/ hosts manual setting overriding
	// when it changes here. Also having a hotkey for it is confusing cause its not global like the others so 
	// just leaving it to a UI flag which HIDES whatever the problem is (i donn't know whats wrong)
	let is_manual_changing = globals.IsAppendManualMode != req.isAppendManualMode && !fromHost;

	let is_appending_data = !isNullOrEmpty(req.appendData);

	let is_append_range_changed =
		req.isAppendManualMode &&
		didSelectionChange(getAppendDocRange(), new_append_range);

	log(' ');
	log(`updateAppendFromHost changes:`);
	log('is_enabling: ' + is_enabling);
	log('is_disabling: ' + is_disabling);
	log('is_append_range_changed: ' + is_append_range_changed);
	log('cur append range: ', getAppendDocRange());
	log('new append range: ', new_append_range);
	log('is_appending_data: ' + is_appending_data);
	log('is_resuming: ' + is_resuming);
	log('is_pausing: ' + is_pausing);
	log('append_data: ' + req.appendData);
	log(' ');

	

	if (is_enabling) {
		enableAppendMode(req.isAppendLineMode, req.isAppendManualMode, fromHost);
	}
	if (is_disabling) {
		disableAppendMode(fromHost);
	}

	if (is_manual_changing) {
		if (req.isAppendManualMode) {
			enableAppendManualMode(fromHost);
		} else {
			disableAppendManualMode(fromHost);
		}
	}

	if (is_pre_changing) {
		if (req.isAppendPreMode) {
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
	if (is_appending_data) {
		appendContentData(req.appendData);
	}

	if (fromhost) {
		globals.IsAppendWIthDestFormattingEnabled = req.isAppendWIthDestFormattingEnabled;
	}
}

function scrollToAppendIdx() {
	//
	if (globals.IsAppendManualMode || globals.IsAppendInsertMode) {
		scrollDocRangeIntoView(getAppendDocRange());
	}
	scrollToEnd();
}

// #endregion Actions

// #region Event Handlers


// #endregion Event Handlers