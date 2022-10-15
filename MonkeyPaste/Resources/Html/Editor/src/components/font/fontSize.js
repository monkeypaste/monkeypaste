const DefaultFontSizes = ['8px', '9px', '10px', '12px', '14px', '16px', '20px', '24px', '32px', '42px', '54px', '68px', '84px', '98px'];
var DefaultFontSize = '12px'
var IsFontSizePickerOpen = false;
var StoredEditorSel = null;

function addFontSizesToQuillContainerOptions(container) {
    let sizes = registerFontSizes();
    container.unshift([{ size: sizes }]);
    return container;
}


function registerFontSizes() {
    var size = Quill.import('attributors/style/size');
    size.whitelist = DefaultFontSizes;
    Quill.register(size, true);

    return DefaultFontSizes;
}

function refreshFontSizePicker(forcedSize = null, sel = null) {
    if (IsFontSizePickerOpen) {
        return;
    }

    sel = sel ? sel : getEditorSelection();
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

function getFontPickerToolbarContainerElement() {
    return document.getElementsByClassName('ql-size ql-picker')[0];
}
function getFontPickerOptionsContainerElement() {
    // should only be accessed when toolbar is opened

    return getFontPickerToolbarContainerElement().getElementsByClassName('ql-picker-options')[0];
}

function onToolbarContainerClick(e) {
    IsFontSizePickerOpen = true;
    StoredEditorSel = getEditorSelection();
    getFontPickerOptionsContainerElement().addEventListener('click', onFontSizeOptionClick);
}

function onFontSizeOptionClick(e) {
    quill.focus();
    setEditorSelection(StoredEditorSel.index, StoredEditorSel.length,'api');
    StoredEditorSel = null;
    IsFontSizePickerOpen = false;
    let sel_font_size = e.target.getAttribute('data-value');
    refreshFontSizePicker(sel_font_size);
    getFontPickerOptionsContainerElement().removeEventListener('click', onFontSizeOptionClick);
    return;
}