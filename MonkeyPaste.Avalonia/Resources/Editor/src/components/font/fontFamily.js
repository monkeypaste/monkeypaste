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

    addClickOrKeyClickEventListener(document.getElementsByClassName('ql-font')[0], onFontFamilyToolbarButtonClick);

}

function initFontFamilyPicker() {
    // needs to be called after quill init
    if (!globals.quill) {
        debugger;
        return;
    }

    addClickOrKeyClickEventListener(getFontFamilyToolbarPicker(), onFontFamilyToolbarButtonClick);
    let ff_picker_items = getFontFamilyToolbarPicker().getElementsByClassName('ql-picker-item');

    for (var i = 0; i < ff_picker_items.length; i++) {
        addClickOrKeyClickEventListener(ff_picker_items[i], onFontPickerItemClick);
    }
}

function initFontWhiteList() {
    if (Quill === undefined) {
        /// host load error case
        debugger;
    }

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
    let elm_idx = globals.IsLoaded ? 1 : 0;
    return document.getElementsByClassName('ql-font')[elm_idx];
}

function getFontFamilyToolbarPicker() {
    if (!globals.quill) {
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

function getElementFontFamilyDataValue(elm) {
    let ff = '';
    if (elm.hasAttribute('data-value')) {
        ff = elm.getAttribute('data-value');
    } else if (elm.hasAttribute('data-label')) {
        ff = elm.getAttribute('data-label');
    }
    
    return getFontFamilyDataValue(ff);
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
    if (!ffa) {
        return ff_dv;
    }
    for (var i = 0; i < ffa.length; i++) {
        let cur_ff_dv = getFontFamilyDataValue(ffa[i]);
        if (cur_ff_dv == ff_dv) {
            return ffa[i];
		}
    }

    log('uncataloged font-family detecte (data-value): ' + ff_dv);
    return ff_dv;
    //debugger;
    //return null;
}

function getFontsByEnv() {
    globals.EnvName = globals.EnvName == null ? globals.WindowsEnv : globals.EnvName;
    let result = null;
    if (globals.EnvName == globals.MacEnv) {
        result = globals.macFonts;
    } else {
        result = globals.winFonts;
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

function getFontFamilyDropDownElement() {
    let ff_picker_elms = Array.from(document.querySelectorAll('.ql-font.ql-picker.ql-expanded'));
    if (ff_picker_elms.length == 0) {
        return null;
    }
    let ff_picker_elm = ff_picker_elms[0];
    let ff_opt_elms = Array.from(ff_picker_elm.querySelectorAll('.ql-picker-options'));
    if (ff_opt_elms.length == 0) {
        return null;
    }
    return ff_opt_elms[0];
}

// #endregion Getters

// #region Setters

function setDocRangeFontFamily(range, ff_dv) {
    formatDocRange(range, { 'font': ff_dv });
}

// #endregion Setters

// #region State

function isFontFamilyDropDownOpen() {
    return getFontFamilyDropDownElement() != null;
}

// #endregion State

// #region Actions

function updateFontFamilyPickerToSelection(force_ff_dv = null, sel = null) {
    if (globals.IsFontFamilyPickerOpen ||
        !isSubSelectionEnabled()) {
        return;
    }
    sel = sel ? sel : getDocSelection();

    let cur_ff_dv = force_ff_dv;
    if (cur_ff_dv == null) {
        //use selection leaf and iterate up tree until font family is defined or return empty string
        cur_ff_dv = findRangeFontFamilyDataValue(sel);
        if (cur_ff_dv == null || cur_ff_dv == '') {
            debugger;
        }
    }

    let picker_label_elm = getFontFamilyToolbarPickerLabel();
    if (isNullOrUndefined(picker_label_elm)) {
        return;
    }
    picker_label_elm.setAttribute('data-label', getFontFamilyDataValueFontFamily(cur_ff_dv));

    
}

function updateFontFamilyPickerItemsToSelection() {
    const sel = getDocSelection();

    let cur_ff_dv = findRangeFontFamilyDataValue(sel);
    const pi_elms = Array.from(getFontFamilyToolbarPicker().querySelectorAll('.ql-picker-item'));

    let sel_y_offset = 0;
    for (var i = 0; i < pi_elms.length; i++) {
        const pi_elm = pi_elms[i];
        const pi_elm_ff_dv = getElementFontFamilyDataValue(pi_elm);
        if (pi_elm.classList.contains('editor-selected') &&
            pi_elm_ff_dv != cur_ff_dv) {
            pi_elm.classList.remove('editor-selected');
        } else if (pi_elm_ff_dv == cur_ff_dv) {
            pi_elm.classList.add('editor-selected');
        }
        if (pi_elm.classList.contains('editor-selected')) {
            sel_y_offset = pi_elm.getBoundingClientRect().height * i;
        }
    }

    getFontFamilyToolbarPicker().firstChild.nextSibling.scrollTop = sel_y_offset;
}

function findRangeFontFamilyDataValue(range) {
    let curFormat = globals.quill.getFormat(range);
    let curFontFamily = curFormat != null && curFormat.hasOwnProperty('font') ? curFormat.font : null;// globals.DefaultFontFamily;
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

function hideFontFamilyDropDown() {
    window.removeEventListener('mousedown', onTempFontFamilyWindowClick);
    const picker_elm = getFontFamilyToolbarSelector();// getAncestorByClassName(e.target, 'ql-picker-options');
    if (!picker_elm) {
        return;
    }
    picker_elm.classList.add('hidden');
}
// #endregion Actions

// #region Event Handlers

function onFontPickerItemClick(e) {
    let ff = e.currentTarget.getAttribute('data-value');
    let ff_dv = getFontFamilyDataValue(ff);
    let sel = getDocSelection();
    if (!sel) {
        return;
    }
    setDocRangeFontFamily(sel, ff_dv);
    hideFontFamilyDropDown();
    updateFontFamilyPickerToSelection();
}

function onFontFamilyToolbarButtonClick(e) {
    getFontFamilyToolbarPicker().classList.remove('hidden');    
    updateFontFamilyPickerItemsToSelection();
    window.addEventListener('mousedown', onTempFontFamilyWindowClick, true);
    updateScrollBarSizeAndPositions();
}

function onTempFontFamilyWindowClick(e) {
    //if (isClassInElementPath(e.currentTarget, 'ql-font')) {
    //    return;
    //}
    if (e.offsetX > e.target.clientWidth || e.offsetY > e.target.clientHeight) {
        // mouse down over scroll element
        return;
    }
    hideFontFamilyDropDown();
}
// #endregion Event Handlers