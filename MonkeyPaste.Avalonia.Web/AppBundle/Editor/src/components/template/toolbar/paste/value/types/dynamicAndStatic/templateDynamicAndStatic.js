// #region Globals

// #endregion Globals

// #region Life Cycle
function initPasteTemplateValue() {
    getPasteValueTextAreaElement().addEventListener('input', onTemplatePasteValueChanged);
    getPasteValueTextAreaElement().addEventListener('focus', onTemplatePasteValueFocus);
    getPasteValueTextAreaElement().addEventListener('focus', onTemplatePasteValueBlur);

    initBouncyTextArea(getPasteValueTextAreaElement());
}

// #endregion Life Cycle

// #region Getters

function getDasTemplateOuterContainerElement() {
    return document.getElementById('pasteTemplateToolbarDasContainer');
}

function getPasteValueTextAreaElement() {
    return document.getElementById('templatePasteValueTextArea');
}

function getPasteValueHintContainerElement() {
    return document.getElementById('templatePasteValueHintOuterContainer');
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function updatePasteValueTextAreaToFocus(ft) {
    let das_container_elm = getDasTemplateOuterContainerElement();
    if (!ft || (ft.templateType.toLowerCase() != 'dynamic' && ft.templateType.toLowerCase() != 'static')) {
        das_container_elm.classList.add('hidden');
        return;
    }
    das_container_elm.classList.remove('hidden');
    getPasteValueTextAreaElement().value = getTemplatePasteValue(ft);
    getPasteValueTextAreaElement().placeholder = `[${ft.templateName}] is empty...`;    
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

    let t = getTemplateDefByGuid(ftguid);
    if (isTemplateDataDriven(t)) {
        // apply data to all editor elements
        setTemplateData(ftguid, newTemplatePasteValue)
        // get new computed paste value
        newTemplatePasteValue = getTemplatePasteValue(t);
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
function onTemplatePasteValueBlur(e) {
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