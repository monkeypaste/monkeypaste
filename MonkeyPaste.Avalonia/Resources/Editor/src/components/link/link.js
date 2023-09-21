

// #region Life Cycle
function initLinks() {
    initLinkClassAttributes();
    addClickOrKeyClickEventListener(getLinkEditorToolbarItemElement(), onLinkToolbarItemClick);
    getLinkEditorToolbarItemElement().innerHTML = getSvgHtml('link',null,false);
}

function initLinkClassAttributes() {
    const Parchment = Quill.imports.parchment;
    let suppressWarning = true;
    let config = {
        scope: Parchment.Scope.INLINE,
    };
    globals.LinkTypeAttrb = new Parchment.ClassAttributor('linkType', 'link-type', config);

    Quill.register(globals.LinkTypeAttrb, suppressWarning);
}



function loadLinkHandlers() {
    let a_elms = Array.from(getEditorElement().querySelectorAll('a'));
    for (var i = 0; i < a_elms.length; i++) {
        let link_elm = a_elms[i];

        link_elm.addEventListener('click', function (event) {
            event.preventDefault();
        }, true);
        link_elm.addEventListener('pointerup', onLinkPointerUp, true);
        link_elm.addEventListener('pointerenter', onLinkPointerEnter, true);
        link_elm.addEventListener('pointerleave', onLinkPointerLeave, true);

        if (link_elm.getAttribute('href') == 'about:blank') {
            link_elm.setAttribute('href', 'javascript:;');
        }
    }
}
// #endregion Life Cycle

// #region Getters

function getLinkEditorToolbarItemElement() {
    return document.getElementById('linkEditorToolbarButton');
}

function getLinkToolbarPopupContainerElement() {
    let tooltip_elms = Array.from(document.querySelectorAll('.ql-tooltip.ql-editing'));
    for (var i = 0; i < tooltip_elms.length; i++) {
        if (tooltip_elms[i].getAttribute('data-mode') == 'link') {
            return tooltip_elms[i];
        }
    }
    return null;
}

function getLinkToolbarPopupSaveButtonElement() {
    let link_tooltip_container_elm = getLinkToolbarPopupContainerElement();
    if (!link_tooltip_container_elm) {
        return null;
    }

    return Array.from(link_tooltip_container_elm.querySelectorAll('.ql-action'))[0];
}

function getLinkToolbarPopupTextInputElement() {
    let link_tooltip_container_elm = getLinkToolbarPopupContainerElement();
    if (!link_tooltip_container_elm) {
        return null;
    }

    return Array.from(link_tooltip_container_elm.querySelectorAll('input'))[0];
}

function getLinkMouseGestureHtml() {
    let gesture_parts = [
        'Click',
        ...globals.RequiredNavigateUriModKeys
    ];

    let result = `<em><strong>${gesture_parts.join('+')}</strong></em> `;
    return result;
}

function getLinkTooltipText(a_elm, includeContext = false) {
    // NOTE omitting context to avoid long tooltips cause still haven't 
    // found solid layout to prevent overlap/overflow issues..
    let result = getLinkMouseGestureHtml();
    if (a_elm.classList.contains('link-type-hexcolor')) {
        if (includeContext) {
            result += `edit '<em>${a_elm.innerHTML}</em>'...`;
        } else {
            result += 'edit color...';
        }
    } else if (a_elm.classList.contains('link-type-delete-item')) {
        if (includeContext) {
            let fli_text = a_elm.parentElement.parentElement.previousSibling.innerText;
            result += `remove '<em>${fli_text}</em>'`;
        } else {
            result += 'remove item';
        }
    } else {
        if (includeContext) {
            result += `goto '<em>${a_elm.getAttribute('href')}</em>'...`;
        } else {
            result += 'to follow...'
        }
    }
    return result;
}

// #endregion Getters

// #region Setters
// #endregion Setters

// #region State

// #endregion State

// #region Actions

function showLinkPopupForSelection() {
    const sel = getDocSelection();

    // pre-exisiting link

    let link_input_val = null;
    let sel_format = getFormatAtDocIdx(sel.index);
    if (!isNullOrUndefined(sel_format.link)) {
        // when link in selection adjust range to link
        let sel_elm = getElementAtDocIdx(sel.index);
        if (sel_elm.nodeType == 3) {
            sel_elm = sel_elm.parentNode;
        }
        let link_doc_range = getElementDocRange(sel_elm);
        setDocSelection(link_doc_range.index, link_doc_range.length, 'silent');
        link_input_val = sel_elm.getAttribute('href');
    }

    globals.quill.theme.tooltip.edit('link', '');
    positionTooltipToDocRange(sel);
    getEditorElement().addEventListener('focus', onLinkPopupClose);

    getLinkToolbarPopupContainerElement().classList.add('ql-preview-override');
    // link input
    let link_input_elm = getLinkToolbarPopupTextInputElement();
    if (link_input_elm) {
        link_input_elm.setAttribute('draggable', false);
        link_input_elm.setAttribute('placeholder', 'Enter url...');
        link_input_elm.addEventListener('keydown', onLinkPopupKeyDown, true);
        link_input_elm.value = link_input_val;
    }

    // link save
    let link_save_button_elm = getLinkToolbarPopupSaveButtonElement();
    if (link_save_button_elm) {
        link_save_button_elm.addEventListener('click', onLinkPopupSaveClick, true);
    }    
}

