﻿
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

	sendMessage('notifyReadOnlyEnabledFromHost', resp);
}

function convertPlainHtml_ext_ntf(convertPlainHtmlReqMsgBase64Str) {
	// input 'MpQuillConvertPlainHtmlToQuillHtmlRequestMessage'
	// output 'MpQuillConvertPlainHtmlToQuillHtmlResponseMessage'

	let req = toJsonObjFromBase64Str(convertPlainHtmlReqMsgBase64Str);
	if (!req || !req.data) {

		sendMessage('notifyPlainHtmlConverted', null);
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

	sendMessage('notifyPlainHtmlConverted', resp);
}