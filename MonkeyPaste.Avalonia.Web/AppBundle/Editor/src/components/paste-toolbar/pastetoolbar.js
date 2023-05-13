// #region Globals
const MIN_TOOLBAR_HEIGHT = 45;
// #endregion Globals

// #region Life Cycle

function initPasteToolbar() {
    // workaround to keep resizer and not override css so initial show doesn't double bounce
    //getPasteToolbarContainerElement().style.bottom = `${-MIN_TOOLBAR_HEIGHT}px`;

    enableResize(getPasteToolbarContainerElement());

    initPasteButton();
    initPasteTemplateToolbarItems();
}

function initPasteButton() {
    addClickOrKeyClickEventListener(getPasteButtonElement(), onPasteButtonClickOrKeyDown);

    let paste_icon_elm = getPasteButtonElement().firstChild;
    paste_icon_elm.replaceWith(createSvgElement('paste', 'svg-icon paste-toolbar-icon'));

    initPastePopup();
}

function loadPasteButton() {

    if (isPastePopupAvailable()) {
        getPasteButtonAndModeContainerElement().classList.add('pasteButtonSpit');
    } else {
        getPasteButtonAndModeContainerElement().classList.remove('pasteButtonSpit');
    }
}
// #endregion Life Cycle

// #region Getters

function getPasteToolbarContainerElement() {
    return document.getElementById('pasteToolbar');
}

function getPasteButtonAndModeContainerElement() {
    return document.getElementById('pasteButtonAndModeContainer');
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

function isPastePopupAvailable() {
    let result = ContentItemType != 'Image';
    return result;
}

function isShowingPasteToolbar() {
    return !getPasteToolbarContainerElement().classList.contains('hidden');
}

// #endregion State

// #region Actions

function showPasteToolbar(isPasting = false) {
    var ptt_elm = getPasteToolbarContainerElement();
    const animate_tb = !isShowingPasteToolbar() || parseInt(ptt_elm.style.bottom) < 0; 

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

    if (animate_tb) {
        // only reset position if actually hidden
        ptt_elm.style.bottom = `${-MIN_TOOLBAR_HEIGHT}px`;
        delay(getToolbarTransitionMs())
            .then(() => {
                getPasteToolbarContainerElement().classList.remove('hidden');
                getPasteToolbarContainerElement().style.bottom = '0px';
            });
    }
}

function hidePasteToolbar() {
    if (isAppendNotifier()) {
        // should always have pastey for appender
        return;
    }
    var ptt_elm = getPasteToolbarContainerElement();
    ptt_elm.style.bottom = `${-MIN_TOOLBAR_HEIGHT}px`;

    delay(getToolbarTransitionMs())
        .then(() => {
            getPasteToolbarContainerElement().classList.add('hidden');
            hidePasteTemplateToolbarItems();
            updateAllElements();
        });

    
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