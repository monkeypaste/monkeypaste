
// #region Globals

var IsPastingTemplate = false;
var IsTemplatePasteValueTextAreaFocused = false;

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
}

function showPasteTemplateToolbar() {
    var ptt = getPasteTemplateToolbarContainerElement();
    ptt.classList.remove('hidden');

    let tl = getTemplateDefs();
    let ftguid = getFocusTemplateGuid();
    for (var i = 0; i < tl.length; i++) {
        let t = tl[i];
        if (isTemplateAnInputType(t)) {
            t.templateText = '';
            if (!ftguid) {
                ftguid = t.templateGuid;
			}
        } else {
            setTemplatePasteValue(t.templateGuid, getTemplatePasteValue(t));
		}
	}
    
    if (ftguid) {
        let ft = getTemplateDefByGuid(ftguid);
        if (!isTemplateAnInputType(ft)) {
            // if focused template doesn't require input
            // find first (in doc order ignoring focus) that does
            // (should have one or show wouldn't be called)
            ftguid = null;
            let tl = getTemplateDefs();
            for (var i = 0; i < tl.length; i++) {
                if (isTemplateAnInputType(tl[i])) {
                    ftguid = tl[i];
                    break;
                }
            }
            if (!ftguid) {
                // whys it think theres an input one?
                debugger;
            }
        }
    }
    if (!ftguid) {
        debugger;
        return;
    }
    focusTemplate(ftguid);
    createTemplateSelector();
    updatePasteTemplateToolbarToFocus();
    updateTemplatesAfterTextChanged();
    updateAllSizeAndPositions();

    document.getElementById('templatePasteValueTextArea').focus();
}

function createTemplateSelector() {
    let sel_div = document.getElementById("pasteTemplateToolbarMenuSelectorDiv");
    let sel_elm = document.getElementById('pasteTemplateToolbarMenuSelector');
    let sel_opt_div = document.getElementById('selectedPasteTemplateOptionDiv');
    let all_opts_div = document.getElementById('pasteOptionsDiv');

    sel_elm.innerHTML = '';
    all_opts_div.innerHTML = '';

    sel_div.addEventListener('click', onTemplateSelectorClick);

    let tl = getTemplateDefs();
    let ftguid = getFocusTemplateGuid();

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
        applyTemplateToDomNode(sel_opt_div, null);
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
    return !document.getElementById('pasteTemplateButton').classList.contains('disabled-button');
}

function isShowingPasteTemplateToolbar() {
    return !getPasteTemplateToolbarContainerElement().classList.contains('hidden');
}
// #endregion State

// #region Actions

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
    if (!t) {
        // when t == null (none selected? show as empty)
        t.templateColor = 'transparent';
        t.templateName = '';
    }
    let default_icon_class_str = 'paste-template-option-icon';
    //opt_div.children[0].style.backgroundColor = t.templateColor;
    let icon_class_str = default_icon_class_str + ' ' + getTemplateTypeIcon(t.templateType);
    opt_div.children[0].setAttribute('class', icon_class_str);
    opt_div.children[0].style.color = t.templateColor;

    opt_div.children[1].innerText = t.templateName;
    if (!isTemplateAnInputType(t)) {
        opt_div.classList.add('no-input-template');
    }
}

function updatePasteTemplateToolbarToFocus() {
    let ftguid = getFocusTemplateGuid();
    let ft = getTemplateDefByGuid(ftguid);

    // UPDATE SELECTOR 

        // update option list
    let opt_elms = document.getElementById('pasteOptionsDiv').getElementsByClassName('paste-template-option-div');
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
        document.getElementById('pasteTemplateButton').classList.remove('disabled-button');
    } else {
        document.getElementById('pasteTemplateButton').classList.add('disabled-button');
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
    let clicked_templateGuid = e.currentTarget.getAttribute('templateGuid');
    focusTemplate(clicked_templateGuid);
    log('templateGuid: ' + clicked_templateGuid);
}

function onTemplateSelectorClick(e) {
    document.getElementById('pasteTemplateToolbarMenuSelectorArrowDiv').classList.add('active');

    let opts_div = document.getElementById('pasteOptionsDiv');
    opts_div.classList.remove('hidden');

    let sel_div = document.getElementById("pasteTemplateToolbarMenuSelectorDiv");

    let opts_div_rect = cleanRect(opts_div.getBoundingClientRect());
    let sel_div_rect = cleanRect(sel_div.getBoundingClientRect());

    let max_opts_height_ratio = 0.75;
    let max_opts_height = getWindowRect().height * max_opts_height_ratio;
    let min_opts_top = getWindowRect().height - max_opts_height;

    let x_pad = 5;
    let y_pad = 5;

    let fitted_opts_win_rect = cleanRect();
    fitted_opts_win_rect.left = sel_div_rect.left + x_pad;
    fitted_opts_win_rect.right = sel_div_rect.right - x_pad;
    fitted_opts_win_rect.bottom = sel_div_rect.top - y_pad;
    fitted_opts_win_rect.top = Math.max(min_opts_top, fitted_opts_win_rect.bottom - opts_div_rect.height - y_pad);
    fitted_opts_win_rect = cleanRect(fitted_opts_win_rect);

    opts_div.style.left = fitted_opts_win_rect.left + 'px';
    opts_div.style.top = fitted_opts_win_rect.top + 'px';
    opts_div.style.width = fitted_opts_win_rect.width + 'px';
    opts_div.style.height = fitted_opts_win_rect.height + 'px';
}

function onTemplatePasteValueChanged(e) {
    let newTemplatePasteValue = document.getElementById('templatePasteValueTextArea').value;
    setTemplatePasteValue(getFocusTemplateGuid(), newTemplatePasteValue);
    checkForReadyToPaste();
}
// #endregion Event Handlers