// these functions wrap window binding so main editor doesn't worry about details
function onContentLoaded_ntf(conentMsg) {
	// output MpQuillEditorContentChangedMessage
	let msgStr = toBase64FromJsonObj(conentMsg);
	sendMessage('notifyLoadComplete', msgStr);
}

function onAnnotationSelected_ntf(ann_guid) {
	// output 'MpQuillAnnotationSelectedMessage'
	let msg = {
		annotationGuid: ann_guid
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyAnnotationSelected', msgStr);
}

function onReadOnlyChanged_ntf(isReadOnly) {
	// output (true) MpQuillEditorContentChangedMessage
	// output (false) MpQuillDisableReadOnlyResponseMessage

	if (!globals.IsLoaded) {
		return;
	}
	if (isReadOnly) {

		let msgStr = toBase64FromJsonObj(getContentAsMessage());
		sendMessage('notifyReadOnlyEnabled', msgStr);
	} else {
		let msg = {
			editorWidth: getEditorWidth(),
			editorHeight: getEditorHeight()
		};
		let msgStr = toBase64FromJsonObj(msg);
		sendMessage('notifyReadOnlyDisabled', msgStr);
	}
}

function onContentChanged_ntf() {
	// output 'MpQuillEditorContentChangedMessage'
	let msgStr = toBase64FromJsonObj(getContentAsMessage());
	sendMessage('notifyContentChanged', msgStr);
}

function onScrollBarVisibilityChanged_ntf(x_visible, y_visible) {
	// output 'MpQuillScrollBarVisibilityChangedNotification'
	const msg = {
		isScrollBarXVisible: x_visible,
		isScrollBarYVisible: y_visible
	};
	const msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyScrollBarVisibilityChanged', msgStr);
}

function onDropCompleted_ntf() {
	sendMessage('notifyDropCompleted', '');
}

function onDragEnter_ntf() {
	sendMessage('notifyDragEnter', '');
}

function onDragLeave_ntf() {
	sendMessage('notifyDragLeave', '');
}

function onDragEnd_ntf(hostEnded, canceled) {
	// output 'MpQuillDragEndMessage''
	let msg = {
		fromHost: hostEnded,
		wasCancel: canceled
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyDragEnd', msgStr);
}


function onSubSelectionEnabledChanged_ntf(isEnabled) {
	// output MpQuillSubSelectionChangedNotification

	let msg = {
		isSubSelectionEnabled: isEnabled
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifySubSelectionEnabledChanged', msgStr);
}


function onPasteRequest_ntf() {
	// isRequest is for paste (when true) or drag drop doesn't care just waits for this msg 
	sendMessage('notifyPasteRequest', '');
}

function onDomLoaded_ntf() {
	sendMessage('notifyDomLoaded', '');
}

function onUserDeletedTemplate_ntf(dtguid) {
	// output 'MpQuillUserDeletedTemplateNotification'
	log('userDeletedTemplate called for tguid: ' + dtguid);

	let ntf = {
		userDeletedTemplateGuid: dtguid
	};
	let msgStr = toBase64FromJsonObj(ntf);
	sendMessage('notifyUserDeletedTemplate', msgStr);
}

function onAddOrUpdateTemplate_ntf(t) {
	// output 'MpQuillTemplateAddOrUpdateNotification'
	log('addOrUpdateTemplate called for: ' + JSON.stringify(t));
	let ntf = {
		addedOrUpdatedTextTemplateBase64JsonStr: toBase64FromJsonObj(t)
	};
	let msgStr = toBase64FromJsonObj(ntf);
	sendMessage('notifyAddOrUpdateTemplate', msgStr);
}

function onException_ntf(exMsg, exUrl, exLine, exCol, exErrorObj) {
	// output 'MpQuillExceptionMessage'

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
	sendMessage('notifyException', msgStr);
} 

function onCreateContentScreenShot_ntf(ss_base64) {
	// output 'MpQuillContentScreenShotNotificationMessage'
	let msg = {
		contentScreenShotBase64: ss_base64
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyContentScreenShot', msgStr);
}

function onFindReplaceVisibleChange_ntf(isVisible) {
	// output 'MpQuillContentFindReplaceVisibleChanedNotificationMessage'
	let msg = {
		isFindReplaceVisible: isVisible
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyFindReplaceVisibleChange', msgStr);
}

function onQuerySearchRangesChanged_ntf(range_count) {
	// output 'MpQuillContentFindReplaceVisibleChanedotificationMessage'
	let msg = {
		rangeCount: range_count
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyQuerySearchRangesChanged', msgStr);
}

function onInitComplete_ntf() {
	//if (typeof notifyInitComplete === 'function') {
	//	notifyInitComplete();
	//}
	sendMessage('notifyInitComplete', '');
}

function onShowCustomColorPicker_ntf(dotnetHexStr,title) {
	// output 'MpQuillShowCustomColorPickerNotification'
	let msg = {
		currentHexColor: dotnetHexStr,
		pickerTitle: title
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyShowCustomColorPicker', msgStr);

	if (!isRunningOnHost()) {
		processCustomColorResult(getRandomColor());
	}
}

function onNavigateUriRequested_ntf(navUri,uriType,docIdx, elmText, curModKeys) {
	// output 'MpQuillNavigateUriRequestNotification'
	let msg = {
		uri: navUri,
		linkDocIdx: docIdx,
		linkType: uriType,
		linkText: elmText,
		modKeys: curModKeys
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyNavigateUriRequested', msgStr);
}

function onSetClipboardRequested_ntf() {
	// output 'MpQuillEditorSetClipboardRequestNotification'
	let msg = {
		// empty
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifySetClipboardRequested', msgStr);
}

function onDataTransferCompleted_ntf(changeDelta, input_dataObj, transfer_label) {
	// output 'MpQuillDataTransferCompletedNotification'
	let msg = {
		changeDeltaJsonStr: changeDelta ? toBase64FromJsonObj(changeDelta) : null,
		sourceDataItemsJsonStr: input_dataObj ? toBase64FromJsonObj(input_dataObj) : null,
		contentChangedMessageFragment: toBase64FromJsonObj(getContentAsMessage()),
		transferLabel: transfer_label
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyDataTransferCompleted', msgStr);
}

function onLastTransactionUndone_ntf() {
	// output 'MpQuillLastTransactionUndoneNotification' (empty)

	sendMessage('notifyLastTransactionUndone', '');
}

function onInternalContextMenuIsVisibleChanged_ntf(isVisible) {
	// output 'MpQuillInternalContextIsVisibleChangedNotification'
	let msg = {
		isInternalContextMenuVisible: isVisible
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyInternalContextMenuIsVisibleChanged', msgStr);
}

function onInternalContextMenuCanBeShownChanged_ntf(canBeShown) {
	// output 'MpQuillInternalContextMenuCanBeShownChangedNotification'
	let msg = {
		canInternalContextMenuBeShown: canBeShown
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyInternalContextMenuCanBeShownChanged', msgStr);
}

function onShowDebugger_ntf(debugReason, breakAfterSend) {
	// output 'MpQuillShowDebuggerNotification'
	let msg = {
		reason: debugReason
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyShowDebugger', msgStr);
	if (breakAfterSend) {
		// NOTE check parent in call stack
		sleep(3000);
		debugger;
	}
}

function onAppendStateChanged_ntf(appendDataStr = null) {	
	// output 'MpQuillAppendStateChangedMessage'
	if (appendDataStr && !isAppendNotifier()) {
		log('append error. only notifier should pass append data. data: ', appendDataStr);
		debugger;
		return;
	}
	let msg = {
		isAppendLineMode: globals.IsAppendLineMode,
		isAppendInsertMode: globals.IsAppendInsertMode,
		isAppendManualMode: globals.IsAppendManualMode,
		isAppendPaused: globals.IsAppendPaused,
		isAppendPreMode: globals.IsAppendPreMode,
		appendDocIdx: getAppendDocRange().index,
		appendDocLength: getAppendDocRange().length,
		appendData: appendDataStr
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyAppendStateChanged', msgStr);
}