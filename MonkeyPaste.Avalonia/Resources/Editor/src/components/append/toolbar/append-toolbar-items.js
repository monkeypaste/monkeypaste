// #region Globals

// #endregion Globals

// #region Life Cycle

function initPasteAppendToolbarItems() {
	hidePasteAppendToolbar();
	updatePasteAppendToolbar();
}


// #endregion Life Cycle

// #region Getters

function getPasteAppendBeginButtonElement() {
	return document.getElementById('pasteAppendBeginButton');
}

function getPasteAppendToolbarContainerElement() {
	return document.getElementById('pasteAppendToolbarContainer');
}

function getPasteAppendToggleInlineButtonElement() {
	return document.getElementById('pasteAppendToggleInlineButton');
}
function getPasteAppendToggleManualButtonElement() {
	return document.getElementById('pasteAppendToggleManualButton');
}
function getPasteAppendToggleBeforeButtonElement() {
	return document.getElementById('pasteAppendToggleBeforeButton');
}

function getPasteAppendPauseAppendButtonElement() {
	return document.getElementById('pasteAppendPauseAppendButton');
}
function getPasteAppendStopAppendButtonElement() {
	return document.getElementById('pasteAppendStopAppendButton');
}

function getPasteAppendPopupOptElement() {
	let elms = Array.from(document.querySelectorAll('.append-toolbar-container.popup'));
	if (elms.length == 0) {
		return null
	}
	return elms[0];
}

function getPasteAppendToolbarButtonData(btnElm) {
	let ap_data = getAppendButtonLookup2();
	for (var i = 0; i < ap_data.length; i++) {
		if (ap_data[i].elm == btnElm) {
			return ap_data[i];
		}
	}
	return null;
}

