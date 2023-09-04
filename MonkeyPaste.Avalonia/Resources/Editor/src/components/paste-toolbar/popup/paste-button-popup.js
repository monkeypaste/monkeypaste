
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
            submenu: []
        };
        let result = await getAppPasteInfoFromDbAsync_get();
        let items = result && result.infoItems ? result.infoItems : [];
        for (var i = 0; i < items.length; i++) {
            // create format item 
            let resp_format_mi = items[i];

            let format_mi = {
                label: resp_format_mi.header,
                id: resp_format_mi.guid,
                icon: resp_format_mi.iconBase64,
                isIconBase64: true
            };
            if (resp_format_mi.submenu.filter(x => x.isEnabled) != null) {
                // NOTE give format w/ enabled preset lighter bg to show it has an enabled child
                format_mi.classes = "partial-selected";
            }
            format_mi.submenu = [];
            for (var j = 0; j < resp_format_mi.submenu.length; j++) {
                // create preset item
                let resp_preset_mi = resp_format_mi.submenu[j];

                let preset_mi = {
                    label: resp_preset_mi.header,
                    id: resp_preset_mi.guid,
                    icon: resp_preset_mi.iconBase64,
                    isIconBase64: true,
                    isEnabled: resp_preset_mi.isEnabled
                };
                if (resp_preset_mi.isEnabled) {
                    preset_mi.classes = "selected";
                }
                format_mi.submenu.push(preset_mi);
            }
            format_mi.submenu.push({ separator: true });

            // create manage format item
            format_mi.submenu.push(
                {
                    icon: 'cog',
                    label: 'Manage...',
                    iconFgColor: getElementComputedStyleProp(document.body, '--editortoolbarbuttoncolor'),
                }
            );

            // add handlers to all submenu items
            for (var k = 0; k < format_mi.submenu.length; k++) {
                format_mi.submenu[k].action = function (option, contextMenuIndex, optionIndex) {
                    onPastePopupFormatMenuItemClick(format_mi.submenu[k].id);
                }
            }
            format_root_mi.submenu.push(format_mi);
        }

        if (format_root_mi.submenu.length > 0) {
            mil.push(format_root_mi);
        }
        
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