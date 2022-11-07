
// #region Globals

const PasteToolbarDefaultWidth = 1125.0;

var IsPastingTemplate = false;
var IsTemplatePasteValueTextAreaFocused = false;

var PasteTemplateTimerInterval = null;

// #endregion Globals

// #region Life Cycle

function initPasteTemplateToolbarItems() {
    addClickOrKeyClickEventListener(getPasteGotoNextButtonElement(), onPasteTemplateGotoNextClickOrKeyDown);
    addClickOrKeyClickEventListener(getPasteGotoPrevButtonElement(), onPasteTemplateGotoPrevClickOrKeyDown);
    addClickOrKeyClickEventListener(getPasteClearTextButtonElement(), onPasteTemplateClearAllValuesClickOrKeyDown);
    addClickOrKeyClickEventListener(getPasteEditFocusTemplateButtonElement(), onPasteEditFocusTemplateClickOrKeyDown);

    getPasteValueTextAreaElement().addEventListener('input', onTemplatePasteValueChanged);
    initBouncyTextArea(getPasteValueTextAreaElement());

    initPasteTemplateFocusSelector();
}


function showPasteTemplateToolbarItems() {
    let ptil = Array.from(document.getElementsByClassName('paste-template-item'));
    for (var i = 0; i < ptil.length; i++) {
        ptil[i].classList.remove('hidden');
    }

    hidePasteTemplateSelectorOptions();

    updatePasteTemplateToolbarToSelection();

    PasteTemplateTimerInterval = setInterval(onPasteTemplateTimer, 300, getEditorElement());
}

function hidePasteTemplateToolbarItems() {
    let ptil = Array.from(document.getElementsByClassName('paste-template-item'));
    for (var i = 0; i < ptil.length; i++) {
        ptil[i].classList.add('hidden');
    }

    clearInterval(PasteTemplateTimerInterval);
}

// #endregion Life Cycle

// #region Getters

function getPasteGotoNextButtonElement() {
    return document.getElementById('nextTemplateButton');
}

function getPasteGotoPrevButtonElement() {
    return document.getElementById('previousTemplateButton');
}

function getPasteClearTextButtonElement() {
    return document.getElementById('clearAllTemplateTextButton');
}

function getPasteEditFocusTemplateButtonElement() {
    return document.getElementById('pasteEditFocusTemplateButton');
}

function getPasteValueTextAreaElement() {
    return document.getElementById('templatePasteValueTextArea');
}

function getTemplatePasteValue(t) {
    if (!t) {
        return '';
    }

    let ttype = t.templateType.toLowerCase();

    if (ttype == 'dynamic') {
        return t.templateText;
	}
    if (ttype == 'static') {
        return t.templateData;
    }
    if (ttype == 'datetime') {
        return jQuery.format.date(new Date(), t.templateData);
    }
    if (ttype == 'contact') {
        if (isNullOrWhiteSpace(t.templateData)) {
            // error, data must be contact field on hide edit template 
            debugger;
            return null;
        }

        if (SelectedContactGuid == null) {
            // TODO should be selected fro drop down in paste toolbar
            return null;
        }

        return getContactFieldValue(SelectedContactGuid, t.templateData);
	}
}

function getPasteTemplateDefs() {
    let paste_sel = getDocSelection(true);
    return getTemplateDefsInRange(paste_sel);
}

// #endregion Getters

// #region Setters

function setTemplatePasteValue(tguid, val) {
    var telms = getTemplateElements(tguid);
    for (var i = 0; i < telms.length; i++) {
        var telm = telms[i];
        let t = getTemplateFromDomNode(telm);
        let bouncing = false;
   //     if (!isTemplateAnInputType(t)) {
   //         if (telm.innerText == val) {
   //             telm.style.transform = 'scale(1)';
   //         } else {
   //             telm.style.transform = 'scale(1.1)'
   //             bouncing = true;

			//}
   //     } 
        let paste_val = getTemplatePasteValue(t);
        let has_changed = paste_val != val;
        if (has_changed) {
            telm.innerText = val;
            telm.setAttribute('templateText', val);
            if (!bouncing) {
                debabyTemplateElement(telm);

            }
		}
        
    }
    //updateTemplatesAfterTextChanged();
}

// #endregion Setters

// #region State

function isTemplateReadyToPaste(t) {
    let t_paste_val = getTemplatePasteValue(t);
    let ttype = t.templateType.toLowerCase();
    if (ttype == 'dynamic' || ttype == 'contact') {
        if (!isNullOrEmpty(t_paste_val)) {
            return true;
        }
        // visit trues after lost focus and allows empty to be pastable
        return t.wasVisited;
    }
    return true;
}

function isPasteButtonEnabled() {
    return !getPasteButtonElement().classList.contains('disabled');
}

function isShowingPasteTemplateToolbar() {
    return !getPasteToolbarContainerElement().classList.contains('hidden');
}

// #endregion State

// #region Actions

