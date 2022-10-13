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

function onDropCompleted_ntf() {
	if (typeof notifyDropCompleted === 'function') {
		notifyDropCompleted();
	}
}
function onDragEnter_ntf() {
	if (typeof notifyDragEnter === 'function') {
		notifyDragEnter();
	}
}
function onDragLeave_ntf() {
	if (typeof notifyDragLeave === 'function') {
		notifyDragLeave();
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

function onReadOnlyChanged_ntf(isReadOnly) {
	// output (true) MpQuillEnableReadOnlyResponseMessage
	// output (false) MpQuillDisableReadOnlyResponseMessage

	if (!IsLoaded) {
		return;
	}
	if (isReadOnly) {
		if (typeof notifyReadOnlyEnabled === 'function') {
			let msg = {
				itemData: getHtml()
			};
			let msgStr = toBase64FromJsonObj(msg);
			notifyReadOnlyEnabled(msgStr);
		}
	} else {
		if (typeof notifyReadOnlyDisabled === 'function') {
			let msg = {
				editorWidth: getEditorWidth(),
				editorHeight: getEditorHeight()
			};
			let msgStr = toBase64FromJsonObj(msg);
			notifyReadOnlyDisabled(msgStr);
		}
	}	
}


function onDomLoaded_ntf() {
	if (typeof notifyDomLoaded === 'function') {
		notifyDomLoaded();
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

async function onCreateContentScreenShot_ntf(sel) {
	// output 'MpQuillContentScreenShotNotificationMessage'
	if (typeof notifyContentScreenShot === 'function') {
		let result = await getContentImageBase64Async(sel);
		let msg = {
			contentScreenShotBase64: result
		};
		let msgStr = toBase64FromJsonObj(msg);
		notifyContentScreenShot(msgStr);
	}	
}
