// #region Globals

// #endregion Globals

// #region Life Cycle

function initPasteAppendToolbarItems() {
	hidePasteAppendToolbarLabelContainer();
}

// #endregion Life Cycle

// #region Getters

function getPasteAppendToolbarLabelContainerElement() {
	return document.getElementById('pasteAppendToolbarLabelContainer');
}

function getPasteAppendIsManualToolbarLabelElement() {
	return document.getElementById('pasteAppendIsManualToolbarLabel');
}
function getPasteAppendToolbarLabelElement() {
	return document.getElementById('pasteAppendToolbarLabel');
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isPasteAppendToolbarLabelContainerVisible() {
	return !getPasteAppendToolbarLabelContainerElement().classList.contains('hidden');
}

function isAppendManualModeAvailable() {
	let result = ContentItemType == 'Text';
	return result;
}

function isAppendInsertModeAvailable() {
	let result = ContentItemType == 'Text';
	return result;
}
// #endregion State

// #region Actions

function showPasteAppendToolbarLabelContainer() {
	getPasteAppendToolbarLabelContainerElement().classList.remove('hidden');
}
function hidePasteAppendToolbarLabelContainer() {
	getPasteAppendToolbarLabelContainerElement().classList.add('hidden');
}

function showPasteAppendIsManualToolbarLabel() {
	getPasteAppendIsManualToolbarLabelElement().classList.remove('hidden');
}
function hidePasteAppendIsManualToolbarLabel() {
	getPasteAppendIsManualToolbarLabelElement().classList.add('hidden');
}

function updatePasteAppendToolbarLabel() {
	if (!isAnyAppendEnabled()) {
		hidePasteAppendToolbarLabelContainer();
		return;
	}
	showPasteAppendToolbarLabelContainer();
	let append_label_text = 'INSERT';
	let append_label_class = 'insert-mode-label';
	if (IsAppendLineMode) {
		append_label_text = 'APPEND';
		append_label_class = 'append-mode-label';
	}
	getPasteAppendToolbarLabelElement().innerText = append_label_text;
	clearElementClasses(getPasteAppendToolbarLabelElement());
	getPasteAppendToolbarLabelElement().classList.add(append_label_class);

	if (IsAppendManualMode) {
		showPasteAppendIsManualToolbarLabel();
	} else {
		hidePasteAppendIsManualToolbarLabel();
	}
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers