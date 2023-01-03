// #region Globals

const RequiredNavigateUriModKeys = [];

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
}

function initLinkClassAttributes() {
    const Parchment = Quill.imports.parchment;
    let suppressWarning = false;
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

    quill.clipboard.addMatcher('A', function (node, delta) {
        if (node.hasAttribute('style')) {
            let bg = getElementComputedStyleProp(node, 'background-color');
            if (bg) {
                bg = cleanHexColor(bg);
            }
            let fg = getElementComputedStyleProp(node, 'color');
            if (fg) {
                fg = cleanHexColor(fg);
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
            //LinkTypeAttrb.add(node, link_type);
        } else {
            log('no type class for link, classes: ' + node.getAttribute('class'));
        }
        return delta;
    });
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