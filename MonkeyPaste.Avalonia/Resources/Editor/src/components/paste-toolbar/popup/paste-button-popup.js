
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
                isIconBase64: true,
                // NOTE give format w/ enabled preset lighter bg to show it has an enabled child
                itemBgColor: resp_format_mi.subItems.filter(x=>x.isEnabled) != null ?
                    getElementComputedStyleProp(document.body, '--inactiveselbgcolor') :
                    'transparent'
            };
            format_mi.submenu = [];
            for (var j = 0; j < resp_format_mi.subItems.length; j++) {
                // create preset item

                let resp_preset_mi = resp_format_mi.subItems[j];
                let preset_mi = {
                    label: resp_preset_mi.header,
                    id: resp_preset_mi.guid,
                    icon: resp_preset_mi.iconBase64,
                    isIconBase64: true,
                    isEnabled: resp_preset_mi.isEnabled,
                    itemBgColor: resp_preset_mi.isEnabled ?
                        getElementComputedStyleProp(document.body, '--selbgcolor') :
                        'transparent'
                };
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
            for (var k = 0; k < format_mi.subItems.length; k++) {
                format_mi.subItems[k].action = function (option, contextMenuIndex, optionIndex) {
                    onPastePopupFormatMenuItemClick(format_mi.subItems[k].id);
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

function preparePastePopupMenuItem(ppmio, i) {
    if (ppmio.separator !== undefined) {
        return ppmio;
    }
    ppmio = setOptKeys(ppmio, i);
    if (!ppmio.subItems || (Array.isArray(ppmio.subItems) && ppmio.subItems.length == 0)) {
        if (ppmio.id == 'formats') {
            // empty formats, hide it
            return null;
        }
        // this should only be the case for begin append..
        ppmio.action = function (option, contextMenuIndex, optionIndex) {
            onPastePopupMenuOptionClick(ppmio);
        };
    }
    return ppmio;
}
function showPasteButtonExpander() {
    window.addEventListener('mousedown', onPastePopupExpandedTempWindowClick, true);

    let exp_elm = getPasteButtonPopupExpanderElement();
    exp_elm.classList.add('expanded'); 
    let exp_elm_rect = exp_elm.getBoundingClientRect();
    let x = exp_elm_rect.left;
    let y = exp_elm_rect.top;
    superCm.destroyMenu();
    let spinner_mil = [
        {
            icon: 'spinner',
            iconFgColor: 'dimgray',
            iconClassList: ['rotate'],
            label: 'Loading...'
        }
    ];
    superCm.createMenu(spinner_mil, { pageX: x, pageY: y });
    let cm_elm = Array.from(document.getElementsByClassName('context-menu'))[0];
    y -= cm_elm.getBoundingClientRect().height;
    setElementComputedStyleProp(cm_elm, 'top', `${y}px`);

    getPastePopupMenuItemsDataAsync()
        .then((result) => {
            result = result ? result : [];
            superCm.destroyMenu();
            superCm.createMenu(result, { pageX: x, pageY: y });
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