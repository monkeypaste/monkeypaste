// #region Globals

var IsAppendMode = false;
var IsAppendLineMode = false;
var AppendIdx = -1;

var IsFreeMode = false;
// #endregion Globals

// #region Life Cycle

function initAppend() {
	initPasteAppendToolbarItems();
}

// #endregion Life Cycle

// #region Getters

function getAppendIdx() {
	let sel = getDocSelection();
	if (sel && IsFreeMode) {
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

function enableAppendMode(isAppendLine) {
	if (isAppendLine) {
		IsAppendLineMode = true;
		IsAppendMode = false;
	} else {
		IsAppendLineMode = false;
		IsAppendMode = true;
	}
	getEditorElement().classList.add('append');
	enableSubSelection();

	if (!IsFreeMode) {
		setDocSelection({ index: getDocLength(), length: 0 });
	}

	drawOverlay();
	log('append mode enabled. IsAppendNotifier: ' + isAppendNotifier());
}

function disableAppendMode() {
	IsAppendLineMode = false;
	IsAppendMode = false;

	getEditorElement().classList.remove('append');
	drawOverlay();
	log('append mode disabled. IsAppendNotifier: ' + isAppendNotifier());
}

// #endregion State

// #region Actions

function scrollToAppendIdx() {
	scrollDocRangeIntoView({ index: getAppendIdx(), length: 0 });
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers