
// #region Life Cycle

function initFontSizes() {
    addClickOrKeyClickEventListener(getFontSizeToolbarElement(), onFontSizeToolbarElementClick);
}


function registerFontSizes() {

    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    var size = Quill.import('attributors/style/size');
    size.whitelist = globals.DefaultFontSizes;
    Quill.register(size, true);

    return globals.DefaultFontSizes;
}
// #endregion Life Cycle

// #region Getters
function getFontSizeToolbarElement() {
    let ff_picker_elms = Array.from(document.querySelectorAll('.ql-size.ql-picker'));
    if (ff_picker_elms.length == 0) {
        return null;
    }
    return ff_picker_elms[0];
}

function getFontSizeDropDownElement() {
    let ff_picker_elms = Array.from(document.querySelectorAll('.ql-size.ql-picker.ql-expanded'));
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

// #endregion Setters

// #region State

function isFontSizeDropDownOpen() {
    return getFontSizeDropDownElement() != null;
}
// #endregion State

// #region Actions

function updateFontSizePickerToSelection(forcedSize = null, sel = null) {
    if (globals.IsFontSizePickerOpen) {
        return;
    }

    sel = sel ? sel : getDocSelection();
    let curFontSize = forcedSize;
    if (curFontSize == null) {
        let curFormat = globals.quill.getFormat(sel);
        curFontSize = curFormat != null && curFormat.hasOwnProperty('size') && curFormat.size.length > 0 ? parseInt(curFormat.size) + 'px' : globals.DefaultFontSize;
    }
    let fontSizeFound = false;

    let font_picker_elm = document.getElementsByClassName('ql-size ql-picker')[0];
    let font_picker_label_elm = font_picker_elm.getElementsByClassName('ql-picker-label')[0];
    let font_picker_options_elm = font_picker_elm.getElementsByClassName('ql-picker-options')[0];

    font_picker_label_elm.setAttribute('data-value', curFontSize);
    Array.from(font_picker_options_elm.children)
        .forEach((fontSizeSpan) => {
            fontSizeSpan.classList.remove('ql-selected');
            if (fontSizeSpan.getAttribute('data-value').toLowerCase() == curFontSize.toLowerCase()) {
                fontSizeSpan.classList.add('ql-selected');
                fontSizeFound = true;
            }
        });

    if (!fontSizeFound) {
        let sizeElm = font_picker_options_elm.firstChild.cloneNode();

        sizeElm.setAttribute('data-value', curFontSize);
        sizeElm.classList.add('ql-selected');
        font_picker_label_elm.innerHTML += sizeElm.outerHTML;
    }

}
// #endregion Actions

// #region Event Handlers

function onFontSizeToolbarElementClick(e) {
    updateScrollBarSizeAndPositions();
}

// #endregion Event Handlers





