
function initDocument() {
	document.addEventListener('selectionchange', onDocumentSelectionChange);
}

function onDocumentSelectionChange(e) {
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
