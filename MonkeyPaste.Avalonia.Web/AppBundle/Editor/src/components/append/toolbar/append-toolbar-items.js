// #region Globals

// #endregion Globals

// #region Life Cycle

function initPasteAppendToolbarItems() {
	hidePasteAppendToolbarLabelContainer();

	addClickOrKeyClickEventListener(getPasteAppendPauseAppendButtonElement(), onPauseAppendButtonClickOrKey);
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
function getPasteAppendPauseAppendButtonElement() {
	return document.getElementById('pasteAppendPauseAppendButton');
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isPasteAppendToolbarLabelContainerVisible() {
	return !getPasteAppendToolbarLabelContainerElement().classList.contains('hidden');
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
		hidePasteAppendIsManualToolbarLabel();
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

function onPauseAppendButtonClickOrKey(e) {
	if (IsAppendPaused) {
		disablePauseAppend();
	} else {
		enablePauseAppend();
	}
}

function onDisableAppendModeClickOrKey(e) {

}
// #endregion Event Handlers