function initDocument() {
	document.addEventListener('selectionchange', onDocumentSelectionChange);
}


function onDocumentSelectionChange(e) {
	// Enabled/Disabled with sub-selection
	//log('doc sel changed');
	updateTemplatesAfterSelectionChange();
	return;
	// NOTE quill only registers selection change on mouse up
	// this event is triggered at any point it changes (mainly for fancy selection)

	let range = getSelection();
	if (range) {
		//log("idx " + range.index + ' length "' + range.length);
		drawOverlay();
	} else {
		log('selection outside editor');
	}
}
