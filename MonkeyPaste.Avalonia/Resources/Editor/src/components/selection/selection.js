// #region Globals

// #endregion Globals

// #region Life Cycle

function initSelection() {
	document.addEventListener('selectionchange', onDocumentSelectionChange, true);
}

// #endregion Life Cycle

// #region Getters

function getTextSelectionFgColor() {
	return getElementComputedStyleProp(document.body, '--selfgcolor');
}

function getTextSelectionBgColor() {
	return getElementComputedStyleProp(document.body, '--selbgcolor');
}
function getTextSelectionInactiveBgColor() {
	return getElementComputedStyleProp(document.body, '--inactiveselbgcolor');
}
function getCaretColor() {
	//return getEditorElement().style.caretColor;
	return getElementComputedStyleProp(document.body, '--caretcolor');
}

function getDocAsRange() {
	return { index: 0, length: getDocLength() };
}

function getDocSelection(isForPaste = false) {
	let doc_sel = convertDomRangeToDocRange(getDomFocusRange());

	if (isForPaste && (!doc_sel || (doc_sel && doc_sel.length == 0))) {
		return getDocAsRange();
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
			if (!globals.CurSelRange) {
				//log('globals.CurSelRange was null, resetting to home');
				globals.CurSelRange = { index: 0, length: 0 };
			}
			//log('Selection focus falling back to last: ' + JSON.stringify(globals.CurSelRange));
			dom_focus_range = convertDocRangeToDomRange(globals.CurSelRange);
		}
	}
	if (!dom_focus_range) {
		dom_focus_range = document.getSelection().getRangeAt(0);
	}

	return dom_focus_range;
}

function getDocumentSelectionHtml(docSel) {
	range = docSel.getRangeAt(0);
	var clonedSelection = range.cloneContents();
	var div = document.createElement('div');
	div.appendChild(clonedSelection);
	let htmlStr = div.innerHTML;
	return htmlStr;
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
	setDomSelectionFromDocRange({ index: doc_idx, length: len });
	globals.CurSelRange = { index: doc_idx, length: len };
	
		
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
	setElementComputedStyleProp(document.body, '--selfgcolor', fgColor);
}

function setTextSelectionBgColor(bgColor) {
	setElementComputedStyleProp(document.body, '--selbgcolor', bgColor);
}


function setCaretColor(caretColor) {
	setElementComputedStyleProp(document.body, '--caretcolor', caretColor);
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
	if (!didSelectionChange(sel_range, globals.LastSelRange)) {
		return false;
	}
	if (!globals.LastSelRange) {
		return true;
	}
	return Math.abs(sel_range.index = globals.LastSelRange.index) > 1;
}

function isNavRight() {
	let sel_range = getDocSelection();
	let last_sel_range = globals.LastSelRange;

	last_sel_range = last_sel_range ? last_sel_range : sel_range;

	let old_closest_idx =
		sel_range.index > last_sel_range.index ?
			last_sel_range.index + last_sel_range.length : last_sel_range.index;

	let is_nav_right = sel_range.index > old_closest_idx && sel_range.length == 0;
	return is_nav_right;
}
// #endregion State

// #region Actions

function updateSelectionColors() {
	let sel = getDocSelection();
	let sel_bg_color = globals.DefaultSelectionBgColor;
	let sel_fg_color = globals.DefaultSelectionFgColor;
	let caret_color = globals.DefaultCaretColor;

	if (isDropping() || isDragging()) {
		if (isDragging()) {
			// ignoring invalidity if external drop
			let is_drop_valid = globals.DropIdx >= 0 || !isDropping();
			if (is_drop_valid) {
				if (isDragCopy()) {
					sel_bg_color = globals.DragCopySelBgColor;
					log('copy recognized in sel draw');
				}

				if (isDropHtml()) {
					sel_fg_color = globals.DragFormatedSelFgColor;
				}
			} else {
				sel_bg_color = globals.DragSelBgColor;
			}
		}
	} else if (isSubSelectionEnabled()) {
		if (isEditorToolbarVisible()) {
			if (isSelAtFocusTemplateInsert()) {
				// hide cursor within focus template
				caret_color = 'transparent';
			}
		} else {
			if (!isAnyAppendEnabled()) {
				// only make caret red when not appending
				caret_color = globals.SubSelCaretColor;

			}			
		}
		if (isPasting()) {
			sel_bg_color = globals.PastingSelBgColor;
		}
	} else {
		// in no select hide cursor
		caret_color = 'transparent';
	}

	setTextSelectionBgColor(sel_bg_color);
	setTextSelectionFgColor(sel_fg_color);

	setCaretColor(caret_color);

	// return sel for performance
	return sel;
}
function selectAll() {
	setDocSelection(0, getDocLength(), 'api');
	if (globals.ContentItemType == 'FileList') {
		selectAllFileItems();
	}
}

function resetSelection() {
	if (globals.ContentItemType == 'FileList') {
		deselectAllFileItems();
	}
	globals.LastSelRange = null;
	globals.CurSelRange = null;
	globals.SelectionOnMouseDown = null;
	clearDomSelectionRanges();
	setDocSelection({ index: 0, length: 0 });
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
	// NOTE creating local range here because it alters doc_range (occurs in file type find replace navigate)
	doc_range = isNullOrUndefined(doc_range) ? { index: 0, length: 0 } : doc_range;
	let copy_doc_range = { index: doc_range.index, length: doc_range.length };

	let clean_range = null;
	let start_elm = null;
	let end_elm = null;
	let start_offset = 0;
	let end_offset = 0;
	try {
		copy_doc_range = cleanDocRange(copy_doc_range);

		start_elm = getElementAtDocIdx(copy_doc_range.index);
		end_elm = getElementAtDocIdx(copy_doc_range.index + copy_doc_range.length);

		start_elm_doc_idx = getElementDocIdx(start_elm);
		end_elm_doc_idx = getElementDocIdx(end_elm);

		start_offset = Math.max(0,copy_doc_range.index - start_elm_doc_idx);
		end_offset = Math.max(0,(copy_doc_range.index + copy_doc_range.length) - end_elm_doc_idx);

		clean_range = document.createRange();
		clean_range.setStart(start_elm, start_offset);
	} catch (ex) {
		copy_doc_range.index--;
		if (copy_doc_range.index < 0) {
			// how do we deal with this case?
			let editor_range = document.createRange();
			editor_range.setStart(getEditorElement(), 0);
			editor_range.setEnd(getEditorElement(), 0);
			return editor_range;

			debugger;
			log('exception converting doc2dom range. range: idx: ' + copy_doc_range.index + ' len: ' + copy_doc_range.length + ' exception: ');
			log(ex);
			return;
		} 
		return convertDocRangeToDomRange(copy_doc_range);
	}

	if (!clean_range) {
		log('dom range null error in doc2dom range. range: idx: ' + copy_doc_range.index + ' len: ' + copy_doc_range.length + ' exception: ');		
		return null;
	}

	try {
		clean_range.setEnd(end_elm, end_offset);
	} catch (ex) {
		if (copy_doc_range.length == 0) {
			debugger;
			return;
		} else {
			copy_doc_range.length--;
			return convertDocRangeToDomRange(copy_doc_range);
		}
		debugger;
	}
	return clean_range;
}

function cleanDocRange(doc_range) {
	// this ensures range is within content limits
	if (!doc_range || doc_range.index === undefined) {
		doc_range = { index: 0, length: 0 };
	}
	if (doc_range.index < 0) {
		doc_range.index = 0;
	}
	// ex. docLength = 2, range = {2,1}

	let max_idx = Math.max(0,getDocLength() - 1); // is 1
	if (doc_range.index > max_idx) {
		doc_range.index = max_idx; // range = {1,1}
	}
	if (doc_range.index + doc_range.length > max_idx) {
		doc_range.length = max_idx - doc_range.index; // range = {1,0}
	}
	return doc_range;
}


// #endregion Actions

// #region Event Handlers

function onDocumentSelectionChange(e) {
	let new_range = getDocSelection();
	if (didSelectionChange(new_range, globals.CurSelRange)) {		
		globals.LastSelRange = globals.CurSelRange;
		globals.CurSelRange = new_range;
		if (new_range && new_range.length > 1) {
			globals.CurSelRects = getRangeRects(new_range);
		} else {			
			globals.CurSelRects = null;
		}
		updateAllElements();
	}
}
// #endregion Event Handlers