
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
function getPasteButtonPopupExpanderLabelElement() {
    return document.getElementById('pasteButtonPopupExpanderLabel');
}

function getPastePopupExpanderButtonInnerHtml() {
    return isPastePopupExpanded() ? "&#9650;" : "&#9660;";
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

function isPopupOptIdxIgnored(opt_idx) {
    if (opt_idx == globals.StartOptIdx) {
        if (isAnyAppendEnabled()) {
            return true;
        }
        return false;
    }

    if (globals.IsAppendLineMode && opt_idx == globals.AppendLineOptIdx) {
        return true;
    }
    if (globals.IsAppendInsertMode && opt_idx == globals.AppendInsertOptIdx) {
        return true;
    }
    if (globals.IsAppendPreMode && opt_idx == globals.AppendPreIdx) {
        return true;
    }
    if (!globals.IsAppendPreMode && opt_idx == globals.AppendPostIdx) {
        return true;
    }
    if (!isAnyAppendEnabled()) {
        return true;
    }
    if (globals.ContentItemType == 'Text') {
        return false;
    }
    if (!isAnyAppendEnabled() &&
         opt_idx == globals.AppendLineOptIdx) {
        // show enable
        return false;
    }
    if (isAnyAppendEnabled() &&
        opt_idx == globals.DoneOptIdx) {
        // always show done if enabled
        return false;
    }
    return opt_idx < globals.MinFileListOptIdx;
}

function isPastePopupExpanded() {
    return getPasteButtonPopupExpanderElement().classList.contains('expanded');
}

// #endregion State

// #region Actions

function showPasteButtonExpander() {
    window.addEventListener('mousedown', onPastePopupExpandedTempWindowClick, true);

    let exp_elm = getPasteButtonPopupExpanderElement();
    exp_elm.classList.add('expanded');

    getPasteButtonPopupExpanderLabelElement().innerHTML = getPastePopupExpanderButtonInnerHtml();
    let cm = [];
    for (var i = 0; i < globals.PastePopupMenuOptions.length; i++) {
        if (isPopupOptIdxIgnored(i)) {
            continue;
        }
        let ppmio = globals.PastePopupMenuOptions[i];
        if (ppmio.separator === undefined) {
            ppmio = setOptKeys(ppmio, i);
            ppmio.action = function (option, contextMenuIndex, optionIndex) {
                onPastePopupMenuOptionClick(globals.PastePopupMenuOptions.indexOf(option));
            };
            let checked = false;
            if (globals.IsAppendLineMode && i == globals.AppendLineOptIdx) {
                checked = true;
            }
            if (globals.IsAppendInsertMode && i == globals.AppendInsertOptIdx) {
                checked = true;
            }
            if (globals.IsAppendManualMode && i == globals.ManualOptIdx) {
                checked = true;
            }
            if (globals.IsAppendPreMode && i == globals.AppendPreIdx) {
                checked = true;
            }
            if (!globals.IsAppendPreMode && i == globals.AppendPostIdx) {
                checked = true;
            }
            if (checked) {
                ppmio.itemBgColor = 'darkturquoise';
            } else {
                delete ppmio.itemBgColor;
            }
        }
        //if (!isAnyAppendEnabled()) {
        //    if (i >= globals.AppendInsertOptIdx) {
        //        continue;
        //    } else {
        //        ppmio.label = 'Stack Mode';
        //    }
        //}
        cm.push(ppmio);
    }
    superCm.destroyMenu();

    let exp_elm_rect = exp_elm.getBoundingClientRect();
    let x = exp_elm_rect.left;
    let y = exp_elm_rect.top;
    superCm.createMenu(cm, { pageX: x, pageY: y });

    let cm_elm = Array.from(document.getElementsByClassName('context-menu'))[0];
    y -= cm_elm.getBoundingClientRect().height;
    setElementComputedStyleProp(cm_elm, 'top', `${y}px`);
}

function hidePasteButtonExpander() {
    window.removeEventListener('mousedown', onPastePopupExpandedTempWindowClick, true);
    getPasteButtonPopupExpanderElement().classList.remove('expanded');
    superCm.destroyMenu();
    getPasteButtonPopupExpanderLabelElement().innerHTML = getPastePopupExpanderButtonInnerHtml();
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

function onPastePopupMenuOptionClick(optIdx) {
    log('clicked append mode idx: ' + optIdx);
    if (optIdx == globals.AppendLineOptIdx) {
        if (globals.IsAppendLineMode) {
            //disableAppendMode(false);
        } else {
            enableAppendMode(true);
        }
    } else if (optIdx == globals.AppendInsertOptIdx) {
        if (globals.IsAppendInsertMode) {
            //disableAppendMode(false);
        } else {
            enableAppendMode(false);
        }
    } else if (optIdx == globals.ManualOptIdx) {
        if (globals.IsAppendManualMode) {
            disableAppendManualMode(false);
        } else {
            enableAppendManualMode(false);
        }
    } else if (optIdx == globals.AppendPreIdx) {
        if (!globals.IsAppendPreMode) {
            enablePreAppend(false);
        } else if (globals.ContentItemType == 'FileList') {
            disableAppendMode(false);
        }
    } else if (optIdx == globals.AppendPostIdx) {
        if (globals.IsAppendPreMode) {
            disablePreAppend(false);
        } else if (globals.ContentItemType == 'FileList') {
            disableAppendMode(false);
        }
    } else if (optIdx == globals.DoneOptIdx) {
        disableAppendMode(false);
    } else if (optIdx == globals.StartOptIdx) {
        enableAppendMode(true);
    }
    hidePasteButtonExpander();
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
// #endregion Event Handlers