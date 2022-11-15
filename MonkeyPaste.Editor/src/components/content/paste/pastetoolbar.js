// #region Globals

var IsReadyToPaste = false;

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


function isShowingPasteToolbar() {
    return !getPasteToolbarContainerElement().classList.contains('hidden');
}

function canPaste() {
    return updatePasteElementInteractivity();
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
    //if (canPaste()) {
    //    if (IsReadyToPaste) {
    //        onPasteRequest_ntf();
    //    } else {
    //        // this state implies there are templates and contentRequest is blocking until IsReadyToPaste=true

    //        // stop blocking and paste w/ current sel/value state
    //        IsReadyToPaste = true;
    //    }
    //}
    onPasteRequest_ntf();
}
// #endregion Event Handlers