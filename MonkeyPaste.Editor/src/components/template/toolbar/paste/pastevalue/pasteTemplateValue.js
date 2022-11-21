// #region Globals

// #endregion Globals

// #region Life Cycle
function initPasteTemplateValue() {

    getPasteValueTextAreaElement().addEventListener('input', onTemplatePasteValueChanged);
    getPasteValueTextAreaElement().addEventListener('focus', onTemplatePasteValueFocus);
    initBouncyTextArea(getPasteValueTextAreaElement());
}

// #endregion Life Cycle

// #region Getters

function getPasteValueTextAreaElement() {
    return document.getElementById('templatePasteValueTextArea');
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function evalIsPasteValueTextAreaReadOnly(ft) {
    if (!ft) {
        return true;
    }
    return !isTemplateAnInputType(ft);
}

function evalPasteTextAreaValue(ft) {
    if (!ft) {
        return '';
    }
    if (isTemplateAnInputType(ft)) {
        return getTemplatePasteValue(ft);
    }
    return '';
}

function evalPasteTextAreaPlaceholderValue(ft) {
    if (!ft) {
        return '';
    }
    if (isTemplateAnInputType(ft)) {
        return `Enter paste text for [${ft.templateName}] here...`;
    }
    return getTemplatePasteValue(ft);
}

// #endregion State

// #region Actions

function updatePasteValueTextAreaToFocus(ft) {
    let pv_textarea_elm = getPasteValueTextAreaElement();
    pv_textarea_elm.readOnly = evalIsPasteValueTextAreaReadOnly(ft);
    pv_textarea_elm.value = evalPasteTextAreaValue(ft);
    pv_textarea_elm.placeholder = evalPasteTextAreaPlaceholderValue(ft);
}
// #endregion Actions

// #region Event Handlers

function onTemplatePasteValueChanged(e) {
    let newTemplatePasteValue = getPasteValueTextAreaElement().value;
    let ftguid = getFocusTemplateGuid();
    if (!ftguid) {
        ftguid = getSelectedOptionTemplateGuid();
        if (!ftguid) {
            debugger;
        } else {
            //focusTemplate(ftguid, false);
        }
        updatePasteTemplateToolbarToSelection();
        //ftguid = getFocusTemplateGuid();
    }
    setTemplatePasteValue(ftguid, newTemplatePasteValue);
    //updatePasteElementInteractivity();
}

function onTemplatePasteValueFocus(e) {
    let stguid = getSelectedOptionTemplateGuid();
    let ftguid = getFocusTemplateGuid();
    if (stguid != ftguid) {
        if (!stguid) {
            debugger;
            return;
        }
        focusTemplate(stguid);
    }

}
// #endregion Event Handlers