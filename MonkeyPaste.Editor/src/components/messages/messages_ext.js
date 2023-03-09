// these are only called from external sources and wrap in/out messaging

function initMain_ext(initMsgStr_base64) {
	// input 'MpQuillInitMainRequestMessage'
	log('initMain_ext: ' + initMsgStr_base64);
	let initMsgObj = toJsonObjFromBase64Str(initMsgStr_base64);
	if (!initMsgObj) {
		log('init error, request null. ignoring');
		return;
	}
	initMain(initMsgObj.envName);
}

function loadContent_ext(loadContentMsgStr_base64) {
	// input 'MpQuillLoadContentRequestMessage'
	log('loadContent_ext: ' + loadContentMsgStr_base64);

	let req = toJsonObjFromBase64Str(loadContentMsgStr_base64);

	let searches = null;
	if (!isNullOrEmpty(req.searchesFragment)) {
		const searchesObj = toJsonObjFromBase64Str(req.searchesFragment);
		searches = searchesObj.searches;
	}
	loadContent(
		req.contentHandle,
		req.contentType,
		req.itemData,
		searches,
		req.isAppendLineMode,
		req.isAppendMode,
		req.annotationsJsonStr);
}

function contentChanged_ext(contentChangedMsgStr_base64) {
	// input 'MpQuillIsHostFocusedChangedMessage'
	log('content changed from host: ' + contentChangedMsgStr_base64);
	let msg = toJsonObjFromBase64Str(contentChangedMsgStr_base64);

	// only update if changed
	let cur_data = getContentAsMessage().itemData;
	if (cur_data == msg.itemData) {
		// no change so avoid infinite loop in append sync
		log('rejecting content changed, no change for me. appender: ' + isAppendNotifier() + ' appendee: ' + isAnyAppendEnabled());
		return;
	}
	log('');
	log('content change accepted. appender: ' + isAppendNotifier() + ' appendee: ' + isAnyAppendEnabled());
	log('mine:')
	log(cur_data);
	log('');
	log('incoming: ');
	log(msg.itemData);
	log('');
	loadContentData(msg.itemData);
}

function hostIsFocusedChanged_ext(hostIsFocusedMsgStr_base64) {
	// input 'MpQuillIsHostFocusedChangedMessage'
	log('hostIsFocusedChanged_ext: ' + hostIsFocusedMsgStr_base64);
	let msg = toJsonObjFromBase64Str(hostIsFocusedMsgStr_base64);
	setInputFocusable(msg.isHostFocused);
}

function updateContents_ext(updateContentsReqStr_base64) {
	// input 'MpQuillUpdateContentRequestMessage'
	let req = toJsonObjFromBase64Str(updateContentsReqStr_base64);
	let delta = toJsonObjFromBase64Str(req.deltaFragmentStr);

	const Delta = Quill.imports.delta;
	delta = new Delta(delta);
	applyDelta(delta);

	loadAnnotations(req.annotationFragmentStr);
}

function contentRequest_ext(contentReqMsgStr_base64) {
	// input 'MpQuillContentDataRequestMessage'
	// output 'MpQuillContentDataResponseMessage' (with 'MpQuillHostDataItemFragment' items)

	log('contentRequest_ext: ' + contentReqMsgStr_base64);
	let req = toJsonObjFromBase64Str(contentReqMsgStr_base64);
	let items = convertContentToFormats(req.forOle, req.formats);
	let respObj = {
		dataItems: items,
		isAllContent: isAllSelected(),
		isNoneSelected: isNoneSelected() 
	};
	let resp = toBase64FromJsonObj(respObj);
	return resp;
}

function convertPlainHtml_ext(convertPlainHtmlReqMsgBase64Str) {
	// input is MpQuillConvertPlainHtmlToQuillHtmlRequestMessage

	let req = toJsonObjFromBase64Str(convertPlainHtmlReqMsgBase64Str);
	if (!req || !req.data) {
		return;
	}
	let url = '';
	let plainHtml = '';
	let qhtml = '';
	let delta = '';

	if (req.isBase64) {
		plainHtml = b64_to_utf8(req.data);
	} else {
		plainHtml = req.data;
	}

	let is_html_cb_data = isHtmlClipboardFragment(plainHtml);
	if (is_html_cb_data) {
		// html is just plain html when coming from internal copy,cut, or drop 
		let cbData = parseHtmlFromHtmlClipboardFragment(plainHtml);
		plainHtml = cbData.html;
		url = cbData.sourceUrl;
	}
	let convert_result = convertPlainHtml(plainHtml, req.dataFormatType);

	let respObj = {
		html: toBase64FromJsonObj(plainHtml),
		quillHtml: toBase64FromJsonObj(convert_result.html),
		quillDelta: toBase64FromJsonObj(convert_result.delta),
		sourceUrl: url
	};
	let resp = toBase64FromJsonObj(respObj);
	return resp;
}

