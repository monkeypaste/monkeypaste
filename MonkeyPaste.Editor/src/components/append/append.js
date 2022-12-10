// #region Globals

var IsAppendMode = false;
var IsAppendLineMode = false;
var AppendIdx = -1;

var IsAppendManualMode = false;
// #endregion Globals

// #region Life Cycle

function initAppend() {
	if (!isAppendNotifier()) {
		return;
	}

	document.addEventListener('selectionchange', onAppendDocumentSelectionChange, true);
}

// #endregion Life Cycle

// #region Getters

function getAppendDocRange() {
	if (IsAppendManualMode) {
		return cleanDocRange(getDocSelection());
	}

	let append_idx = getDocLength();
	if (IsAppendMode) {
		append_idx = Math.max(0, append_idx - 1);
	}
	return {
		index: append_idx,
		length: 1
	};
}
// #endregion Getters

// #region Setters


// #endregion Setters

// #region State

function isAppendNotifier() {
	return window.location.search.toLowerCase().endsWith(APPEND_NOTIFIER_PARAMS.toLowerCase());
}

function isAppendee() {
	return isAnyAppendEnabled() && !isAppendNotifier();
}

function isAnyAppendEnabled() {
	return IsAppendMode || IsAppendLineMode;
}

function enableAppendMode(isAppendLine, isAppendManual, fromHost = false) {
	isAppendManual = isAppendManual == null ? IsAppendManualMode : isAppendManual;
	let did_append_mode_change = IsAppendLineMode != isAppendLine;
	let did_manual_mode_change = IsAppendManualMode != isAppendManual;

	if (isAppendLine) {
		IsAppendLineMode = true;
		IsAppendMode = false;
	} else {
		IsAppendLineMode = false;
		IsAppendMode = true;
	}

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
	IsAppendMode = false;
	// NOTE dont allow manual to notify here (regardless of source) to avoid double messages
	disableAppendManualMode(false);

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
// #endregion State

// #region Actions

function updateAppendModeState(
	isAppendLine,
	isAppend,
	isAppendManual,
	appendDocIdx,
	appendDocLength,
	appendData,
	fromHost) {
	let new_append_range = cleanDocRange({ index: appendDocIdx, length: appendDocLength });

	let is_enabling = !isAnyAppendEnabled() && (isAppendLine || isAppend);
	let is_disabling = isAnyAppendEnabled() && !isAppendLine && !isAppend;

	let is_updating_state =
		IsAppendLineMode != isAppendLine ||
		IsAppendMode != isAppend ||
		IsAppendManualMode != isAppendManual;

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
	log('append_data: ' + appendData);
	log(' ');

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
	//scrollDocRangeIntoView({ index: getAppendIdx(), length: 0 }, 0 , 15);
	if (IsAppendManualMode) {
		return;
	}
	scrollToEnd();
}

// #endregion Actions

// #region Event Handlers


// #endregion Event Handlers