function findPasteFocusTemplate(sel) {
    let tl = getTemplateDefsInRange(sel);
    let ftguid = getFocusTemplateGuid();
    if (ftguid &&
        (tl.filter(x => x.templateGuid == ftguid).length == 0 || // last focus template is not in range
            !isTemplateAnInputType(getTemplateDefByGuid(ftguid)) // last focus was not input type
        )) {
        // flag for reset to current sel range
        ftguid = null;
    }
    if (!ftguid && tl.length > 0) {
        let focusable_tl = tl.filter(x => isTemplateAnInputType(x));
        if (focusable_tl.length > 0) {
            // prefer input template
            ftguid = focusable_tl[0].templateGuid;
        } else {
            // fallback to any 
            ftguid = tl[0].templateGuid;
		}
    }
    clearTemplateFocus();
    return ftguid;
}

function gotoNextTemplate(force_tguid = null) {
    let ftguid = force_tguid ? force_tguid : getFocusTemplateGuid();
    let sel_tl = getPasteTemplateDefs();
    // ignore non-input if input is in selection
    let ignoreNonInputTemplates = sel_tl.filter(x => isTemplateAnInputType(x)).length > 0;
    let curIdx = 0;
    for (var i = 0; i < sel_tl.length; i++) {
        if (sel_tl[i].templateGuid == ftguid) {
            curIdx = i;
            break;
        }
    }
    let nextIdx = curIdx + 1;
    if (nextIdx >= sel_tl.length) {
        nextIdx = 0;
    }
    let t = sel_tl[nextIdx];

    if (ignoreNonInputTemplates && !isTemplateAnInputType(t)) {
        gotoNextTemplate(t.templateGuid);
        return;
    }

    focusTemplate(t.templateGuid);
}

function gotoPrevTemplate(force_tguid = null) {
    let ftguid = force_tguid ? force_tguid : getFocusTemplateGuid();
    let sel_tl = getPasteTemplateDefs();

    // ignore non-input if input is in selection
    let ignoreNonInputTemplates = sel_tl.filter(x => isTemplateAnInputType(x)).length > 0;
    let curIdx = 0;
    for (var i = 0; i < sel_tl.length; i++) {
        if (sel_tl[i].templateGuid == ftguid) {
            curIdx = i;
            break;
        }
    }
    let prevIdx = curIdx - 1;
    if (prevIdx < 0) {
        prevIdx = sel_tl.length - 1;
    }
    let t = sel_tl[prevIdx];
    if (ignoreNonInputTemplates && !isTemplateAnInputType(t)) {
        gotoPrevTemplate(t.templateGuid);
        return
    }
    focusTemplate(t.templateGuid);
}

function clearAllTemplateText() {
    var tl = getTemplateDefs();
    for (var i = 0; i < tl.length; i++) {
        setTemplatePasteValue(tl[i].templateGuid, '');
    }
}

function updatePasteToolbarSizesAndPositions() {
    updatePasteValueTextAreaSize();
    updatePasteTemplateOptionsBounds();
}

function updatePasteValueTextAreaSize() {
    // sanity padding to avoid single line y scroll bar

    getPasteValueTextAreaElement().style.height = '0px';

    let ptth = getPasteToolbarHeight();
    let pvta_elm = getPasteValueTextAreaElement();

    let ta_parent_y_margin =
        parseFloat(getElementComputedStyleProp(pvta_elm, 'margin-top')) +
        parseFloat(getElementComputedStyleProp(pvta_elm, 'margin-bottom'));

    let ta_parent_y_padding =
        parseFloat(getElementComputedStyleProp(pvta_elm, 'padding-top')) +
        parseFloat(getElementComputedStyleProp(pvta_elm, 'padding-bottom'));

    let pvta_parent_offset = ta_parent_y_margin;// + ta_parent_y_padding;

    let ta_height = ptth - pvta_parent_offset;
    getPasteValueTextAreaElement().style.height = ta_height + 'px';
}

function updatePasteTemplateToolbarToSelection(force_ftguid) {
    let paste_sel = getDocSelection(true);
    let pre_ftguid = getFocusTemplateGuid();

    let ftguid = force_ftguid;
    if (!ftguid) {
        // either from template click or editor selection
        updatePasteTemplateValues();
        ftguid = findPasteFocusTemplate(paste_sel);

        focusTemplate(ftguid);
    } else {
        // called from focus template when either:
        // 1. template blot was clicked
        // 2. nav button clicked
        
	}
    
    createTemplateSelector(ftguid, paste_sel);
    updatePasteTemplateToolbarToFocus(ftguid, paste_sel);
    //updateTemplatesAfterTextChanged();
    //updateAllElements();
}

