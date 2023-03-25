// #region Globals

const PastePopupMenuOptions = [
    {
        label: 'Append',
        icon: 'append-outline'
    },
    {
        label: 'Insert',
        icon: 'insert-outline'
    },
    {
        separator: true
    },
    {
        label: 'Manual',
        icon: 'text-insert-caret-outline'
    }
];

const AppendLineOptIdx = 0;
const AppendOptIdx = 1;
const ManualOptIdx = 3;
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

function getAppendLineSvgKey() {
    return IsAppendLineMode ? 'append-solid' : 'append-outline';
}
function getAppendSvgKey() {
    return IsAppendMode ? 'insert-solid' : 'insert-outline';
}
function getAppendIsManualSvgKey() {
    return IsAppendMode ? 'text-insert-caret-solid' : 'text-insert-caret-outline';
}

function getPastePopupExpanderButtonInnerHtml() {
    return isPastePopupExpanded() ? "&#9650;" : "&#9660;";
}

function getPastePopupSvgKeyAtOptIndex(idx) {
    if (idx == AppendLineOptIdx) {
        return getAppendLineSvgKey();
    }
    if (idx == AppendOptIdx) {
        return getAppendSvgKey();
    }
    if (idx == ManualOptIdx) {
        return getAppendIsManualSvgKey();
    }
    return '';
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

    getPasteButtonPopupExpanderLabelElement().innerHTML = getPastePopupExpanderButtonInnerHtml();
    let cm = [];
    for (var i = 0; i < PastePopupMenuOptions.length; i++) {
        let ppmio = PastePopupMenuOptions[i];
        if (ppmio.separator === undefined) {
            ppmio.icon = getPastePopupSvgKeyAtOptIndex(i);
            ppmio.action = function (option, contextMenuIndex, optionIndex) {
                onPastePopupMenuOptionClick(optionIndex);
            };
        }
        if (!isAnyAppendEnabled() && i > AppendOptIdx) {
            continue;
        }
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
    if (optIdx == AppendLineOptIdx) {
        if (IsAppendLineMode) {
            disableAppendMode();
        } else {
            enableAppendMode(true);
        }
    } else if (optIdx == AppendOptIdx){
        if (IsAppendMode) {
            disableAppendMode();
        } else {
            enableAppendMode(false);
        }
    } else if (optIdx == ManualOptIdx) {
        if (IsAppendManualMode) {
            disableAppendManualMode();
        } else {
            enableAppendManualMode();
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