// #region Globals

var DefaultEditorWidth = 1200;

var IgnoreNextTextChange = false;
var IgnoreNextSelectionChange = false;

var IsSubSelectionEnabled = false;

var EditorTheme = 'light';

var IsReadOnly = false;

// #endregion Globals

// #region Life Cycle

function initEditor() {
	if (IsLoaded) {
		log('editor already initialized, ignoring init');
		return;
	}

	initQuill();
	initEditorScroll();

	//quill.on("selection-change", onEditorSelectionChanged);
	quill.on("text-change", onEditorTextChanged);

	getEditorElement().addEventListener('focus', onEditorFocus);
	getEditorElement().addEventListener('blur', onEditorBlur);
}
// #endregion Life Cycle

// #region Getters

function getTotalHeight() {
	var totalHeight =
		getEditorToolbarHeight() + getEditorHeight() + getTemplateToolbarsHeight();
	return totalHeight;
}

function getEditorContainerElement() {
	return document.getElementById("editor");
}

function getEditorElement() {
	return document.getElementById("quill-editor");
}

function getEditorContainerRect() {
	let editor_container_rect = getEditorContainerElement().getBoundingClientRect();
	editor_container_rect = cleanRect(editor_container_rect);
	return editor_container_rect;
}

function getEditorSelection(isForPaste = false) {
	let sel = getDocumentSelection();
	if (isForPaste && (!sel || (sel && sel.length == 0))) {
		return { index: 0, length: getDocLength() };
	}

	return sel;
}

function getEditorWidth() {
	var editorRect = getEditorElement().getBoundingClientRect();
	//var editorHeight = parseInt($('.ql-editor').wi());
	return editorRect.width;
}

function getEditorHeight() {
	var editorRect = getEditorElement().getBoundingClientRect();
	//var editorHeight = parseInt($('.ql-editor').outerHeight());
	return editorRect.height;
}

// #endregion Getters

// #region Setters

function setEditorSelection(doc_idx, len, source = 'user') {
	//getEditorContainerElement().style.userSelect = 'auto';
	LastSelRange = { index: doc_idx, length: len };
	quill.setSelection(doc_idx, len, source);
	if (source == 'silent') {
		onEditorSelectionChanged_ntf({ index: doc_idx, length: len });
	}
}

// #endregion Setters

// #region State

function isShowingEditorToolbar() {
	$(".ql-toolbar").css("display") != "none";
}

function isAllSelected() {
	// NOTE doc length is never 0 and there's always an extra unselectable \n character at end so minus 1 for length to check here
	let doc_len = getDocLength() - 1;
	let sel = getEditorSelection_safe();
	let result = sel.index == 0 && sel.length == doc_len;
	return result;
}

function isNoneSelected() {
	let sel = getEditorSelection();
	return !sel || sel.length == 0;
}

function isContentEditable() {
	let isEditable = parseBool(getEditorElement().getAttribute('contenteditable'));
	return isEditable;
}

function isReadOnly() {
	return !isEditorToolbarVisible();
}

function isEditorElement(elm) {
	if (elm instanceof HTMLElement) {
		return elm.classList.contains('ql-editor');
	}
	return false;
}

function isRunningInHost() {
	return typeof notifyException === 'function';
}

function isEditorHidden() {
	let isHidden = getEditorContainerElement().classList.contains('hidden');
	return isHidden;
}

// #endregion State

// #region Actions

function scrollToHome() {
	getEditorElement().scrollTop = 0;
}

function hideAllToolbars() {
	hideEditorToolbar();
	hideEditTemplateToolbar();
	hidePasteTemplateToolbar();
}

function updateEditorSizesAndPositions() {
	let wh = window.visualViewport.height;
	let frth = getFindReplaceToolbarHeight();
	let eth = getEditorToolbarHeight();
	let tth = getTemplateToolbarsHeight();

	let et = eth + frth;
	getEditorContainerElement().style.top = et + 'px';
	let eh = wh - eth - tth - frth;
	getEditorContainerElement().style.height = eh + 'px';
}

function selectAll() {
	setEditorSelection(0, getDocLength(), 'api');
}

function deselectAll(forceCaretDocIdx = 0) {
	setEditorSelection(forceCaretDocIdx, 0, 'api');
}

function focusEditor() {
	getEditorElement().focus();
}

function hideScrollbars() {
	//document.querySelector('body').style.overflow = 'hidden';
	getEditorContainerElement().style.overflow = "hidden";
	getEditorElement().style.overflow = "hidden";
}

function showScrollbars() {
	//document.querySelector('body').style.overflow = 'scroll';
	getEditorElement().style.overflow = "auto";
}

function disableTextWrapping() {
	getEditorElement().style.whiteSpace = 'nowrap';
	getEditorElement().style.width = Number.MAX_SAFE_INTEGER + 'px';
}

function enableTextWrapping() {
	getEditorElement().style.whiteSpace = '';
	getEditorElement().style.width = '';
}

function createLink() {
	var range = quill.getSelection(true);
	if (range) {
		var text = getText(range);
		quill.deleteText(range.index, range.length);
		var ts =
			'<a class="square_btn" href="https://www.google.com">' + text + "</a>";
		insertHtml(range.index, ts);

		log("text:\n" + getText());
		console.table("\nhtml:\n" + getHtml());
	}
}


function enableReadOnly(fromHost = false) {
	IsReadOnly = true;
	//setEditorContentEditable(false);	

	hideAllToolbars();

	//startClipboardHandler();

	scrollToHome();
	//hideScrollbars();

	getEditorContainerElement().classList.remove('editable');
	getEditorContainerElement().classList.remove('sub-select');
	getEditorContainerElement().classList.add('no-select');

	updateAllSizeAndPositions();
	disableSubSelection();
	drawOverlay();

	if (!fromHost) {
		onReadOnlyChanged_ntf(IsReadOnly);
	}
	log('ReadOnly: ENABLED');
}

function disableReadOnly(fromHost = false) {
	if (ContentItemType != 'Text') {
		return;
	}

	IsReadOnly = false;

	showEditorToolbar();
	enableSubSelection();

	//showScrollbars();
	getEditorContainerElement().classList.remove('no-select');
	getEditorContainerElement().classList.add('editable');
	//stopClipboardHandler();

	//setEditorContentEditable(true);
	updateAllSizeAndPositions();

	//refreshFontSizePicker();
	//updateFontFamilyPickerToSelection();

	drawOverlay();

	if (!fromHost) {
		onReadOnlyChanged_ntf(IsReadOnly);
	}

	log('ReadOnly: DISABLED');
}

function enableSubSelection(fromHost = false) {
	//if (ContentItemType != 'Text') {
	//	return;
	//}
	IsSubSelectionEnabled = true;

	getEditorContainerElement().classList.remove('no-select');
	getEditorContainerElement().classList.add('sub-select');

	disableDragOverlay();

	let telms = getTemplateElements();
	for (var i = 0; i < telms.length; i++) {
		let telm = telms[i];
		if (IsReadOnly) {
			// disable pointer-events on templates w/ sub-selection
			telm.classList.add('no-select');
		} else {
			telm.classList.remove('no-select');
		}
	}

	showPasteTemplateToolbar();
	updateAllSizeAndPositions();
	drawOverlay();

	if (!fromHost) {
		onSubSelectionEnabledChanged_ntf(IsSubSelectionEnabled);
	}
	log('sub-selection ENABLED from: ' + (fromHost ? 'HOST' : 'INTERNAL'));
}

function disableSubSelection(fromHost = false) {
	IsSubSelectionEnabled = false;

	DragSelectionRange = null;

	let sel = getEditorSelection();
	deselectAll(sel ? sel.index : 0);

	getEditorContainerElement().classList.add('no-select');
	getEditorContainerElement().classList.remove('sub-select');

	enableDragOverlay();

	hidePasteTemplateToolbar();

	updateAllSizeAndPositions();
	drawOverlay();

	if (!fromHost) {
		onSubSelectionEnabledChanged_ntf(IsSubSelectionEnabled);
	}
	log('sub-selection DISABLED from: '+(fromHost ? 'HOST':'INTERNAL'));
}

function hideAllToolbars() {
	hideEditorToolbar();
	hideEditTemplateToolbar();
	hidePasteTemplateToolbar();
	hideCreateTemplateToolbarContextMenu();
	hideColorPaletteMenu();
}
// #endregion Actions

// #region Event Handlers

function onEditorFocus(e) {
	log('editor got focus');
	BlurredSelectionRects = null;
	drawOverlay();
}

function onEditorBlur(e) {
	log('editor lost focus');
	//if (isTemplateFocused()) {
	//	return;
	//}
	BlurredSelectionRects = getRangeRects(getEditorSelection());
	drawOverlay();
}


function onEditorSelectionChanged(range, oldRange, source) {
	return;
	let logRange = range ? range : { index: -1, length: 0 };
	let logOldRange = oldRange ? oldRange : { oldRange: -1, length: 0 };
	//log('Sel Changed. range.index: ' + logRange.index + ' range.length: ' + logRange.length + ' oldRange.index: ' + logOldRange.index + ' oldRange.length: ' + logOldRange.length + ' source: ' + source);


	if (IgnoreNextSelectionChange) {
		IgnoreNextSelectionChange = false;
		drawOverlay();
		return;
	}

	//if (range) {
	//	refreshFontSizePicker();
	//	updateFontFamilyPickerToSelection();
	//	//updateTemplatesAfterSelectionChange(range, oldRange);
	//	onEditorSelectionChanged_ntf(range);
	//} else {
	//	log("Cursor not in the editor");
	//}

	//let was_blur = false;
	//if (!range && !isEditTemplateTextAreaFocused()) {
	//	if (oldRange) {
	//		was_blur = true;
	//		//blur occured
	//		//setEditorSelection(oldRange.index, oldRange.length,'silent');
	//	}
	//}
	//if (was_blur && isEditorToolbarVisible()) {
	//	// only do this to show selection when in toolbar drop down
	//	BlurredSelectionRange = oldRange;
	//	BlurredSelectionRects = getRangeRects(oldRange);
	//} else {
	//	BlurredSelectionRange = null;
	//	BlurredSelectionRects = null;
	//}
	drawOverlay();
}

function onEditorTextChanged(delta, oldDelta, source) {
	updateAllSizeAndPositions();
	updateTemplatesAfterTextChanged();
	WasTextChanged = true;

	if (!IsLoaded) {
		return;
	}
	if (IgnoreNextTextChange) {
		IgnoreNextTextChange = false;
		return;
	}
	let srange = getEditorSelection();
	if (!srange) {
		return;
	}


	onContentLengthChanged_ntf();
	drawOverlay();
}
// #endregion Event Handlers
