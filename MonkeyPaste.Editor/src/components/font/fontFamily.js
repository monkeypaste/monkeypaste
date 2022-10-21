// #region Globals

var DefaultFontFamily = 'Arial';
var IsFontFamilyPickerOpen = false;

// #endregion Globals

// #region Life Cycle

function initFontFamilyPicker() {
    // needs to be called after quill init

    let ffp_elm = document.getElementsByClassName('ql-font ql-picker')[0];
    ffp_elm.addEventListener('click', (e) => {
        return;
        IsFontFamilyPickerOpen = true;
        let blurred_sel = getEditorSelection();
        if (!blurred_sel) {
            blurred_sel = BlurredSelectionRange;
            if (!blurred_sel) {
                // should be caught in onEditorSelChange but who knows anymore
                debugger;
                return;
            }
        }
        let ffp_opts_elm = ffp_elm.getElementsByClassName('ql-picker-options')[0];
        ffp_opts_elm.addEventListener('click', (e2) => {
            let ff = e2.target.getAttribute('data-value');
            if (ff.length == 0) {
                debugger;
                return;
            }
            ffp_elm.classList.remove('ql-expanded');
            IsFontFamilyPickerOpen = false;
            //quill.focus();
            //quill.formatText(blurred_sel.index, blurred_sel.length, 'font', ff);
            refreshFontFamilyPicker(ff);
            return;
        });

        return;
    });
}


function registerFontFamilys() {
    let fontFamilys = getFontsByEnv();
    var fontNames = fontFamilys.map(x => x.toLowerCase().replaceAll(' ', '-'));

    let fonts = Quill.import('formats/font');

    //var fonts = Quill.import('attributors/class/font');
    fonts.whitelist = fontNames;
    Quill.register(fonts, true);

    return fonts;
}

function registerFontStyles() {
    //// Add fonts to CSS style
    //var fontStyles = "";
    //fontFamilys.forEach(function (font) {
    //    var fontName = getFontName(font);
    //    fontStyles += ".ql-snow .ql-picker.ql-font .ql-picker-label[data-value=" + fontName + "]::before, .ql-snow .ql-picker.ql-font .ql-picker-item[data-value=" + fontName + "]::before {" +
    //        "content: '" + font + "';" +
    //        "font-family: '" + font + "', sans-serif;" +
    //        "}" +
    //        ".ql-font-" + fontName + "{" +
    //        " font-family: '" + font + "', sans-serif;" +
    //        "}";
    //});

    //return fontStyles;

    let fontFamilys = getFontsByEnv();

    let fontNames = fontFamilys.map(x => getFontFamilyCssStr(x)).join(' ');
    return fontNames;

    //// Add fonts to CSS style
    //var fontstyles = "";

    //fontFamilys.forEach(function (font) {
    //        var fontName = getFontFamilyDataValue(font);
    //        fontstyles += ".ql-snow .ql-picker.ql-font .ql-picker-label[data-value=" + fontName + "]::before, .ql-snow .ql-picker.ql-font .ql-picker-item[data-value=" + fontName + "]::before {" +
    //            "content: '" + font + "';" +
    //            "font-family: '" + font + "', sans-serif;" +
    //            "}" +
    //            ".ql-font-" + fontName + "{" +
    //            " font-family: '" + font + "', sans-serif;" +
    //            "}";
    //    });

    //return fontstyles;
}
// #endregion Life Cycle

// #region Getters
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

function getFontsByEnv() {
    EnvName = EnvName == null ? WindowsEnv : EnvName;
    if (EnvName == MacEnv) {
        return macFonts;
    }
    return winFonts;
}

