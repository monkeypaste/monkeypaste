
function initDocument() {
	document.addEventListener('selectionchange', onDocumentSelectionChange);
}

function onDocumentSelectionChange(e) {
	if (IsDragging && DragSelectionRange) {
		setEditorSelection(DragSelectionRange.index, DragSelectionRange.length, 'api');
		return;
	}
	if (WasDragCanceled && DragSelectionRange) {
		setEditorSelection(DragSelectionRange.index, DragSelectionRange.length, 'api');
		// NOTE only clearing drag range on cancel or this will always override sel
		DragSelectionRange = null;
		return;
	}

	return;
	if (IsDragging || IsDropping || WindowMouseDownLoc == null) {
		return;
	}
	let safe_range = getEditorSelection();
	//log('safe range idx: ' + safe_range.index + ' length: ' + safe_range.length);
	updateTemplatesAfterSelectionChange(safe_range);
	return;
	// NOTE quill only registers selection change on mouse up
	// this event is triggered at any point it changes (mainly for fancy selection)

	let range = getEditorSelection();
	if (range) {
		//log("idx " + range.index + ' length "' + range.length);
		drawOverlay();
	} else {
		log('selection outside editor');
	}
}

function getDocumentSelection() {
	let sel = getSelectionRelativeToElement(getEditorElement());
	return sel;
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

function getSelectionRelativeToElement(elm) {
	var range = window.getSelection().getRangeAt(0);
	var preSelectionRange = range.cloneRange();

	preSelectionRange.selectNodeContents(elm);
	preSelectionRange.setEnd(range.startContainer, range.startOffset);
	var start = preSelectionRange.toString().length;

	return {
		start: start,
		end: start + range.toString().length
	}
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
