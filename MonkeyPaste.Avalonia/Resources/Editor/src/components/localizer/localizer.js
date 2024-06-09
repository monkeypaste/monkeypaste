
// #region Life Cycle
function initLocalizer(cc) {
    let src_str = `${globals.wwwroot}src/components/localizer/UiStrings.${cc}.js`;
    let culture_script_elm = document.createElement('script');
    culture_script_elm.classList.add('culture-script');
    culture_script_elm.setAttribute('defer', null);
    culture_script_elm.setAttribute('async', null);
    culture_script_elm.setAttribute('src', src_str);
    culture_script_elm.addEventListener('load', (e) => {
        let cur_culture_elms = Array.from(document.head.querySelectorAll('.culture-script'));
        for (var i = 0; i < cur_culture_elms.length; i++) {
            if (cur_culture_elms[i].getAttribute('src') == src_str) {
                continue;
            }
            cur_culture_elms[i].remove();
        }
        toggleRightToLeft(globals.IsRtl);
        localizeCss();
        localizeGlobals();
        initLocalizerDomWatcher();
        getLocalizableElements().forEach(x => localizeElement(x));
        updateEditorPlaceholderText();
        log('culture set to: ' + cc);
    });
    document.head.appendChild(culture_script_elm);
    //document.write(culture_script_elm.outerHTML);
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
function localizeCss() {
    setElementComputedStyleProp(document.body, '--linkLabel', 'POOP');
}
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

function localizeTemplates() {
    for (var i = 0; i < globals.TemplateTypesMenuOptions.length; i++) {
        if (globals.TemplateTypesMenuOptions[i].separator) {
            // separator
            continue;
        }
        if (isNullOrEmpty(globals.TemplateTypesMenuOptions[i].id)) {
            continue;
        }
        let content_key = globals.TemplateTypesMenuOptions[i].id.replaceAll('#', '');
        globals.TemplateTypesMenuOptions[i].label = UiStrings[content_key];
    }
}
function localizeGlobals() {
    // labels are '#<ResourceKeyName>#'

    // TEMPLATE NAME 
    localizeTemplates();

    // TEMPLATE CUSTOM DATETIME
    let custom_label_key = globals.CUSTOM_TEMPLATE_LABEL_VAL.replaceAll('#', '');
    globals.CUSTOM_TEMPLATE_LABEL_VAL = UiStrings[custom_label_key];

    initGlobals();
}
function toggleRightToLeft(isRightToLeft) {
    let dir = isRightToLeft ? 'rtl' : 'ltr';
    let align = isRightToLeft ? 'right' : 'left';

    globals.quill.format('direction', dir);
    globals.quill.format('align', align);
    if (isRightToLeft) {
        document.body.classList.add('right-to-left');
    } else {
        document.body.classList.remove('right-to-left');
    }

    log('editor right-to-left enabled: ' + isRightToLeft);
}


// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers