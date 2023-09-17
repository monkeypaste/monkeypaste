// #region Globals

// #endregion Globals

// #region Life Cycle

function initPasteAppendToolbarItems() {
	hidePasteAppendToolbar();
	attachAppendButtonHandlers();
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

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions
function attachAppendButtonHandlers() {
	const capture = true;
	addClickOrKeyClickEventListener(getPasteAppendBeginButtonElement(), onAppendBeginButtonClickOrKey, capture);
	addClickOrKeyClickEventListener(getPasteAppendToggleInlineButtonElement(), onAppendToggleInlineButtonClickOrKey, capture);
	addClickOrKeyClickEventListener(getPasteAppendToggleManualButtonElement(), onAppendToggleManualButtonClickOrKey, capture);
	addClickOrKeyClickEventListener(getPasteAppendToggleBeforeButtonElement(), onAppendToggleBeforeButtonClickOrKey, capture);

	addClickOrKeyClickEventListener(getPasteAppendPauseAppendButtonElement(), onPauseAppendButtonClickOrKey, capture);
	addClickOrKeyClickEventListener(getPasteAppendStopAppendButtonElement(), onStopAppendButtonClickOrKey, capture);

	if (globals.ContentItemType == 'FileList') {
		getPasteAppendToggleInlineButtonElement().classList.add('disabled');
	} else {
		getPasteAppendToggleInlineButtonElement().classList.remove('disabled');
	}
}

function getAppendButtonLookup() {
	let ap_btn_lookup = [
		[
			isAppendInsertMode(),
			getPasteAppendToggleInlineButtonElement(),
			[
				'text-insert-caret-outline',
				`${globals.AppendInlineModeLabel} ##ToggleAppendInsertMode##`
			],
			[
				'paragraph',
				`${globals.AppendLineModeLabel} ##ToggleAppendLineMode##`
			],
		],
		[
			isAppendManualMode(),
			getPasteAppendToggleManualButtonElement(),
			[
				'scope',
				`${globals.AppendNonManualModeLabel}`
			],
			[
				'scope',
				`${globals.AppendManualModeLabel}`
			]
		],
		[
			isAppendPreMode(),
			getPasteAppendToggleBeforeButtonElement(),
			[
				'triangle-up',
				`${globals.AppendPreLabel} ##ToggleAppendPreMode##`
			],
			[
				'triangle-down',
				`${globals.AppendPostLabel} ##ToggleAppendPreMode##`
			],
		],
		[
			isAppendPaused(),
			getPasteAppendPauseAppendButtonElement(),	
			[
				'pause',
				`${globals.AppendResumeLabel}`
			],
			[
				'pause',
				`${globals.AppendPauseLabel}`
			],
			
		],
	];
	if (globals.ContentItemType == 'FileList') {
		// NOTE file list is block-only, ensure
		// item0 always shows paragraph thing
		ap_btn_lookup[0][0] = false;
	}
	return ap_btn_lookup;
}

function showPasteAppendToolbar() {
	getPasteAppendToolbarContainerElement().classList.add('expanded');
	getPasteAppendToolbarContainerElement().classList.remove('hover-border');
}

function hidePasteAppendToolbar() {
	getPasteAppendToolbarContainerElement().classList.remove('expanded');
	getPasteAppendToolbarContainerElement().classList.add('hover-border'); 
}
function updatePasteAppendToolbar() {
	if (!isAnyAppendEnabled()) {
		hidePasteAppendToolbar();
		return;
	}
	showPasteAppendToolbar();
	let ap_btn_lookup = getAppendButtonLookup();

	for (var i = 0; i < ap_btn_lookup.length; i++) {
		let append_btn = ap_btn_lookup[i];

		let is_enabled = append_btn[0];
		let elm = append_btn[1];
		let enabled_info = append_btn[2];
		let disabled_info = append_btn[3];

		let enabled_svg_key = enabled_info[0];
		let enabled_tt_text = enabled_info[1];
		let disabled_svg_key = disabled_info[0];
		let disabled_tt_text = disabled_info[1];

		let svg_key = '', tt_text = '';
		if (is_enabled) {
			elm.classList.add('enabled');
			svg_key = enabled_svg_key;
			tt_text = enabled_tt_text;
		} else {
			elm.classList.remove('enabled');
			svg_key = disabled_svg_key;
			tt_text = disabled_tt_text;
		}
		//tt_text = decodeStringWithShortcut(tt_text);
		elm.innerHTML = getSvgHtml(svg_key);
		elm.setAttribute('hover-tooltip', tt_text);
	}
	drawOverlay();
}
// #endregion Actions

// #region Event Handlers

function onPauseAppendButtonClickOrKey(e) {
	e.currentTarget.classList.toggle('enabled');
	let is_paused = e.currentTarget.classList.contains('enabled');
	if (is_paused) {
		enablePauseAppend(false);
	} else {
		disablePauseAppend(false);
	}
	updatePasteAppendToolbar();
}
function onStopAppendButtonClickOrKey(e) {
	disableAppendMode(false);
	updatePasteAppendToolbar();
}

function onAppendBeginButtonClickOrKey(e) {
	showPasteAppendToolbar();
	enableAppendMode(true, false);
	scrollToAppendIdx();
	updatePasteAppendToolbar();
}
function onAppendToggleInlineButtonClickOrKey(e) {
	if (globals.ContentItemType == 'FileList') {
		// ignore click on file item
		return;
	}
	e.currentTarget.classList.toggle('enabled');

	let is_line = !e.currentTarget.classList.contains('enabled');
	enableAppendMode(is_line, false);
	scrollToAppendIdx();
	updatePasteAppendToolbar();
}
function onAppendToggleManualButtonClickOrKey(e) {
	e.currentTarget.classList.toggle('enabled');

	let is_manual = e.currentTarget.classList.contains('enabled');
	if (is_manual) {
		enableAppendManualMode(false);
	} else {
		disableAppendManualMode(false);
	}
	scrollToAppendIdx();
	updatePasteAppendToolbar();
}
function onAppendToggleBeforeButtonClickOrKey(e) {
	e.currentTarget.classList.toggle('enabled');

	let is_before = e.currentTarget.classList.contains('enabled');
	if (is_before) {
		enablePreAppend(false);
	} else {
		disablePreAppend(false);
	}
	scrollToAppendIdx();
	updatePasteAppendToolbar();
}

// #endregion Event Handlers