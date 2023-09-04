
// #region Life Cycle

function initPastePopup() {
    initPasteAppendToolbarItems();
    addClickOrKeyClickEventListener(getPasteButtonPopupExpanderElement(), onPasteButtonExpanderClickOrKeyDown);

    hidePasteButtonExpander();
}


// #endregion Life Cycle

// #region Getters

function getPasteButtonPopupExpanderElement() {
    return document.getElementById('pasteButtonPopupExpander');
}


async function getPastePopupMenuItemsDataAsync() {
    let mil = [];
    if (isPasteInfoAvailable()) {
        // NOTE adding empty submenu here
        let format_root_mi = {
            label: globals.PasteButtonFormatsLabel,
            id: 'formats',
            icon: 'fontfg',

        };
        format_root_mi.action = function (option, contextMenuIndex, optionIndex) {
            onPastePopupMenuOptionClick(format_root_mi);
        };
        mil.push(format_root_mi);
        
    }
    if (isAnyAppendEnabled()) {
        return mil;
    }
    if (mil.length > 0) {
        mil.push({ separator: true });
    }
    let append_begin_mi = {
        label: globals.PasteButtonAppendBeginLabel,
        id: 'appendBegin',
        icon: 'append-outline'
    };
    append_begin_mi.action = function (option, contextMenuIndex, optionIndex) {
        onPastePopupMenuOptionClick(append_begin_mi);
    };
    mil.push(append_begin_mi);
    return mil;
}
// #endregion Getters

// #region Setters

function setOptKeys(opt, idx) {
    let keys = null;
    if (idx == globals.AppendLineOptIdx) {
        keys = globals.ShortcutKeysLookup['ToggleAppendLineMode'];
    } else if (idx == globals.AppendInsertOptIdx) {
        keys = globals.ShortcutKeysLookup['ToggleAppendInsertMode'];
    } else if (idx == globals.AppendPreIdx) {
        keys = globals.ShortcutKeysLookup['ToggleAppendPreMode'];
    } else if (idx == globals.AppendLineOptIdx) {
        keys = globals.ShortcutKeysLookup['ToggleAppendLineMode'];
    } else if (idx == globals.ManualOptIdx) {
        //keys = globals.ShortcutKeysLookup['ToggleAppendManualMode'];
    } else if (idx == globals.DoneOptIdx) {
        if (globals.IsAppendLineMode) {
            keys = globals.ShortcutKeysLookup['ToggleAppendLineMode'];
        } else if (globals.IsAppendInsertMode) {
            keys = globals.ShortcutKeysLookup['ToggleAppendInsertMode'];
        }
    }
    if (!keys || keys === undefined) {
        return opt;
    }
    opt.keys = keys;
    return opt;
}

// #endregion Setters

// #region State


function isPastePopupExpanded() {
    return getPasteButtonPopupExpanderElement().classList.contains('expanded');
}

// #endregion State

// #region Actions

function showPasteButtonExpander() {
    window.addEventListener('mousedown', onPastePopupExpandedTempWindowClick, true);

    let exp_elm = getPasteButtonPopupExpanderElement();
    exp_elm.classList.add('expanded');
    let anchor_corner = 'top-right';
    let menu_anchor_corner = 'bottom-right';
    let offset = { x: 0, y: -5 };
    showContextMenu(exp_elm, getBusySpinnerMenuItem(), anchor_corner, menu_anchor_corner,offset.x,offset.y);

    getPastePopupMenuItemsDataAsync()
        .then((result) => {
            result = result ? result : [];
            showContextMenu(exp_elm, result, anchor_corner, menu_anchor_corner, offset.x, offset.y);
        });
}

function hidePasteButtonExpander() {
    window.removeEventListener('mousedown', onPastePopupExpandedTempWindowClick, true);
    getPasteButtonPopupExpanderElement().classList.remove('expanded');
    superCm.destroyMenu();

    // finish doesn't always need to be called but blindly calling keeps state simpler 
    finishPasteInfoQueryRequest();
}

function togglePasteButtonExpander() {
    if (isPastePopupExpanded()) {
        hidePasteButtonExpander();
    } else {
        showPasteButtonExpander();
    }
}


// #endregion Actions

// #region Event Handlers


function onPasteButtonExpanderClickOrKeyDown(e) {
    togglePasteButtonExpander();
}

function onPastePopupMenuOptionClick(ppmio) {
    hidePasteButtonExpander();
    if (!ppmio || ppmio.id === undefined) {
        return;
    }
    if (ppmio.id == 'appendBegin') {
        enableAppendMode(true);
        return;
    }
    if (ppmio.id == 'formats') {
        onPasteInfoFormatsClicked_ntf(globals.CurPasteInfoId);
        return;
    }
    log('unknown paste menu item clicked. id: ' + ppmio.id);
}

function onPastePopupExpandedTempWindowClick(e) {
    if (isChildOfElement(e.target, getPasteButtonPopupExpanderElement())) {
        return;
    }
    if (isClassInElementPath(e.target, 'context-menu-option')) {
        return;
    }
    hidePasteButtonExpander();
}

function onPastePopupFormatMenuItemClick(pluginOrPresetGuid) {
    onPasteInfoItemClicked_ntf(pluginOrPresetGuid);

}
// #endregion Event Handlers