function enableReadOnly_ext() {
	// output 'MpQuillEditorContentChangedMessage'

	enableReadOnly(true);

	let edit_dt_msg_str = null;
	if (LastTextChangedDelta != null) {
		let dti_msg = {
			dataItems: [
				{
					format: URI_LIST_FORMAT,
					data: JSON.stringify([`${LOCAL_HOST_URL}/?type=UserDevice&id=-1`])
				}
			]
		};
		let edit_dt_msg = {
			changeDeltaJsonStr: toBase64FromJsonObj(JSON.stringify(LastTextChangedDelta)),
			sourceDataItemsJsonStr: toBase64FromJsonObj(dti_msg),
			transferLabel: 'Edited'
		};
		edit_dt_msg_str = toBase64FromJsonObj(edit_dt_msg);

		// clear delta log
		clearLastDelta();
	}

	let qrmObj = getContentAsMessage();
	qrmObj.dataTransferCompletedRespFragment = edit_dt_msg_str;
	let resp = toBase64FromJsonObj(qrmObj);

	return resp;
}

function disableReadOnly_ext(disableReadOnlyReqStrOrObj) {
	// input MpQuillDisableReadOnlyRequestMessage

	//let disableReadOnlyMsg = toJsonObjFromBase64Str(disableReadOnlyReqStrOrObj);
	//availableTemplates = disableReadOnlyMsg.allAvailableTextTemplates;
	disableReadOnly(true);
	clearLastDelta();

	// output MpQuillDisableReadOnlyResponseMessage

	let respObj = { editorWidth: DefaultEditorWidth };
	let resp = toBase64FromJsonObj(respObj);

	return resp; 
}

function enableSubSelection_ext() {
	enableSubSelection(true);
}

function disableSubSelection_ext() {
	disableSubSelection(true);
}

function updateModifierKeysFromHost_ext(modKeyMsgStr) {
	// input MpQuillModifierKeysNotification
	log('mod key msg from host recvd: ' + modKeyMsgStr);

	let modKeyMsg = toJsonObjFromBase64Str(modKeyMsgStr);
	modKeyMsg.fromHost = true;
	updateModKeys(modKeyMsg);
	drawOverlay();
}


function showFindAndReplace_ext() {
	if (isShowingFindReplaceToolbar()) {
		return;
	}
	showFindReplaceToolbar(true);
}

function hideFindAndReplace_ext() {
	if (!isShowingFindReplaceToolbar()) {
		return;
	}
	hideFindReplaceToolbar(true);
}

function activateFindReplace_ext(msgBase64Str) {
	// input 'MpQuillContentSearchRangeNavigationMessage'

	// NOTE ignoring isAbsoluteOffset because its a different msg
	// its usage is tbd
	let msg = toJsonObjFromBase64Str(msgBase64Str);
	log('findrepalce activated');
	if (msg) {
		// NOTE to not overcomplicate state change, incrementally nav to msg offset
		const diff = msg.curIdxOffset - CurFindReplaceDocRangeIdx;
		const dir = diff == 0 ? 0 : diff > 0 ? 1: -1;
		if (dir != 0) {
			while (CurFindReplaceDocRangeIdx != msg.curIdxOffset) {
				navigateFindReplaceResults(dir);
			}
		}
		
	}
	activateFindReplace();
}

function deactivateFindReplace_ext() {
	log('findrepalce deactivated');
	deactivateFindReplace();
}
function searchNavOffsetChanged_ext(msgBase64Str) {
	// input 'MpQuillContentSearchRangeNavigationMessage'

	let msg = toJsonObjFromBase64Str(msgBase64Str);
	navigateFindReplaceResults(msg.curIdxOffset);
}

