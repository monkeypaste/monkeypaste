// #region Globals

var DefaultEditorWidth = 1200;

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

	quill = initQuill();
	getEditorContainerElement().firstChild.setAttribute('id', 'quill-editor');

	initEditorToolbar();
	initEditTemplateToolbar();
	initPasteToolbar();

	initEditorScroll();
	initTemplates();
	initExtContentSourceBlot();

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
	getEditorContainerElement().scrollTop = 0;
}

function scrollToEnd() {
	getEditorElement().scrollTop = getEditorContainerElement().getBoundingClientRect().bottom;
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
	disableSubSelection();

	getEditorContainerElement().classList.remove('editable');
	getEditorContainerElement().classList.remove('sub-select');
	getEditorContainerElement().classList.add('no-select');

	scrollToHome();
	updateAllElements();
	disableTemplateSubSelection();

	drawOverlay();

	if (!fromHost) {
		onReadOnlyChanged_ntf(isReadOnly());
	}
	log('ReadOnly: ENABLED fromHost: ' + fromHost);
}

function disableReadOnly(fromHost = false) {
	if (!canDisableReadOnly()) {
		log('disableReadOnly ignored, not text item. fromHost: ' + fromHost);
		return;
	}
	if (!isReadOnly()) {
		log('disableReadOnly ignored, already editable. fromHost: ' + fromHost);
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

function enableSubSelection(fromHost = false) {
	if (!canEnableSubSelection()) {
		log('enableSubSelection ignored, content is an image. fromHost: ' + fromHost);
		return;
	}
	if (isSubSelectionEnabled()) {
		log('enableSubSelection ignored, already sub-selectable. fromHost: ' + fromHost);
		return;
	}

	getEditorContainerElement().classList.remove('no-select');
	getEditorContainerElement().classList.add('sub-select');
	getEditorContainerElement().classList.add('underline-content');

	showScrollbars();
	disableDragOverlay();
	enableTemplateSubSelection();
	showPasteToolbar();
	updateAllElements();

	drawOverlay();

	if (!fromHost) {
		onSubSelectionEnabledChanged_ntf(isSubSelectionEnabled());
	}
	log('sub-selection ENABLED fromHost: ' + fromHost);
}

function disableSubSelection(fromHost = false) {
	if (!canDisableSubSelection()) {
		log('disableSubSelection ignored, this is appender. fromHost: ' + fromHost);
		return;
	}
	if (!isSubSelectionEnabled()) {
		log('disableSubSelection ignored, already sub-selectable. fromHost: ' + fromHost);
		return;
	}

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
	hideAllPopups();
	getEditorElement().classList.add('focused');
	drawOverlay();
}

function onEditorBlur(e) {
	log('editor lost focus');
	//if (isTemplateFocused()) {
	//	return;
	//}
	BlurredSelectionRects = getRangeRects(getDocSelection());
	getEditorElement().classList.remove('focused');
	drawOverlay();
}


function onEditorTextChanged(delta, oldDelta, source) {
	log('quill event: text changed');

	updateAllElements();
	//WasTextChanged = true;

	if (!IsLoaded) {
		return;
	}

	if (!IsTemplatePaddingAfterTextChange) {
		updateTemplatesAfterTextChanged();
	}

	if (isShowingEditorToolbar()) {
		// NOTE for performance only update db when tile goes back to readonly so..
		// ignore notification
	} else {
		// an ole operation has occured
		onContentChanged_ntf();
	}
	drawOverlay();
}
// #endregion Event Handlers
