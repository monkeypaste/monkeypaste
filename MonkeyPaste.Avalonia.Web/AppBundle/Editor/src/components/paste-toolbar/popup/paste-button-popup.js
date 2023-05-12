// #region Globals

const PastePopupMenuOptions = [
    {
        label: 'Block',
        icon: 'append-outline'
    },
    {
        label: 'Inline',
        icon: 'insert-outline'
    },
    {
        separator: true
    },
    {
        label: 'Before',
        icon: 'arrow-left'
    },
    {
        label: 'After',
        icon: 'arrow-right'
    },
    {
        separator: true
    },
    {
        label: 'Manual',
        icon: 'text-insert-caret-outline'
    },
    {
        separator: true
    },
    {
        label: 'Done',
        icon: 'sign-out'
    },
    {
        label: 'Stack Mode',
        icon: 'append-outline'
    }
];


const AppendLineOptIdx = 0;
const AppendInsertOptIdx = 1;
const AppendPreIdx = 3;
const AppendPostIdx = 4;
const ManualOptIdx = 6;
const DoneOptIdx = 8;
const StartOptIdx = 9;

const MinFileListOptIdx = AppendPreIdx;
// #endregion Globals

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
    if (idx == AppendLineOptIdx) {
        keys = ShortcutKeysLookup['ToggleAppendLineMode'];
    } else if (idx == AppendInsertOptIdx) {
        keys = ShortcutKeysLookup['ToggleAppendInsertMode'];
    } else if (idx == AppendPreIdx) {
        keys = ShortcutKeysLookup['ToggleAppendPreMode'];
    } else if (idx == AppendLineOptIdx) {
        keys = ShortcutKeysLookup['ToggleAppendLineMode'];
    } else if (idx == ManualOptIdx) {
        keys = ShortcutKeysLookup['ToggleAppendManualMode'];
    } else if (idx == DoneOptIdx) {
        if (IsAppendLineMode) {
            keys = ShortcutKeysLookup['ToggleAppendLineMode'];
        } else if (IsAppendInsertMode) {
            keys = ShortcutKeysLookup['ToggleAppendInsertMode'];
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
    if (opt_idx == StartOptIdx) {
        if (isAnyAppendEnabled()) {
            return true;
        }
        return false;
    }

    if (IsAppendLineMode && opt_idx == AppendLineOptIdx) {
        return true;
    }
    if (IsAppendInsertMode && opt_idx == AppendInsertOptIdx) {
        return true;
    }
    if (IsAppendPreMode && opt_idx == AppendPreIdx) {
        return true;
    }
    if (!IsAppendPreMode && opt_idx == AppendPostIdx) {
        return true;
    }
    if (!isAnyAppendEnabled()) {
        return true;
    }
    if (ContentItemType == 'Text') {
        return false;
    }
    if (!isAnyAppendEnabled() &&
         opt_idx == AppendLineOptIdx) {
        // show enable
        return false;
    }
    if (isAnyAppendEnabled() &&
        opt_idx == DoneOptIdx) {
        // always show done if enabled
        return false;
    }
    return opt_idx < MinFileListOptIdx;
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
    for (var i = 0; i < PastePopupMenuOptions.length; i++) {
        if (isPopupOptIdxIgnored(i)) {
            continue;
        }
        let ppmio = PastePopupMenuOptions[i];
        if (ppmio.separator === undefined) {
            ppmio = setOptKeys(ppmio, i);
            ppmio.action = function (option, contextMenuIndex, optionIndex) {
                onPastePopupMenuOptionClick(PastePopupMenuOptions.indexOf(option));
            };
            let checked = false;
            if (IsAppendLineMode && i == AppendLineOptIdx) {
                checked = true;
            }
            if (IsAppendInsertMode && i == AppendInsertOptIdx) {
                checked = true;
            }
            if (IsAppendManualMode && i == ManualOptIdx) {
                checked = true;
            }
            if (IsAppendPreMode && i == AppendPreIdx) {
                checked = true;
            }
            if (!IsAppendPreMode && i == AppendPostIdx) {
                checked = true;
            }
            if (checked) {
                ppmio.itemBgColor = 'darkturquoise';
            } else {
                delete ppmio.itemBgColor;
            }
        }
        //if (!isAnyAppendEnabled()) {
        //    if (i >= AppendInsertOptIdx) {
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
    if (optIdx == AppendLineOptIdx) {
        if (IsAppendLineMode) {
            //disableAppendMode(false);
        } else {
            enableAppendMode(true);
        }
    } else if (optIdx == AppendInsertOptIdx) {
        if (IsAppendInsertMode) {
            //disableAppendMode(false);
        } else {
            enableAppendMode(false);
        }
    } else if (optIdx == ManualOptIdx) {
        if (IsAppendManualMode) {
            disableAppendManualMode(false);
        } else {
            enableAppendManualMode(false);
        }
    } else if (optIdx == AppendPreIdx) {
        if (!IsAppendPreMode) {
            enablePreAppend(false);
        } else if (ContentItemType == 'FileList') {
            disableAppendMode(false);
        }
    } else if (optIdx == AppendPostIdx) {
        if (IsAppendPreMode) {
            disablePreAppend(false);
        } else if (ContentItemType == 'FileList') {
            disableAppendMode(false);
        }
    } else if (optIdx == DoneOptIdx) {
        disableAppendMode(false);
    } else if (optIdx == StartOptIdx) {
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