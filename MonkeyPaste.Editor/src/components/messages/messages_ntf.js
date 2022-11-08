// these functions wrap window binding so main editor doesn't worry about details

function onDocSelectionChanged_ntf(range,isSelChangeBegin) {
	// output MpQuillContentSelectionChangedMessage

	let text = getText(range,false); 
	let selChangedObj = {
		//contentHandle: ContentHandle,
		index: range.index,
		length: range.length,
		isChangeBegin: isSelChangeBegin,
		selText: text
	};

	let base64Str = toBase64FromJsonObj(selChangedObj);
	if (typeof notifyDocSelectionChanged === 'function') {
		notifyDocSelectionChanged(base64Str);
	}
}

function onContentLengthChanged_ntf() {
	// output MpQuillContentLengthChangedMessage

	let docLength = getDocLength();
	if (typeof notifyContentLengthChanged === 'function') {
		let clMsg = {
			//copyItemId: ContentHandle,
			length: docLength,
			lines: parseInt_safe(getLineCount())
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

function onContentLoaded_ntf() {
	if (typeof notifyLoadComplete === 'function') {
		let respObj = {
			contentWidth: getContentWidth(),
			contentHeight: getContentHeight(),
			lineCount: parseInt_safe(getContentHeightByType()),
			charCount: parseInt_safe(getContentWidthByType()),
			hasTemplates: hasTemplates()
		}
		log('load content resp msg: ' + respObj);
		let resp = toBase64FromJsonObj(respObj);
		notifyLoadComplete(resp);
	}
}

function onSubSelectionEnabledChanged_ntf(isEnabled) {
	// output MpQuillSubSelectionChangedNotification

	if (typeof notifySubSelectionEnabledChanged === 'function') {
		let msg = {
			isSubSelectionEnabled: isEnabled
		};
		if (isEnabled && hasTemplates()) {
			msg.editorWidth = PasteToolbarDefaultWidth;
		}
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

function onPasteTemplateRequest_ntf() {
	// isRequest is for paste (when true) or drag drop doesn't care just waits for this msg 
	if (typeof notifyPasteTemplateRequest === 'function') {
		notifyPasteTemplateRequest();
	}
}

function onDomLoaded_ntf() {
	if (typeof notifyDomLoaded === 'function') {
		notifyDomLoaded();
	}
}

function onUserDeletedTemplate_ntf(dtguid) {
	// output 'MpQuillUserDeletedTemplateNotification'
	log('userDeletedTemplate called for tguid: ' + dtguid);

	if (typeof notifyUserDeletedTemplate === 'function') {
		let ntf = {
			userDeletedTemplateGuid: dtguid
		};

		notifyUserDeletedTemplate(toBase64FromJsonObj(ntf));
	}
}

function onAddOrUpdateTemplate_ntf(t) {
	// output 'MpQuillTemplateAddOrUpdateNotification'
	log('addOrUpdateTemplate called for: ' + JSON.stringify(t));
	if (typeof notifyAddOrUpdateTemplate === 'function') {
		let ntf = {
			addedOrUpdatedTextTemplateBase64JsonStr: toBase64FromJsonObj(t)
		};
		notifyAddOrUpdateTemplate(toBase64FromJsonObj(ntf));
	}
}

function onException_ntf(exType, exData) {
	// output 'MpQuillExceptionMessage'

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

function onFindReplaceVisibleChange_ntf(isVisible) {
	// output 'MpQuillContentFindReplaceVisibleChanedNotificationMessage'
	if (typeof notifyFindReplaceVisibleChange === 'function') {
		let msg = {
			isFindReplaceVisible: isVisible
		};
		let msgStr = toBase64FromJsonObj(msg);
		notifyFindReplaceVisibleChange(msgStr);
	}
}

function onQuerySearchRangesChanged_ntf(range_count) {
	// output 'MpQuillContentFindReplaceVisibleChanedotificationMessage'
	if (typeof notifyQuerySearchRangesChanged === 'function') {
		let msg = {
			rangeCount: range_count
		};
		let msgStr = toBase64FromJsonObj(msg);
		notifyQuerySearchRangesChanged(msgStr);
	}
}

function onInitComplete_ntf() {
	if (typeof notifyInitComplete === 'function') {
		notifyInitComplete();
	}
}

function onShowCustomColorPicker_ntf(hexStr,title) {
	// output 'MpQuillShowCustomColorPickerNotification'
	if (typeof notifyShowCustomColorPicker === 'function') {
		let msg = {
			currentHexColor: hexStr,
			pickerTitle: title
		};
		let msgStr = toBase64FromJsonObj(msg);
		notifyShowCustomColorPicker(msgStr);
	}
}