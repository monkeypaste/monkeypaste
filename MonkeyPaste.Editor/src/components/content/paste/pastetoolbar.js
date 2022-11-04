// #region Globals

// #endregion Globals

// #region Life Cycle

function initPasteToolbar() {
    enableResize(getPasteToolbarContainerElement());

    addClickOrKeyClickEventListener(getPasteButtonElement(), onPasteButtonClickOrKeyDown);

    initPasteTemplateToolbarItems();
    
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

function isPasteAvailable() {
    return isSubSelectionEnabled();
}

// #endregion State

// #region Actions

function showPasteToolbar() {
    var ptt_elm = getPasteToolbarContainerElement();
    ptt_elm.classList.remove('hidden');

    if (hasTemplates()) {
        showPasteTemplateToolbarItems();
    } else {
        hidePasteTemplateToolbarItems();
    }

    setPasteToolbarDefaultFocus();
}

function hidePasteTemplateToolbar() {
    var ptt = getPasteToolbarContainerElement();
    ptt.classList.add('hidden');

    hidePasteTemplateToolbarItems();
}

// #endregion Actions

// #region Event Handlers


function onPasteButtonClickOrKeyDown(e) {
    if (!isRunningInHost()) {
        alert(getText(getDocSelection(true), true));
    }

    let can_paste = updatePasteElementInteractivity();
    if (can_paste) {
        onPasteTemplateRequest_ntf();
    } else {

    }


}
// #endregion Event Handlers