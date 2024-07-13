// #region Globals
// #endregion Globals

// #region Life Cycle

function initPasteToolbar() {
    // workaround to keep resizer and not override css so initial show doesn't double bounce
    //getPasteToolbarContainerElement().style.bottom = `${-globals.MIN_TOOLBAR_HEIGHT}px`;

    //enableResize(getPasteToolbarContainerElement());

    initPasteAppendToolbarItems();
    initPasteButton();
    initPasteTemplateToolbarItems();
}

function initPasteButton() {
    if (globals.IsMobile) {
        document.getElementById('pasteButtonText').setAttribute('ui-content-key', 'EditorPasteButtonLabelMobile');
        localizeElement(document.getElementById('pasteButtonText'));
    }
    addClickOrKeyClickEventListener(getPasteButtonElement(), onPasteButtonClickOrKeyDown);
    addClickOrKeyClickEventListener(getPasteButtonPopupExpanderElement(), onPasteButtonExpanderClickOrKeyDown);


    updatePasteButtonInfo(null);   
}
// #endregion Life Cycle

// #region Getters

function getPasteButtonPopupExpanderElement() {
    return document.getElementById('pasteButtonPopupExpander');
}
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

function setPasteButtonContent(new_paste_icon_key_or_base64, tooltip) {
    let new_paste_icon_elm = null;
    if (isNullOrEmpty(new_paste_icon_key_or_base64)) {
        new_paste_icon_elm = createSvgElement('paste', 'svg-icon paste-toolbar-icon');
    } else if (new_paste_icon_key_or_base64 == 'spinner') {
        new_paste_icon_elm = createSvgElement('spinner', 'svg-icon paste-toolbar-icon rotate');
    } else {
        new_paste_icon_elm = document.createElement('img');
        new_paste_icon_elm.src = `data:image/png;base64,${new_paste_icon_key_or_base64}`;
    }
    let paste_icon_elm = getPasteButtonElement().children[0];
    getPasteButtonElement().replaceChild(new_paste_icon_elm, paste_icon_elm);
    getPasteButtonElement().setAttribute('hover-tooltip', tooltip);

}

function setPasteButtonState(isEnabled, isCustom) {
    if (isEnabled) {
        getPasteButtonElement().classList.remove('disabled');
        getPasteButtonPopupExpanderElement().classList.remove('disabled');
    } else {
        getPasteButtonElement().classList.add('disabled');
        getPasteButtonPopupExpanderElement().classList.add('disabled');
    }
    if (isCustom === undefined) {
        // when pasting
        return;
    }

    isCustom ?
        getPasteButtonAndModeContainerElement().classList.add('custom') :
        getPasteButtonAndModeContainerElement().classList.remove('custom');
}

// #endregion Setters

// #region State

function isPasting() {
    let result = getEditorContainerElement().classList.contains('pasting');
    return result;
}
function isPastePopupExpanded() {
    return getPasteButtonPopupExpanderElement().classList.contains('expanded');
}


function isShowingPasteToolbar() {
    return !getPasteToolbarContainerElement().classList.contains('hidden');
}

// #endregion State

// #region Actions

function showPasteToolbar(isPasting = false) {
    if (isDropping()) {
        // ensure paste toolbar is hidden during drop
        hidePasteToolbar();
        return;
    }
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
    if (isAnyAppendEnabled()) {
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

    let paste_icon_elm = getPasteButtonElement().children[0];
    let new_paste_icon_base64 = pasteButtonInfoObj ? pasteButtonInfoObj.pasteButtonIconBase64 : null;
    let new_paste_tooltip_info_part = pasteButtonInfoObj ? pasteButtonInfoObj.pasteButtonTooltipHtml : 'Unknown';
    let new_paste_info_id = pasteButtonInfoObj ? pasteButtonInfoObj.infoId : null;
    let new_paste_info_is_default = pasteButtonInfoObj ? pasteButtonInfoObj.isFormatDefault : true;
    //const new_paste_tooltip = `Paste to: <em><i class="paste-tooltip-suffix">${new_paste_tooltip_info_part}</i></em>`;
    setPasteButtonContent(new_paste_icon_base64, new_paste_tooltip_info_part);

    let is_enabled = !isNullOrWhiteSpace(new_paste_info_id);
    let is_custom = is_enabled && !new_paste_info_is_default;
    setPasteButtonState(is_enabled, is_custom);

    globals.LastRecvdPasteInfoMsgObj = pasteButtonInfoObj;
    globals.CurPasteInfoId = new_paste_info_id;
    log('paste button updated. tooltip set to: ' + new_paste_tooltip_info_part);
}


function unexpandPasteButton(fromHost = false) {
    getPasteButtonPopupExpanderElement().classList.remove('expanded');
    if (fromHost) {
        return;
    }
    onPasteInfoFormatsClicked_ntf(globals.CurPasteInfoId, false);
}
function expandPasteButton(fromHost = false) {
    getPasteButtonPopupExpanderElement().classList.add('expanded');
    if (fromHost) {
        return;
    }

    let offset = getRectCornerByName(
        cleanRect(getPasteButtonPopupExpanderElement().getBoundingClientRect()),
        'top-right');
    onPasteInfoFormatsClicked_ntf(globals.CurPasteInfoId, true, offset.x,offset.y);
}

function startPasteButtonBusy() {
    setPasteButtonContent('spinner', UiStrings.CommonBusyLabel);
    getPasteButtonElement().classList.add('disabled');
    getPasteButtonPopupExpanderElement().classList.add('disabled');
    getEditorContainerElement().classList.add('pasting');
    globals.PasteButtonBusyStartDt = Date.now();

    drawOverlay();
}
function endPasteButtonBusy() {
    // get ms since paste button went busy
    let busy_dt = Date.now() - globals.PasteButtonBusyStartDt;
    // wait 0 or remaining ms compared to min ms (1second)
    let wait_ms = Math.max(0, globals.MinPasteBusyMs - busy_dt);

    delay(wait_ms)
        .then(() => {
            updatePasteButtonInfo(globals.LastRecvdPasteInfoMsgObj);
            getEditorContainerElement().classList.remove('pasting');
            drawOverlay();
            if (!isRunningOnHost()) {
                alert(getText(getDocSelection(true), true));
            }
        });
}
// #endregion Actions

// #region Event Handlers


function onPasteButtonClickOrKeyDown(e) {
    startPasteButtonBusy();

    if (!isRunningOnHost()) {
        endPasteButtonBusy();
    }
    
    onPasteRequest_ntf();
}

function onPasteButtonExpanderClickOrKeyDown(e) {
    if (isPastePopupExpanded()) {
        unexpandPasteButton(false);
    } else {
        expandPasteButton(false);
    }
}
// #endregion Event Handlers