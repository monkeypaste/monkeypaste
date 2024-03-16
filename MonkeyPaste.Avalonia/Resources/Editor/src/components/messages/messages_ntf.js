// these functions wrap window binding so main editor doesn't worry about details
function onContentLoaded_ntf(conentMsg) {
	// output MpQuillEditorContentChangedMessage
	let msgStr = toBase64FromJsonObj(conentMsg);
	sendMessage('notifyLoadComplete', msgStr);
}
function onContentImageLoaded_ntf(w, h) {
	// output 'MpQuillContentImageLoadedNotification'
	let msg = {
		width: w,
		height: h
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyContentImageLoaded', msgStr);
}

function onAnnotationSelected_ntf(ann_guid, dblClick) {
	// output 'MpQuillAnnotationSelectedMessage'
	let msg = {
		annotationGuid: ann_guid,
		isDblClick: dblClick
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

function onShowToolTip_ntf(is_visible, anchor_p, tt_html, tt_text, gesture_text) {
	// output 'MpQuillShowToolTipNotification'
	tt_html = isNullOrEmpty(tt_html) ? null : globals.DomParser.parseFromString(tt_html, 'text/html').documentElement.outerHTML;
	let msg = {
		tooltipHtml: tt_html,
		tooltipText: tt_text,
		gestureText: gesture_text,
		anchorX: anchor_p ? anchor_p.x : 0,
		anchorY: anchor_p ? anchor_p.y : 0,
		isVisible: is_visible
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyShowToolTip', msgStr);
}

function onContentChanged_ntf() {
	// output 'MpQuillEditorContentChangedMessage'
	let msgStr = toBase64FromJsonObj(getContentAsMessage());
	sendMessage('notifyContentChanged', msgStr);
}

function onScrollBarVisibilityChanged_ntf(can_x, can_y) {
	// output 'MpQuillOverrideScrollNotification'
	const msg = {
		canScrollX: can_x,
		canScrollY: can_y
	};
	//log('can x: ' + can_x + ' can y: ' + can_y);
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

function onException_ntf(ex_label,ex_msg) {
	// output 'MpQuillExceptionMessage'
	if (!isRunningOnHost()) {
		debugger;
	}
	log('');
	log('exception! ');
	log('Label: ' + ex_label);
	log(ex_msg);
	log('');

	let msg = {
		label: ex_label,
		msg: ex_msg
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
function appendStateChangeComplete_ntf() {
	sendMessage('notifyAppendStateChangeComplete', '');
}
function onQuerySearchRangesChanged_ntf(range_count,hl_html,range_offsets_csv) {
	// output 'MpQuillContentQuerySearchRangesChangedNotificationMessage'
	let msg = {
		rangeCount: range_count,
		highlightHtmlFragment: utf8_to_b64(hl_html),
		matchOffsetsCsvFragment: range_offsets_csv
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyQuerySearchRangesChanged', msgStr);
}

function onInitComplete_ntf() {
	// output 'MpQuillInitMainResponseMessage'
	let resp = '';
	if (isPlainHtmlConverter()) {
		let msg = {
			userAgent: navigator.userAgent
		};
		resp = toBase64FromJsonObj(msg);
	}
	sendMessage('notifyInitComplete', resp);
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

function onNavigateUriRequested_ntf(navUri,uriType,docIdx, elmText, curModKeys, confirm) {
	// output 'MpQuillNavigateUriRequestNotification'
	let msg = {
		uri: navUri,
		linkDocIdx: docIdx,
		linkType: uriType,
		linkText: elmText,
		modKeys: curModKeys,
		needsConfirm: confirm === undefined ? false : confirm
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
		changeDeltaJsonStr: changeDelta,
		sourceDataItemsJsonStr: input_dataObj,
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
function onPasteInfoFormatsClicked_ntf(pasteInfoGuid, doShow, x, y) {
	// output 'MpQuillPasteInfoFormatsClickedNotification'
	let msg = {
		infoId: pasteInfoGuid,
		isExpanded: doShow,
		offsetX: x || 0,
		offsetY: y || 0
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyPasteInfoFormatsClicked', msgStr);
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
	if (appendDataStr && !isAnyAppendEnabled()) {
		log('append error. only notifier should pass append data. data: ', appendDataStr);
		debugger;
		return;
	}
	let msg = {
		isAppendLineMode: isAppendLineMode(),
		isAppendInsertMode: isAppendInsertMode(),
		isAppendManualMode: isAppendManualMode(),
		isAppendPaused: isAppendPaused(),
		isAppendPreMode: isAppendPreMode(),
		appendDocIdx: getAppendDocRange().index,
		appendDocLength: getAppendDocRange().length,
		appendData: appendDataStr
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyAppendStateChanged', msgStr);
}

function onAnnDblClick_ntf(annGuid) {

}

function onPointerEvent_ntf(evtType, mp, is_left) {
	// output 'MpQuillPointerEventMessage'
	let msg = {
		clientX: mp.x,
		clientY: mp.y,
		eventType: evtType,
		isLeft: is_left
	};
	let msgStr = toBase64FromJsonObj(msg);
	sendMessage('notifyPointerEvent', msgStr);

}