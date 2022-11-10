// #region Globals

// #endregion Globals

// #region Life Cycle

function initPasteTemplateFocusSelector() {
    getPasteFocusTemplateOptionsElement().classList.add('hidden');

    getPasteFocusTemplateContainerElement().addEventListener('click', onTemplateSelectorClick, true);
}


function showPasteTemplateSelectorOptions() {
    if (isElementDisabled(getPasteFocusTemplateOptionsElement(), true)) {
        return;
    }
    if (isPasteTemplateHaveOptions()) {
        getPasteFocusTemplateOptionsElement().classList.remove('hidden');
        updatePasteTemplateOptionsBounds();
    }
    getPasteTemplateSelectorArrowElement().classList.add('active');
}

function hidePasteTemplateSelectorOptions() {
    getPasteFocusTemplateOptionsElement().classList.add('hidden');
    getPasteTemplateSelectorArrowElement().classList.remove('active');
}

// #endregion Life Cycle

// #region Getters

function getPasteFocusTemplateContainerElement() {
    return document.getElementById('pasteTemplateToolbarMenuSelectorDiv');
}

function getPasteFocusSelectedTemplateElement() {
    return document.getElementById('selectedPasteTemplateOptionDiv');
}

function getPasteTemplateSelectorArrowElement() {
    return document.getElementById("pasteTemplateToolbarMenuSelectorArrowDiv");
}

function getPasteTemplateSelectorHiddenSelectElement() {
    return document.getElementById('pasteTemplateToolbarMenuHiddenSelect');
}


function getPasteFocusTemplateOptionsElement() {
    return document.getElementById('pasteOptionsDiv');
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isPasteTemplateHaveOptions() {
    let opts_div = getPasteFocusTemplateOptionsElement();

    if (opts_div.children.length <= 1) {
        // when 1 item is available it'll be selected so don't show the one item
        return false;
    }
    return true;
}

// #endregion State

// #region Actions

function createTemplateSelector(ftguid, paste_sel) {
    // NOTE this clears all options
    // and sets selected div w / ftguid(or empty if null) to the given selection range

    // clear hidden selector options
    let sel_elm = getPasteTemplateSelectorHiddenSelectElement();
    sel_elm.innerHTML = '';

    // sel_opt_div is used to clone option elements (and is set to ftguid)
    let sel_opt_div = getPasteFocusSelectedTemplateElement();

    // clear current options
    let all_opts_div = getPasteFocusTemplateOptionsElement();
    all_opts_div.innerHTML = '';

    let tl = getTemplateDefsInRange(paste_sel);
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
    //if (!ftguid) {
        applyTemplateToOptionDiv(sel_opt_div, null);
    //}
    // NOTE adding close template selector at end so event signaled before show (if on selector) 
    document.addEventListener('click', onDocumenClickToClosePasteTemplateSelector);
}

function updatePasteTemplateOptionsBounds() {
    if (!isPasteTemplateHaveOptions()) {
        return;
	}
    let opts_div = getPasteFocusTemplateOptionsElement();
    let sel_div = getPasteFocusTemplateContainerElement();

    let opt_item_height = opts_div.children[0].getBoundingClientRect().height;
    let opts_actual_height = opt_item_height * opts_div.children.length;

    let t = getPasteFocusTemplateContainerElement().getBoundingClientRect().top - opts_actual_height;
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

    //   //let bottom = getPasteToolbarContainerElement().getBoundingClientRect().top;
    //   //let top =
    //   opts_div.style.left = fitted_opts_win_rect.left + 'px';
    //   opts_div.style.top = fitted_opts_win_rect.top + 'px';
    //   opts_div.style.minWidth = fitted_opts_win_rect.width + 'px';
    //opts_div.style.height = fitted_opts_win_rect.height + 'px';



}

function applyTemplateToOptionDiv(opt_div, t) {
    let icon_elm = document.createElement('DIV');
    let label_elm = document.createElement('SPAN');
    opt_div.replaceChildren(icon_elm, label_elm);


    let icon_color = t ? t.templateColor : 'black';
    icon_elm.classList.add('paste-template-option-icon');
    icon_elm.style.backgroundColor = icon_color;

    let icon_svg_elm = document.createElement('SVG');
    icon_elm.appendChild(icon_svg_elm);
    let icon_svg_key = t ? getTemplateTypeSvgKey(t.templateType) : 'empty';
    icon_svg_elm.outerHTML = getSvgHtml(icon_svg_key,'');
    //setSvgElmColor(icon_svg_elm, icon_color);

    let label_text = t ? t.templateName : '';
    label_elm.classList.add('paste-template-option-label');
    label_elm.innerText = label_text;

    if (isTemplateAnInputType(t)) {
        opt_div.classList.remove('no-input-template');
    } else {
        opt_div.classList.add('no-input-template');
    }
}

function toggleShowPasteTemplateSelectorOptions() {
    let opts_div = getPasteFocusTemplateOptionsElement();
    if (opts_div.classList.contains('hidden')) {
        showPasteTemplateSelectorOptions();
    } else {
        hidePasteTemplateSelectorOptions();
	}
}
// #endregion Actions

// #region Event Handlers

function onDocumenClickToClosePasteTemplateSelector(e) {
    if (isChildOfElement(e.target, getPasteFocusTemplateContainerElement())) {
        return;
    }
    hidePasteTemplateSelectorOptions();
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
    toggleShowPasteTemplateSelectorOptions()
}
// #endregion Event Handlers