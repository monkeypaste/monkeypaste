// #region Globals

const DefaultSelectionBgColor = 'lightblue';
const DefaultSelectionFgColor = 'black';
const DefaultCaretColor = 'black';

var BlurredSelectionRects = null;

var LastSelRange = { index: 0, length: 0 };

var SelectionOnMouseDown = null;

var WasTextChanged = false;

var SelTimerInterval = null;

// #endregion Globals

// #region Life Cycle

function initSelection() {
	document.addEventListener('selectionchange', onDocumentSelectionChange, true);
	//SelTimerInterval = setInterval(onSelectionCheckTick, 100);
}

function resetSelection() {
	LastSelRange = null;
	BlurredSelectionRects = null;
}

// #endregion Life Cycle

// #region Getters

function getTextSelectionFgColor() {
	return getElementComputedStyleProp(document.body, '--selfgcolor');
}

function getTextSelectionBgColor() {
	return getElementComputedStyleProp(document.body, '--selbgcolor');
}

function setCaretColor(caretColor) {
	getEditorElement().style.caretColor = caretColor;
}

function getCaretColor() {
	return getEditorElement().style.caretColor;
}

function getDocumentSelection() {
	let cur_sel = null;

	if (window.getSelection().rangeCount == 0 || (quill && !quill.hasFocus())) {
		log('no window selection, falling back to last: ' + JSON.stringify(LastSelRange));
		cur_sel = LastSelRange;
	} else {
		let range = window.getSelection().getRangeAt(0);
		cur_sel = convertDocRangeToEditorRange(range);
		//LastSelRange = cur_sel;
	}

	return cur_sel;
}

function getDocumentSelectionHtml(docSel) {
	//if (docSel.rangeCount > 0) {
	range = docSel.getRangeAt(0);
	var clonedSelection = range.cloneContents();
	var div = document.createElement('div');
	div.appendChild(clonedSelection);
	let htmlStr = div.innerHTML;
	return htmlStr;
	//}
}

function getCaretLine(forceDocIdx = -1) {
	let caret_doc_idx = forceDocIdx;
	if (caret_doc_idx < 0) {
		let sel = getEditorSelection();
		if (!sel) {
			log('no selection, cannot get caret line');
			return;
		}
		if (sel.length > 0) {
			log('warning should only get caret line when selection empty')
			debugger;
			return;
		}
		caret_doc_idx = sel.index;
	}

	let editor_rect = getEditorContainerRect();
	let caret_rect = getCharacterRect(caret_doc_idx);

	let caret_line = { x1: caret_rect.left, y1: caret_rect.top, x2: caret_rect.left, y2: caret_rect.bottom };
	let left_clamp = 0;
	let right_clamp = editor_rect.width;
	if (caret_line.x1 < 0) {
		caret_line.x1 = left_clamp;
		caret_line.x2 = left_clamp;
		log('caret_line was < editor_rect.left: ' + left_clamp);
	} else if (caret_line.x1 > right_clamp) {
		caret_line.x1 = right_clamp;
		caret_line.x2 = right_clamp;
		log('caret_line was > editor_rect.right: ' + right_clamp);
	}
	if (caret_line.x1 < 0 || caret_line.x2 < 0) {
		caret_line.x1 = 0;
		caret_line.x2 = 0;
	}
	caret_line = cleanLine(caret_line);
	return caret_line;
}


// #endregion Getters

// #region Setters

function setTextSelectionFgColor(fgColor) {
	//document.body.style.setProperty('--selfgcolor', fgColor);
	setElementComputedStyleProp(document.body, '--selfgcolor', fgColor);
}

function setTextSelectionBgColor(bgColor) {
	//document.body.style.setProperty('--selbgcolor', bgColor);
	setElementComputedStyleProp(document.body, '--selbgcolor', bgColor);
}
// #endregion Setters

// #region State

function didSelectionChange(old_sel, new_sel) {
	if (!old_sel && !new_sel) {
		return false;
	}
	if (old_sel && !new_sel) {
		return true;
	}
	if (new_sel && !old_sel) {
		return true;
	}
	return old_sel.index != new_sel.index || old_sel.length != new_sel.length;
}
// #endregion State

