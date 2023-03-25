// #region Globals
const DefaultFontSizes = ['8px', '9px', '10px', '12px', '14px', '16px', '20px', '24px', '32px', '42px', '54px', '68px', '84px', '98px'];
var DefaultFontSize = '12px'
var IsFontSizePickerOpen = false;
var StoredEditorSel = null;

// #endregion Globals

// #region Life Cycle

function initFontSizes() {
    initFontSizeMatcher();
}

function initFontSizeMatcher() {
    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    let Delta = Quill.imports.delta;

    quill.clipboard.addMatcher(Node.ELEMENT_NODE, function (node, delta) {
        let fs_class = Array.from(node.classList).find(x => x.startsWith('ql-size'));
        if (!fs_class) {
            return delta;
        }
        let size_val = fs_class.replace('ql-size-', '');
        if (delta && delta.ops !== undefined && delta.ops.length > 0) {
            for (var i = 0; i < delta.ops.length; i++) {
                if (delta.ops[i].insert === undefined) {
                    continue;
                }
                if (delta.ops[i].attributes === undefined) {
                    delta.ops[i].attributes = {};
                }
                delta.ops[i].attributes.size = size_val;

            }
        }
        return delta;
    });
}

function registerFontSizes() {

    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    var size = Quill.import('attributors/style/size');
    size.whitelist = DefaultFontSizes;
    Quill.register(size, true);

    return DefaultFontSizes;
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
    if (IsFontSizePickerOpen) {
        return;
    }

    sel = sel ? sel : getDocSelection();
    let curFontSize = forcedSize;
    if (curFontSize == null) {
        let curFormat = quill.getFormat(sel);
        curFontSize = curFormat != null && curFormat.hasOwnProperty('size') && curFormat.size.length > 0 ? parseInt(curFormat.size) + 'px' : DefaultFontSize;
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





