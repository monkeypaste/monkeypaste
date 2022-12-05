// #region Globals

var IsAppendMode = false;
var IsAppendLineMode = false;
var AppendIdx = -1;

var IsAppendManualMode = false;
// #endregion Globals

// #region Life Cycle

function initAppend() {
}

// #endregion Life Cycle

// #region Getters

function getAppendIdx() {
	let sel = getDocSelection();
	if (sel && IsAppendManualMode) {
		return sel.index + sel.length;
	}

	let eof_idx = getDocLength();
	if (IsAppendLineMode) {
		return eof_idx;
	}
	if (IsAppendMode) {
		return Math.max(0,eof_idx - 1);
	}
	return 0;
}
// #endregion Getters

// #region Setters


// #endregion Setters

// #region State

function isAppendNotifier() {
	return window.location.search.toLowerCase().endsWith(APPEND_NOTIFIER_PARAMS.toLowerCase());
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
		enableAppendManualMode();
	} else {
		disableAppendManualMode();
	}

	getEditorElement().classList.add('append');
	enableSubSelection();

	updatePasteAppendToolbarLabel();

	if (!fromHost && (did_append_mode_change || did_manual_mode_change)) {
		onAppendModeChanged_ntf();
	}

	drawOverlay();
	log('append mode enabled. IsAppendNotifier: ' + isAppendNotifier());
}

function disableAppendMode(fromHost = false) {
	let did_append_mode_change = isAnyAppendEnabled();
	let did_manual_mode_change = IsAppendManualMode;

	IsAppendLineMode = false;
	IsAppendMode = false;
	IsAppendManualMode = false;

	getEditorElement().classList.remove('append');
	updatePasteAppendToolbarLabel();

	if (!fromHost && (did_append_mode_change || did_manual_mode_change)) {
		onAppendModeChanged_ntf();
	}
	drawOverlay();
	log('append mode disabled. IsAppendNotifier: ' + isAppendNotifier());
}

function enableAppendManualMode(fromHost = false) {
	let did_manual_mode_change = !IsAppendManualMode;

	IsAppendManualMode = true;
	updatePasteAppendToolbarLabel();
	drawOverlay();
	log('append manual mode enabled. IsAppendNotifier: ' + isAppendNotifier());
	if (!fromHost && did_manual_mode_change) {
		onAppendModeChanged_ntf();
	}
}

function disableAppendManualMode(fromHost = false) {
	let did_manual_mode_change = IsAppendManualMode;
	IsAppendManualMode = false;

	setDocSelection({ index: getAppendIdx(), length: 0 });
	updatePasteAppendToolbarLabel();

	drawOverlay();
	log('append manual mode disabled. IsAppendNotifier: ' + isAppendNotifier());

	if (!fromHost && did_manual_mode_change) {
		onAppendModeChanged_ntf();
	}
}
// #endregion State

// #region Actions

function scrollToAppendIdx() {
	scrollDocRangeIntoView({ index: getAppendIdx(), length: 0 });
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers