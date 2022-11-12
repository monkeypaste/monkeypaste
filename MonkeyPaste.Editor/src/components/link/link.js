// #region Globals

const RequiredNavigateUriModKeys = [];

// #endregion Globals

// #region Life Cycle
function initLinks() {
    addClickOrKeyClickEventListener(getLinkEditorToolbarItemElement(), onLinkToolbarItemClick);
}

function loadLinkHandlers() {
    let a_elms = Array.from(getEditorElement().querySelectorAll('a'));
    for (var i = 0; i < a_elms.length; i++) {
        let a_elm = a_elms[i];
        a_elm.addEventListener('click', onLinkClick, true);
	}
}
// #endregion Life Cycle

// #region Getters

function getLinkEditorToolbarItemElement() {
    return document.getElementById('linkEditorToolbarButton');
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State


// #endregion State

// #region Actions
function showLinkPopupForSelection() {
    const sel = getDocSelection();
    let preview_text = '';
    if (sel.length > 0) {
        preview_text = getText(sel);
    }
    quill.theme.tooltip.edit('link', preview_text);
    positionTooltipToDocRange(sel);
    getEditorElement().addEventListener('focus', onLinkPopupClose);
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
    let can_nav = RequiredNavigateUriModKeys.every(x => down_mod_keys.includes(x));
    if (!can_nav) {
        return;
    }
    e.preventDefault();
    e.stopPropagation();
    onNavigateUriRequested_ntf(e.currentTarget.href, down_mod_keys);
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

function onLinkPopupClose(e) {
    loadLinkHandlers();
}
// #endregion Event Handlers