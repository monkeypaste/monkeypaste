// #region Life Cycle

function initMain(initObj) {
	try {
		globals.EnvName = initObj && initObj.envName ? initObj.envName : globals.WindowsEnv;

		initLocalizer(initObj.defaults.cultureCode);
		initGlobals();
		initDefaults(initObj.defaults);
		initTheme();
		//initExceptionHandler();

		if (initObj.isConverter) {
			initPlainHtmlConverter();
			log('Main Initialized.(Converter)');
			return;
		}
		if (!isRunningOnHost()) {
			document.body.classList.add('no-host');
		}

		initWindow();

		initInput();

		initDrop();
		initDrag();

		initEditor();
		setEditorIsLoaded(true);

		if (isAnyAppendEnabled()) {
			log('Main Initialized.(Appender)');
			enableSubSelection();
		} else {
			log('Main Initialized.(Content)');
		}	
	} catch (ex) {
		onException_ntf('init error', ex);
	}

	onInitComplete_ntf();
}

function initDefaults(defaultsObj) {
	// input 'MpQuillDefaultsRequestMessage'

	if(isNullOrUndefined(defaultsObj)) {
		return;
	}
	if (!isNullOrUndefined(defaultsObj.isDataTransferDestFormattingEnabled)) {
		globals.isDataTransferDestFormattingEnabled = defaultsObj.isDataTransferDestFormattingEnabled;
	}
		
	
	if (!isNullOrUndefined(defaultsObj.maxUndo)) {
		globals.MaxUndoLimit = defaultsObj.maxUndo;
	}
	if (!isNullOrUndefined(defaultsObj.minLogLevel)) {
		globals.MinLogLevel = defaultsObj.minLogLevel;
	}
	if (!isNullOrUndefined(defaultsObj.isDebug)) {
		globals.IsDebug = defaultsObj.isDebug;
	}
	if (!isNullOrUndefined(defaultsObj.isRightToLeft)) {
		globals.IsRtl = defaultsObj.isRightToLeft;
	}
	
	if (!isNullOrUndefined(defaultsObj.bgOpacity)) {
		setElementComputedStyleProp(document.body, '--editableopacity', parseFloat(defaultsObj.bgOpacity));
	}

	const bg_opacity =  isRunningOnHost() ? 0:30;// parseFloat(getElementComputedStyleProp(document.body, '--editableopacity'));
	if(!isNullOrUndefined(defaultsObj.currentTheme)) {
		globals.EditorTheme = defaultsObj.currentTheme;

		let no_sel_bg = 'transparent';		
		let edit_bg = `rgba(255,248,220,${bg_opacity})`;
		let edit_op_bg = `white`;
		let def_content_fg = 'black';
		let edit_tb_bg_color = 'white';
		let edit_tb_sep_bg_color = 'silver';
		let edit_tb_button_color = 'dimgray';
		let paste_template_bg_color = 'teal';
		let paste_toolbar_button_color = 'dodgerblue';
		let edit_template_bg_color = 'palegreen';
		let sel_fg = globals.DefaultSelectionFgColor;
		let caret_color = globals.DefaultCaretColor;
		let sub_sel_bg = `rgba(189,188,188,${bg_opacity})`;
		let copy_color = 'lime';
		let hover_color = 'gold';
		let link_color = 'blue';
		let link_hover_color = 'red';

		if (defaultsObj.currentTheme.toLowerCase() == 'dark') {
			getEditorContainerElement().classList.remove('light');
			getEditorContainerElement().classList.add('dark');

			no_sel_bg = `rgba(30,30,30,${bg_opacity})`;
			sub_sel_bg = `rgba(67,67,67,${bg_opacity})`;
			edit_bg = `rgba(30,30,30,${bg_opacity})`;
			edit_op_bg = `black`;
			def_content_fg = 'white';
			edit_tb_bg_color = 'dimgray';
			edit_tb_sep_bg_color = 'black';
			edit_tb_button_color = 'white';
			paste_template_bg_color = 'darkslategray';
			paste_toolbar_button_color = 'darkblue';
			edit_template_bg_color = 'darkolivegreen';
			sel_fg = 'black';
			caret_color = 'white';
			copy_color = 'lime';
			hover_color = 'yellow';
			link_color = 'cyan';
			link_hover_color = 'salmon';
		} else {
			getEditorContainerElement().classList.add('light');
			getEditorContainerElement().classList.remove('dark');
		}

		globals.DefaultSelectionFgColor = sel_fg; 
		globals.DefaultCaretColor = caret_color;
		globals.DropCopyLineColor = copy_color;
		globals.DragCopySelBgColor = copy_color;

		setElementComputedStyleProp(document.body,'--noselectbgcolor',cleanHexColor(no_sel_bg,null,false));
		setElementComputedStyleProp(document.body, '--subselecteditorbgcolor', cleanHexColor(sub_sel_bg,null,false));
		setElementComputedStyleProp(document.body, '--editableeditorbgcolor', cleanHexColor(edit_bg,null,false));
		setElementComputedStyleProp(document.body, '--editableeditorbgcolor_opaque', cleanHexColor(edit_op_bg));
		setElementComputedStyleProp(document.body, '--defcontentfgcolor', cleanHexColor(def_content_fg,null,false));
		setElementComputedStyleProp(document.body, '--selfgcolor', cleanHexColor(sel_fg,null,false));
		setElementComputedStyleProp(document.body, '--caretcolor', cleanHexColor(caret_color,null,false));
		setElementComputedStyleProp(document.body, '--editortoolbarbgcolor', cleanHexColor(edit_tb_bg_color,null,false));
		setElementComputedStyleProp(document.body, '--editortoolbarsepbgcolor', cleanHexColor(edit_tb_sep_bg_color,null,false));
		setElementComputedStyleProp(document.body, '--editortoolbarbuttoncolor', cleanHexColor(edit_tb_button_color,null,false));
		setElementComputedStyleProp(document.body, '--pastetoolbarbgcolor', cleanHexColor(paste_template_bg_color,null,false));
		setElementComputedStyleProp(document.body, '--pastetoolbarbuttoncolor', cleanHexColor(paste_toolbar_button_color,null,false));
		setElementComputedStyleProp(document.body, '--edittemplatebgcolor', cleanHexColor(edit_template_bg_color,null,false));
		setElementComputedStyleProp(document.body, '--hovercolor', cleanHexColor(hover_color,null,false));
		setElementComputedStyleProp(document.body, '--linkcolor', cleanHexColor(link_color,null,false));
		setElementComputedStyleProp(document.body, '--linkhovercolor', cleanHexColor(link_hover_color,null,false));

	}
	if(!isNullOrUndefined(defaultsObj.defaultFontFamily)) {
		setElementComputedStyleProp(document.body,'--defaultFontFamily',defaultsObj.defaultFontFamily);

		//log('font family set to: ' + getElementComputedStyleProp(document.body, '--defaultFontFamily'));
	}
	if (!isNullOrUndefined(defaultsObj.defaultFontSize)) {
		globals.DefaultFontSize = Math.max(8, parseInt(defaultsObj.defaultFontSize)) + 'px';
		setElementComputedStyleProp(document.body, '--defaultFontSize', globals.DefaultFontSize);
		//log('font size set to: '+getElementComputedStyleProp(document.body,'--defaultFontSize'));
	}
	if (!isNullOrUndefined(defaultsObj.isSpellCheckEnabled)) {
		globals.IsSpellCheckEnabled = parseBool(defaultsObj.isSpellCheckEnabled);
		getSpellCheckableElements().forEach(x=>{
			if(x) {
				x.setAttribute("spellcheck",globals.IsSpellCheckEnabled);
			}
		});
	}

	initShortcuts(defaultsObj.shortcutFragmentStr);
}
// #endregion Life Cycle

// #region Getters

function getSpellCheckableElements() {
	return [
		document.getElementById('findInput'),
		document.getElementById('templateNameTextArea'),
		document.getElementById('templatePasteValueTextArea'),
		getEditorElement(),
		...
		getTemplateElements()
	];
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State



function isElementDisabled(elm, ignoreHidden = false) {
	if (!elm) {
		debugger;
		return true;
	}
	if (!ignoreHidden && elm.classList.contains('hidden')) {
		return true;
	}
	return elm.classList.contains('disabled');
}

function isElementHidden(elm) {
	if (!elm) {
		return true;
	}
	if (elm.classList.contains('hidden')) {
		return true;
	}
	return elm.style.display != 'none';
}

function isMouseOrKeyboardButtonClick(e, suppressWhenDisabled = true) {
	if (suppressWhenDisabled && isElementDisabled(e.currentTarget)) {
		log('suppressing button key or click, elm disabled');
		return false;
	}

	if (e.key !== undefined && isKeyboardButtonClick(e)) {
		return true;
	}
	if (e.buttons !== undefined && e.buttons == 1) {
		return true;
	}
	return false;
}
// #endregion State

// #region Actions


function updateAllSelectionDependantElements() {
	updateFontSizePickerToSelection();
	updateFontFamilyPickerToSelection();

	updateTemplatesAfterSelectionChange();

	updateFindReplaceRangeRects();
	updateAddListItemToolbarButtonIsEnabled();
	updateCreateTableToolbarButtonIsEnabled();
	updateCreateTemplateToolbarButtonToSelection();
	updateFontColorToolbarItemsToSelection();
	updateTablesSizesAndPositions();
	updateBoldToolbarButtonToSelection();
	updateItalicToolbarButtonToSelection();
}

function updateAllSizeAndPositions() {
	updateContentSizeAndPosition();
	updateTemplateToolbarSizesAndPositions();
	updateTooltipToolbarSizesAndPositions();
	updateEditorSizesAndPositions();
	updateFindReplaceToolbarSizesAndPositions();
	updateAnnotationSizesAndPositions();
	updateScrollBarSizeAndPositions();
	updateTablesSizesAndPositions();

	if (globals.EnvName == "android") {
		//var viewportBottom = window.scrollY + window.innerHeight;
		//let tbh = $(".ql-toolbar").outerHeight();
		//if (y <= 0) {
		//    //keyboard is not visible
		//    $(".ql-toolbar").css("top", y);
		//    $("#editor").css("top", y + tbh);
		//} else {
		//    $(".ql-toolbar").css("top", y - tbh);
		//    $("#editor").css("top", 0);
		//}
		//$("#editor").css("bottom", viewportBottom - tbh);
	}
}

function updateAllElements() {
	if (!globals.IsLoaded) {
		return;
	}
	updateAllSizeAndPositions();
	updateAllSelectionDependantElements();
	drawOverlay();
}


function hideAllToolbars() {
	hideAllPopups();
	hideEditorToolbar();
	hideEditTemplateToolbar();
	hidePasteToolbar();
}

function hideAllPopups() {
	hidePasteTemplateSelectorOptions();
	hideColorPaletteMenu();
	hideCreateTemplateToolbarContextMenu();
	hideCreateTableContextMenu();
	hideEditorAlignMenu();
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers