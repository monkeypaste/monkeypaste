// #region Globals

// #endregion Globals

// #region Life Cycle

function initPasteToolbar() {
    enableResize(getPasteToolbarContainerElement());

    addClickOrKeyClickEventListener(getPasteButtonElement(), onPasteButtonClickOrKeyDown);

    initPasteTemplateToolbarItems();

    initPastePopup();
}
// #endregion Life Cycle

// #region Getters

function getPasteToolbarContainerElement() {
    return document.getElementById('pasteToolbar');
}

function getPasteButtonElement() {
    return document.getElementById('pasteButton');
}

function getPasteToolbarHeight() {
    return getPasteToolbarContainerElement().getBoundingClientRect().height;
}
// #endregion Getters

// #region Setters

function setPasteToolbarDefaultFocus() {
    // focus template text area, paste button then editor when all nothing enabled

    let pvta_elm = getPasteValueTextAreaElement();
    let pb_elm = getPasteButtonElement();

    if (isElementDisabled(pvta_elm)) {
        if (isElementDisabled(pb_elm)) {
            getEditorElement().focus();
        } else {
            pb_elm.focus({ focusVisible: true });
        }
    } else {
        getPasteValueTextAreaElement().focus({ focusVisible: true });
    }
}
// #endregion Setters

// #region State


function isShowingPasteToolbar() {
    return !getPasteToolbarContainerElement().classList.contains('hidden');
}

// #endregion State

// #region Actions

function showPasteToolbar(isPasting = false) {
    var ptt_elm = getPasteToolbarContainerElement();
    ptt_elm.classList.remove('hidden');

    let can_show_templates = hasTemplates() && (!isReadOnly() || isPasting);
    if (can_show_templates) {
        if (!isSubSelectionEnabled()) {
            enabledSubSelection();
        }
        showPasteTemplateToolbarItems();
    } else {
        hidePasteTemplateToolbarItems();
    }

    setPasteToolbarDefaultFocus();
}

function hidePasteToolbar() {
    if (isAppendNotifier()) {
        // should always have pastey for appender
        return;
    }
    var ptt = getPasteToolbarContainerElement();
    ptt.classList.add('hidden');

    hidePasteTemplateToolbarItems();

    updateAllElements();
}

// #endregion Actions

// #region Event Handlers


function onPasteButtonClickOrKeyDown(e) {
    if (!isRunningInHost()) {
        alert(getText(getDocSelection(true), true));
    }
    onPasteRequest_ntf();
}
// #endregion Event Handlers