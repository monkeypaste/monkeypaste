// #region Globals

var RequiredNavigateUriModKeys = [
    //'Control'
];

const LinkTypes = [
    'fileorfolder',
    'uri',
    'email',
    'phonenumber',
    'currency',
    'hexcolor',
    'streetaddress'
];

var LinkTypeAttrb = null;

// #endregion Globals

// #region Life Cycle
function initLinks() {
    initLinkClassAttributes();
    initLinkMatcher();
    addClickOrKeyClickEventListener(getLinkEditorToolbarItemElement(), onLinkToolbarItemClick);
    getLinkEditorToolbarItemElement().innerHTML = getSvgHtml('link',null,false);
}

function initLinkClassAttributes() {
    const Parchment = Quill.imports.parchment;
    let suppressWarning = true;
    let config = {
        scope: Parchment.Scope.INLINE,
    };
    LinkTypeAttrb = new Parchment.ClassAttributor('linkType', 'link-type', config);

    Quill.register(LinkTypeAttrb, suppressWarning);
}

function initLinkMatcher() {
    // NOTE! quill renders all li's with data-list attr (bullet|ordered|checked|unchecked)
    // delta-html converter clears ordered and bullet li's attrs and encloses in ol|ul respectively
    // delta-html converter substitutes li's w/ data-list attr (checked|unchecked) w/ data-checked attr (true|false)

    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    let Delta = Quill.imports.delta;

    quill.clipboard.addMatcher('a', function (node, delta) {
        if (node.hasAttribute('style')) {
            let bg = getElementComputedStyleProp(node, 'background-color');
            if (bg) {
                bg = cleanHexColor(bg,1,true);
            }
            let fg = getElementComputedStyleProp(node, 'color');
            if (fg) {
                fg = cleanHexColor(fg,1,true);
            }

            log('link text: ' + node.innerText + ' bg: ' + bg + ' fg: ' + fg);
            if (delta && delta.ops !== undefined && delta.ops.length > 0) {
                for (var i = 0; i < delta.ops.length; i++) {
                    if (delta.ops[i].insert === undefined) {
                        continue;
                    }
                    if (delta.ops[i].attributes === undefined) {
                        delta.ops[i].attributes = {};
                    }
                    if (bg) {
                        delta.ops[i].attributes.color = bg;
                    }

                    if (fg) {
                        delta.ops[i].attributes.color = fg;
                    }

                }
            }
        }
        let link_type = Array.from(node.classList).find(x => LinkTypes.includes(x));
        if (link_type) {
            log('link class type: ' + link_type);

            if (delta && delta.ops !== undefined && delta.ops.length > 0) {
                for (var i = 0; i < delta.ops.length; i++) {
                    if (delta.ops[i].insert === undefined) {
                        continue;
                    }
                    if (delta.ops[i].attributes === undefined) {
                        delta.ops[i].attributes = {};
                    }
                    delta.ops[i].attributes.linkType = link_type;
                    if (link_type == 'hexcolor') {
                        delta.ops[i].attributes.background = node.innerText;
                        delta.ops[i].attributes.color = isBright(node.innerText) ? 'black' : 'white';
                    }
                }
            }
            LinkTypeAttrb.add(node, link_type);
        } else {
            log('no type class for link, classes: ' + node.getAttribute('class'));
        }
        return delta;
    });
}

function loadLinkHandlers() {
    if (ContentItemType == 'FileList') {
        RequiredNavigateUriModKeys = ['Alt'];
    } else {
        RequiredNavigateUriModKeys = [];
    }

    let a_elms = Array.from(getEditorElement().querySelectorAll('a'));
    for (var i = 0; i < a_elms.length; i++) {
        let link_elm = a_elms[i];
        link_elm.addEventListener('click', onLinkClick, true);
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

    quill.theme.tooltip.edit('link', '');
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

function onLinkClick(e) {
    if (e.currentTarget === undefined || e.currentTarget.href === undefined) {
        debugger;
        return;
    }
    let down_mod_keys = getDownModKeys(e);
    e.preventDefault();
    e.stopPropagation();

    let can_nav = RequiredNavigateUriModKeys.every(x => down_mod_keys.includes(x));
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
    let link_doc_range = getElementDocRange(link_elm);

    onNavigateUriRequested_ntf(e.currentTarget.href, link_type, link_doc_range.index, e.currentTarget.innerText, down_mod_keys);
    return false;
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
            quill.formatText(sel.index, sel.length, link_format);
        }        
    }
    hideLinkPopup();
}

function onLinkPointerEnter(e) {
    if (!isSubSelectionEnabled() ||
        WindowMouseDownLoc != null) {
        return;
    }
    let a_elm = e.currentTarget;
    let mod_key_text = '';
    if (RequiredNavigateUriModKeys.length > 0) {
        mod_key_text = `[+ ${RequiredNavigateUriModKeys.join(' ')}]`;
    }
    let link_action_text = '';
    if (a_elm.classList.contains('link-type-hexcolor')) {
        link_action_text = `edit '<em>${a_elm.innerHTML}</em>'`;
    } else {
        //link_action_text = `goto '<em>${a_elm.getAttribute('href')}</em>'`;
        link_action_text = 'follow...';
    }
    showOverlayTooltip(a_elm, `Click${mod_key_text} to ${link_action_text}`);
}

function onLinkPointerLeave(e) {
    hideOverlayTooltip();
}
function onLinkPopupClose(e) {
    loadLinkHandlers();
}
// #endregion Event Handlers