function getFontFamilyCssStr(ff) {
    let fontFamilyDropDownCssTemplateStr = "" +
        ".ql-snow .ql-picker.ql-font .ql-picker-label[data-value='times-new-roman']::before, " +
        ".ql-snow .ql-picker.ql-font .ql-picker-item[data-value='times-new-roman']::before {" +
        "content: 'Times New Roman';" +
        "font-family: 'Times New Roman', sans-serif; }";

    //Set the font-family content used for the HTML content.
    let fontFamilyContentTemplateStr = "" +
        ".ql-font-times-new-roman {" +
        "font-family: 'Times New Roman', sans-serif;" +
        "}";

    return fontFamilyDropDownCssTemplateStr
        .replaceAll('times-new-roman', ff.toLowerCase().replaceAll(' ', '-'))
        .replaceAll('Times New Roman', ff) +
        ' ' +
        fontFamilyContentTemplateStr
            .replaceAll('times-new-roman', ff.toLowerCase().replaceAll(' ', '-'))
            .replaceAll('Times New Roman', ff);
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function refreshFontFamilyPicker(forceFamily = null, sel = null) {
    if (IsFontFamilyPickerOpen) {
        return;
    }
    sel = sel ? sel : getEditorSelection();

    let curFontFamily = forceFamily;
    if (curFontFamily == null) {
        //use selection leaf and iterate up tree until font family is defined or return empty string
        curFontFamily = findRangeFontFamily(sel);
        if (curFontFamily == null || curFontFamily == '') {
            // debugger;
        }
    }
    let curFontFamily_dataValue = getFontFamilyDataValue(curFontFamily);

    let fontFamilyFound = false;

    //set font family picker to found font family (may need to use default if none found)
    let font_family_picker_elm = document.getElementsByClassName('ql-font ql-picker')[0];
    let font_family_picker_label_elm = font_family_picker_elm.getElementsByClassName('ql-picker-label')[0];
    let font_family_picker_options_elm = font_family_picker_elm.getElementsByClassName('ql-picker-options')[0];

    font_family_picker_label_elm.setAttribute('data-value', curFontFamily_dataValue);

    let opts = Array.from(font_family_picker_options_elm.children);

    //iterate through font picker items and clear selection and if there's match set as selected
    for (var i = 0; i < opts.length; i++) {
        let fontFamilySpan = opts[i];
        fontFamilySpan.classList.remove('ql-selected');
        if (fontFamilySpan.getAttribute('data-value') == curFontFamily_dataValue) {
            fontFamilySpan.classList.add('ql-selected');
            fontFamilyFound = true;
        }
    }
    //Array.from(font_family_picker_options_elm.children)
    //    .forEach((fontFamilySpan) => {
    //        fontFamilySpan.classList.remove('ql-selected');
    //        if (fontFamilySpan.getAttribute('data-value') == curFontFamily_dataValue) {
    //            fontFamilySpan.classList.add('ql-selected');
    //            fontFamilyFound = true;
    //        }
    //    });

    if (!fontFamilyFound) {
        let familyElm = font_family_picker_options_elm.firstChild.cloneNode();

        familyElm.setAttribute('data-value', curFontFamily);
        familyElm.classList.add('ql-selected');
        font_family_picker_label_elm.innerHTML += familyElm.outerHTML;
    }
}

function findRangeFontFamily(range) {
    let curFormat = quill.getFormat(range);
    let curFontFamily = curFormat != null && curFormat.hasOwnProperty('font') ? curFormat.font : null;// DefaultFontFamily;
    if (curFontFamily) {
        return curFontFamily;
    }
    var selection = range;
    if (!selection) {
        // selection outside editor, shouldn't happen since EditorSelectionChange handler forces oldSelection but check timing if occurs
        debugger;
        return '';
    }
    let [leaf, offset] = quill.getLeaf(selection.index);
    if (leaf && leaf.parent && leaf.parent.domNode) {
        let parentBlot = leaf.parent;
        while (parentBlot) {
            let fontFamilyParts = parentBlot.domNode.style.fontFamily.split(',');
            if (fontFamilyParts.length > 0 && fontFamilyParts[0].length > 0) {
                curFontFamily = fontFamilyParts[0].trim().replace(/"/g, '');
                break;
            }
            parentBlot = parentBlot.parent;
        }
        //     let parent_elm = leaf.parent.domNode;
        //     while (parent_elm) {
        //         if (parent_elm.style['fontFamily'] != null && parent_elm.style['fontFamily'] != '') {
        //             let fontFamilyParts = parent_elm.style.fontFamily.split(','); //getElementFontFamily(parent_elm).split(',');
        //             if (fontFamilyParts.length > 0 && fontFamilyParts[0].length > 0) {
        //                 let found_font_family = fontFamilyParts[0].trim().replace(/"/g, '');
        //                 return found_font_family;
        //             }
        //}
        //         parent_elm = parent_elm.parentNode;
        //     }
    }
    return '';
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers