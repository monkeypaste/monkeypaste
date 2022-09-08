
function checkIsEditorLoaded_ext() {
	if (IsLoaded) {
		return true;
	}
	return false;
}

function init_ext(initMsgStr_base64) {
	log("init request: " + initMsgStr_base64);
	let initMsg = null;


	if (typeof initMsgStr_base64 === 'string' || initMsgStr_base64 instanceof String) {
		//if (hasJsonStructure(initMsgStr)) {

		//}
		let initMsgStr = atob(initMsgStr_base64);
		initMsg = JSON.parse(initMsgStr);
	} else {
		log('init_ext error initMsgStr: ' + initMsgStr_base64);
	}

	init(initMsg);

	if (initMsg.isReadOnlyEnabled) {
		enableReadOnly();
	}

	// init response is serialized 'MpQuillLoadResponseMessage'
	let initResponseMsg = {
		contentWidth: getContentWidth(),
		contentHeight: getContentHeight(),
		decodedTemplateGuids: getDecodedTemplateGuids()
	}
	let initResponseMsgStr = JSON.stringify(initResponseMsg);
	log('init Response: ');
	log(initResponseMsgStr);

	return initResponseMsgStr;
}

function getDocIdxFromPoint_ext(editorPointMsgStr) {
	// NOTE fallbackIdx is handy when user is dragging a template instance
	// so location freeze's when drag is out of bounds

	let editorPointMsgObj = null;
	if (typeof editorPointMsgStr === 'string' || editorPointMsgStr instanceof String) {
		editorPointMsgObj = JSON.parse(editorPointMsgStr);
	}

	if (editorPointMsgObj) {
		let p = { x: editorPointMsgObj.x, y: editorPointMsgObj.y };
		let fallbackIdx = editorPointMsgObj.fallbackIdx;
		let snapToLine = editorPointMsgObj.snapToLine;

		if (snapToLine) {
			return getDocIdxFromPoint(p, fallbackIdx);
		}
		return getDocIdxFromPoint(p, fallbackIdx);
	}
	return -1;
}

function setSelection_ext(selMsg) {
	let index = 0;
	let length = 0;
	if (typeof selMsg === 'string' || selMsg instanceof String) {
		selMsg = JSON.parse(selMsg);
	} else {
		log('setSelection_ext error parsing selMsg: ' + selMsg);
		return;
	}

	index = selMsg.index;
	length = selMsg.length;

	setEditorSelection(index, length);
}

function getCharacterRect_ext(docIdxStr) {
	let idxVal = parseInt(docIdxStr);

	let rect = getCharacterRect(idxVal);
	let rectJsonStr = JSON.stringify(rect);
	return rectJsonStr;
}

function getHtml_ext() {
	return getHtml();
}

function setHtml_ext(html) {
	setHtml(html);
}

function getText_ext(rangeObjParamStrOrNull) {
	let rangeObj = null;
	if (typeof rangeObjParamStrOrNull === 'string' || rangeObjParamStrOrNull instanceof String) {
		rangeObj = JSON.parse(rangeObjParamStrOrNull);
	} else if (rangeObjParamStrOrNull) {
		rangeObj = rangeObjParamStrOrNull;
	} else {
		rangeObj = { index: 0, length: quill.getLength() };
	}
	let text = getText(rangeObj);
	return text;
}

function setTextInRange_ext(textAndRangeMsgStr) {
	let rangeObj = { index: 0, length: quill.getLength() };
	let rangeText = '';
	if (typeof textAndRangeMsgStr === 'string' || textAndRangeMsgStr instanceof String) {
		let textAndRangeMsg = JSON.parse(textAndRangeMsgStr);
		rangeObj = { index: textAndRangeMsg.index, length: textAndRangeMsg.length };
		rangeText = textAndRangeMsg.text;
	} 

	setTextInRange(rangeObj, rangeText);
}

function getDecodedTemplateGuids_ext() {
	return getDecodedTemplateGuids();
}

function enableReadOnly_ext() {
	enableReadOnly();

	//return 'MpQuillResponseMessage'  updated master collection of templates
	let qrmObj = {
		itemEncodedHtmlData: getEncodedHtml(),
		userDeletedTemplateGuids: userDeletedTemplateGuids,
		updatedAllAvailableTextTemplates: IsLoaded ? getAvailableTemplateDefinitions() : []
	};
	let qrmJsonStr = JSON.stringify(qrmObj);

	//log("enableReadOnly() response msg:");
	//log(qrmJsonStr);

	return qrmJsonStr; //btoa(qrmJsonStr);
}

function disableReadOnly_ext(disableReadOnlyReqStrOrObj) {
	log('read-only: DISABLED');
	log('disableReadOnly msg:');
	log(disableReadOnlyReqStrOrObj);

	let disableReadOnlyMsg = null;

	if (disableReadOnlyReqStrOrObj == null) {
		disableReadOnlyMsg = {
			allAvailableTextTemplates: [],
			editorHeight: window.visualViewport.height,
			isSilent: false
		};
	} else if (typeof disableReadOnlyReqStrOrObj === 'string' || disableReadOnlyReqStrOrObj instanceof String) {
		//let disableReadOnlyReqStr_decoded = atob(disableReadOnlyReqStr);
		//disableReadOnlyMsg = JSON.parse(disableReadOnlyReqStr_decoded);
		disableReadOnlyMsg = JSON.parse(disableReadOnlyReqStrOrObj);
	} else {
		disableReadOnlyMsg = disableReadOnlyReqStrOrObj;
	}

	availableTemplates = disableReadOnlyMsg.allAvailableTextTemplates;

	disableReadOnly(disableReadOnlyMsg.isSilent);

	let droMsgObj = { editorWidth: DefaultEditorWidth };
	let droMsgJsonStr = JSON.stringify(droMsgObj);

	//log("disableReadOnly() response msg:");
	//log(droMsgJsonStr);

	return droMsgJsonStr; //btoa(droMsgJsonStr);
}

function enableSubSelection_ext() {
	enableSubSelection();
}

function disableSubSelection_ext() {
	disableSubSelection();
}

function isAllSelected_ext() {
	let result = isAllSelected();
	return result;
}

function getDropIdx_ext() {
	return DropIdx;
}
