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

	let searchStateObj = null;
	if (!isNullOrEmpty(req.searchText)) {
		searchStateObj = {
			searchText: req.searchText,
			replaceText: null,
			isReplace: false,
			isCaseSensitive: req.isCaseSensitive,
			isWholeWordMatch: req.isWholeWord,
			useRegEx: req.useRegex
		};
	}
	loadContent(req.contentHandle, req.contentType, req.itemData, req.isPasteRequest, searchStateObj, req.isAppendLineMode, req.isAppendMode);
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

function contentRequest_ext(contentReqMsgStr_base64) {
	// input 'MpQuillContentDataRequestMessage'
	// output 'MpQuillContentDataResponseMessage' (with 'MpQuillContentDataResponseFormattedDataItemFragment' items)

	log('contentRequest_ext: ' + contentReqMsgStr_base64);
	let req = toJsonObjFromBase64Str(contentReqMsgStr_base64);
	//if (req.forPaste && hasTemplates()) {
	//	onWaitTillPasteIsReady_ntf(req);
	//	return;
	//}

	let items = convertContentToFormats(req.forOle, req.formats);
	let respObj = {
		dataItems: items,
		isAllContent: isAllSelected()
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
	qhtml = convertPlainHtml(plainHtml, req.dataFormatType);

	let respObj = {
		quillHtml: qhtml,
		sourceUrl: url
	};
	let resp = toBase64FromJsonObj(respObj);
	return resp;
}

function enableReadOnly_ext() {
	// output 'MpQuillResponseMessage'  updated master collection of templates

	enableReadOnly(true);

	let qrmObj = {
		itemData: getContentData()
	};
	let resp = toBase64FromJsonObj(qrmObj);

	return resp;
}

function disableReadOnly_ext(disableReadOnlyReqStrOrObj) {
	// input MpQuillDisableReadOnlyRequestMessage

	//let disableReadOnlyMsg = toJsonObjFromBase64Str(disableReadOnlyReqStrOrObj);
	//availableTemplates = disableReadOnlyMsg.allAvailableTextTemplates;
	disableReadOnly(true);

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

function dragEnd_ext(dragEndMsg_base64str) {
	// input MpQuillDragEndMessage
	let dragEnd_e = toJsonObjFromBase64Str(dragEndMsg_base64str);
	onDragEnd(dragEnd_e);
	return 'done';
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

function appendModeChanged_ext(reqMsgBase64Str) {
	// input 'MpQuillAppendModeChangedMessage'
	let msg = toJsonObjFromBase64Str(reqMsgBase64Str);
	if (msg.isAppendLineMode || msg.isAppendMode) {
		enableAppendMode(msg.isAppendLineMode, msg.isAppendManualMode, true);
	} else {
		disableAppendMode(true);
	}	
}

function appendData_ext(reqMsgBase64Str) {
	// input 'MpQuillAppendDataRequestMessage'
	log('append requested: ' + reqMsgBase64Str);
	let req = toJsonObjFromBase64Str(reqMsgBase64Str);
	appendContentData(req.appendData);
}

function setSelection_ext(selMsgBase64Str) {
	// input 'MpQuillSelectionChangedMessage'
	let req = toJsonObjFromBase64Str(selMsgBase64Str);
	if (didSelectionChange(CurSelRange, req)) {
		SuppressNextSelChangedHostNotification = true;
		setDocSelection(req);
	}	
}
function setScroll_ext(scrollMsgBase64Str) {
	// input 'MpQuillScrollChangedMessage'
	let req = toJsonObjFromBase64Str(scrollMsgBase64Str);
	if (didEditorScrollChange(getEditorScroll(), req)) {
		SuppressNextEditorScrollChangedNotification = true;
		setEditorScroll(req);
	}	
}