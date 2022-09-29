var DefaultEditorWidth = 1200;

var IgnoreNextTextChange = false;
var IgnoreNextSelectionChange = false;

var IsSubSelectionEnabled = false;

var EditorContainerElement = null;
var QuillEditorElement = null;

var IsReadOnly = false;

function initEditor(useBetterTable) {
	if (IsLoaded) {
		log('editor already initialized, ignoring init');
		return;
	}

	initQuill(useBetterTable);



	quill.on("selection-change", onEditorSelectionChanged);
	quill.on("text-change", onEditorTextChanged);
}

function focusEditor() {
	document.getElementById("editor").focus();
}
function hideScrollbars() {
	//document.querySelector('body').style.overflow = 'hidden';
	getEditorContainerElement().style.overflow = "hidden";
	getEditorElement().style.overflow = "hidden";
}

function showScrollbars() {
	//document.querySelector('body').style.overflow = 'scroll';
	document.getElementById("editor").style.overflow = "auto";
}

function disableTextWrapping() {
	getEditorElement().style.whiteSpace = 'nowrap';
	getEditorElement().style.width = Number.MAX_SAFE_INTEGER + 'px';
}

function enableTextWrapping() {
	getEditorElement().style.whiteSpace = '';
	getEditorElement().style.width = '';
}


function getTotalHeight() {
	var totalHeight =
		getEditorToolbarHeight() + getEditorHeight() + getTemplateToolbarHeight();
	return totalHeight;
}

function updateAllSizeAndPositions() {
	//if (isRunningInHost()) {
	//	getEditorContainerElement().style.height = '100%';
	//}
	$(".ql-toolbar").css("top", 0);

	if (isEditorToolbarVisible()) {
		$("#editor").css("top", $(".ql-toolbar").outerHeight());
	} else {
		$("#editor").css("top", 0);
	}
	

	let wh = window.visualViewport.height;
	let eth = getEditorToolbarHeight();
	let tth = getTemplateToolbarHeight();

	$("#editor").css("height", wh - eth - tth);

	updateEditTemplateToolbarPosition();
	updatePasteTemplateToolbarPosition();

	drawOverlay();

	if (EnvName == "android") {
		//var viewportBottom = window.scrollY + window.innerHeight;
		//let tbh = $(".ql-toolbar").outerHeight();
		//if (y <= 0) {
		//    //keyboard is not visible
		//    $(".ql-toolbar").css("top", y);
		//    $("#editor").css("top", y + tbh);
		//} else {
		//    $(".ql-toolbar").css("top", y - tbh);
		//    $("#editor").css("top", 0);
		//}
		//$("#editor").css("bottom", viewportBottom - tbh);
	}
}




function onEditorSelectionChanged(range, oldRange, source) {
	//LastSelectedHtml = SelectedHtml;
	//SelectedHtml = getSelectedHtml();

	let logRange = range ? range : { index: -1, length: 0 };
	let logOldRange = oldRange ? oldRange : { oldRange: -1, length: 0 };
	log('Sel Changed. range.index: ' + logRange.index + ' range.length: ' + logRange.length + ' oldRange.index: ' + logOldRange.index + ' oldRange.length: ' + logOldRange.length + ' source: ' + source);
	
	drawOverlay();

	if (IgnoreNextSelectionChange) {
		IgnoreNextSelectionChange = false;
		return;
	}

	if (range) {
		refreshFontSizePicker();
		refreshFontFamilyPicker();
		updateTemplatesAfterSelectionChange(range,oldRange);
		//coereceSelectionWithTemplatePadding(range, oldRange, source);

		onEditorSelectionChanged_ntf(range);
	} else {
		log("Cursor not in the editor");
	}
	let was_blur = false;
	if (!range && !isEditTemplateTextAreaFocused()) {
		if (oldRange) {
			was_blur = true;
			//blur occured
			//setEditorSelection(oldRange.index, oldRange.length,'silent');
		} 
	}
	if (was_blur && isEditorToolbarVisible()) {
		// only do this to show selection when in toolbar drop down
		BlurredSelectionRange = oldRange;
		BlurredSelectionRects = getRangeRects(oldRange);
	} else {
		BlurredSelectionRange = null;
		BlurredSelectionRects = null;
	}
	drawOverlay();
}

