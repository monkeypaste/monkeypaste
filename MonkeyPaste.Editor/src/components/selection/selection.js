// #region Globals

const DefaultSelectionBgColor = 'lightblue';
const DefaultSelectionFgColor = 'black';
const DefaultCaretColor = 'black';

var SuppressNextSelChangedHostNotification = false;

var BlurredSelectionRects = null;

var SelectionHistory = [];

var LastSelRange = null;
var CurSelRange = { index: 0, length: 0 };

var SelectionOnMouseDown = null;

var WasTextChanged = false;

var SelTimerInterval = null;

// #endregion Globals

// #region Life Cycle

function initSelection() {
	document.addEventListener('selectionchange', onDocumentSelectionChange, true);
	//SelTimerInterval = setInterval(onSelectionCheckTick, 100);
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

function getDocSelection(isForPaste = false) {

	let doc_sel = convertDomRangeToDocRange(getDomFocusRange());

	if (isForPaste && (!doc_sel || (doc_sel && doc_sel.length == 0))) {
		return { index: 0, length: getDocLength() };
	}
	return doc_sel;
}

function getDomFocusRange(forceToEditor = true) {
	let dom_focus_range = null;
	
	if (forceToEditor) {
		let needs_fallback = false;
		if (!window.getSelection().focusNode) {
			//log("There is currently NO focus node will fallback");
			needs_fallback = true;
		} else if (!isChildOfElement(window.getSelection().focusNode, getEditorElement())) {
			//log('Document focus is not within the editor, will fallback');
			needs_fallback = true;
		} else if (document.getSelection().rangeCount == 0) {
			//log('no sel ranges in document, will fallback')
			needs_fallback = true;
		}

		if (needs_fallback) {
			if (!CurSelRange) {
				//log('CurSelRange was null, resetting to home');
				CurSelRange = { index: 0, length: 0 };
			}
			//log('Selection focus falling back to last: ' + JSON.stringify(CurSelRange));
			dom_focus_range = convertDocRangeToDomRange(CurSelRange);
		}
	}
	if (!dom_focus_range) {
		dom_focus_range = document.getSelection().getRangeAt(0);
	}

	return dom_focus_range;
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
		let sel = getDocSelection();
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

function getDocRangeScrollOffset(doc_range) {
	let scroll_offset = { top: 0, left: 0 };
	if (!doc_range) {
		return scroll_offset;
	}
	let dom_range = convertDocRangeToDomRange(doc_range);

	scroll_offset.top = dom_range.getBoundingClientRect().top;
	scroll_offset.left = dom_range.getBoundingClientRect().left;

	return scroll_offset;
}
// #endregion Getters

// #region Setters

function setDocSelection(doc_idx, len, source = 'user') {
	if (!quill.hasFocus()) {
		quill.focus();
	}
	CurSelRange = { index: doc_idx, length: len };
	quill.setSelection(doc_idx, len, source);
		
}

function setDomSelection(domRange) {
	document.getSelection().removeAllRanges();
	document.getSelection().addRange(domRange);
}

function setDomSelectionFromDocRange(docRange) {
	let domRange = convertDocRangeToDomRange(docRange);
	document.getSelection().removeAllRanges();
	document.getSelection().addRange(domRange);
}

function setDocSelectionRanges(docRanges, retainFocus = true) {
	let dom_focus_range = getDomFocusRange();

	clearDomSelectionRanges();

	if (docRanges) {
		for (var i = 0; i < docRanges.length; i++) {
			let dom_sel = convertDocRangeToDomRange(docRanges[i]);
			if (retainFocus && isDomRangeEqual(dom_sel, dom_focus_range)) {
				// if focus is in ranges wait till after looping so it stays focus
				continue;
			}
			document.getSelection().addRange(dom_sel);
		}
	}


	if (retainFocus && dom_focus_range) {
		document.getSelection().addRange(dom_focus_range);
	}
}

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

function isDomRangeEqual(dom_range_1, dom_range_2) {
	if (!dom_range_1 && !dom_range_2) {
		return true;
	}
	if (!dom_range_1) {
		return false;
	}
	if (!dom_range_2) {
		return false;
	}

	return
		dom_range_1.startContainer == dom_range_2.startContainer &&
		dom_range_1.endContainer == dom_range_2.endContainer &&
		dom_range_1.startOffset == dom_range_2.startOffset &&
		dom_range_1.endOffset == dom_range_2.endOffset;

}

function isNavJump() {
	let sel_range = getDocSelection();
	if (!sel_range || sel_range.length > 0) {
		return false;
	}
	if (!didSelectionChange(sel_range, LastSelRange)) {
		return false;
	}
	if (!LastSelRange) {
		return true;
	}
	return Math.abs(sel_range.index = LastSelRange.index) > 1;
}

function isNavRight() {
	let sel_range = getDocSelection();
	let last_sel_range = LastSelRange;

	last_sel_range = last_sel_range ? last_sel_range : sel_range;


	let old_closest_idx =
		sel_range.index > last_sel_range.index ?
			last_sel_range.index + last_sel_range.length : last_sel_range.index;

	let is_nav_right = sel_range.index > old_closest_idx && sel_range.length == 0;
	return is_nav_right;
}
// #endregion State

// #region Actions

function resetSelection() {
	LastSelRange = null;
	CurSelRange = null;
	SelectionOnMouseDown = null;
	BlurredSelectionRects = null;
	DragSelectionRange = null;
	clearDomSelectionRanges();
}

function clearDomSelectionRanges() {
	document.getSelection().removeAllRanges();
}

function convertDomRangeToDocRange(dom_range) {
	let sel = { index: 0, length: 0 };
	if (!dom_range ||
		dom_range.startContainer === undefined ||
		dom_range.endContainer === undefined) {
		return sel;
	}
	let start_elm_doc_idx = getElementDocIdx(dom_range.startContainer);
	let end_elm_doc_idx = getElementDocIdx(dom_range.endContainer);

	sel.index = start_elm_doc_idx + dom_range.startOffset;
	sel.length = (end_elm_doc_idx + dom_range.endOffset) - sel.index;

	return sel;
}

function convertDocRangeToDomRange(doc_range) {
	if (!doc_range || doc_range.index === undefined) {
		doc_range = { index: 0, length: 0 };
	}
	let start_elm = getElementAtDocIdx(doc_range.index);
	let end_elm = getElementAtDocIdx(doc_range.index + doc_range.length);

	let start_elm_doc_idx = getElementDocIdx(start_elm);
	let end_elm_doc_idx = getElementDocIdx(end_elm);


	let start_offset = doc_range.index - start_elm_doc_idx;
	let end_offset = (doc_range.index + doc_range.length) - end_elm_doc_idx;

	let clean_range = document.createRange();
	try {
		clean_range.setStart(start_elm, start_offset);
	} catch (ex) {
		log('exception converting doc2dom range. range: idx: ' + doc_range.index + ' len: ' + doc_range.length + ' exception: ');
		log(ex);
		debugger;
		//debugger;
		if (doc_range.index == 0) {
			// how do we deal with this case?
			debugger;
			return;
		} else {
			doc_range.index--;
			return convertDocRangeToDomRange(doc_range);
		}
	}
	

	try {
		clean_range.setEnd(end_elm, end_offset);
	} catch (ex) {
		if (doc_range.length == 0) {
			debugger;
			return;
		} else {
			doc_range.length--;
			return convertDocRangeToDomRange(doc_range);
		}
		debugger;
	}
	return clean_range;
}

function coerceCleanSelection(new_range,old_range) {
	if (didSelectionChange(new_range, old_range)) {
		//log('Sel Changed from Timer.');
		if (IsDragging) {
			if (DragSelectionRange) {
				new_range = DragSelectionRange;
			}
			log('drag detected sel timer overriding selection. LastRange: ', old_range, ' DragRange: ', DragSelectionRange);
			setDocSelection(old_range.index, old_range.length, 'silent');

			//drawOverlay();
			return new_range;
		}
	}
	return new_range;
}

function calculateTotalOffset(node, offset) {
	let total = offset
	let curNode = node

	while (curNode != getEditorElement()) {
		if (isClassInElementPath(curNode, TemplateEmbedClass)) {
			while (true) {
				curNode = curNode.parentElement;
				if (curNode.nodeType == 3 ||
					!curNode.classList.contains(TemplateEmbedClass)) {
					continue;
				}
				break;
			}
			
		}
		if (!curNode.includes(TemplateEmbedClass) && curNode.hasAttribute === 'function' &&
			parseBool(curNode.getAttribute('contenteditable')) === false) {
			curNode = curNode.parentElement;
		}
		if (curNode.previousSibling) {
			total += curNode.previousSibling.textContent.length

			curNode = curNode.previousSibling
		} else {
			curNode = curNode.parentElement
		}
	}

	return total
}


// #endregion Actions

// #region Event Handlers

function onDocumentSelectionChange(e) {
	// Selection Change issues:
	// 1. 
	let new_range = getDocSelection();
	new_range = coerceCleanSelection(new_range, CurSelRange);

	if (didSelectionChange(new_range, CurSelRange)) {
		LastSelRange = CurSelRange;
		CurSelRange = new_range;
		updateAllElements();

		if (SuppressNextSelChangedHostNotification) {
			SuppressNextSelChangedHostNotification = false;
		} else {
			onSelectionChanged_ntf(CurSelRange);
		}
	}
}

function onSelectionCheckTick(e) {
	CurSelRange = coerceCleanSelection();
}
// #endregion Event Handlers