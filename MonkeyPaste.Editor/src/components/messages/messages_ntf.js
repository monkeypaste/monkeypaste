// these functions wrap window binding so main editor doesn't worry about details
function onContentLoaded_ntf(conentMsg) {
	// output MpQuillEditorContentChangedMessage
	if (typeof notifyLoadComplete === 'function') {
		let msgStr = toBase64FromJsonObj(conentMsg);
		notifyLoadComplete(msgStr);
	}
}

function onReadOnlyChanged_ntf(isReadOnly) {
	// output (true) MpQuillEditorContentChangedMessage
	// output (false) MpQuillDisableReadOnlyResponseMessage

	if (!IsLoaded) {
		return;
	}
	if (isReadOnly) {
		if (typeof notifyReadOnlyEnabled === 'function') {
			let msgStr = toBase64FromJsonObj(getContentAsMessage());
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

function onContentChanged_ntf() {
	// output MpQuillEditorContentChangedMessage
	if (typeof notifyContentChanged === 'function') {
		let msgStr = toBase64FromJsonObj(getContentAsMessage());
		return notifyContentChanged(msgStr);
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


function onPasteRequest_ntf() {
	// isRequest is for paste (when true) or drag drop doesn't care just waits for this msg 
	if (typeof notifyPasteRequest === 'function') {
		notifyPasteRequest();
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

function onException_ntf(exMsg, exUrl, exLine, exCol, exErrorObj) {
	// output 'MpQuillExceptionMessage'

	if (typeof notifyException === 'function') {
		log('');
		log('exception! ');
		log('Msg: ' + exMsg);
		log('URL: ' + exUrl);
		log('Line: ' + exLine);
		log('');

		let msg = {
			msg: exMsg,
			url: exUrl,
			lineNum: exLine,
			colNum: exCol,
			errorObjJsonStr: JSON.stringify(exErrorObj)
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

async function onWaitTillPasteIsReady_ntf(req) {
	// output 'MpQuillContentDataResponseMessage' (with 'MpQuillContentDataResponseFormattedDataItemFragment' items)
	if (typeof notifyPasteIsReady === 'function') {
		showPasteToolbar(true);

		while (!IsReadyToPaste) {
			await delay(100);
		}

		let items = convertContentToFormats(true, req.formats);
		let respObj = {
			dataItems: items
		};
		let resp = toBase64FromJsonObj(respObj);

		// reset IsReadyToPaste so pasteButtonClick doesn't make extra paste request on subsequent click
		IsReadyToPaste = !hasAnyInputRequredTemplate();

		notifyPasteIsReady(resp);
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

function onNavigateUriRequested_ntf(navUri, curModKeys) {
	// output 'MpQuillNavigateUriRequestNotification'
	if (typeof notifyNavigateUriRequested === 'function') {
		let msg = {
			uri: navUri,
			modKeys: curModKeys
		};
		let msgStr = toBase64FromJsonObj(msg);
		notifyNavigateUriRequested(msgStr);
	}
}

function onSetClipboardRequested_ntf() {
	// output 'MpQuillEditorSetClipboardRequestNotification'
	if (typeof notifySetClipboardRequested === 'function') {
		let msg = {
			// empty
		};
		let msgStr = toBase64FromJsonObj(msg);
		notifySetClipboardRequested(msgStr);
	}
}

function onDataTransferCompleted_ntf(sourceUrl) {
	// output 'MpQuillDataTransferCompletedNotification'
	if (typeof notifyDataTransferCompleted === 'function') {
		let msg = {
			dataTransferSourceUrl: sourceUrl
		};
		let msgStr = toBase64FromJsonObj(msg);
		notifyDataTransferCompleted(msgStr);
	}
}

//function onAppendComplete_ntf(sourceUrl) {
//	// output 'MpQuillDataTransferCompletedNotification'
//	if (typeof notifyDataTransferCompleted === 'function') {
//		let msg = {
//			dataTransferSourceUrl: sourceUrl
//		};
//		let msgStr = toBase64FromJsonObj(msg);
//		notifyDataTransferCompleted(msgStr);
//	}
//}