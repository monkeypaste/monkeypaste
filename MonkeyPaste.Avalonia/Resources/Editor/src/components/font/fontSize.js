
// #region Life Cycle

function initFontSizes() {
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

function getFontPickerToolbarContainerElement() {
    return document.getElementsByClassName('ql-size ql-picker')[0];
}
function getFontPickerOptionsContainerElement() {
    // should only be accessed when toolbar is opened

    return getFontPickerToolbarContainerElement().getElementsByClassName('ql-picker-options')[0];
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

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

// #endregion Event Handlers





