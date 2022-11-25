// these are only called from external sources and wrap in/out messaging

function initMain_ext(initMsgStr_base64) {
	// input 'MpQuillInitMainRequestMessage'
	log('initMain_ext: ' + initMsgStr_base64);
	let initMsgObj = toJsonObjFromBase64Str(initMsgStr_base64);

	if (initMsgObj && initMsgObj.isPlainHtmlConverter) {
		initPlainHtmlConverter(initMsgObj.envName);
		log('plainHtml converter initialized.');
	} else {
		initMain(initMsgObj.envName);
	}
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
	loadContent(req.contentHandle, req.contentType, req.itemData, req.isPasteRequest, searchStateObj);

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

function getState_ext() {
	let cur_state = getState();
	let resp = toBase64FromJsonObj(cur_state);
	return resp;
}

function setState_ext(stateObjBase64Str) {
	let new_state = toJsonObjFromBase64Str(stateObjBase64Str);
	setState(new_state,true);
	return 'done';
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