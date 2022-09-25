// these are only called from external sources and wrap in/out messaging

function initMain_ext(initMsgStr_base64) {
	// input 'MpQuillInitMainRequestMessage'
	// output 'MpQuillInitMainResponseMessage'

	log("init request: " + initMsgStr_base64);
	let initMsgObj = toJsonObjFromBase64Str(initMsgStr_base64);

	if (initMsgObj && initMsgObj.isEditorPlainHtmlConverter) {
		initPlainHtmlConverter();
		log('plainHtml converter initialized.');
		return;
	}

	let respMsg = {};
	respMsg.status = initMain(initMsgObj.envName);
	let resp = toBase64FromJsonObj(respMsg);

	return resp;
}

function loadContent_ext(loadContentMsgStr_base64) {
	// input 'MpQuillLoadContentRequestMessage'
	// output 'MpQuillLoadContentResponseMessage'

	let req = toJsonObjFromBase64Str(loadContentMsgStr_base64);

	loadContent(req.contentHandle, req.contentType, req.itemData, req.usedTextTemplates, req.isPasteRequest);

	let respObj = {
		contentWidth: getContentWidth(),
		contentHeight: getContentHeight(),
		//decodedTemplateGuids: getDecodedTemplateGuids(),
		//contentLength: getDocLength(),
		hasTemplates: HasTemplates
	}
	let resp = toBase64FromJsonObj(respObj);
	//log('init Response: ');
	//log(initResponseMsgStr);

	return resp;
}

function contentRequest_ext(contentReqMsgStr_base64) {
	// input 'MpQuillContentDataRequestMessage'
	// output 'MpQuillContentDataResponseMessage'

	let req = toJsonObjFromBase64Str(contentReqMsgStr_base64);

	let respObj = {
		formattedContentData: ''
	};

	//if (req.formatRequested == 'Text') {
	//	respObj.formattedContentData = getHtml();
	//} else {
	//	// todo add other types as necessary?
	//}
	respObj.formattedContentData = getHtml();
	let resp = toBase64FromJsonObj(respObj);
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

function setSelectionFromEdiotrPoint_ext(selMsgReq) {
	// input MpQuillSetSelectionFromEditorPointMessage

	let selMsg = toJsonObjFromBase64Str(selMsgReq);
	if (!selMsg) {
		log('cannot parse setSelection_ext msg: ' + selMsgReq);
		return '';
	}
	//let was_down = false;
	
	//if (can_drag && (selMsg.state == 'move' || selMsg.state == 'down')) {
	//	was_down = selMsg.state == 'down';
	//	selMsg.state = 'drag';
	//}
	//if (can_drag && selMsg.state == 'up') {
	//	selMsg.state = 'drop';
	//}

	let doc_idx = getDocIdxFromPoint(selMsg);
	if (selMsg.state == 'down') {

		setEditorContentEditable(true);
		setEditorSelection({ index: doc_idx, length: 0 });
		return;
	}
	if (selMsg.state == 'move') {
		let sel = getEditorSelection();
		if (doc_idx < sel.index) {
			let temp = sel.index;
			sel.index = doc_idx;
			sel.length = temp - doc_idx;
		} else {
			sel.length = doc_idx - sel.index;
		}

		setEditorSelection(sel);
		return;
	}

	if (selMsg.state == 'drag') {

		let modKeyMsg = toJsonObjFromBase64Str(selMsg.modkeyBase64Msg);

		let dt = new DataTransfer();
		dt.setData('text/plain', getSelectedText());
		dt.setData('text/html', getSelectedHtml());
		dt.setData('application/json/quill-delta', getSelectedDeltaJson());

		let e = {
			dataTransfer: dt,
			clientX: selMsg.x,
			clientY: selMsg.y,
			curretTarget: getEditorContainerElement(),
			ctrlKey: modKeyMsg.ctrlKey,
			altKey: modKeyMsg.altKey,
			shiftKey: modKeyMsg.shiftKey
		};
		let editor_rect = getEditorContainerRect();
		if (isDropping()) {
			if (!isPointInRect(editor_rect, { x: selMsg.x, y: selMsg.y })) {
				onContentDraggableChanged_ntf(false);
				onDragLeave(e);
			} else {
				onDragOver(e);
			}
			
		} else {
			onDragEnter(e);
		}
	}
	if (selMsg.state == 'drop') {
		let dt = new DataTransfer();
		dt.setData('text/plain', getSelectedText());
		dt.setData('text/html', getSelectedHtml());
		dt.setData('application/json/quill-delta', getSelectedDeltaJson());
		let e = {
			dataTransfer: dt
		};
		onDrop(e);
	}
	if (selMsg.state == 'up') {
		let can_drag = checkCanDrag(selMsg);
		onContentDraggableChanged_ntf(can_drag);
		//setEditorContentEditable(false);
		return;
	}
	log('unhandled selMsg.state: ' + selMsg.state);
	
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
	enableReadOnly(true);

	// output 'MpQuillResponseMessage'  updated master collection of templates
	let qrmObj = {
		itemData: getEncodedHtml(),
		//userDeletedTemplateGuids: userDeletedTemplateGuids,
		//updatedAllAvailableTextTemplates: IsLoaded ? getAvailableTemplateDefinitions() : []
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
	let sel = getEditorSelection();
	if (!sel) {
		return;
	}

	setEditorSelection(0, 0);
	if (IsSubSelectionEnabled) {
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

function updateIsDraggingFromHost_ext(isDraggingMsgStr) {
	// input MpQuillIsHostDraggingMessage

	// NOTE this msg is needed so its known to only reset drop and not drag after dragLeave
	// for drag feedback

	let isDraggingMsg = toJsonObjFromBase64Str(isDraggingMsgStr);
	if (isDraggingMsg.isDragging) {
		startDrag(true);
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
	resetDragDrop(true);
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