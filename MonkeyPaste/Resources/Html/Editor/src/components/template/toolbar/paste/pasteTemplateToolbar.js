
var IsPastingTemplate = false;
var IsTemplatePasteValueTextAreaFocused = false;

function initPasteTemplateToolbar() {
    let resizers = Array.from(document.getElementsByClassName('resizable-textarea'));
    for (var i = 0; i < resizers.length; i++) {
        let rta = resizers[i];

        new ResizeObserver(() => {
            updatePasteTemplateToolbarPosition();
        }).observe(rta);
    }

    enableResize(getPasteTemplateToolbarContainerElement());

    document.getElementById('nextTemplateButton').addEventListener('click', function (e) {
        gotoNextTemplate();
    });
    document.getElementById('nextTemplateButton').addEventListener('keydown', function (e) {
        gotoNextTemplate();
    });
    document.getElementById('previousTemplateButton').addEventListener('click', function (e) {
        gotoPrevTemplate();
    });
    document.getElementById('previousTemplateButton').addEventListener('keydown', function (e) {
        gotoPrevTemplate();
    });
    document.getElementById('clearAllTemplateTextButton').addEventListener('click', function (e) {
        clearAllTemplateText();
    });
    document.getElementById('clearAllTemplateTextButton').addEventListener('keydown', function (e) {
        clearAllTemplateText();
    });
    document.getElementById('pasteTemplateButton').addEventListener('click', function (e) {
        isCompleted = true;
    });
    document.getElementById('pasteTemplateButton').addEventListener('keydown', function (e) {
        isCompleted = true;
    });

    document.getElementById('templatePasteValueTextArea').addEventListener('input', onTemplatePasteValueChanged);
    initBouncyTextArea('templatePasteValueTextArea');
}

function showPasteTemplateToolbar() {
    var ptt = getPasteTemplateToolbarContainerElement();
    ptt.classList.remove('hidden');

    let ftguid = getFocusTemplateGuid();
    if (!ftguid) {
        let tl = getUsedTemplateDefinitions();
        if (tl && tl.length > 0) {
            ftguid = tl[0].templateGuid;
		}        
    }
    if (!ftguid) {
        debugger;
        return;
    }
    focusTemplate(ftguid);
    createTemplateSelector();
    updateAllSizeAndPositions();

    document.getElementById('templatePasteValueTextArea').focus();
}

function isShowingPasteTemplateToolbar() {
    return !getPasteTemplateToolbarContainerElement().classList.contains('hidden');
}

function hidePasteTemplateToolbar() {
    var ptt = getPasteTemplateToolbarContainerElement();
    ptt.classList.add('hidden');
}

function updatePasteTemplateToolbarPosition() {
    return;
    let wh = window.visualViewport.height;
    let ptth = $("#pasteTemplateToolbar").outerHeight();
    $("#pasteTemplateToolbar").css("top", wh - ptth);
}

function gotoNextTemplate() {
    let ftguid = getFocusTemplateGuid();
    var tl = getUsedTemplateDefinitions();
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
    focusTemplate(tl[nextIdx].templateGuid);
}

function gotoPrevTemplate() {
    let ftguid = getFocusTemplateGuid();
    var tl = getUsedTemplateDefinitions();
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
    focusTemplate(tl[prevIdx].templateGuid);
}

function clearAllTemplateText() {
    var tl = getUsedTemplateDefinitions();
    for (var i = 0; i < tl.length; i++) {
        setTemplateText(tl[i].templateGuid, '');
    }
}

function setTemplateText(tguid, text) {
    //var stl = document.getElementsByClassName("ql-template-embed-blot");
    var telms = getTemplateElements(tguid);
    for (var i = 0; i < telms.length; i++) {
        var telm = telms[i];
        telm.innerText = text;
        telm.templateText = text;
    }
}

function createTemplateSelector() {
    let sel_div = document.getElementById("pasteTemplateToolbarMenuSelectorDiv");
    let sel_elm = document.getElementById('pasteTemplateToolbarMenuSelector');
    let sel_opt_div = document.getElementById('selectedPasteTemplateOptionDiv');
    let all_opts_div = document.getElementById('pasteOptionsDiv');

    sel_elm.innerHTML = '';
    all_opts_div.innerHTML = '';

    sel_div.addEventListener('click', onTemplateSelectorClick);

   
    let tl = getUsedTemplateDefinitions();
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
}

function applyTemplateToOptionDiv(opt_div, t) {
    if (!t) {
        // when t == null (none selected? show as empty)
        t.templateColor = 'transparent';
        t.templateName = '';
	}
    opt_div.children[0].style.backgroundColor = t.templateColor;
    opt_div.children[1].innerText = t.templateName;
}

function updatePasteTemplateSelectorToFocus() {
    let ftguid = getFocusTemplateGuid();
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

    let ft = getTemplateDefByGuid(ftguid);
    // update selector item
    let sel_opt_div = document.getElementById('selectedPasteTemplateOptionDiv');
    applyTemplateToOptionDiv(sel_opt_div, ft);
}

function setTemplatePasteValue(tguid, val) {
    let t = getTemplateDefByGuid(tguid);
    t.templateText = val;
    var telms = getTemplateElements(tguid);
    for (var i = 0; i < telms.length; i++) {
        var telm = telms[i];
        applyTemplateToDomNode(telm, t);
    }
}
function getPasteTemplateToolbarContainerElement() {
    return document.getElementById('pasteTemplateToolbar');
}
