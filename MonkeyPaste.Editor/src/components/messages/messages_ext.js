// these are only called from external sources and wrap in/out messaging

function initMain_ext(initMsgStr_base64) {
	// input 'MpQuillInitMainRequestMessage'
	// output 'MpQuillInitMainResponseMessage'

	let initMsgObj = toJsonObjFromBase64Str(initMsgStr_base64);

	if (initMsgObj && initMsgObj.isPlainHtmlConverter) {
		initPlainHtmlConverter(initMsgObj.envName);
		log('plainHtml converter initialized.');
	} else {
		initMain(initMsgObj.envName);
	}

	onInitComplete_ntf();
}

function loadContent_ext(loadContentMsgStr_base64) {
	// input 'MpQuillLoadContentRequestMessage'
	// output 'MpQuillLoadContentResponseMessage'
	let req = toJsonObjFromBase64Str(loadContentMsgStr_base64);

	log('load content request msg: ' + JSON.stringify(req));

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

function hostIsSelectedChanged_ext(hostIsSelectedMsgStr_base64) {
	let msg = toJsonObjFromBase64Str(hostIsSelectedMsgStr_base64);
	setInputFocusable(msg.isHostSelected);
}

function contentRequest_ext(contentReqMsgStr_base64) {
	// input 'MpQuillContentDataRequestMessage'
	// output 'MpQuillContentDataResponseMessage' (with 'MpQuillContentDataResponseFormattedDataItemFragment' items)

	let req = toJsonObjFromBase64Str(contentReqMsgStr_base64);
	let is_for_ole = req.forPaste || req.forDragDrop || req.forCutOrCopy;

	let respObj = {
		dataItems: convertContentToFormats(is_for_ole, req.formats)
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

function getDocIdxFromPoint_ext(editorPointMsgStr) {
	// input MpQuillEditorIndexFromPointRequestMessage

	// NOTE fallbackIdx is handy when user is dragging a template instance
	// so location freeze's when drag is out of bounds

	let editorPointMsgObj = toJsonObjFromBase64Str(editorPointMsgStr);
	let doc_idx = -1;

	if (editorPointMsgObj) {
		let p = { x: editorPointMsgObj.x, y: editorPointMsgObj.y };
		let fallbackIdx = editorPointMsgObj.fallbackIdx;
		let snapToLine = editorPointMsgObj.snapToLine;

		if (snapToLine) {
			doc_idx = getDocIdxFromPoint(p, fallbackIdx);
		} else {
			doc_idx = getDocIdxFromPoint(p, fallbackIdx);
		}
	}

	// output MpQuillEditorIndexFromPointResponseMessage
	let respObj = {
		docIdx: doc_idx
	};
	let resp = toBase64FromJsonObj(respObj);
	return resp;
}

function setSelection_ext(selMsgReq) {
	// input MpQuillSetSelectionRangeRequestMessage

	let selMsg = toJsonObjFromBase64Str(selMsgReq);
	if (!selMsg || selMsg.index === undefined) {
		log('cannot parse setSelection_ext msg: ' + selMsgReq);
		return '';
	}
	log('selection set from external: ', selMsg);

	setDocSelection(selMsg.index, selMsg.length,'api');
}


function getHtml_ext() {
	// output MpQuillGetRangeHtmlResponseMessage
	let respObj = {
		html: getHtml()
	};
	let resp = toBase64FromJsonObj(respObj);
	return resp;
}

function getText_ext(rangeObjParamStrOrNull) {
	// input MpQuillGetRangeTextRequestMessage
	let rangeReq = toJsonObjFromBase64Str(rangeObjParamStrOrNull)
	if (!rangeReq || rangeReq.index === undefined) {
		rangeReq = { index: 0, length: quill.getLength() };
	}
	let textStr = getText(rangeReq);

	// output MpQuillGetRangeTextResponseMessage

	let respObj = {
		text: textStr
	};
	let resp = toBase64FromJsonObj(respObj);
	return resp;
}

function setTextInRange_ext(setTextInRangeMsgStr) {
	// input MpQuillContentSetTextRangeMessage

	let setTextInRangeMsg = toJsonObjFromBase64Str(setTextInRangeMsgStr);

	let rangeObj = { index: setTextInRangeMsg.index, length: setTextInRangeMsg.length };
	let rangeText = setTextInRangeMsg.text;

	setTextInRange(rangeObj, rangeText);
}

function enableReadOnly_ext() {
	enableReadOnly(true);

	// output 'MpQuillResponseMessage'  updated master collection of templates
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

function selectAll_ext() {
	selectAll();
}

function deselectAll_ext() {
	let sel = getDocSelection();
	if (!sel) {
		return;
	}

	setDocSelection(0, 0);
	if (isSubSelectionEnabled()) {
		return;
	}
	getEditorContainerElement().style.userSelect = 'none';
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

function isAllSelected_ext() {
	// output MpQuillIsAllSelectedResponseMessage
	let is_all_selected = isAllSelected();
	let respObj = {
		isAllSelected: is_all_selected
	}
	let resp = toBase64FromJsonObj(respObj);
	return resp;
}


function dragEnd_ext(dragEndMsg_base64str) {
	// input MpQuillDragEndMessage
	let dragEnd_e = toJsonObjFromBase64Str(dragEndMsg_base64str);
	onDragEnd(dragEnd_e);
	return 'done';
}

function getEncodedDataFromRange_ext(encRangeMsgBase64Str) {
	// input MpQuillGetEncodedRangeDataRequestMessage
	let encRangeReqMsg = toJsonObjFromBase64Str(encRangeMsgBase64Str);
	if (encRangeReqMsg == null) {
		log('error opening host msg in "getEncodedDataFromRange_ext" data was (ignoring): ' + encRangeMsgBase64Str);
		return null;
	}
	let rangeData = getHtmlFromDocRange({ index: encRangeReqMsg.index, length: encRangeReqMsg.length });

	if (encRangeReqMsg.isPlainText) {
		let range_doc = DomParser.parseFromString(rangeData);
		rangeData = range_doc.body.innerText;
	}

	// output MpQuillGetEncodedRangeDataResponseMessage
	let encRangeRespMsg = {
		encodedRangeData: rangeData
	};
	let resp = toBase64FromJsonObj(encRangeRespMsg);
	return resp;
}

function getEditorScreenShot_ext() {
	// output MpQuillGetEditorScreenshotResponseMessage
	let base64Str = getEditorScreenShot();
	let ssRespMsg = {
		base64ImgStr: base64Str
	};
	let resp = toBase64FromJsonObj(ssRespMsg);
	return resp;
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