function provideCustomColorPickerResult_ext(msgBase64Str) {
	// input 'MpQuillCustomColorResultMessage'
	let msg = toJsonObjFromBase64Str(msgBase64Str);
	processCustomColorResult(msg.customColorResult);
}

function getRequestResponse_ext(getRespBase64Str) {
	// input 'MpQuillGetResponseNotification'
	let msg = toJsonObjFromBase64Str(getRespBase64Str);
	PendingGetResponses.push(msg);
}

function disableWindowResizeUpdate_ext() {
	IsWindowResizeUpdateEnabled = false;
}

function enableWindowResizeUpdate_ext() {
	IsWindowResizeUpdateEnabled = true;
	onWindowResize();
}

function appendDataToNotifier_ext(reqMsgBase64Str) {
	// input 'MpQuillAppendStateChangedMessage'
	if (!isAppendNotifier()) {
		log('append error. only notifier should get data upate');
		debugger;
		return;
	}
	let req = toJsonObjFromBase64Str(reqMsgBase64Str);

	updateAppendModeState(
		req.isAppendLineMode,
		req.isAppendMode,
		req.isAppendManualMode,
		req.appendDocIdx,
		req.appendDocLength,
		req.appendData,
		false); // sync notifier w/ host state and append data

	onAppendStateChanged_ntf(req.appendData);
	onContentChanged_ntf();
}

function appendStateChanged_ext(reqMsgBase64Str) {
	// input 'MpQuillAppendStateChangedMessage'
	let req = toJsonObjFromBase64Str(reqMsgBase64Str);
	log('appendStateChanged_ext: ', req);

	updateAppendModeState(
		req.isAppendLineMode,
		req.isAppendMode,
		req.isAppendManualMode,
		req.appendDocIdx,
		req.appendDocLength,
		req.appendData,
		true);	
}

function annotationSelected_ext(reqMsgBase64Str) {
	// output 'MpQuillAnnotationSelectedMessage'
	let req = toJsonObjFromBase64Str(reqMsgBase64Str);
	selectAnnotation(req.annotationGuid, true);
}

function dragEventFromHost_ext(dragEnterMsgBase64Str) {
	// input 'MpQuillDragDropEventMessage'
	let req = toJsonObjFromBase64Str(dragEnterMsgBase64Str);
	log('drag event from host received. eventType: ' + req.eventType + ' screenX: '+req.screenX + ' screenY: '+req.screenY);
	req.buttons = 1;
	req.fromHost = true;
	req.target = getEditorContainerElement();
	req.dataTransfer = convertHostDataItemsToDataTransfer(req.dataItemsFragment);

	
	if (req.dataItemsFragment && req.dataItemsFragment.effectAllowed) {
		// NOTE cannot set dt 'effectAllowed' so manually specifiying in parent obj
		req.effectAllowed_override = req.dataItemsFragment.effectAllowed;
	} else {
		if (req.eventType != 'dragleave') {
			debugger;
		}
		req.effectAllowed_override = 'none';
	}
	
	if (req.eventType == 'dragenter') {
		onDragEnter(req);
	} else if (req.eventType == 'dragover') {
		onDragOver(req);
	} else if (req.eventType == 'dragleave') {
		onDragLeave(req);
	} else if (req.eventType == 'drop') {
		onDrop(req);
	}	
}
//function setSelection_ext(selMsgBase64Str) {
//	// input 'MpQuillSelectionChangedMessage'
//	let req = toJsonObjFromBase64Str(selMsgBase64Str);
//	log('recvd setSelection msg from host.did change: ' + (didSelectionChange(CurSelRange, req)) + ' value: ' + req);
//	if (didSelectionChange(CurSelRange, req)) {
//		SuppressNextSelChangedHostNotification = true;
//		setDocSelection(req);
//	}
//}
//function setScroll_ext(scrollMsgBase64Str) {
//	// input 'MpQuillScrollChangedMessage'
//	let req = toJsonObjFromBase64Str(scrollMsgBase64Str);
//	if (didEditorScrollChange(getEditorScroll(), req)) {
//		SuppressNextEditorScrollChangedNotification = true;
//		setEditorScroll(req);
//	}
//}


//function dragEnd_ext(dragEndMsg_base64str) {
//	// input MpQuillDragEndMessage
//	let dragEnd_e = toJsonObjFromBase64Str(dragEndMsg_base64str);
//	onDragEnd(dragEnd_e);
//	return 'done';
//}