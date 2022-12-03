// #region Globals

const PastePopupMenuOptions = ['Append Line', 'Append Inline'];
const AppendLineOptIdx = 0;
const AppendOptIdx = 1;
// #endregion Globals

// #region Life Cycle

function initPastePopup() {
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

// #endregion Getters

// #region Setters

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

    getPasteButtonPopupExpanderLabelElement().innerHTML = "&#9650;";// String.fromCharCode(parseInt(2650));
    let cm = [];
    for (var i = 0; i < PastePopupMenuOptions.length; i++) {
        let label_tail = '';
        if (i == AppendOptIdx && IsAppendMode) {
            label_tail = ' [ENABLED]';
        } else if (i == AppendLineOptIdx && IsAppendLineMode) {
            label_tail = ' [ENABLED]';
        }
        let ppmi = {
            //icon: 'fa-solid fa-plus',
            //iconFgColor: 'lime',
            label: PastePopupMenuOptions[i] + label_tail,
            action: function (option, contextMenuIndex, optionIndex) {
                onPastePopupMenuOptionClick(optionIndex);
            },
        };
        cm.push(ppmi);
    }
    superCm.destroyMenu();

    let exp_elm_rect = exp_elm.getBoundingClientRect();
    let x = exp_elm_rect.left;
    let y = exp_elm_rect.top - 80;
    superCm.createMenu(cm, { pageX: x, pageY: y });
}

function hidePasteButtonExpander() {
    window.removeEventListener('mousedown', onPastePopupExpandedTempWindowClick, true);
    getPasteButtonPopupExpanderElement().classList.remove('expanded');
    superCm.destroyMenu();
    getPasteButtonPopupExpanderLabelElement().innerHTML = "&#9660;";//String.fromCharCode(parseInt(2660));
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

function onPastePopupMenuOptionClick(e) {
    let is_append_line_click = e == AppendLineOptIdx;
    if (is_append_line_click) {
        if (IsAppendLineMode) {
            disableAppendMode();
        } else {
            enableAppendMode(true);
        }
    } else {
        if (IsAppendMode) {
            disableAppendMode();
        } else {
            enableAppendMode(false);
        }
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