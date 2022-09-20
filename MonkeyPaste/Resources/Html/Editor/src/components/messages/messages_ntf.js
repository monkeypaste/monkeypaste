// these functions wrap window binding so main editor doesn't worry about details

function onEditorSelectionChanged_ntf(range,isSelChangeBegin) {
	// output MpQuillContentSelectionChangedMessage

	let text = getSelectedText();
	let selChangedObj = {
		//contentHandle: ContentHandle,
		index: range.index,
		length: range.length,
		isChangeBegin: isSelChangeBegin,
		selText: text
	};

	let base64Str = toBase64FromJsonObj(selChangedObj);
	if (typeof notifyEditorSelectionChanged === 'function') {
		notifyEditorSelectionChanged(base64Str);
	}
}

function onContentLengthChanged_ntf() {
	// output MpQuillContentLengthChangedMessage

	let docLength = getDocLength();
	if (typeof notifyContentLengthChanged === 'function') {
		let clMsg = {
			//copyItemId: ContentHandle,
			length: docLength
		};
		let msgStr = toBase64FromJsonObj(clMsg);
		return notifyContentLengthChanged(msgStr);
	}	
}

function onContentDraggableChanged_ntf(isDraggable) {
	// output MpQuillContentDraggableChangedMessage

	// should only be called on mouse down...
	//log('is_draggable: ' + isDraggable);
	if (typeof notifyContentDraggableChanged === 'function') {
		let msg = {
			//copyItemId: ContentHandle,
			isDraggable: isDraggable
		};
		let msgStr = toBase64FromJsonObj(msg);
		notifyContentDraggableChanged(msgStr);
	}
}

function onDragStart_ntf() {
	// output MpQuillDragDropDataObjectMessage

	// should only be called on mouse down...
	if (typeof notifyDragStart === 'function') {
		let sel = getEditorSelection();
		let dragDropDataMsg = createHostMsgDataObjectObjectForRange(sel, 'drag');
		let msgStr = toBase64FromJsonObj(dragDropDataMsg);
		startDrag(true);
		notifyDragStart(msgStr);
	}
}

function onSubSelectionEnabledChanged_ntf(isEnabled) {
	// output MpQuillSubSelectionChangedNotification

	if (typeof notifySubSelectionEnabledChanged === 'function') {
		let msg = {
			isSubSelectionEnabled: isEnabled
		};
		let msgStr = toBase64FromJsonObj(msg);
		notifySubSelectionEnabledChanged(msgStr);
	}
}

function onDropEffectChanged_ntf(dropEffectStr) {
	// output MpQuillDropEffectChangedNotification

	if (!dropEffectStr) {
		dropEffectStr = 'none';
	}
	//log('drop effect: ' + dropEffectStr);
	if (typeof notifyDropEffectChanged === 'function') {
		let msg = {
			dropEffect: dropEffectStr
		};
		let msgStr = toBase64FromJsonObj(msg);
		notifyDropEffectChanged(msgStr);
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
			//copyItemId: ContentHandle,
			exType: exType,
			exData: exDataStr
		};
		let msgStr = toBase64FromJsonObj(msg);
		notifyException(msgStr);
	}	
} 
