// these functions wrap window binding so main editor doesn't worry about details

function onEditorSelectionChanged_ntf(range) {
	//let selRangeRects = getRangeRects(getSelection(), false, false);
	//let selRangeRectsJsonBase64 = toBase64(selRangeRects);
	let selChangedObj = {
		copyItemId: CopyItemId,
		index: range.index,
		length: range.length,
		//selJsonRectListBase64Str: selRangeRectsJsonBase64
		//selRects: selRangeRects
	};

	let base64Str = toBase64(selChangedObj);
	if (typeof notifyEditorSelectionChanged === 'function') {
		// is MpQuillContentSelectionChangedMessage
		notifyEditorSelectionChanged(base64Str);
	}
}

function onContentDraggableChanged_ntf(isDraggable) {
	// should only be called on mouse down...
	log('is_draggable: ' + isDraggable);
	if (typeof notifyContentDraggableChanged === 'function') {
		// is MpQuillContentDraggableChangedMessage
		let msg = {
			copyItemId: CopyItemId,
			isDraggable: isDraggable
		};
		let msgStr = JSON.stringify(msg);
		notifyContentDraggableChanged(msgStr);
	}
}
