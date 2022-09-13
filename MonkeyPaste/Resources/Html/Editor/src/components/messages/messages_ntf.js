// these functions wrap window binding so main editor doesn't worry about details

function onEditorSelectionChanged_ntf(range,isSelChangeBegin) {
	// output MpQuillContentSelectionChangedMessage
	let selChangedObj = {
		copyItemId: CopyItemId,
		index: range.index,
		length: range.length,
		isChangeBegin: isSelChangeBegin
	};

	let base64Str = toBase64FromJsonObj(selChangedObj);
	if (typeof notifyEditorSelectionChanged === 'function') {
		notifyEditorSelectionChanged(base64Str);
	}
}

function onContentLengthChanged_ntf() {
	// output MpQuillContentLengthChangedMessage

	if (typeof notifyContentLengthChanged === 'function') {
		let clMsg = {
			copyItemId: CopyItemId,
			length: getDocLength()
		};
		let msgStr = toBase64FromJsonObj(clMsg);
		return notifyContentLengthChanged(msgStr);
	}	
}

function onContentDraggableChanged_ntf(isDraggable) {
		// output MpQuillContentDraggableChangedMessage
	// should only be called on mouse down...
	log('is_draggable: ' + isDraggable);
	if (typeof notifyContentDraggableChanged === 'function') {
		let msg = {
			copyItemId: CopyItemId,
			isDraggable: isDraggable
		};
		let msgStr = toBase64FromJsonObj(msg);
		notifyContentDraggableChanged(msgStr);
	}
}


function onException_ntf(exType, exData) {
		// output MpQuillExceptionMessage
	if (typeof notifyException === 'function') {
		let exDataStr = null;
		if (typeof exData === 'string' || exData instanceof String) {
			exDataStr = exData;
		} else {
			exDataStr = JSON.stringify(exData);
		}
		log('');
		log('exception! ');
		log('exType: ' + exType);
		log('exDataStr: ' + exDataStr);
		log('');
		// out MpQuillExceptionMessage
		let msg = {
			copyItemId: CopyItemId,
			exType: exType,
			exData: exDataStr
		};
		let msgStr = toBase64FromJsonObj(msg);
		notifyException(msgStr);
	}	
} 
