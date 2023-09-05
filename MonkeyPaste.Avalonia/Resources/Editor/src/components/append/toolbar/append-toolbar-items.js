// #region Globals

// #endregion Globals

// #region Life Cycle

function initPasteAppendToolbarItems() {
	hidePasteAppendToolbar();
	createAppendButtonLookup();
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
	addClickOrKeyClickEventListener(getPasteAppendBeginButtonElement(), onAppendBeginButtonClickOrKey);
	addClickOrKeyClickEventListener(getPasteAppendToggleInlineButtonElement(), onAppendToggleInlineButtonClickOrKey);
	addClickOrKeyClickEventListener(getPasteAppendToggleManualButtonElement(), onAppendToggleManualButtonClickOrKey);
	addClickOrKeyClickEventListener(getPasteAppendToggleBeforeButtonElement(), onAppendToggleBeforeButtonClickOrKey);

	addClickOrKeyClickEventListener(getPasteAppendPauseAppendButtonElement(), onPauseAppendButtonClickOrKey);
	addClickOrKeyClickEventListener(getPasteAppendStopAppendButtonElement(), onStopAppendButtonClickOrKey);

	if (globals.ContentItemType == 'FileList') {
		getPasteAppendToggleInlineButtonElement().classList.add('disabled');
	} else {
		getPasteAppendToggleInlineButtonElement().classList.remove('disabled');
	}
}

function createAppendButtonLookup() {
	globals.AppendButtonLookup = [
		[
			globals.IsAppendInsertMode,
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
			globals.IsAppendManualMode,
			getPasteAppendToggleManualButtonElement(),
			[
				'scope',
				`${globals.AppendManualModeLabel}`
			],
			[
				'scope',
				`${globals.AppendNonManualModeLabel}`
			]
		],
		[
			globals.IsAppendPreMode,
			getPasteAppendToggleBeforeButtonElement(),
			[
				'triangle-up',
				`${globals.AppendPreLabel} ##ToggleAppendPreMode##`
			],
			[
				'triangle-down',
				`${globals.AppendPostLabel} ##ToggleAppendPostMode##`
			],
		],
		[
			globals.IsAppendPaused,
			getPasteAppendPauseAppendButtonElement(),			
			[
				'pause',
				`${globals.AppendPauseLabel}`
			],
			[
				'play',
				`${globals.AppendResumeLabel}`
			],
		],
	];
	if (globals.ContentItemType == 'FileList') {
		// NOTE file list is block-only, ensure
		// item0 always shows paragraph thing
		globals.AppendButtonLookup[0][0] = false;
	}
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
	createAppendButtonLookup();

	for (var i = 0; i < globals.AppendButtonLookup.length; i++) {
		let append_btn = globals.AppendButtonLookup[i];

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
		elm.innerHTML = getSvgHtml(svg_key);
		elm.setAttribute('hover-tooltip', tt_text);
	}
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
}
function onStopAppendButtonClickOrKey(e) {
	disableAppendMode(false);
}

function onAppendBeginButtonClickOrKey(e) {
	showPasteAppendToolbar();
	enableAppendMode(true, false);
}
function onAppendToggleInlineButtonClickOrKey(e) {
	if (globals.ContentItemType == 'FileList') {
		// ignore click on file item
		return;
	}
	e.currentTarget.classList.toggle('enabled');

	let is_line = !e.currentTarget.classList.contains('enabled');
	enableAppendMode(is_line, false);
}
function onAppendToggleManualButtonClickOrKey(e) {
	e.currentTarget.classList.toggle('enabled');

	let is_manual = e.currentTarget.classList.contains('enabled');
	if (is_manual) {
		enableAppendManualMode(false);
	} else {
		disableAppendManualMode(false);
	}
}
function onAppendToggleBeforeButtonClickOrKey(e) {
	e.currentTarget.classList.toggle('enabled');

	let is_before = e.currentTarget.classList.contains('enabled');
	if (is_before) {
		enablePreAppend(false);
	} else {
		disablePreAppend(false);
	}
}

// #endregion Event Handlers