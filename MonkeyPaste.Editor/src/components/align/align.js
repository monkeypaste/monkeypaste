// #region Globals
const AlignOptionItems = [
    {
        icon: 'align-left'
    },
    {
        icon: 'align-center'
    },
    {
        icon: 'align-right'
    },
    {
        icon: 'align-justify'
    }
];
const AlignLeftOptIdx = 0;
const AlignCenterOptIdx = 1;
const AlignRightOptIdx = 2;
const AlignJustifyOptIdx = 3;
// #endregion Globals

// #region Life Cycle
function initAlignEditorToolbarButton() {
    addClickOrKeyClickEventListener(getAlignEditorToolbarItemElement(), onAlignToolbarButtonClick);

    getAlignEditorToolbarItemElement().innerHTML = getSvgHtml('align-left');
}

// #endregion Life Cycle

// #region Getters

function getAlignEditorToolbarItemElement() {
    return document.getElementById('alignEditorToolbarButton');
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isAlignEditorToolbarMenuVisible() {
    return getAlignEditorToolbarItemElement().classList.contains('expanded');
}
// #endregion State

// #region Actions
function showEditorAlignMenu() {
    window.addEventListener('mousedown', onEditorAlignMenuTempWindowClick, true);

    getAlignEditorToolbarItemElement().classList.add('expanded');
    let cm = [];
    for (var i = 0; i < AlignOptionItems.length; i++) {
        let aomi = AlignOptionItems[i];
        aomi.action = function (option, contextMenuIndex, optionIndex) {
            onAlignToolbarItemClick(optionIndex);
        };
        cm.push(aomi);
    }
    superCm.destroyMenu();

    let align_tb_elm_rect = getAlignEditorToolbarItemElement().getBoundingClientRect();
    let x = align_tb_elm_rect.left;
    let y = align_tb_elm_rect.bottom;
    superCm.createMenu(cm, { pageX: x, pageY: y });
}

function hideEditorAlignMenu() {
    getAlignEditorToolbarItemElement().classList.remove('expanded');
    window.removeEventListener('mousedown', onEditorAlignMenuTempWindowClick, true);
    superCm.destroyMenu();
}
// #endregion Actions

// #region Event Handlers

function onAlignToolbarItemClick(idx) {
    let align_val = null;
    if (idx == AlignLeftOptIdx) {
        // stupid quill ignores 'left' have to use false
        align_val = false;
    } else if (idx == AlignCenterOptIdx) {
        align_val = 'center';
    } else if (idx == AlignRightOptIdx) {
        align_val = 'right';
    } else if (idx == AlignJustifyOptIdx) {
        align_val = 'justify';
    }
    if (align_val == null) {
        log('align click error, unknown idx: ' + idx);
        return;
    }
    quill.focus();
    formatSelection('align', align_val, 'silent');
}

function onEditorAlignMenuTempWindowClick(e) {
    if (isChildOfElement(e.target, getAlignEditorToolbarItemElement())) {
        return;
    }
    if (isClassInElementPath(e.target, 'context-menu-option')) {
        return;
    }
    hideEditorAlignMenu();
}

function onAlignToolbarButtonClick(e) {
    if (isAlignEditorToolbarMenuVisible()) {
        hideEditorAlignMenu();
    } else {
        showEditorAlignMenu();
    }
}
// #endregion Event Handlers