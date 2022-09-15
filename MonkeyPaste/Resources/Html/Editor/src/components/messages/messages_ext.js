// this are only called from external sources and wrap in/out messaging
function checkIsEditorLoaded_ext() {
	if (IsLoaded) {
		return true;
	}
	return false;
}

function init_ext(initMsgStr_base64) {
	// input 'MpQuillLoadResponseMessage'

	log("init request: " + initMsgStr_base64);
	let initMsgObj = toJsonObjFromBase64Str(initMsgStr_base64);

	if (initMsgObj && initMsgObj.isEditorPlainHtmlConverter) {
		initPlainHtmlConverter();
		log('plainHtml converter initialized.');
		return;
	} 

	init(initMsgObj);

	if (initMsgObj.isReadOnlyEnabled) {
		enableReadOnly();
	}
	
	let initResponseMsg = {
		contentWidth: getContentWidth(),
		contentHeight: getContentHeight(),
		decodedTemplateGuids: getDecodedTemplateGuids(),
		contentLength: getDocLength()
	}
	let resp = toBase64FromJsonObj(initResponseMsg);
	//log('init Response: ');
	//log(initResponseMsgStr);

	return resp;
}

function convertPlainHtml_ext(convertPlainHtmlReqMsgBase64Str) {
	// input is MpQuillConvertPlainHtmlToQuillHtmlRequestMessage

	let convertPlainHtmlReqMsgObj = toJsonObjFromBase64Str(convertPlainHtmlReqMsgBase64Str);
	if (!convertPlainHtmlReqMsgObj || !convertPlainHtmlReqMsgObj.plainHtml) {
		return;
	}
	let qhtml = convertPlainHtml(convertPlainHtmlReqMsgObj.plainHtml);

	// output MpQuillConvertPlainHtmlToQuillHtmlResponseMessage
	let convertPlainHtmlRespMsg = {
		quillHtml: qhtml
	};
	let resp = toBase64FromJsonObj(convertPlainHtmlRespMsg);
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

	setEditorSelection(selMsg.index, selMsg.length);
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

function getDecodedTemplateGuids_ext() {
	// output MpQuillActiveTemplateGuidsRequestMessage

	let tgl = getDecodedTemplateGuids();
	let tgMsg = {
		templateGuids: tgl
	};
	let resp = toBase64FromJsonObj(tgMsg);
	return resp;
}

function enableReadOnly_ext() {
	enableReadOnly();

	// output 'MpQuillResponseMessage'  updated master collection of templates
	let qrmObj = {
		itemData: getEncodedHtml(),
		userDeletedTemplateGuids: userDeletedTemplateGuids,
		updatedAllAvailableTextTemplates: IsLoaded ? getAvailableTemplateDefinitions() : []
	};
	let resp = toBase64FromJsonObj(qrmObj);

	return resp;
}

function disableReadOnly_ext(disableReadOnlyReqStrOrObj) {
	// input MpQuillDisableReadOnlyRequestMessage

	let disableReadOnlyMsg = toJsonObjFromBase64Str(disableReadOnlyReqStrOrObj);
	availableTemplates = disableReadOnlyMsg.allAvailableTextTemplates;
	disableReadOnly(disableReadOnlyMsg.isSilent);

	// output MpQuillDisableReadOnlyResponseMessage

	let respObj = { editorWidth: DefaultEditorWidth };
	let resp = toBase64FromJsonObj(respObj);

	return resp; 
}

function enableSubSelection_ext() {
	enableSubSelection();
}

function disableSubSelection_ext() {
	disableSubSelection();
}

function selectAll_ext() {
	selectAll();
}

function updateModifierKeysFromHost_ext(modKeyMsgStr) {
	// input MpQuillModifierKeysNotification

	let modKeyMsg = toJsonObjFromBase64Str(modKeyMsgStr);
	modKeyMsg.fromHost = true;
	updateModKeys(modKeyMsg);
}

function updateIsDraggingFromHost_ext(isDraggingMsgStr) {
	// input MpQuillModifierKeysNotification

	let isDraggingMsg = toJsonObjFromBase64Str(isDraggingMsgStr);
	if (isDraggingMsg.isDragging) {
		startDrag();
	} else {
		endDrag();
	}
	
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

function resetDragDrop_ext() {
	resetDragDrop();
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
		let range_doc = domParser.parseFromString(rangeData);
		rangeData = range_doc.body.innerText;
	}

	// output MpQuillGetEncodedRangeDataResponseMessage
	let encRangeRespMsg = {
		encodedRangeData: rangeData
	};
	let resp = toBase64FromJsonObj(encRangeRespMsg);
	return resp;
}

async function getContentImageBase64Async_ext() {
	// output MpQuillGetEditorScreenshotResponseMessage
	let base64Str = await getContentImageBase64Async();
	let ssRespMsg = {
		base64ImgStr: base64Str
	};
	let resp = toBase64FromJsonObj(ssRespMsg);
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

function onDragEvent_ext(ddoMsgStr) {
	// input MpQuillDragDropDataObjectMessage

	let ddoMsg = toJsonObjFromBase64Str(ddoMsgStr);

	let sim_event = {
		dataTransfer: convertHostDataToDataTransferObject(ddoMsg)
	};
	sim_event.dataTransfer.fromHost = true;
	if (ddoMsg.eventType == 'dragenter') {
		onDragEnter(sim_event);
		return;
	}
	if (ddoMsg.eventType == 'dragover') {
		onDragOver(sim_event);
		return;
	}
	if (ddoMsg.eventType == 'dragleave') {
		//onDragLeave(sim_event);
		resetDragDrop();
		return;
	}
	if (ddoMsg.eventType == 'drop') {
		onDrop(sim_event);
		return;
	}
}
// unused


function getDropIdx_ext() {
	return DropIdx;
}

function getCharacterRect_ext(docIdxStr) {
	let idxVal = parseInt(docIdxStr);

	let rect = getCharacterRect(idxVal);
	let rectJsonStr = JSON.stringify(rect);
	return rectJsonStr;
}

function setHostDataObject_ext(hostDataObjMsgStr) {
	// input MpQuillDragDropDataObjectMessage
	let hostDataObj = toJsonObjFromBase64Str(hostDataObjMsgStr);
	if (hostDataObj && hostDataObj.items) {
		hostDataObj.items.forEach((item) => {
			log('data-item format: ' + item.format + ' data: ' + item.data);
		});
		CefDragData = hostDataObj;
	}

}