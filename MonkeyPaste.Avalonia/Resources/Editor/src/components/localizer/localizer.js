
// #region Life Cycle
function initLocalizer() {
    initLocalizerDomWatcher();
    getLocalizableElements().forEach(x => localizeElement(x));
}

function initLocalizerDomWatcher() {
    let observer = new MutationObserver(mutations => {
        for (let mutation of mutations) {
            // examine new nodes, there is something to highlight
            for (let node of mutation.addedNodes) {
                if (!(node instanceof HTMLElement)) {
                    continue;
                }
                if (!node.hasAttribute(globals.LOCALIZER_UI_STRING_CONTENT_ATTR_NAME) &&
                    !node.hasAttribute(globals.LOCALIZER_UI_STRING_TOOLTIP_ATTR_NAME) &&
                    !node.hasAttribute(globals.LOCALIZER_UI_STRING_PLACEHOLDER_ATTR_NAME)) {
                    continue;
                }
                localizeElement(node);
            }
        }
    });

    observer.observe(document.body, {
        childList: true,
        subtree: true,
        attributes: true,
    });
}
// #endregion Life Cycle

// #region Getters

function getLocalizableElements() {
    return Array.from(document.querySelectorAll(`[${globals.LOCALIZER_UI_STRING_CONTENT_ATTR_NAME}],[${globals.LOCALIZER_UI_STRING_TOOLTIP_ATTR_NAME}],[${globals.LOCALIZER_UI_STRING_PLACEHOLDER_ATTR_NAME}]`));
}

// #endregion Getters

// #region Setters


// #endregion Setters

// #region State

// #endregion State

// #region Actions
function localizeElement(elm, args) {
    if (elm.hasAttribute(globals.LOCALIZER_UI_STRING_CONTENT_ATTR_NAME)) {
        // has content key
        let content_key = elm.getAttribute(globals.LOCALIZER_UI_STRING_CONTENT_ATTR_NAME);
        elm.innerHTML = UiStrings[content_key];
        //log('ui-content-key: ' + content_key + ' str: ' + elm.innerHTML);
    }
    if (elm.hasAttribute(globals.LOCALIZER_UI_STRING_TOOLTIP_ATTR_NAME)) {
        // has tooltip key
        let tt_key = elm.getAttribute(globals.LOCALIZER_UI_STRING_TOOLTIP_ATTR_NAME);
        elm.setAttribute(globals.TOOLTIP_HOVER_ATTRB_NAME, UiStrings[tt_key]);
        //log('ui-tooltip-key: ' + tt_key + ' str: ' + UiStrings[tt_key]);
    }
    if (elm.hasAttribute(globals.LOCALIZER_UI_STRING_PLACEHOLDER_ATTR_NAME)) {
        // has tooltip key
        let tt_key = elm.getAttribute(globals.LOCALIZER_UI_STRING_PLACEHOLDER_ATTR_NAME);
        elm.setAttribute('placeholder', UiStrings[tt_key]);
        //log('ui-tooltip-key: ' + tt_key + ' str: ' + UiStrings[tt_key]);
    }
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers