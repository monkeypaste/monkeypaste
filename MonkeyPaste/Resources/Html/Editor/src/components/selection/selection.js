const DefaultSelectionBgColor = 'lightblue';
const DefaultSelectionFgColor = 'black';
const DefaultCaretColor = 'black';

var BlurredSelectionRects = null;

var LastSelRange = { index: 0, length: 0 };

var WasTextChanged = false;

var SelTimerInterval = null;

function initSelection() {	
	SelTimerInterval = setInterval(onSelectionCheckTick, 100);
}

function resetSelection() {
	LastSelRange = null;
	BlurredSelectionRects = null;
}

function onSelectionCheckTick(e) {
	//if (WasTextChanged) {
	//	LastSelRange = getDocumentSelection_internal();
	//	WasTextChanged = false;
	//	return;
	//}
	let cur_sel_range = getDocumentSelection();
	if (didSelectionChange(cur_sel_range, LastSelRange)) {		
		log('Sel Changed from Timer.');
		if (IsDragging) {
			if (DragSelectionRange) {
				LastSelRange = DragSelectionRange;
			}
			log('drag detected sel timer overriding selection. LastRange: ', LastSelRange, ' DragRange: ', DragSelectionRange);
			setEditorSelection(LastSelRange.index, LastSelRange.length, 'api');

			drawOverlay();
			return;
		}

		log('timer: index: ', cur_sel_range.index, ' length: ', cur_sel_range.length);

		let qsel = getEditorSelection();
		log('quill: index: ', qsel.index, ' length: ', qsel.length);

		let oldRange = LastSelRange;
		// updating Last
		LastSelRange = cur_sel_range;

		if (cur_sel_range) {
			refreshFontSizePicker(null,cur_sel_range);
			refreshFontFamilyPicker(null, cur_sel_range);
			updateTemplatesAfterSelectionChange(cur_sel_range, oldRange);
			onEditorSelectionChanged_ntf(cur_sel_range);
		}
		LastSelRange = cur_sel_range;

		drawOverlay();
	}
}

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

function getDocumentSelection() {
	let cur_sel = getDocumentSelection_internal();
	if (!cur_sel) {
		return LastSelRange;
	}
	return cur_sel;
}

function getDocumentSelection_internal() {
	if (window.getSelection().rangeCount == 0 || !quill || !quill.hasFocus()) {
		return null;
	}

	let range = window.getSelection().getRangeAt(0);
	let start_elm_doc_idx = getElementDocIdx(range.startContainer);
	let end_elm_doc_idx = getElementDocIdx(range.endContainer);

	let sel = {index: 0, length: 0};
	sel.index = start_elm_doc_idx + range.startOffset;
	sel.length = (end_elm_doc_idx + range.endOffset) - sel.index;
	return sel;
}

// unused
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


function setSelectionRelativeToElement(containerEl, savedSel) {
	var charIndex = 0, range = document.createRange();
	range.setStart(containerEl, 0);
	range.collapse(true);
	var nodeStack = [containerEl], node, foundStart = false, stop = false;

	while (!stop && (node = nodeStack.pop())) {
		if (node.nodeType == 3) {
			var nextCharIndex = charIndex + node.length;
			if (!foundStart && savedSel.start >= charIndex && savedSel.start <= nextCharIndex) {
				range.setStart(node, savedSel.start - charIndex);
				foundStart = true;
			}
			if (foundStart && savedSel.end >= charIndex && savedSel.end <= nextCharIndex) {
				range.setEnd(node, savedSel.end - charIndex);
				stop = true;
			}
			charIndex = nextCharIndex;
		} else {
			var i = node.childNodes.length;
			while (i--) {
				nodeStack.push(node.childNodes[i]);
			}
		}
	}

	var sel = window.getSelection();
	sel.removeAllRanges();
	sel.addRange(range);
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

// SEL FG/BG SET/GET

function setTextSelectionBgColor(bgColor) {
    document.body.style.setProperty('--selbgcolor', bgColor);
}

function getTextSelectionBgColor() {
    let bodyStyles = window.getComputedStyle(document.body);
    let bg_color = bodyStyles.getPropertyValue('--selbgcolor');
    return bg_color;
}

function setTextSelectionFgColor(fgColor) {
    document.body.style.setProperty('--selfgcolor', fgColor);
}

function getTextSelectionFgColor() {
    let bodyStyles = window.getComputedStyle(document.body);
    let fg_color = bodyStyles.getPropertyValue('--selfgcolor');
    return bg_color;
}

function setCaretColor(caretColor) {
    getEditorElement().style.caretColor = caretColor;
}

function getCaretColor() {
    return getEditorElement().style.caretColor;
}