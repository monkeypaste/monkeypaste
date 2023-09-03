// #region Globals
// #endregion Globals

// #region Life Cycle

function initPasteToolbar() {
    // workaround to keep resizer and not override css so initial show doesn't double bounce
    //getPasteToolbarContainerElement().style.bottom = `${-globals.MIN_TOOLBAR_HEIGHT}px`;

    //enableResize(getPasteToolbarContainerElement());

    initPasteButton();
    initPasteTemplateToolbarItems();
}

function initPasteButton() {
    addClickOrKeyClickEventListener(getPasteButtonElement(), onPasteButtonClickOrKeyDown);

    updatePasteButtonInfo(null);    

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

// #endregion Setters

// #region State
function isPasteInfoAvailable() {
    let is_available = globals.LastRecvdPasteInfoMsgObj != null;
    return is_available;
}

function isPastePopupAvailable() {
    let result = globals.ContentItemType != 'Image';
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

    focusTemplatePasteValueElement();

    if (animate_tb) {
        // only reset position if actually hidden
        ptt_elm.style.bottom = `${-globals.MIN_TOOLBAR_HEIGHT}px`;
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
    ptt_elm.style.bottom = `${-globals.MIN_TOOLBAR_HEIGHT}px`;

    delay(getToolbarTransitionMs())
        .then(() => {
            getPasteToolbarContainerElement().classList.add('hidden');
            hidePasteTemplateToolbarItems();
            updateAllElements();
        });    
}

function updatePasteButtonInfo(pasteButtonInfoObj) {
    // NOTE always track info updates with last recvd, 
    // so button is as accurate as possible
    globals.LastRecvdPasteInfoMsgObj = pasteButtonInfoObj;

    let paste_icon_elm = getPasteButtonElement().children[0];
    let new_paste_icon_elm = null;
    let new_paste_icon_base64 = pasteButtonInfoObj ? pasteButtonInfoObj.pasteButtonIconBase64 : null;
    let new_paste_tooltip = pasteButtonInfoObj ? pasteButtonInfoObj.pasteButtonTooltipText : 'Unknown';
    let new_paste_info_id = pasteButtonInfoObj ? pasteButtonInfoObj.infoId : null;

    if (globals.IsCurPasteInfoUpdateInProgress &&
        new_paste_info_id != globals.CurPasteInfoId) {
            // custom paste info edit in progress, ignore info update until
        // paste pop up closes.
        log('paste info locked on id ' + globals.CurPasteInfoId + ' ignoring new' + new_paste_info_id);
        return;
    }
    globals.CurPasteInfoId = new_paste_info_id;

    if (isNullOrEmpty(new_paste_icon_base64)) {
        // TODO maybe use nice question mark icon for fallback instead
        // fallback to default 
        new_paste_icon_elm = createSvgElement('paste', 'svg-icon paste-toolbar-icon');
    } else {
        new_paste_icon_elm = document.createElement('img');
        new_paste_icon_elm.src = `data:image/png;base64,${new_paste_icon_base64}`;
    }
    getPasteButtonElement().replaceChild(new_paste_icon_elm, paste_icon_elm);

    getPasteButtonElement().setAttribute('hover-tooltip', `Paste to: <em><i class="paste-tooltip-suffix">${new_paste_tooltip}</i></em>`);

    log('paste button updated. tooltip set to: ' + new_paste_tooltip);
}

function startPasteInfoQueryRequest() {
    // called right before host get req
    globals.IsCurPasteInfoUpdateInProgress = true;
}
function finishPasteInfoQueryRequest() {
    // called when paste popup is closed, which no matter
    // will end paste info locking
    globals.IsCurPasteInfoUpdateInProgress = false;
    // use last recvd paste info to update, should be fine if theres no change
    updatePasteButtonInfo(globals.LastRecvdPasteInfoMsgObj);
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