function onEditorTextChanged(delta, oldDelta, source) {
	updateAllSizeAndPositions();
	updateTemplatesAfterTextChanged(delta, oldDelta, source);

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

function selectAll() {
	setEditorSelection(0, getDocLength(),'api');
}

function deselectAll(forceCaretDocIdx = 0) {
	setEditorSelection(forceCaretDocIdx, 0, 'api');
}

function isAllSelected() {
	// NOTE doc length is never 0 and there's always an extra unselectable \n character at end so minus 1 for length to check here
	let doc_len = getDocLength() - 1;
	let sel = getEditorSelection_safe();
	let result = sel.index == 0 && sel.length == doc_len;
	return result;
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

function isContentEditable() {
	let isEditable = parseBool(getEditorElement().getAttribute('contenteditable'));
	return isEditable;
}

function isReadOnly() {
	return !isEditorToolbarVisible();
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
	//refreshFontFamilyPicker();

	drawOverlay();

	if (!fromHost) {
		onReadOnlyChanged_ntf(IsReadOnly);
	}

	log('ReadOnly: DISABLED');
}

function enableSubSelection(fromHost = false) {
	IsSubSelectionEnabled = true;
	
	getEditorContainerElement().classList.remove('no-select');
	getEditorContainerElement().classList.add('sub-select');

	getDragOverlayElement().classList.add('drag-overlay-disabled');
	getDragOverlayElement().classList.remove('drag-overlay-enabled');
	getDragOverlayElement().setAttribute('draggable', false);

	if (IsReadOnly) {
		// disable pointer-events on templates w/ sub-selection
		getTemplateElements().forEach((te) => te.classList.add('no-select'));
	} else {
		getTemplateElements().forEach((te) => te.classList.remove('no-select'));
	}

	updateAllSizeAndPositions();
	drawOverlay();

	if (!fromHost) {
		onSubSelectionEnabledChanged_ntf(IsSubSelectionEnabled);
	}
	log('sub-selection ENABLED');
}

function disableSubSelection(fromHost = false) {
	IsSubSelectionEnabled = false;

	let sel = getEditorSelection();
	deselectAll(sel ? sel.index : 0);

	getEditorContainerElement().classList.add('no-select');
	getEditorContainerElement().classList.remove('sub-select');

	getDragOverlayElement().classList.remove('drag-overlay-disabled');
	getDragOverlayElement().classList.add('drag-overlay-enabled');
	getDragOverlayElement().setAttribute('draggable', true);

	//getTemplateElements().forEach((te) => te.classList.add('no-select'));

	updateAllSizeAndPositions();
	drawOverlay();

	if (!fromHost) {
		onSubSelectionEnabledChanged_ntf(IsSubSelectionEnabled);
	}
	log('sub-selection DISABLED');
}

function hideAllToolbars() {
	hideEditorToolbar();
	hideEditTemplateToolbar();
	hidePasteTemplateToolbar();
	hideTemplateColorPaletteMenu();
	hideTemplateToolbarContextMenu();
}

function getEditorSelection() {
	//let docSel = null;
	//if (IsLoaded && !isContentEditable()) {
	//	// when non-editable is when selection has problems
	//	for (var i = 0; i < document.getSelection().rangeCount; i++) {
	//		if (docSel == null) {
	//			docSel = [];
	//		}
	//		docSel.push(document.getSelection().getRangeAt(i));
	//	}
	//	setEditorContentEditable(true);
	//}
	let selection = quill.getSelection();
	//if (docSel) {
	//	setEditorContentEditable(false);
	//	document.getSelection().removeAllRanges();
	//	for (var i = 0; i < docSel.length; i++) {
	//		let range = docSel[i];
	//		document.getSelection().addRange(range);
	//	}
	//}
	return selection;
}

function setEditorSelection(doc_idx, len, source = 'user') {
	//getEditorContainerElement().style.userSelect = 'auto';
	quill.setSelection(doc_idx, len, source);
	if (source == 'silent') {
		onEditorSelectionChanged_ntf({ index: doc_idx, length: len });
	}
}


function isShowingEditorToolbar() {
	$(".ql-toolbar").css("display") != "none";
}

function hideEditorAndAllToolbars() {
	hideEditorToolbar();
	hideEditor();
	hideEditTemplateToolbar();
	hidePasteTemplateToolbar();
}

function showEditor() {
	getEditorContainerElement().classList.remove('hidden');
}

function hideEditor() {
	getEditorContainerElement().classList.add('hidden');
}

function isEditorHidden() {
	let isHidden = getEditorContainerElement().classList.contains('hidden');
	return isHidden;
}



function getEditorWidth() {
	var editorRect = document.getElementById("editor").getBoundingClientRect();
	//var editorHeight = parseInt($('.ql-editor').wi());
	return editorRect.width;
}

function getEditorHeight() {
	var editorRect = document.getElementById("editor").getBoundingClientRect();
	//var editorHeight = parseInt($('.ql-editor').outerHeight());
	return editorRect.height;
}



function scrollToHome() {
	document.getElementById("editor").scrollTop = 0;
}

function getEditorContainerElement() {
	if (EditorContainerElement == null) {
		EditorContainerElement = document.getElementById("editor");
	}
	return EditorContainerElement;
}

function getEditorElement() {
	if (QuillEditorElement == null) {
		QuillEditorElement = getEditorContainerElement().firstChild;
	}
	return QuillEditorElement;
}

function getEditorContainerRect() {
	let editor_container_rect = getEditorContainerElement().getBoundingClientRect();
	editor_container_rect = cleanRect(editor_container_rect);
	return editor_container_rect;
}

function isEditorElement(elm) {
	if (elm instanceof HTMLElement) {
		return elm.classList.contains('ql-editor');
	}
	return false;
}

async function getContentImageBase64Async() {
	let base64Str = await getBase64ScreenshotOfElementAsync(getEditorElement());

	return base64Str;
}

function getContentImageBase64() {
	let base64Str = getBase64ScreenshotOfElement(getEditorElement());

	return base64Str;
}

function isRunningInHost() {
	return typeof notifyException === 'function';
}

