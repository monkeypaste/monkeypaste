// these are only called from external sources and wrap in/out messaging

function initMain_ext(initMsgStr_base64) {
	// input 'MpQuillInitMainRequestMessage'
	log('initMain_ext: ' + initMsgStr_base64);
	let initMsgObj = toJsonObjFromBase64Str(initMsgStr_base64);
	if (!initMsgObj) {
		log('init error, request null. ignoring');
		return;
	}
	initMain(initMsgObj);
}

function initDefaults_ext(defaultsMsgStr_base64) {
	let defaults_obj = toJsonObjFromBase64Str(defaultsMsgStr_base64);
	initDefaults(defaults_obj);
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

	let append_state = null;
	if (!isNullOrEmpty(req.appendStateFragment)) {
		append_state = toJsonObjFromBase64Str(req.appendStateFragment);
	}

	loadContent(
		req.isReadOnly,
		req.isSubSelectionEnabled,
		req.contentHandle,
		req.contentType,
		req.itemData,
		searches,
		append_state,
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

	loadAnnotations(req.annotationFragmentStr, false);
}

function disableReadOnly_ext() {
	// input NONE

	//let disableReadOnlyMsg = toJsonObjFromBase64Str(disableReadOnlyReqStrOrObj);
	//globals.availableTemplates = disableReadOnlyMsg.allAvailableTextTemplates;
	disableReadOnly(true);
	clearLastDelta();

	// output MpQuillDisableReadOnlyResponseMessage

	let respObj = { editorWidth: globals.DefaultEditorWidth };
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
		const diff = msg.curIdxOffset - globals.CurFindReplaceDocRangeIdx;
		const dir = diff == 0 ? 0 : diff > 0 ? 1: -1;
		if (dir != 0) {
			while (globals.CurFindReplaceDocRangeIdx != msg.curIdxOffset) {
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
	globals.PendingGetResponses.push(msg);
}

function disableWindowResizeUpdate_ext() {
	globals.IsWindowResizeUpdateEnabled = false;
}

function enableWindowResizeUpdate_ext() {
	globals.IsWindowResizeUpdateEnabled = true;
	onWindowResize();
}


function appendStateChanged_ext(reqMsgBase64Str) {
	// input 'MpQuillAppendStateChangedMessage'
	let req = toJsonObjFromBase64Str(reqMsgBase64Str);
	log('appendStateChanged_ext: ', req);

	updateAppendModeState(req, true);	
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

function updateShortcuts_ext(shortcutsMsgBase64Str) {
	// input 'MpQuillEditorShortcutKeystringMessage'
	initShortcuts(shortcutsMsgBase64Str);
}

function sharedTemplateChanged_ext(changedTemplateTypeMsgBase64Str) {
	// input 'MpQuillSharedTemplateDataChangedMessage'
	let req = toJsonObjFromBase64Str(changedTemplateTypeMsgBase64Str);

	if (!isNullOrEmpty(req.deletedTemplateGuid)) {
		// change is delete
		let t_to_delete = getTemplateDefs().find(x => x.templateGuid == req.deletedTemplateGuid);
		if (!t_to_delete) {
			// no ref here ignore
			log('shared template ' + req.deletedTemplateGuid + ' NOT FOUND (for delete)');
			return;
		}
		removeTemplatesByGuid(req.deletedTemplateGuid);
		log('shared template ' + req.deletedTemplateGuid + ' REMOVED by host');
		return;
	}

	let changed_t = toJsonObjFromBase64Str(req.changedTemplateFragmentStr);

	// check if content contains changed template
	let t_to_update = getTemplateDefs().find(x => x.templateGuid == changed_t.templateGuid);
	if (!t_to_update) {
		// nothing to update
		log('shared template ' + req.deletedTemplateGuid + ' NOT FOUND (for update)');
		return;
	}
	// NOTE retain local template state but use all other updated data
	changed_t.templateState = t_to_update.templateState;

	// update all local instances of template and re-evaluate paste value
	setAllTemplateData(t_to_update.templateGuid, changed_t);
	log('shared template ' + req.deletedTemplateGuid + ' UPDATED by host');
}
//function setSelection_ext(selMsgBase64Str) {
//	// input 'MpQuillSelectionChangedMessage'
//	let req = toJsonObjFromBase64Str(selMsgBase64Str);
//	log('recvd setSelection msg from host.did change: ' + (didSelectionChange(globals.CurSelRange, req)) + ' value: ' + req);
//	if (didSelectionChange(globals.CurSelRange, req)) {
//		SuppressNextSelChangedHostNotification = true;
//		setDocSelection(req);
//	}
//}
//function setScroll_ext(scrollMsgBase64Str) {
//	// input 'MpQuillScrollChangedMessage'
//	let req = toJsonObjFromBase64Str(scrollMsgBase64Str);
//	if (didEditorScrollChange(getEditorScroll(), req)) {
//		globals.SuppressNextEditorScrollChangedNotification = true;
//		setEditorScroll(req);
//	}
//}


//function dragEnd_ext(dragEndMsg_base64str) {
//	// input MpQuillDragEndMessage
//	let dragEnd_e = toJsonObjFromBase64Str(dragEndMsg_base64str);
//	onDragEnd(dragEnd_e);
//	return 'done';
//}