// #region Actions

function convertDocRangeToEditorRange(docRange) {
	let start_elm_doc_idx = getElementDocIdx(docRange.startContainer);
	let end_elm_doc_idx = getElementDocIdx(docRange.endContainer);

	let sel = { index: 0, length: 0 };
	sel.index = start_elm_doc_idx + docRange.startOffset;
	sel.length = (end_elm_doc_idx + docRange.endOffset) - sel.index;

	return sel;
}

function convertEditorRangeToDocRange(editorRange) {
	let start_elm = getElementAtDocIdx(editorRange.index);
	let end_elm = getElementAtDocIdx(editorRange.index + editorRange.length);

	let start_elm_doc_idx = getElementDocIdx(start_elm);
	let end_elm_doc_idx = getElementDocIdx(end_elm);

	let start_offset = editorRange.index - start_elm_doc_idx;
	let end_offset = (editorRange.index + editorRange.length) - end_elm_doc_idx;

	let clean_range = document.createRange();
	clean_range.setStart(start_elm, start_offset);
	clean_range.setEnd(end_elm, end_offset);

	return clean_range;
}

function cleanDocumentSelection(cur_sel) {
	if (IsDragging || IsDropping || !WindowMouseDownLoc || !WindowMouseLoc || (quill && !quill.hasFocus())) {
		return cur_sel;
	}


	let mp_down_dist = dist(WindowMouseDownLoc, WindowMouseLoc);
	if (mp_down_dist > 1) {
		// this is drag check time
		return cur_sel;
	}

	let mp_down_doc_idx = getDocIdxFromPoint(WindowMouseDownLoc);
	let mp_cur_doc_idx = getDocIdxFromPoint(WindowMouseLoc);

	let start_idx = Math.min(mp_cur_doc_idx, mp_down_doc_idx);
	let end_idx = Math.max(mp_cur_doc_idx, mp_down_doc_idx);
	if (start_idx >= 0 && end_idx >= 0) {
		let clean_sel = { index: start_idx, length: end_idx - start_idx };
		
		if (clean_sel.index != cur_sel.index || clean_sel.length != cur_sel.length) {
			log(`Trying to clean selection from [idx:${cur_sel.index} len:${cur_sel.length}] to [idx:${clean_sel.index} len:${clean_sel.length}]`);
			let clean_win_range = convertEditorRangeToDocRange(clean_sel);

			let win_sel = window.getSelection();
			win_sel.removeAllRanges();

			win_sel.addRange(clean_win_range);
			return clean_sel;
		}
	}
	
	return cur_sel;
}

function coerceCleanSelection() {
	let cur_sel_range = getDocumentSelection();
	if (didSelectionChange(cur_sel_range, LastSelRange)) {
		//log('Sel Changed from Timer.');
		if (IsDragging) {
			if (DragSelectionRange) {
				cur_sel_range = DragSelectionRange;
			}
			log('drag detected sel timer overriding selection. LastRange: ', LastSelRange, ' DragRange: ', DragSelectionRange);
			setEditorSelection(LastSelRange.index, LastSelRange.length, 'silent');

			drawOverlay();
			return cur_sel_range;
		}

		log('timer: index: ', cur_sel_range.index, ' length: ', cur_sel_range.length);

		let qsel = getEditorSelection();
		log('quill: index: ', qsel.index, ' length: ', qsel.length);

		let oldRange = LastSelRange;
		// updating Last

		if (cur_sel_range) {
			LastSelRange = cur_sel_range;
			refreshFontSizePicker(null, cur_sel_range);
			refreshFontFamilyPicker(null, cur_sel_range);
			//updateTemplatesAfterSelectionChange(cur_sel_range, oldRange);
			onEditorSelectionChanged_ntf(cur_sel_range);
			LastSelRange = cur_sel_range;
		}
		drawOverlay();
	}
	return cur_sel_range;
}

// #endregion Actions

// #region Event Handlers

function onDocumentSelectionChange(e) {
	// Selection Change issues:
	// 1. 
	LastSelRange = coerceCleanSelection();
}

function onSelectionCheckTick(e) {
	LastSelRange = coerceCleanSelection();
}
// #endregion Event Handlers