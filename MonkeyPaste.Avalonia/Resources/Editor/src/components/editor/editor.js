
// #region Life Cycle

function initEditor() {
	if (globals.IsLoaded) {
		log('editor already initialized, ignoring init');
		return;
	}

	globals.quill = initQuill();

	initLocalizer();
	initClipboard();
	initSvgElements();
	initEditorToolbar();
	initEditTemplateToolbar();
	initPasteToolbar();

	initContentClassAttributes();

	initScroll();
	initTooltip();
	initTemplates();
	initMacros();
	initOverlay();
	//initHistory();
	//initExtContentSourceBlot();

	globals.quill.on("selection-change", onEditorSelChanged);
	globals.quill.on("text-change", onEditorTextChanged);

	getEditorElement().addEventListener('focus', onEditorFocus);
	getEditorElement().addEventListener('blur', onEditorBlur, true);

	globals.ContentLoadedEvent = new Event("onContentLoaded");
}

function initEditorToolbars() {
	if (globals.IsToolbarsLoaded) {
		return;
	}

	globals.IsToolbarsLoaded = true;
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

function getEditorContainerRect(includeScrollBars = true) {
	let editor_container_rect = getEditorContainerElement().getBoundingClientRect();
	editor_container_rect = cleanRect(editor_container_rect);
	if (!includeScrollBars) {
		editor_container_rect.right -= getEditorVerticalScrollBarWidth();
		editor_container_rect.bottom -= getEditorHorizontalScrollBarHeight();
		editor_container_rect = cleanRect(editor_container_rect);
	}
	
	return editor_container_rect;
}

function getEditorWidth() {
	return getEditorContainerRect().width;
}

function getEditorHeight() {
	return getEditorContainerRect().height;
}

function getEditorVisibleHeight() {
	let frth = getFindReplaceToolbarHeight();
	let eth = getEditorToolbarHeight();
	let tth = getTemplateToolbarsHeight();

	return getEditorContainerElement().getBoundingClientRect().height - frth - eth - tth;
}

// #endregion Getters

// #region Setters

function setEditorZoom(scale) {
	// scale should be 0-3ish number
	scale = isNullOrUndefined(scale) ? 1 : scale;
	let scale_val = parseFloat(scale);
	let is_custom = scale_val != 1;
	if (is_custom) {
		// update img content selector to not clamp size
		getEditorContainerElement().classList.add('custom-zoom');
	} else {
		getEditorContainerElement().classList.remove('custom-zoom');
	}

	let zoom_val = scale_val * 100;
	setElementComputedStyleProp(document.body, '--editorzoom', zoom_val +'%');
}
function setEditorPlaceholderText(text) {
	getEditorElement().setAttribute('data-placeholder', text);
}

function setEditorIsLoaded(isLoaded) {
	globals.IsLoaded = isLoaded;
}

// #endregion Setters

// #region State

function isShowingEditorToolbar() {
	$(".ql-toolbar").css("display") != "none";
}

function isAllSelected() {
	if (globals.ContentItemType != 'Text') {
		return true;
	}
	// NOTE doc length is never 0 and there's always an extra unselectable \n character at end so minus 1 for length to check here
	let doc_len = getDocLength() - 1;
	let sel = getDocSelection();
	let result = sel.index == 0 && sel.length == doc_len;
	return result;
}

function isNoneSelected() {
	if (globals.ContentItemType != 'Text') {
		return false;
	}
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

function isEditorFocused() {
	return isChildOfElement(document.activeElement, getEditorElement());
}

// #endregion State

// #region Actions

function updateEditorPlaceholderText() {
	let plt = UiStrings.EditorWatermark;
	setEditorPlaceholderText(plt);
}
function hideEditorScrollbars() {
	getEditorContainerElement().classList.remove('show-scrollbars');
	getEditorContainerElement().classList.add('hide-scrollbars');

}

function showEditorScrollbars() {
	showElementScrollbars(getEditorContainerElement());
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

function focusEditor() {
	globals.quill.focus();
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
	var range = globals.quill.getSelection(true);
	if (range) {
		var text = getText(range);
		globals.quill.deleteText(range.index, range.length);
		var ts =
			'<a class="square_btn" href="https://www.google.com">' + text + "</a>";
		insertHtml(range.index, ts);

		log("text:\n" + getText());
		console.table("\nhtml:\n" + getHtml());
	}
}


function enableReadOnly(fromHost = false) {
	getEditorElement().style.caretColor = 'transparent';
	if (isReadOnly()) {
		log('enableReadOnly ignored, already read-only. fromHost: ' + fromHost);
		return;
	}

	hideAllToolbars();
	//disableSubSelection();

	getEditorContainerElement().classList.remove('editable');
	//getEditorContainerElement().classList.remove('sub-select');
	//getEditorContainerElement().classList.add('no-select');

	scrollToHome();
	updateAllElements();
	//disableTemplateSubSelection();

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
	getEditorElement().style.caretColor = 'black';
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

function enableSubSelection(fromHost = false, paste_button_info = null) {
	if (fromHost) {
		updatePasteButtonInfo(paste_button_info);
	}
	if (!canEnableSubSelection()) {
		log('enableSubSelection ignored, content is an image. fromHost: ' + fromHost);
		return;
	}

	getEditorContainerElement().classList.remove('no-select');
	getEditorContainerElement().classList.add('sub-select');
	getEditorContainerElement().classList.add('underline-content');

	showAllScrollbars();
	enableTemplateSubSelection();
	showPasteToolbar();
	showAnnotations();
	updateAllElements();


	if (fromHost) {
		focusEditor();
	} else {
		onSubSelectionEnabledChanged_ntf(isSubSelectionEnabled());
	}
	drawOverlay();
	log('sub-selection ENABLED fromHost: ' + fromHost);
}

function disableSubSelection(fromHost = false) {
	if (!canDisableSubSelection()) {
		log('disableSubSelection ignored, drop in progress or this is appender or appendee. fromHost: ' + fromHost);
		if (fromHost) {
			// notify host sub-selection canceled
			onSubSelectionEnabledChanged_ntf(isSubSelectionEnabled());
		}
		return;
	}

	resetSelection();

	getEditorContainerElement().classList.add('no-select');
	getEditorContainerElement().classList.remove('sub-select');
	getEditorContainerElement().classList.remove('underline-content');

	scrollToHome();
	hideAllScrollbars();
	hideAnnotations();
	hidePasteToolbar();
	clearTableSelectionStates();
	disableTemplateSubSelection();

	updateAllElements();
	drawOverlay();

	if (!fromHost) {
		onSubSelectionEnabledChanged_ntf(isSubSelectionEnabled());
	}
	log('sub-selection DISABLED from: '+(fromHost ? 'HOST':'INTERNAL'));
}

function suppressTextChanged(guid = null) {
	guid = guid == null ? generateGuid() : guid;
	let was_found = false;
	let stcs = globals.SuppressTextChangeStack;
	for (var i = 0; i < stcs.length; i++) {
		if (stcs[i].guid != guid) {
			continue;
		}
		stcs[i].time = Date.now();
		was_found = true;
		break;
	}
	if (!was_found) {
		stcs.push({ guid: guid, time: Date.now() });
	}
	globals.SuppressTextChangeStack = stcs;
	return guid;
}
function unsupressTextChanged(guid) {
	if (!guid) {
		debugger;
	}
	// this is a workaround because often quill doesn't 'emit' text change
	// immediatly and will happen AFTER unsupressing so this waits a few cycles before unsetting

	delay(300)
		.then(() => {
			let stcs = [];
			for (var i = 0; i < globals.SuppressTextChangeStack.length; i++) {
				if (globals.SuppressTextChangeStack[i].guid == guid) {
					continue;
				}
				stcs.push(globals.SuppressTextChangeStack[i]);
			}
			globals.SuppressTextChangeStack = stcs;
	});		
}

function isTextChangeSupressed() {
	let result = globals.SuppressTextChangeStack.length > 0;
	return result;
}
// #endregion Actions

// #region Event Handlers

function onEditorFocus(e) {
	log('editor got focus');
	hideAllPopups();
	getEditorContainerElement().classList.add('editor-focused');
	drawOverlay();
}

function onEditorBlur(e) {
	log('editor lost focus');
	getEditorContainerElement().classList.remove('editor-focused');
	drawOverlay();
}

function onEditorSelChanged(range, oldRange, source) {
	if (isAppendPreMode() && source == 'user' && range) {
		globals.FixedAppendIdx = range.index;
	}
	// showing ops menu takes focus

	if (!isDragging()) {
		return;
	}
	return;
}
function onEditorTextChanged(delta, oldDelta, source) {
	if (isTextChangeSupressed()) {
		return;
	}
	log('editor text changed');
	
	updateAllElements();

	if (!globals.IsLoaded || globals.IsLoadingContent) {
		return;
	}
	addHistoryItem(delta, oldDelta);

	if (!globals.IsTemplatePaddingAfterTextChange) {
		updateTemplatesAfterTextChanged();
	}

	loadLinkHandlers(true);
	populateFindReplaceResults();

	let suppress_text_change_ntf = isTextChangeSupressed();

	if (globals.IsLoaded &&
		!suppress_text_change_ntf &&		
		isDeltaContainTemplate(delta) &&
		isAnyTemplateToolbarElementFocused()) {
		// NOTE template data changes need to be suppressed until they lost focus
		suppress_text_change_ntf = true;
	}

	if (suppress_text_change_ntf) {
		log('text change ntf suppressed');
	} else {
		onContentChanged_ntf();
	}
	
	drawOverlay();
}
// #endregion Event Handlers
