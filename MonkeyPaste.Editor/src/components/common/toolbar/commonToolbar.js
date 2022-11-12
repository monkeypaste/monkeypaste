// #region Globals

// #endregion Globals

// #region Life Cycle
function initBouncyTextArea(elm) {
    elm.addEventListener('focus', onBouncyTextAreaFocus);
    elm.addEventListener('blur', onBouncyTextAreaBlur);
    elm.addEventListener('keydown', onBouncyTextAreaKeyUp);
    elm.addEventListener('keyup', onBouncyTextAreaKeyDown);
}
// #endregion Life Cycle

// #region Getters

function getTemplateToolbarsHeight() {
    let total_h = getEditTemplateToolbarHeight();
    total_h += getPasteToolbarHeight();
    return total_h;
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function updateTemplateToolbarSizesAndPositions() {
    if (isShowingPasteToolbar()) {
        updatePasteToolbarSizesAndPositions();
    }
    if (isShowingEditTemplateToolbar()) {
        updateEditTemplateToolbarSizesAndPositions()
    }
}

function addClickOrKeyClickEventListener(elm, handler, capture = false) {
    elm.addEventListener('click', function (e) {
        if (!isElementDisabled(e.currentTarget)) {
            handler(e);

            // prevent parent/children handlers from receiving
            e.stopPropagation();
        }
    }, capture);
    elm.addEventListener('keydown', function (e) {
        if (isMouseOrKeyboardButtonClick(e)) {
            handler(e);
        }
    }, capture);
}
async function scaleFocusTemplates(scaleType, tguid) {
    if (!tguid) {
        tguid = getFocusTemplateGuid();
        if (!tguid) {
            return;
        }
    }

    let f_cit_elm_l = getTemplateElements(tguid);
    for (var i = 0; i < f_cit_elm_l.length; i++) {
        let f_cit_elm = f_cit_elm_l[i];
        scaleElement(f_cit_elm, scaleType);
    }
}

function scaleElement(elm, scaleType) {
    if (scaleType == 'bigger') {
        elm.classList.remove('ql-template-embed-blot-display-key-up');
        elm.classList.add('ql-template-embed-blot-display-key-down');
    } else if (scaleType == 'default') {
        elm.classList.remove('ql-template-embed-blot-display-key-down');
        elm.classList.add('ql-template-embed-blot-display-key-up');
    } else {

        elm.classList.remove('ql-template-embed-blot-display-key-down');
        elm.classList.remove('ql-template-embed-blot-display-key-up');
    }
}


function clearAllTemplateEditClasses() {
    let telms = getTemplateElements();
    for (var i = 0; i < telms.length; i++) {
        let telm = telms[i];
        telm.classList.remove('ql-template-embed-blot-display-key-up');
        telm.classList.remove('ql-template-embed-blot-display-key-down');
    }
}

async function bounceElement(elm) {
    sleep(100);
    elm.style.transform = 'scale(1.3)';
    //scaleElement(elm, 'bigger');
    sleep(300);
    elm.style.transform = 'scale(1.0)';
    //scaleElement(elm, 'default');
}

// unused
async function jiggleFocusTemplates(resetOnComplete = false) {
    return;
    //let f_cit = getFocusTemplateElement();
    let tguid = getFocusTemplateGuid();
    if (!tguid) {
        return;
    }
    let scale_ms = 100;
    for (var i = 0; i < 2; i++) {
        await scaleFocusTemplates('bigger', tguid);
        await delay(scale_ms);
        await scaleFocusTemplates('default', tguid);
        await delay(scale_ms);
    }
    if (resetOnComplete) {
        scaleFocusTemplates('reset', tguid);
    }
}
// #endregion Actions

// #region Event Handlers

function onBouncyTextAreaFocus(e) {
    jiggleFocusTemplates();
}

function onBouncyTextAreaBlur(e) {
    jiggleFocusTemplates(true);
}


async function onBouncyTextAreaKeyDown(e) {
    await scaleFocusTemplates('bigger');
}

async function onBouncyTextAreaKeyUp(e) {
    await scaleFocusTemplates('default');
    await delay(100);
    clearAllTemplateEditClasses();
}
// #endregion Event Handlers