function getAppendButtonLookup2() {
	let finish_enc_sc_str =
		isAppendInsertMode() ?
			getShortcutEncStr(globals.APD_INSERT_SCT_IDX) :
			getShortcutEncStr(globals.APD_LINE_SCT_IDX);


	let ap_btn_lookup = [
		{
			type: 'begin',
			enabled: isAnyAppendEnabled(),
			elm: getPasteAppendBeginButtonElement(),
			enabledInfo: {
				svgKey: 'stack',
				ttText: `${UiStrings.EditorPasteButtonAppendBeginLabel} ${getShortcutEncStr(globals.APD_LINE_SCT_IDX)}`
			},
			disabledInfo: {
				svgKey: 'stack',
				ttText: `${UiStrings.EditorPasteButtonAppendBeginLabel} ${getShortcutEncStr(globals.APD_LINE_SCT_IDX)}`
			},
			handler: function (e) {
				onPasteAppendOptButtonClick(e, 'begin');
			}
		},
		{
			type: 'insert',
			enabled: isAppendInsertMode(),
			elm: getPasteAppendToggleInlineButtonElement(),
			enabledInfo: {
				svgKey: 'text-insert-caret-outline',
				ttText: `${UiStrings.EditorAppendInlineModeLabel} ${getShortcutEncStr(globals.APD_LINE_SCT_IDX)}`
			},
			disabledInfo: {
				svgKey: 'paragraph',
				ttText: `${UiStrings.EditorAppendLineModeLabel} ${getShortcutEncStr(globals.APD_INSERT_SCT_IDX)}`
			},
			handler: function (e) {
				onPasteAppendOptButtonClick(e, 'insert');
			}
		},
		{
			type: 'manual',
			enabled: isAppendManualMode(),
			elm: getPasteAppendToggleManualButtonElement(),
			enabledInfo: {
				svgKey: 'scope',
				ttText: `${UiStrings.EditorAppendNonManualModeLabel} ${getShortcutEncStr(globals.APD_MANUAL_SCT_IDX)}`
			},
			disabledInfo: {
				svgKey: 'scope',
				ttText: `${UiStrings.EditorAppendManualModeLabel} ${getShortcutEncStr(globals.APD_MANUAL_SCT_IDX)}`
			},
			handler: function (e) {
				onPasteAppendOptButtonClick(e, 'manual');
			}
		},
		{
			type: 'pre',
			enabled: isAppendPreMode(),
			elm: getPasteAppendToggleBeforeButtonElement(),
			enabledInfo: {
				svgKey: 'triangle-up',
				ttText: `${UiStrings.EditorAppendPreLabel} ${getShortcutEncStr(globals.APD_PRE_SCT_IDX)}`
			},
			disabledInfo: {
				svgKey: 'triangle-down',
				ttText: `${UiStrings.EditorAppendPostLabel} ${getShortcutEncStr(globals.APD_PRE_SCT_IDX)}`
			},
			handler: function (e) {
				onPasteAppendOptButtonClick(e, 'pre');
			}
		},
		{
			type: 'paused',
			enabled: isAppendPaused(),
			elm: getPasteAppendPauseAppendButtonElement(),
			enabledInfo: {
				svgKey: 'pause',
				ttText: `${UiStrings.EditorAppendResumeLabel} ${getShortcutEncStr(globals.APD_PAUSED_SCT_IDX)}`
			},
			disabledInfo: {
				svgKey: 'pause',
				ttText: `${UiStrings.EditorAppendPauseLabel} ${getShortcutEncStr(globals.APD_PAUSED_SCT_IDX)}`
			},
			handler: function (e) {
				onPasteAppendOptButtonClick(e, 'paused');
			}
		},
		{
			type: 'stop',
			enabled: true,
			elm: getPasteAppendStopAppendButtonElement(),
			enabledInfo: {
				svgKey: 'stop',
				ttText: `${UiStrings.EditorAppendCloseLabel} ${finish_enc_sc_str}`
			},
			disabledInfo: {
				svgKey: 'stop',
				ttText: `${UiStrings.EditorAppendCloseLabel} ${finish_enc_sc_str}`
			},
			handler: function (e) {
				onPasteAppendOptButtonClick(e, 'stop');
			}
		}
	];
	return ap_btn_lookup;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isShowingPasteAppendPopupOptElement() {
	if (getPasteAppendPopupOptElement() != null) {
		return true;
	}
	return false;
}

// #endregion State

// #region Actions

function showPasteAppendToolbar() {
	getPasteAppendToolbarContainerElement().classList.add('expanded');
	getPasteAppendToolbarContainerElement().classList.remove('hover-border');
}

function hidePasteAppendToolbar() {
	getPasteAppendToolbarContainerElement().classList.remove('expanded');
	getPasteAppendToolbarContainerElement().classList.add('hover-border'); 
}
function hideAppendPopupOption() {
	let pu_elm = getPasteAppendPopupOptElement();
	if (!pu_elm) {
		return;
	}
	pu_elm.remove();
}

function updatePasteAppendToolbar() {
	isAnyAppendEnabled() ? showPasteAppendToolbar() : hidePasteAppendToolbar();
	let ap_btn_lookup = getAppendButtonLookup2();

	let start_idx = isAnyAppendEnabled() ? 1 : 0;
	let len = isAnyAppendEnabled() ? ap_btn_lookup.length : 1;

	for (var i = start_idx; i < len; i++) {
		let append_btn = ap_btn_lookup[i];
		let elm = append_btn.elm;

		append_btn.enabled ? elm.classList.add('enabled') : elm.classList.remove('enabled');
		let info = append_btn.enabled ? append_btn.enabledInfo : append_btn.disabledInfo;
		elm.innerHTML = getSvgHtml(info.svgKey);
		elm.setAttribute('hover-tooltip', info.ttText);
		
		elm.addEventListener('click', onPasteAppendToolbarButtonClick);
		//addClickOrKeyClickEventListener(elm, onClickHandler);
	}
	scrollToAppendIdx();
	drawOverlay();
}


function toggleShowAppendPopupOption(btnElm, type) {
	if (type == 'begin') {
		getAppendButtonLookup2()[0].handler(null, type);
		return;
	}
	hideAppendPopupOption();

	let apn_lookup = getAppendButtonLookup2();
	for (var i = 0; i < apn_lookup.length; i++) {
		let ap_data = apn_lookup[i];
		let ap_elm = ap_data.elm;
		if (ap_elm != btnElm) {
			continue;
		}
		let pu_info = ap_data.enabled ? ap_data.disabledInfo : ap_data.enabledInfo;

		let pu_btn_elm = document.createElement('button');
		pu_btn_elm.classList.add('popup');
		pu_btn_elm.innerHTML = getSvgHtml(pu_info.svgKey);
		pu_btn_elm.setAttribute('hover-tooltip', pu_info.ttText);

		let pu_btn_cnt_elm = document.createElement('div');
		pu_btn_cnt_elm.classList.add('append-toolbar-container');
		pu_btn_cnt_elm.classList.add('popup');
		pu_btn_cnt_elm.appendChild(pu_btn_elm);
		document.body.appendChild(pu_btn_cnt_elm);

		let anchor_rect = cleanRect(ap_elm.getBoundingClientRect());
		let cnt_rect = cleanRect(pu_btn_cnt_elm.getBoundingClientRect());

		let pos = calculateMenuOrigin(anchor_rect, cnt_rect, 'top-left', 'bottom-left', -5, -10);
		moveAbsoluteElement(pu_btn_cnt_elm, pos);
		initTooltip();
		
		addClickOrKeyClickEventListener(pu_btn_elm, ap_data.handler);
		break;
	}
	return true;
}
// #endregion Actions

// #region Event Handlers
function onWindowClickWithPasteAppendOptOpen(e) {
	if (isChildOfElement(e.target, getPasteAppendPopupOptElement()) ||
		isChildOfElement(e.target, getPasteAppendToolbarContainerElement())) {
		return;
	}
	hideAppendPopupOption();
}
function onPasteAppendToolbarButtonClick(e) {
	let btn_data = getPasteAppendToolbarButtonData(e.currentTarget);
	btn_data.handler(e);
	return;
	if (isShowingPasteAppendPopupOptElement()) {
		hideAppendPopupOption();
		return;
	}
	
	window.addEventListener('mousedown', onWindowClickWithPasteAppendOptOpen, true);

	if (btn_data.type == 'stop') {
		btn_data.handler(e);
		return;
	}
	toggleShowAppendPopupOption(e.currentTarget, btn_data.type)
}

function onPasteAppendOptButtonClick(e, type) {
	hideAppendPopupOption();
	switch (type) {
		case 'begin':
			enableAppendMode(true, false);
			break;
		case 'insert':
			enableAppendMode(!isAppendLineMode(), false);
			break;
		case 'manual':
			isAppendManualMode() ? disableAppendManualMode(false) : enableAppendManualMode(false);
			break;
		case 'pre':
			isAppendPreMode() ? disablePreAppend(false) : enablePreAppend(false);
			break;
		case 'paused':
			isAppendPaused() ? disablePauseAppend(false) : enablePauseAppend(false);
			break;
		case 'stop':
			disableAppendMode(false);
			break;
	}
	updatePasteAppendToolbar();
}

//function onPauseAppendButtonClickOrKey(e) {
//	e.currentTarget.classList.toggle('enabled');
//	let is_paused = e.currentTarget.classList.contains('enabled');
//	if (is_paused) {
//		enablePauseAppend(false);
//	} else {
//		disablePauseAppend(false);
//	}
//	updatePasteAppendToolbar();
//}

//function onStopAppendButtonClickOrKey(e) {
//	disableAppendMode(false);
//	updatePasteAppendToolbar();
//}

//function onAppendBeginButtonClickOrKey(e) {
//	showPasteAppendToolbar();
//	enableAppendMode(true, false);
//	scrollToAppendIdx();
//	updatePasteAppendToolbar();
//}
//function onAppendToggleInlineButtonClickOrKey(e) {
//	if (globals.ContentItemType == 'FileList') {
//		// ignore click on file item
//		return;
//	}
//	if (!toggleShowAppendPopupOption(e.currentTarget)) {
//		// no change
//		return;
//	}

//	//e.currentTarget.classList.toggle('enabled');


//	let is_line = !e.currentTarget.classList.contains('enabled');
//	enableAppendMode(is_line, false);
//	scrollToAppendIdx();
//	updatePasteAppendToolbar();
//}
//function onAppendToggleManualButtonClickOrKey(e) {
//	toggleShowAppendPopupOption(e.currentTarget);
//	e.currentTarget.classList.toggle('enabled');

//	let is_manual = e.currentTarget.classList.contains('enabled');
//	if (is_manual) {
//		enableAppendManualMode(false);
//	} else {
//		disableAppendManualMode(false);
//	}
//	scrollToAppendIdx();
//	updatePasteAppendToolbar();
//}
//function onAppendToggleBeforeButtonClickOrKey(e) {
//	e.currentTarget.classList.toggle('enabled');

//	let is_before = e.currentTarget.classList.contains('enabled');
//	if (is_before) {
//		enablePreAppend(false);
//	} else {
//		disablePreAppend(false);
//	}
//	scrollToAppendIdx();
//	updatePasteAppendToolbar();
//}

// #endregion Event Handlers