function updatePasteTemplateToolbarToFocus(ftguid, paste_sel) {
    let ft = null;
    if (ftguid) {
        ft = getTemplateDefByGuid(ftguid);
	} else {
        // occurs when no input req'd templates are in selection
        let sel_tl = getTemplateDefsInRange(paste_sel);
        if (sel_tl.length > 0) {
            ft = sel_tl[0];
		}
    }

    // UPDATE SELECTOR 

        // update option list
    let opt_elms = getPasteFocusTemplateOptionsElement().children;//.getElementsByClassName('paste-template-option-div');
    for (var i = 0; i < opt_elms.length; i++) {
        let opt_elm = opt_elms[i];
        if (opt_elm.getAttribute('templateGuid') == ftguid) {
            opt_elm.classList.add('selected-paste-option');
        } else {
            opt_elm.classList.remove('selected-paste-option');
        }
    }    
        // update selector item
    let sel_opt_div = document.getElementById('selectedPasteTemplateOptionDiv');
    applyTemplateToOptionDiv(sel_opt_div, ft);

    // UPDATE INPUT
    let pv_textarea_elm = getPasteValueTextAreaElement();
    
    if (ft && isTemplateAnInputType(ft)) {
        // TODO need have html/css for contact here
        // but just pass field name by text for now...
        pv_textarea_elm.readOnly = false;
        pv_textarea_elm.value = getTemplatePasteValue(ft);
        pv_textarea_elm.placeholder = `Enter paste text for [${ft.templateName}] here...`;
    } else if (ft) {
        pv_textarea_elm.readOnly = true;
        pv_textarea_elm.value = '';
        pv_textarea_elm.placeholder = getTemplatePasteValue(ft);
    } else {
        pv_textarea_elm.readOnly = true;
        pv_textarea_elm.value = '';
        pv_textarea_elm.placeholder = 'No templates to paste...';
	}

    // CHECK FOR READY
    updatePasteElementInteractivity();
}

function updatePasteTemplateValues(fromTimer = false) {
    let tl = getTemplateDefs();
    for (var i = 0; i < tl.length; i++) {
        let t = tl[i];
        if (fromTimer && TemplateBeforeEdit && TemplateBeforeEdit.templateGuid == t.templateGuid) {
            // don't auto-update template if is editing
            continue;
		}
        setTemplatePasteValue(t.templateGuid, getTemplatePasteValue(t));
    }
}

function updatePasteElementInteractivity() {
    let paste_t_defs = getPasteTemplateDefs();
    let can_navigate =  // when input in sel only nav to them otherwise allow if there's more than one non input...
        paste_t_defs.filter(x => isTemplateAnInputType(x)).length > 1 ||
        (paste_t_defs.filter(x => isTemplateAnInputType(x)).length == 0 && paste_t_defs.length > 1)
    let is_selector_enabled = paste_t_defs.length > 0;

    let can_clear =
        paste_t_defs.filter(x => isTemplateAnInputType(x) && !isNullOrEmpty(getTemplatePasteValue(x))).length > 0;

    let can_edit = paste_t_defs.length > 0;

    let can_paste =
        paste_t_defs.filter(x => !isTemplateReadyToPaste(x)).length == 0;

    let template_button_classes = 'disabled';
    if (getTemplateDefs().length == 0) {
        template_button_classes = 'hidden';
    }

    if (can_navigate) {
        getPasteGotoPrevButtonElement().classList.remove(template_button_classes);
        getPasteGotoNextButtonElement().classList.remove(template_button_classes);
    } else {
        getPasteGotoPrevButtonElement().classList.add(template_button_classes);
        getPasteGotoNextButtonElement().classList.add(template_button_classes);
    }

    if (is_selector_enabled) {
        getPasteFocusTemplateContainerElement().classList.remove(template_button_classes);
    } else {
        getPasteFocusTemplateContainerElement().classList.add(template_button_classes);
    }

    if (can_clear) {
        getPasteClearTextButtonElement().classList.remove(template_button_classes);
    } else {
        getPasteClearTextButtonElement().classList.add(template_button_classes);
    }

    if (can_edit) {
        getPasteEditFocusTemplateButtonElement().classList.remove(template_button_classes);
        getPasteValueTextAreaElement().classList.remove(template_button_classes);
    } else {
        getPasteEditFocusTemplateButtonElement().classList.add(template_button_classes);
        getPasteValueTextAreaElement().classList.add(template_button_classes);
	}

    if (can_paste) {
        getPasteButtonElement().classList.remove('disabled');
    } else {
        getPasteButtonElement().classList.add('disabled');
	}

    return can_paste;
}


// #endregion Actions

// #region Event Handlers

function onPasteTemplateTimer(e) {
    updatePasteTemplateValues(true);
}

function onTemplatePasteValueChanged(e) {
    let newTemplatePasteValue = getPasteValueTextAreaElement().value;
    let ftguid = getFocusTemplateGuid();
    if (!ftguid) {
        debugger;
        updatePasteTemplateToolbarToSelection();
        ftguid = getFocusTemplateGuid();
	}
    setTemplatePasteValue(ftguid, newTemplatePasteValue);
    updatePasteElementInteractivity();
}

function onPasteTemplateGotoNextClickOrKeyDown(e) {
    gotoNextTemplate();
}

function onPasteTemplateGotoPrevClickOrKeyDown(e) {
    gotoPrevTemplate();
}

function onPasteTemplateClearAllValuesClickOrKeyDown(e) {
    clearAllTemplateText();
}

function onPasteEditFocusTemplateClickOrKeyDown(e) {
    if (isShowingEditTemplateToolbar()) {
        hideEditTemplateToolbar();
    } else {
        showEditTemplateToolbar();
	}
}

// #endregion Event Handlers