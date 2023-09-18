// #region Globals



// #endregion Globals

// #region Life Cycle

function initAppend() {
	if (!isAnyAppendEnabled()) {
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
		const sel_idx = sel_rows.length > 0 && isAppendManualMode() ? sel_rows[0] : isAppendPreMode() ? 0 : getFileListRowElements().length - 1;
		let sel_doc_range = getFileListItemDocRange(sel_idx);
		if (!sel_doc_range) {
			return { index: 0, length: 0, mode: 'pre' };
		}
		sel_doc_range.mode = (sel_doc_range.index == 0 || isAppendManualMode()) && isAppendPreMode() ? 'pre' : 'post';
		return sel_doc_range;
	}
	if (isAppendManualMode()) {
		let sel = cleanDocRange(getDocSelection());
		if (isAppendPreMode()) {
			if (isAppendInsertMode()) {
				//return { index: globals.FixedAppendIdx, length: 0, mode:'inline' };
				return { index: sel.index, length: 0, mode:'inline' };
			} else {
				// NOTE need to make block range in terms of the leading block unless its the first
				// for dt to handle blocks since pre is just for before first block
				//let line_start_idx = getLineStartDocIdx(globals.FixedAppendIdx);
				//return { index: globals.FixedAppendIdx, length: 0, mode: line_start_idx == 0 ? 'pre' : 'post' };
				let line_start_idx = getLineStartDocIdx(sel.index);
				return { index: sel.index, length: 0, mode: line_start_idx == 0 ?'pre': 'post' };
			}
		} else {
			if (isAppendInsertMode()) {
				sel.mode = 'inline';
				return { index: sel.index + sel.length, length: 0, mode: 'inline' };
			} else {
				return { index: getLineEndDocIdx(sel.index + sel.length), length: 0, mode:'post' };
			}
		}		
	} else {
		if (isAppendPreMode()) {
			if (isAppendInsertMode()) {
				return { index: 0, length: 0, mode: 'inline' };
			} else {
				return { index: 0, length: 0, mode: 'pre' };
			}
		} else {
			if (isAppendInsertMode()) {
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

function isAnyAppendEnabled() {
	return getEditorContainerElement().classList.contains('append');
}
function isAppendLineMode() {
	return getEditorContainerElement().classList.contains('append-line');
}
function isAppendInsertMode() {
	return getEditorContainerElement().classList.contains('append-insert');
}
function isAppendManualMode() {
	return getEditorContainerElement().classList.contains('append-manual');
}
function isAppendPreMode() {
	return getEditorContainerElement().classList.contains('append-pre');
}
function isAppendPaused() {
	return getEditorContainerElement().classList.contains('append-paused');
}

function enableAppendMode(isAppendLine, fromHost = false) {
	let did_append_mode_change = isAppendLineMode() != isAppendLine;

	getEditorContainerElement().classList.add('append');
	if (isAppendLine) {
		getEditorContainerElement().classList.remove('append-insert');
		getEditorContainerElement().classList.add('append-line');
	} else {
		getEditorContainerElement().classList.remove('append-line');
		getEditorContainerElement().classList.add('append-insert');
	}

	if (!isSubSelectionEnabled()) {
		enableSubSelection();
	}	

	updatePasteAppendToolbar();

	scrollToAppendIdx();

	if (did_append_mode_change) {
		updateOverlayPad(true);
		if (!fromHost) {
			onAppendStateChanged_ntf();
		}		
	}

	drawOverlay();
	log('append mode enabled. IsAppendNotifier: ' + isAnyAppendEnabled());
}

function disableAppendMode(fromHost = false) {
	let did_append_mode_change = isAnyAppendEnabled();
	let did_manual_mode_change = isAppendManualMode();
	let did_paused_change = isAppendPaused();

	getEditorContainerElement().classList.remove('append');
	getEditorContainerElement().classList.remove('append-line');
	getEditorContainerElement().classList.remove('append-insert');
	getEditorContainerElement().classList.remove('append-pre');
	getEditorContainerElement().classList.remove('append-manual');
	getEditorContainerElement().classList.remove('append-paused');

	updatePasteAppendToolbar();

	if (did_append_mode_change || did_manual_mode_change || did_paused_change) {
		updateOverlayPad(false);
		if (!fromHost) {
			// BUG getting mismatches after disabling append
			// with file item content not matching data object
			// so forcing content update, maybe bad
			onContentChanged_ntf();
			onAppendStateChanged_ntf();
		}
	}
	drawOverlay();
	log('append mode disabled. IsAppendNotifier: ' + isAnyAppendEnabled());
}

function enableAppendManualMode(fromHost = false) {
	let did_manual_mode_change = !isAppendManualMode();
	getEditorContainerElement().classList.add('append-manual');

	scrollToAppendIdx();
	updatePasteAppendToolbar();

	drawOverlay();
	log('append manual mode enabled. IsAppendNotifier: ' + isAnyAppendEnabled());
	if (!fromHost && did_manual_mode_change) {
		onAppendStateChanged_ntf();
	}
}

function disableAppendManualMode(fromHost = false) {
	let did_manual_mode_change = isAppendManualMode();
	getEditorContainerElement().classList.remove('append-manual');

	if (isAnyAppendEnabled()) {
		setDocSelection(getAppendDocRange());
		scrollToAppendIdx();
	}

	log('append manual mode disabled. IsAppendNotifier: ' + isAnyAppendEnabled());

	updatePasteAppendToolbar();
	if (!fromHost && did_manual_mode_change) {
		onAppendStateChanged_ntf();
	}
}

function enablePauseAppend(fromHost = false) {
	const did_pause_append_change = !isAppendPaused();
	getEditorContainerElement().classList.add('append-paused');

	updatePasteAppendToolbar();
	if (!fromHost && did_pause_append_change) {
		onAppendStateChanged_ntf();
	}
}

function disablePauseAppend(fromHost = false) {
	const did_pause_append_change = isAppendPaused();
	getEditorContainerElement().classList.remove('append-paused');

	updatePasteAppendToolbar();
	if (!fromHost && did_pause_append_change) {
		onAppendStateChanged_ntf();
		// trigger content change to refresh clipboard to append buffer
		onContentChanged_ntf();
	}
}

function enablePreAppend(fromHost = false) {
	const did_pre_append_change = !isAppendPreMode();
	getEditorContainerElement().classList.add('append-pre');

	globals.FixedAppendIdx = getDocSelection().index;
	updatePasteAppendToolbar();
	if (!fromHost && did_pre_append_change) {
		onAppendStateChanged_ntf();
	}
}

function disablePreAppend(fromHost = false) {
	const did_pre_append_change = isAppendPreMode();
	getEditorContainerElement().classList.remove('append-pre');

	globals.FixedAppendIdx = -1;

	updatePasteAppendToolbar();
	if (!fromHost && did_pre_append_change) {
		onAppendStateChanged_ntf();
	}
}
// #endregion State

// #region Actions

function updateAppendModeStateFromHost(req, fromHost = false) {
	if (req == null) {
		if (isAnyAppendEnabled()) {
			disableAppendMode(fromHost);
		}
		return;
	}
	let new_append_range = cleanDocRange({ index: req.appendDocIdx, length: req.appendDocLength });

	let is_enabling = !isAnyAppendEnabled() && (req.isAppendLineMode || req.isAppendInsertMode);
	let is_disabling = isAnyAppendEnabled() && !req.isAppendLineMode && !req.isAppendInsertMode;
	let is_resuming = isAppendPaused() && !req.isAppendPaused;
	let is_pausing = !isAppendPaused() && req.isAppendPaused;
	let is_pre_changing = isAppendPreMode() != req.isAppendPreMode;
	// NOTE manual mode update from host was disabled cause the state keeps getting enabled somehow
	// I think its an async msg problem, the design is supposed to only have 1 append state flag changed
	// at a time so host/editor stays in sync but something weird happens w/ hosts manual setting overriding
	// when it changes here. Also having a hotkey for it is confusing cause its not global like the others so 
	// just leaving it to a UI flag which HIDES whatever the problem is (i donn't know whats wrong)
	let is_manual_changing = isAppendManualMode() != req.isAppendManualMode && !fromHost;

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
	//if (is_append_range_changed) {
	//	setDocSelection(new_append_range);
	//}
	if (is_appending_data) {
		appendContentData(req.appendData);
	}

	if (fromHost) {
		globals.isAppendWithDestFormattingEnabled = req.isAppendWithDestFormattingEnabled;
		// NOTE host blocks until this message is returned to avoid
		// collisions of (missing) quickly successive changes
		// ie. switching block/inline then immediatly toggling
		// pre mode and it only scrolls the preview line (state) doesn't change
		// because the host reports back old pre state AFTER internally changed so 
		// it unchanges
		appendStateChangeComplete_ntf();
	}
}

function scrollToAppendIdx() {
	if (globals.IsLoadingContent) {
		log('Warning! content not loaded, will wait till done to scroll to append idx');
		getEditorContainerElement().addEventListener('onContentLoaded', scrollToAppendIdx);
		return;
	}


	if (isAppendManualMode()) {
		// do default for manual
		scrollDocRangeIntoView(getAppendDocRange());
		return;
	} 
	if (isAppendPreMode()) {
		scrollToHome();
		return;
	}
	scrollToEnd();
}

// #endregion Actions

// #region Event Handlers


// #endregion Event Handlers