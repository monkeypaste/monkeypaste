
// #region Globals

var IsLoaded = false;
//var quill;
//var availableTemplates = null;

const DomParser = new DOMParser();
const DomSerializer = new XMLSerializer();

var IsDebug = true;

var IsTesting = false;

var IsSpellCheckEnabled = false;

const WindowsEnv = 'Windows';
const IosEnv = 'Ios';
const AndroidEnv = 'Android';
const MacEnv = 'Mac';
const LinuxEnv = 'Linux';
const WebEnv = 'Web';
const UknownEnv = 'Unknown';

var EnvName = "";
var MaxUndoLimit = -1;

// #endregion Globals

// #region Life Cycle

function initMain(initObj) {
	EnvName = !initObj.envName ? WindowsEnv : initObj.envName;
	initDefaults(initObj.defaults);
	//initExceptionHandler();

	if (isPlainHtmlConverter()) {
		initPlainHtmlConverter();
		log('Main Initialized.(Converter)');
		return;
	}

	initClipboard();

	initWindow();

	initInput();

	initDrop();
	initDrag();

	initEditor();

	IsLoaded = true;
	if (isAppendNotifier()) {
		log('Main Initialized.(Appender)');
		enableSubSelection();
	} else {
		log('Main Initialized.(Content)');
	}	

	onInitComplete_ntf();
}

function initDefaults(initObj) {
	if(isNullOrUndefined(initObj)) {
		return;
	}
	if (!isNullOrUndefined(initObj.maxUndo)) {
		MaxUndoLimit = parseInt(initObj.maxUndo);
	}
	
	if (!isNullOrUndefined(initObj.bgOpacity)) {
		setElementComputedStyleProp(document.body, '--editableopacity', parseFloat(initObj.bgOpacity));
	}

	const bg_opacity = parseFloat(getElementComputedStyleProp(document.body, '--editableopacity'));
	if(!isNullOrUndefined(initObj.currentTheme)) {
		EditorTheme = initObj.currentTheme;

		let no_sel_bg = 'transparent';
		let sub_sel_bg = `rgba(189,188,188,${bg_opacity})`;
		let edit_bg = `rgba(255,248,220,${bg_opacity})`;
		let edit_op_bg = `white`;
		let def_content_fg = 'black';
		let sel_fg = 'black';
		let caret_color = 'black';
		let edit_tb_bg_color = 'white';
		let edit_tb_sep_bg_color = 'silver';
		let edit_tb_button_color = 'dimgray';
		let paste_template_bg_color = 'teal';
		let paste_toolbar_button_color = 'dodgerblue';
		let edit_template_bg_color = 'palegreen';

		if(initObj.currentTheme.toLowerCase() == 'dark') {
			no_sel_bg = `rgba(0,0,0,${bg_opacity})`;
			sub_sel_bg = `rgba(67,67,67,${bg_opacity})`;
			edit_bg = `rgba(0,0,0,${bg_opacity})`;
			edit_op_bg = `black`;
			def_content_fg = 'white';
			sel_fg = 'white';
			caret_color = 'white';
			edit_tb_bg_color = 'dimgray';
			edit_tb_sep_bg_color = 'black';
			edit_tb_button_color = 'white';
			paste_template_bg_color = 'darkslategray';
			paste_toolbar_button_color = 'darkblue';
			edit_template_bg_color = 'darkolivegreen';
		}

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
		setElementComputedStyleProp(document.body, '--pastetemplatebgcolor', cleanHexColor(paste_template_bg_color,null,false));
		setElementComputedStyleProp(document.body, '--pastetoolbarbuttoncolor', cleanHexColor(paste_toolbar_button_color,null,false));
		setElementComputedStyleProp(document.body, '--edittemplatebgcolor', cleanHexColor(edit_template_bg_color,null,false));

	}
	if(!isNullOrUndefined(initObj.defaultFontFamily)) {
		setElementComputedStyleProp(document.body,'--defaultFontFamily',initObj.defaultFontFamily);

		log('font family set to: ' + getElementComputedStyleProp(document.body, '--defaultFontFamily'));
	}
	if (!isNullOrUndefined(initObj.defaultFontSize)) {
		DefaultFontSize = Math.max(8, parseInt(initObj.defaultFontSize)) + 'px';
		setElementComputedStyleProp(document.body, '--defaultFontSize', DefaultFontSize);
		log('font size set to: '+getElementComputedStyleProp(document.body,'--defaultFontSize'));
	}
	if (!isNullOrUndefined(initObj.isSpellCheckEnabled)) {
		IsSpellCheckEnabled = parseBool(initObj.isSpellCheckEnabled);
		getSpellCheckableElements().forEach(x=>{
			if(x) {
				x.setAttribute("spellcheck",IsSpellCheckEnabled);
			}
		});
	}
}
// #endregion Life Cycle

// #region Getters

function getSpellCheckableElements() {
	return [
		document.getElementById('findInput'),
		document.getElementById('templateNameTextArea'),
		document.getElementById('templateDetailTextArea'),
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
}

function updateAllSizeAndPositions() {
	updateContentSizeAndPosition();
	updateTemplateToolbarSizesAndPositions();
	updateEditorSizesAndPositions();
	updateFindReplaceToolbarSizesAndPositions();
	updateAnnotationSizesAndPositions();
	updateScrollBarSizeAndPositions();

	if (EnvName == "android") {
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
	if (!IsLoaded) {
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
	hidePasteButtonExpander();
	hideEditorAlignMenu();
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers