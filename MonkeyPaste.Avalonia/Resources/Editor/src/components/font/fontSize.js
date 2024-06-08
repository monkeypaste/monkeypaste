
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

function getFontSizeAtDocIdx(docIdx, fallbackToDefault = true) {
    let insert_range = { index: docIdx, length: 1 };
    while (isDocIdxLineEnd(insert_range.index) || isDocIdxLineStart(insert_range.index)) {
        // keep backtracking until non block extent idx found
        insert_range.index = Math.max(insert_range.index - 1, 0);
        if (insert_range.index == 0) {
            break;
        }
    }
    let curFormat = getFormatForDocRange(insert_range);
    if (curFormat != null && curFormat.hasOwnProperty('size') && curFormat.size.length > 0) {
        curFontSize = parseInt(curFormat.size) + 'px';
        return curFontSize;
    }
    if (fallbackToDefault) {
        return globals.DefaultFontSize;
    }
    return null;
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
function setSelectionFontSize(fs) {
    let sel = getDocSelection();
    if (!sel) {
        return;
    }
    formatDocRange(sel, { size: fs }, 'user');

    updateFontSizePickerToSelection(fs, sel);
}
function updateFontSizePickerToSelection(forcedSize = null, sel = null) {
    if (globals.IsFontSizePickerOpen) {
        return;
    }

    sel = sel ? sel : getDocSelection();
    let curFontSize = forcedSize;
    if (curFontSize == null) {
        curFontSize = getFontSizeAtDocIdx(sel.index);
    }
    let fontSizeFound = false;

    if (document.getElementsByClassName('ql-size ql-picker').length == 0) {
        return;
    }
    let font_picker_elm = document.getElementsByClassName('ql-size ql-picker')[0];
    let font_picker_label_elm = font_picker_elm.getElementsByClassName('ql-picker-label')[0];
    let font_picker_options_elm = font_picker_elm.getElementsByClassName('ql-picker-options')[0];

    if (font_picker_label_elm) {
        font_picker_label_elm.setAttribute('data-value', curFontSize);
    }
    if (font_picker_options_elm && font_picker_options_elm.children) {
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
}

function hideFontSizeDropDown() {
    window.removeEventListener('mousedown', onTempFontSizeWindowClick);
    getFontSizeToolbarElement().classList.remove('ql-expanded');
    updateScrollBarSizeAndPositions();
}
// #endregion Actions

// #region Event Handlers

function onFontSizeToolbarElementClick(e) {
    let size_item_elms = Array.from(document.querySelectorAll('span.ql-size .ql-picker-item'));
    size_item_elms.forEach(x => addClickOrKeyClickEventListener(x, onFontSizeItemClick,true));
    //window.addEventListener('mousedown', onTempFontSizeWindowClick, true);

    updateScrollBarSizeAndPositions();
}

function onFontSizeItemClick(e) {
    e.preventDefault();
    e.stopPropagation();
    if (!e.currentTarget || !e.currentTarget.hasAttribute('data-value')) {
        return;
    }
    let fs = e.currentTarget.getAttribute('data-value');
    // hide picker
    hideFontSizeDropDown();

    setSelectionFontSize(fs);

    log(fs);
}

function onTempFontSizeWindowClick(e) {
    if (e.offsetX > e.target.clientWidth || e.offsetY > e.target.clientHeight) {
        // mouse down over scroll element
        return;
    }
    hideFontSizeDropDown();
}

// #endregion Event Handlers





