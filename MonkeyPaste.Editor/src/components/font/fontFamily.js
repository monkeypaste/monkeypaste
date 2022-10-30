// #region Globals

var DefaultFontFamily = 'Arial';
var IsFontFamilyPickerOpen = false;

// #endregion Globals

// #region Life Cycle
function initFontFamilySelector(active_ff_dv) {
    // NOTE must be called before quill init or:
    // 1. fonts won't initialize right
    // 2. getFontFamilyToolbarSelector() will return the toolbar span quill creates

    initFontFamilyStyles();
    initFontWhiteList();

    active_ff_dv = active_ff_dv ? active_ff_dv : getFontFamilyDataValue(getDefaultFontFamily());
    let font_families = getFontsByEnv();
    let fontSelector_elm = getFontFamilyToolbarSelector();
    fontSelector_elm.innerHTML = '';

    for (var i = 0; i < font_families.length; i++) {
        let ff = font_families[i];

        let opt_elm = document.createElement('OPTION');
        let ff_dv = getFontFamilyDataValue(ff);
        if (ff_dv == active_ff_dv) {
            opt_elm.setAttribute('selected', '');
        } else {
            opt_elm.setAttribute('value', ff);
        }
        opt_elm.innerText = ff;
        fontSelector_elm.appendChild(opt_elm);        
    }
}

function initFontFamilyPicker() {
    // needs to be called after quill init
    if (!quill) {
        debugger;
        return;
    }
    let ff_picker_items = getFontFamilyToolbarPicker().getElementsByClassName('ql-picker-item');

    for (var i = 0; i < ff_picker_items.length; i++) {
        addClickOrKeyClickEventListener(ff_picker_items[i], onFontPickerItemClick);
	}
}


function initFontWhiteList() {
    let fontFamilys = getFontsByEnv();
    // TODO may need to remove current/selected font family here...

    var fontNames = fontFamilys.map(x => getFontFamilyDataValue(x));

    let fonts = Quill.import('formats/font');
    fonts.whitelist = fontNames;
    Quill.register(fonts, true);
}

function initFontFamilyStyles() {
    let fontFamilys = getFontsByEnv();

    let fontNames = fontFamilys.map(x => getFontFamilyCssStr(x)).join(' ');
    var node = document.createElement("style");
    node.innerHTML = fontNames;
    document.body.appendChild(node);
    return fontNames;
}
// #endregion Life Cycle

// #region Getters

function getFontFamilyToolbarSelector() {
    let elm_idx = IsLoaded ? 1 : 0;
    return document.getElementsByClassName('ql-font')[elm_idx];
}

function getFontFamilyToolbarPicker() {
    if (!quill) {
        return null;
    }
    // NOTE if called before init will return select and not span
    return document.getElementsByClassName('ql-font')[0];
}
function getFontFamilyToolbarPickerLabel() {
    let picker_elm = getFontFamilyToolbarPicker();
    if (!picker_elm) {
        return null;
    }
    return getFontFamilyToolbarPicker().firstChild;
}

function getDefaultFontFamily() {
    return getElementComputedStyleProp(document.body, "--defaultFontFamily");
}


function getElementFontFamily(elm) {
    let elmStyles = window.getComputedStyle(elm);
    let ff = elmStyles.getPropertyValue('font-family');
    return ff;
}

function getFontFamilyDataValue(fontFamily) {
    // Generate code-friendly font names
    if (fontFamily) {
        return fontFamily.toLowerCase().replace(/\s/g, "-");
    }
    return '';
}

function getFontFamilyDataValueFontFamily(ff_dv) {
    if (ff_dv.includes(' ')) {
        debugger;
	}
    let ffa = getFontsByEnv();
    for (var i = 0; i < ffa.length; i++) {
        let cur_ff_dv = getFontFamilyDataValue(ffa[i]);
        if (cur_ff_dv == ff_dv) {
            return ffa[i];
		}
    }
    debugger;
    return null;
}

function getFontsByEnv() {
    EnvName = EnvName == null ? WindowsEnv : EnvName;
    let result = null;
    if (EnvName == MacEnv) {
        result = macFonts;
    } else {
        result = winFonts;
    }

    return result.sort();
}

function getFontFamilyCssStr(ff) {
    let fontFamilyDropDownCssTemplateStr = "" +
        `#editorToolbar .ql-font span[data-label='${ff}']::before {`  +
        `font-family: '${ff}'; }`;

    let ff_dv = getFontFamilyDataValue(ff);

    let fontFamilyContentTemplateStr = "" +
        `.ql-font-${ff_dv} { font-family: '${ff}';}`;

    return fontFamilyDropDownCssTemplateStr + ' ' + fontFamilyContentTemplateStr;
}
// #endregion Getters

// #region Setters

function setDocRangeFontFamily(range, ff_dv) {
    formatDocRange(range, { 'font': ff_dv });
}

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function updateFontFamilyPickerToSelection(force_ff_dv = null, sel = null) {
    if (IsFontFamilyPickerOpen) {
        return;
    }
    sel = sel ? sel : getEditorSelection();

    let cur_ff_dv = force_ff_dv;
    if (cur_ff_dv == null) {
        //use selection leaf and iterate up tree until font family is defined or return empty string
        cur_ff_dv = findRangeFontFamilyDataValue(sel);
        if (cur_ff_dv == null || cur_ff_dv == '') {
            debugger;
        }
    }

    let picker_label_elm = getFontFamilyToolbarPickerLabel();
    picker_label_elm.setAttribute('data-label', getFontFamilyDataValueFontFamily(cur_ff_dv));
    //initFontFamilySelector(cur_ff_dv);
}

function findRangeFontFamilyDataValue(range) {
    let curFormat = quill.getFormat(range);
    let curFontFamily = curFormat != null && curFormat.hasOwnProperty('font') ? curFormat.font : null;// DefaultFontFamily;
    if (curFontFamily) {
        return getFontFamilyDataValue(curFontFamily);
    }
    var selection = range;
    if (!selection) {
        // selection outside editor, shouldn't happen since EditorSelectionChange handler forces oldSelection but check timing if occurs
        debugger;
        return '';
    }
    let sel_elm = getElementAtDocIdx(selection.index, true);
    let sel_ff_str = getElementComputedStyleProp(sel_elm, 'font-family');
    if (!sel_ff_str || sel_ff_str.length == 0) {
        return getFontFamilyDataValue(getDefaultFontFamily());
    }
    return getFontFamilyDataValue(sel_ff_str.split(',')[0].replaceAll('"',''));
}
// #endregion Actions

// #region Event Handlers

function onFontPickerItemClick(e) {
    let ff = e.currentTarget.getAttribute('data-value');
    let ff_dv = getFontFamilyDataValue(ff);
    let sel = getEditorSelection();
    if (!sel) {
        return;
    }
    setDocRangeFontFamily(sel, ff_dv);
    updateFontFamilyPickerToSelection();
}
// #endregion Event Handlers