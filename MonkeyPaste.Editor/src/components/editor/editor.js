// #region Globals

var DefaultEditorWidth = 1200;

var IgnoreNextTextChange = false;
var IgnoreNextSelectionChange = false;

var IsSubSelectionEnabled = false;
var IsReadOnly = false;

var EditorTheme = 'light';


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

function getEditorWidth() {
	var editorRect = getEditorElement().getBoundingClientRect();
	//var editorHeight = parseInt($('.ql-editor').wi());
	return editorRect.width;
}

function getEditorHeight() {
	var editorRect = getEditorElement().getBoundingClientRect();
	return editorRect.height;
}

function getEditorVisibleHeight() {
	let frth = getFindReplaceToolbarHeight();
	let eth = getEditorToolbarHeight();
	let tth = getTemplateToolbarsHeight();

	return getEditorContainerElement().getBoundingClientRect().height - frth - eth - tth;
}

// #endregion Getters

// #region Setters


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
	let sel = getDocSelection();
	return !sel || sel.length == 0;
}

function isContentEditable() {
	let isEditable = parseBool(getEditorElement().getAttribute('contenteditable'));
	return isEditable;
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

function isReadOnly() {
	return !getEditorContainerElement().classList.contains('editable');
}

function isSubSelectionEnabled() {
	return getEditorContainerElement().classList.contains('sub-select');
}

// #endregion State

// #region Actions

function scrollToHome() {
	getEditorElement().scrollTop = 0;
}

function hideAllToolbars() {
	hideEditorToolbar();
	hideEditTemplateToolbar();
	hidePasteToolbar();
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
	setDocSelection(0, getDocLength(), 'api');
}

function deselectAll(forceCaretDocIdx = 0) {
	setDocSelection(forceCaretDocIdx, 0, 'api');
}

function focusEditor() {
	getEditorElement().focus();
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


function hideScrollbars() {
	getEditorContainerElement().classList.remove('show-scrollbars');
	getEditorContainerElement().classList.add('hide-scrollbars');
}

function showScrollbars() {
	getEditorContainerElement().classList.add('show-scrollbars');
	getEditorContainerElement().classList.remove('hide-scrollbars');
}
function enableReadOnly(fromHost = false) {
	if (isReadOnly()) {
		log('enableReadOnly ignored, already read-only. fromHost: ' + fromHost);
		return;
	}

	hideAllToolbars();

	getEditorContainerElement().classList.remove('editable');
	getEditorContainerElement().classList.remove('sub-select');
	getEditorContainerElement().classList.add('no-select');

	scrollToHome();
	updateAllElements();
	disableSubSelection();
	disableTemplateSubSelection();

	drawOverlay();

	if (!fromHost) {
		onReadOnlyChanged_ntf(isReadOnly());
	}
	log('ReadOnly: ENABLED fromHost: ' + fromHost);
}

function disableReadOnly(fromHost = false) {
	if (!isReadOnly()) {
		log('disableReadOnly ignored, already editable. fromHost: ' + fromHost);
		return;
	}
	if (ContentItemType != 'Text') {
		return;
	}

	showEditorToolbar();

	getEditorContainerElement().classList.remove('no-select');
	getEditorContainerElement().classList.add('editable');

	enableSubSelection();
	enableTemplateSubSelection();

	updateFontSizePickerToSelection();
	updateFontFamilyPickerToSelection();

	updateAllElements();

	drawOverlay();

	if (!fromHost) {
		onReadOnlyChanged_ntf(isReadOnly());
	}

	log('ReadOnly: DISABLED fromHost: ' + fromHost);
}

function enableSubSelection(fromHost = false, showUnderlines = true, showPaste = true) {
	if (isSubSelectionEnabled()) {
		log('enableSubSelection ignored, already sub-selectable. fromHost: ' + fromHost);
		return;
	}

	getEditorContainerElement().classList.remove('no-select');
	getEditorContainerElement().classList.add('sub-select');
	if (showUnderlines) {
		getEditorContainerElement().classList.add('underline-content');
	} else {
		getEditorContainerElement().classList.remove('underline-content');
	}
	showScrollbars();
	disableDragOverlay();
	enableTemplateSubSelection();

	if (showPaste) {
		showPasteToolbar();
	} else {
		hidePasteToolbar();
	}
	updateAllElements();

	drawOverlay();

	if (!fromHost) {
		onSubSelectionEnabledChanged_ntf(isSubSelectionEnabled());
	}
	log('sub-selection ENABLED fromHost: ' + fromHost);
}

function disableSubSelection(fromHost = false) {
	if (!isSubSelectionEnabled()) {
		log('disableSubSelection ignored, already sub-selectable. fromHost: ' + fromHost);
		return;
	}

	//DragSelectionRange = null;
	//BlurredSelectionRects = null;

	//let sel = getDocSelection();
	//deselectAll(sel ? sel.index : 0);
	resetSelection();

	getEditorContainerElement().classList.add('no-select');
	getEditorContainerElement().classList.remove('sub-select');
	getEditorContainerElement().classList.remove('underline-content');
	hideScrollbars();
	enableDragOverlay();

	hidePasteToolbar();

	updateAllElements();
	drawOverlay();

	if (!fromHost) {
		onSubSelectionEnabledChanged_ntf(isSubSelectionEnabled());
	}
	log('sub-selection DISABLED from: '+(fromHost ? 'HOST':'INTERNAL'));
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
	BlurredSelectionRects = getRangeRects(getDocSelection());
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
	//	updateFontSizePickerToSelection();
	//	updateFontFamilyPickerToSelection();
	//	//updateTemplatesAfterSelectionChange(range, oldRange);
	//	onDocSelectionChanged_ntf(range);
	//} else {
	//	log("Cursor not in the editor");
	//}

	//let was_blur = false;
	//if (!range && !isEditTemplateTextAreaFocused()) {
	//	if (oldRange) {
	//		was_blur = true;
	//		//blur occured
	//		//setDocSelection(oldRange.index, oldRange.length,'silent');
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
	log('quill event: text changed');

	updateAllElements();
	updateTemplatesAfterTextChanged();
	WasTextChanged = true;

	if (!IsLoaded) {
		return;
	}
	if (IgnoreNextTextChange) {
		IgnoreNextTextChange = false;
		return;
	}
	let srange = getDocSelection();
	if (!srange) {
		return;
	}

	onContentLengthChanged_ntf();
	drawOverlay();
}
// #endregion Event Handlers