function hideLinkPopup() {
    let link_popup_elm = getLinkToolbarPopupContainerElement();
    if (link_popup_elm) {
        link_popup_elm.classList.add('ql-hidden');
        link_popup_elm.classList.remove('ql-preview-override');
    }
}

function convertLinkElementToHostNav(a_elm) {
    if (!a_elm) {
        return;
    }
    let href_val = a_elm.getAttribute('href');
    const disabled_href = 'javascript: void(0)';
    if (href_val != disabled_href) {

    }
}

function formatRangeAsLink(range, source = 'user') {
    formatDocRange(range, 'link', source);
}
// #endregion Actions

// #region Event Handlers

function onLinkPointerUp(e) {
    if (e.currentTarget === undefined || e.currentTarget.href === undefined) {
        debugger;
        return;
    }
    let down_mod_keys = getDownModKeys(e);
    e.preventDefault();
    e.stopPropagation();

    let can_nav = globals.RequiredNavigateUriModKeys.every(x => down_mod_keys.includes(x));
    if (!can_nav) {
        return;
    }
    let link_type = 'unknown';
    let link_type_vals = Array.from(e.currentTarget.classList).filter(x => !isNullOrEmpty(x) && x.startsWith('link-type'));
    if (link_type_vals && link_type_vals.length > 0) {
        link_type = link_type_vals[0].replace('link-type-', '');
    }

    let link_elm = e.currentTarget;
    if (link_elm.nodeType == 3) {
        link_elm = link_elm.parentNode;
    }
    if (link_elm.getAttribute('href') == 'about:blank') {
        link_elm.setAttribute('href', 'javascript:;');
    }

    if (link_type == 'delete-item') {
        // only occurs for file item remove
        removeFileItemByAnchorElement(link_elm);
        return false;
    }
    let link_doc_range = getElementDocRange(link_elm);

    onNavigateUriRequested_ntf(e.currentTarget.href, link_type, link_doc_range.index, e.currentTarget.innerText, down_mod_keys);
    return true;
}

function onLinkToolbarItemClick(e) {
    // NOTE based on snow.js 116-131
    const sel = getDocSelection();
    if (sel == null) {
        return;
    }
    if (sel.length === 0) {
        showLinkPopupForSelection();
        return;
    } 
    let sel_text = getText(sel);
    if (isValidUri(sel_text)) {
        formatRangeAsLink(sel);
    } else {
        showLinkPopupForSelection();
    }

    e.preventDefault();
    e.stopPropagation();
    return false;
}

function onLinkPopupKeyDown(e) {
    if (e.key == 'Enter') {
        e.preventDefault();
        e.stopPropagation();
        onLinkPopupSaveClick(e);
        return;
    }
    if (e.key == 'Escape') {
        hideLinkPopup();
        return;
    }
}

function onLinkPopupSaveClick(e) {
    e.preventDefault();
    e.stopPropagation();

    let link_popup_input_elm = getLinkToolbarPopupTextInputElement();
    if (link_popup_input_elm) {
        let link_href = link_popup_input_elm.value;
        let link_format = {
            link: link_href,
            linkType: 'uri'
        };
        let sel = getDocSelection();
        if (!sel) {
            sel = { index: 0, length: 0 };
        }

        if (sel.length == 0) {
            insertText(sel.index, link_href, link_format);
        } else {
            globals.quill.formatText(sel.index, sel.length, link_format);
        }        
    }
    hideLinkPopup();
}

function onLinkPointerEnter(e) {
    if (!isSubSelectionEnabled() ||
        globals.WindowMouseDownLoc != null) {
        return;
    }
    let a_elm = e.currentTarget;
    
    let link_tooltip_text = getLinkTooltipText(a_elm);
    
    if (globals.IsTooltipToolbarEnabled) {
        showTooltipToolbar(a_elm, link_tooltip_text);
    } else if (globals.IsTooltipOverlayEnabled) {
        showTooltipOverlay(a_elm, link_tooltip_text);
    }    
}

function onLinkPointerLeave(e) {
    if (globals.IsTooltipToolbarEnabled) {
        hideTooltipToolbar();
    } else if (globals.IsTooltipOverlayEnabled) {
        hideTooltipOverlay();
    }
}

function onLinkPopupClose(e) {
    loadLinkHandlers();
}
// #endregion Event Handlers