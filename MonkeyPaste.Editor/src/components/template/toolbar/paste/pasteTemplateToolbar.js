
// #region Globals

var IsPastingTemplate = false;
var IsTemplatePasteValueTextAreaFocused = false;
var IsPasteToolbarLoading = false;
// #endregion Globals

// #region Life Cycle

function initPasteTemplateToolbar() {
    //let resizers = Array.from(document.getElementsByClassName('resizable-textarea'));
    //for (var i = 0; i < resizers.length; i++) {
    //    let rta = resizers[i];

    //    new ResizeObserver(() => {
    //        updatePasteTemplateToolbarPosition();
    //    }).observe(rta);
    //}

    enableResize(getPasteTemplateToolbarContainerElement());

    document.getElementById('nextTemplateButton').addEventListener('click', function (e) {
        gotoNextTemplate();
    });
    document.getElementById('nextTemplateButton').addEventListener('keydown', function (e) {
        if (!isKeyboardButtonClick(e)) {
            return;
        }
        gotoNextTemplate();
    });
    document.getElementById('previousTemplateButton').addEventListener('click', function (e) {
        gotoPrevTemplate();
    });
    document.getElementById('previousTemplateButton').addEventListener('keydown', function (e) {
        if (!isKeyboardButtonClick(e)) {
            return;
        }
        gotoPrevTemplate();
    });
    document.getElementById('clearAllTemplateTextButton').addEventListener('click', function (e) {
        
        clearAllTemplateText();
    });
    document.getElementById('clearAllTemplateTextButton').addEventListener('keydown', function (e) {
        if (!isKeyboardButtonClick(e)) {
            return;
        }
        clearAllTemplateText();
    });
    document.getElementById('pasteTemplateButton').addEventListener('click', function (e) {
        if (!isPasteButtonEnabled()) {
            return;
        }
        alert(getText(getEditorSelection(true), true));

        onPasteTemplateRequest_ntf();
	});
    document.getElementById('pasteTemplateButton').addEventListener('keydown', function (e) {
        if (!isPasteButtonEnabled()) {
            return;
        }

        if (isKeyboardButtonClick(e)) {
            onPasteTemplateRequest_ntf();
        }
	});

    document.getElementById('templatePasteValueTextArea').addEventListener('input', onTemplatePasteValueChanged);
    initBouncyTextArea('templatePasteValueTextArea');

    document.getElementById('pasteOptionsDiv').classList.add('hidden');
}

function showPasteTemplateToolbar() {
    IsPasteToolbarLoading = true;
    var ptt = getPasteTemplateToolbarContainerElement();
    ptt.classList.remove('hidden');

    updatePasteTemplateToolbarToSelection();

    document.getElementById('templatePasteValueTextArea').focus();
    IsPasteToolbarLoading = false;
}

function createTemplateSelector(ftguid, paste_sel) {
    let sel_div = document.getElementById("pasteTemplateToolbarMenuSelectorDiv");
    let sel_elm = document.getElementById('pasteTemplateToolbarMenuSelector');
    let sel_opt_div = document.getElementById('selectedPasteTemplateOptionDiv');
    let all_opts_div = document.getElementById('pasteOptionsDiv');

    sel_elm.innerHTML = '';
    all_opts_div.innerHTML = '';

    sel_div.addEventListener('click', onTemplateSelectorClick, true);

    let tl = getTemplateDefsInRange(paste_sel); //getTemplateDefs();
    //let ftguid = getFocusTemplateGuid();

    for (var i = 0; i < tl.length; i++) {
        let t = tl[i];
        let option_value = t.templateGuid;
        let option_onChange = `focusTemplate('${t.templateGuid}');`;
        let t_option_str = `<option class="templateOption" value="${option_value}" onchange="${option_onChange}">${t.templateName}</option>`;
        sel_elm.innerHTML += t_option_str;

        let cur_option_div = sel_opt_div.cloneNode(true);
        all_opts_div.appendChild(cur_option_div);
        cur_option_div.removeAttribute('id');

        applyTemplateToOptionDiv(cur_option_div, t);
        cur_option_div.addEventListener('click', onTemplateOptionClick);
        if (t.templateGuid == ftguid) {
            cur_option_div.classList.add('selected-paste-option');
            applyTemplateToOptionDiv(sel_opt_div, t);
        }

        cur_option_div.setAttribute('templateGuid', t.templateGuid);
    }
    if (!ftguid) {
        applyTemplateToOptionDiv(sel_opt_div, null);
    }
    // NOTE adding close template selector at end so event signaled before show (if on selector) 
    document.addEventListener('click', onDocumenClickToClosePasteTemplateSelector);
}

function hidePasteTemplateToolbar() {
    var ptt = getPasteTemplateToolbarContainerElement();
    ptt.classList.add('hidden');
}
// #endregion Life Cycle

// #region Getters

function getPasteTemplateToolbarContainerElement() {
    return document.getElementById('pasteTemplateToolbar');
}

function getTemplatePasteValue(t) {
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

// #endregion Getters

// #region Setters

function setTemplatePasteValue(tguid, val) {
    let t = getTemplateDefByGuid(tguid);
    if (!t) {
        debugger;
	}
    t.templateText = val;
    var telms = getTemplateElements(tguid);
    for (var i = 0; i < telms.length; i++) {
        var telm = telms[i];
        applyTemplateToDomNode(telm, t);
    }
    updateTemplatesAfterTextChanged();
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
    return !document.getElementById('pasteTemplateButton').classList.contains('disabled');
}

function isShowingPasteTemplateToolbar() {
    return !getPasteTemplateToolbarContainerElement().classList.contains('hidden');
}
// #endregion State

// #region Actions

function findPasteFocusTemplate(sel) {
    let tl = getTemplateDefsInRange(sel);
    let ftguid = getFocusTemplateGuid();
    if (tl.find(x => x.templateGuid == ftguid) == null) {
        // last focus template not in selection so ignore last focus
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

function gotoNextTemplate(ignoreNonInputTemplates = true, force_tguid = null) {
    let ftguid = force_tguid ? force_tguid : getFocusTemplateGuid();
    var tl = getTemplateDefs();
    var curIdx = 0;
    for (var i = 0; i < tl.length; i++) {
        if (tl[i].templateGuid == ftguid) {
            curIdx = i;
            break;
        }
    }
    var nextIdx = curIdx + 1;
    if (nextIdx >= tl.length) {
        nextIdx = 0;
    }
    let t = tl[nextIdx];

    if (ignoreNonInputTemplates && !isTemplateAnInputType(t)) {
        gotoNextTemplate(true, t.templateGuid);
        return;
    }

    focusTemplate(t.templateGuid);
}

function gotoPrevTemplate(ignoreNonInputTemplates = true, force_tguid = null) {
    let ftguid = force_tguid ? force_tguid : getFocusTemplateGuid();
    var tl = getTemplateDefs();
    var curIdx = 0;
    for (var i = 0; i < tl.length; i++) {
        if (tl[i].templateGuid == ftguid) {
            curIdx = i;
            break;
        }
    }
    var prevIdx = curIdx - 1;
    if (prevIdx < 0) {
        prevIdx = tl.length - 1;
    }
    let t = tl[prevIdx];
    if (ignoreNonInputTemplates && !isTemplateAnInputType(t)) {
        gotoPrevTemplate(true, t.templateGuid);
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

function applyTemplateToOptionDiv(opt_div, t) {

    if (opt_div.id == 'selectedPasteTemplateOptionDiv') {
        if (t) {
            opt_div.classList.remove('hidden');
        } else {
            opt_div.classList.add('hidden');
            return;
        }
    } else {
        if (!t) {
            debugger;
		}
    }

    opt_div.removeChild(opt_div.children[0]);
    opt_div.removeChild(opt_div.children[0]);

    let icon_i = document.createElement('I');
    icon_i.style.color = t.templateColor;

    icon_i.classList.add('paste-template-option-icon');
    icon_i.classList.add('fa-solid');
    icon_i.classList.add(getTemplateTypeIcon(t.templateType));
    opt_div.appendChild(icon_i);

    let label_span = document.createElement('SPAN');
    label_span.innerText = t.templateName;
    label_span.classList.add('paste-template-option-label');
   
    opt_div.appendChild(label_span);
    if (isTemplateAnInputType(t)) {
        opt_div.classList.remove('no-input-template');
    } else {
        opt_div.classList.add('no-input-template');
    }
}

function updatePasteTemplateToolbarToSelection(force_ftguid) {
    let paste_sel = getEditorSelection(true);
    let pre_ftguid = getFocusTemplateGuid();

    let ftguid = force_ftguid;
    if (!ftguid) {
        // either from template click or editor selection
        updatePasteTemplateValues();
        ftguid = findPasteFocusTemplate(paste_sel);

        if (pre_ftguid != ftguid) {
            // terminates a circular ref
            focusTemplate(ftguid);
        }
    } else {
        // called from focus template when either:
        // 1. template blot was clicked
        // 2. nav button clicked
        
	}
    
    createTemplateSelector(ftguid, paste_sel);
    updatePasteTemplateToolbarToFocus(ftguid, paste_sel);
    //updateTemplatesAfterTextChanged();
    updateAllSizeAndPositions();
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
    if (ft) {
        // reset disabled stuff...

        document.getElementById('pasteTemplateToolbarMenuSelectorDiv').classList.remove('disabled');
        document.getElementById('templatePasteValueTextArea').classList.remove('disabled');
        document.getElementById('clearAllTemplateTextButton').classList.remove('disabled');
        document.getElementById('previousTemplateButton').classList.remove('disabled');
        document.getElementById('nextTemplateButton').classList.remove('disabled');
	} else {
        // occurs when no templates at all are in selection
        // TODO disable everything but paste button
        document.getElementById('pasteTemplateToolbarMenuSelectorDiv').classList.add('disabled');
        document.getElementById('templatePasteValueTextArea').classList.add('disabled');
        document.getElementById('templatePasteValueTextArea').readOnly = true;

        document.getElementById('clearAllTemplateTextButton').classList.add('disabled');
        document.getElementById('previousTemplateButton').classList.add('disabled');
        document.getElementById('nextTemplateButton').classList.add('disabled');

        document.getElementById('pasteTemplateButton').classList.remove('disabled');
        return;
	}

    // UPDATE SELECTOR 

        // update option list
    let opt_elms = document.getElementById('pasteOptionsDiv').getElementsByClassName('paste-template-option-div');
    for (var i = 0; i < opt_elms.length; i++) {
        let opt_elm = opt_elms[i];
        if (opt_elm.getAttribute('templateGuid') == ft.templateGuid) {
            opt_elm.classList.add('selected-paste-option');
        } else {
            opt_elm.classList.remove('selected-paste-option');
        }
    }    
        // update selector item
    let sel_opt_div = document.getElementById('selectedPasteTemplateOptionDiv');
    applyTemplateToOptionDiv(sel_opt_div, ft);

    // UPDATE INPUT
    let pv_textarea_elm = document.getElementById('templatePasteValueTextArea');
    pv_textarea_elm.value = getTemplatePasteValue(ft);
    if (isTemplateAnInputType(ft)) {
        // TODO need have html/css for contact here
        // but just pass field name by text for now...
        pv_textarea_elm.readOnly = false;
        pv_textarea_elm.placeholder = `Enter paste text for [${ft.templateName}] here...`;
    } else {
        pv_textarea_elm.readOnly = true;
    }

    // CHECK FOR READY
    checkForReadyToPaste();
}

function updatePasteTemplateValues() {
    let tl = getTemplateDefs();
    for (var i = 0; i < tl.length; i++) {
        let t = tl[i];
        setTemplatePasteValue(t.templateGuid, getTemplatePasteValue(t));
    }
}

function checkForReadyToPaste() {
    let isReadyToPaste = true;
    let tl = getTemplateDefs();
    for (var i = 0; i < tl.length; i++) {
        let t = tl[i];
        if (isTemplateReadyToPaste(t)) {
            continue;
        }
        isReadyToPaste = false;
    }
    if (isReadyToPaste) {
        document.getElementById('pasteTemplateButton').classList.remove('disabled');
    } else {
        document.getElementById('pasteTemplateButton').classList.add('disabled');
	}

    return isReadyToPaste;
}

// #endregion Actions

// #region Event Handlers

function onDocumenClickToClosePasteTemplateSelector(e) {
    if (isChildOfElement(e.target, document.getElementById("pasteTemplateToolbarMenuSelectorDiv"))) {
        return;
    }
    document.getElementById('pasteOptionsDiv').classList.add('hidden');
    document.getElementById('pasteTemplateToolbarMenuSelectorArrowDiv').classList.remove('active');
}

function onTemplateOptionClick(e) {
    if (e.currentTarget.classList.contains('no-input-template')) {
        return;
    }

    let clicked_templateGuid = e.currentTarget.getAttribute('templateGuid');
    focusTemplate(clicked_templateGuid);
    log('templateGuid: ' + clicked_templateGuid);
}

function onTemplateSelectorClick(e) {
    let opts_div = document.getElementById('pasteOptionsDiv');
    opts_div.classList.toggle('hidden');
    if (opts_div.classList.contains('hidden')) {
        return;
    }
    if (opts_div.children.length == 0) {
        return;
	}
    let sel_div = document.getElementById("pasteTemplateToolbarMenuSelectorDiv");

    let opt_item_height = opts_div.children[0].getBoundingClientRect().height;
    let opts_actual_height = opt_item_height * opts_div.children.length;

    let t = getPasteTemplateToolbarContainerElement().getBoundingClientRect().top - opts_actual_height;
    let w = sel_div.getBoundingClientRect().width;
    opts_div.style.left = sel_div.getBoundingClientRect().left + 'px';
    opts_div.style.top = t + 'px';
    opts_div.style.width = w + 'px';
    //t = Math.max()


 //   let opts_div_rect = cleanRect(opts_div.getBoundingClientRect());
 //   let sel_div_rect = cleanRect(sel_div.getBoundingClientRect());
 //   if (opts_div_rect.height == 0) {
 //       debugger;
	//}
 //   let max_opts_height_ratio = 0.5;
 //   let max_opts_height = getWindowRect().height * max_opts_height_ratio;
 //   let min_opts_top = getWindowRect().height - max_opts_height;

 //   let x_pad = 0;
 //   let y_pad = 0;

 //   let fitted_opts_win_rect = cleanRect();
 //   fitted_opts_win_rect.left = sel_div_rect.left + x_pad;
 //   fitted_opts_win_rect.right = sel_div_rect.right - x_pad;
 //   fitted_opts_win_rect.bottom = sel_div_rect.top - y_pad;
 //   fitted_opts_win_rect.top = Math.max(min_opts_top, fitted_opts_win_rect.bottom - opts_div_rect.height - y_pad);
 //   fitted_opts_win_rect = cleanRect(fitted_opts_win_rect);

 //   //let bottom = getPasteTemplateToolbarContainerElement().getBoundingClientRect().top;
 //   //let top = 
 //   opts_div.style.left = fitted_opts_win_rect.left + 'px';
 //   opts_div.style.top = fitted_opts_win_rect.top + 'px';
 //   opts_div.style.minWidth = fitted_opts_win_rect.width + 'px';
    //opts_div.style.height = fitted_opts_win_rect.height + 'px';

    document.getElementById('pasteTemplateToolbarMenuSelectorArrowDiv').classList.add('active');
}

function onTemplatePasteValueChanged(e) {
    let newTemplatePasteValue = document.getElementById('templatePasteValueTextArea').value;
    let ftguid = getFocusTemplateGuid();
    if (!ftguid) {
        debugger;
        updatePasteTemplateToolbarToSelection();
        ftguid = getFocusTemplateGuid();
	}
    setTemplatePasteValue(ftguid, newTemplatePasteValue);
    checkForReadyToPaste();
}
// #endregion Event Handlers