
function contentDataObjectRequest_ext_ntf(contentReqMsgStr_base64) {
	// input 'MpQuillContentDataObjectRequestMessage'
	// output 'MpQuillContentDataObjectResponseMessage' (with 'MpQuillHostDataItemFragment' items)

	log('contentRequest_ext: ' + contentReqMsgStr_base64);
	let req = toJsonObjFromBase64Str(contentReqMsgStr_base64);
	let items = convertContentToFormats(req.forOle, req.formats);
	let respObj = {
		dataItems: items,
		isAllContent: isAllSelected(),
		isNoneSelected: isNoneSelected()
	};
	let resp = toBase64FromJsonObj(respObj);
	sendMessage('notifyDataObjectResponse', resp);
}


function enableReadOnly_ext_ntf() {
	// output 'MpQuillEditorContentChangedMessage'

	enableReadOnly(true);

	let edit_dt_msg_str = null;
	if (hasTextChangedDelta()) {
		let edit_dt_msg = getLastTextChangedDataTransferMessage();
		edit_dt_msg_str = toBase64FromJsonObj(edit_dt_msg);

		// clear delta log
		clearLastDelta();
	}

	let qrmObj = getContentAsMessage();
	qrmObj.dataTransferCompletedRespFragment = edit_dt_msg_str;
	let resp = toBase64FromJsonObj(qrmObj);

	sendMessage('notifyReadOnlyEnabledFromHost', resp);
}

function convertPlainHtml_ext_ntf(convertPlainHtmlReqMsgBase64Str) {
	// input 'MpQuillConvertPlainHtmlToQuillHtmlRequestMessage'
	// output 'MpQuillConvertPlainHtmlToQuillHtmlResponseMessage'
	let respObj = null;
	try {
		let req = toJsonObjFromBase64Str(convertPlainHtmlReqMsgBase64Str);
		if (!req || !req.data) {
			throw new Error('could not decode conversion request');
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

		respObj = {
			html: toBase64FromJsonObj(plainHtml),
			quillHtml: toBase64FromJsonObj(convert_result.html),
			quillDelta: toBase64FromJsonObj(convert_result.delta),
			sourceUrl: url,
			success: true
		};
	} catch (ex) {
		onException_ntf('error converting item', ex);

	}
	let resp = toBase64FromJsonObj(respObj);
	sendMessage('notifyPlainHtmlConverted', resp);
}
function selectionStateRequest_ext_ntf() {
	// output 'MpQuillEditorSelectionStateMessage'
	let sel = cleanDocRange(getDocSelection());
	let scroll = getEditorScroll();
	let respObj = {
		index: sel.index,
		length: sel.length,
		scrollLeft: scroll.left,
		scrollTop: scroll.top
	};

	let resp = toBase64FromJsonObj(respObj);

	sendMessage('notifySelectionState', resp);
}