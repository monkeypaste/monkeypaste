// #region Globals

// #endregion Globals

// #region Life Cycle

function initPasteTemplateFocusSelector() {
    getPasteTemplateSelectorOptionsElement().classList.add('hidden');

    getPasteFocusTemplateContainerElement().addEventListener('click', onTemplateSelectorClick, true);
}


function showPasteTemplateSelectorOptions() {
    if (isElementDisabled(getPasteTemplateSelectorOptionsElement(), true)) {
        return;
    }
    if (isPasteTemplateHaveOptions()) {
        getPasteTemplateSelectorOptionsElement().classList.remove('hidden');
        updatePasteTemplateOptionsBounds();

        getPasteTemplateSelectorArrowElement().classList.add('active');
        getPasteTemplateSelectorArrowElement().classList.remove('hidden');
        
    } 
}

function hidePasteTemplateSelectorOptions() {
    getPasteTemplateSelectorOptionsElement().classList.add('hidden');
    getPasteTemplateSelectorArrowElement().classList.remove('active');
}

// #endregion Life Cycle

// #region Getters

function getPasteFocusTemplateContainerElement() {
    return document.getElementById('pasteTemplateToolbarMenuSelectorDiv');
}

function getPasteTemplateSelectorHeaderElement() {
    return document.getElementById('selectedPasteTemplateOptionDiv');
}

function getPasteTemplateSelectorArrowElement() {
    return document.getElementById("pasteTemplateToolbarMenuSelectorArrowDiv");
}


function getPasteTemplateSelectorOptionsElement() {
    return document.getElementById('pasteOptionsDiv');
}

function getSelectedPasteOptionDiv() {
    return document.getElementById('selectedPasteTemplateOptionDiv');
}

function getSelectedOptionTemplateGuid() {
    const opt_elm = getSelectedPasteOptionDiv();
    if (!opt_elm) {
        return null;
    }

    return opt_elm.getAttribute('templateGuid');
}

// #endregion Getters

// #region Setters

function setPasteTemplateSelectorTemplate(tguid) {
    let cur_sel_tguid = getSelectedOptionTemplateGuid();
    if (cur_sel_tguid) {
        if (cur_sel_tguid == tguid) {
            // nothing to do
            return;
        }
        // unset current

    }
}
// #endregion Setters

// #region State

function isPasteTemplateHaveOptions() {
    let opts_div = getPasteTemplateSelectorOptionsElement();

    if (opts_div.children.length <= 1) {
        // when 1 item is available it'll be selected so don't show the one item
        return false;
    }
    return true;
}

// #endregion State

// #region Actions

function clearAllTemplateElementsSelectorFlag() {
    let telms = getTemplateElements();
    for (var i = 0; i < telms.length; i++) {
        let telm = telms[i];
        telm.classList.remove('')
	}
}

function createTemplateSelector(ftguid, paste_sel) {
    // NOTE this clears all options
    // and sets selected div w / ftguid(or empty if null) to the given selection range
    let new_item_elms = [];
    let tl = getTemplateDefs();
    let stguid = ftguid ? ftguid : tl.length > 0 ? tl[0].templateGuid : null;
    for (var i = 0; i < tl.length; i++) {
        let item_elm = createTemplateSelectorItem(tl[i], paste_sel, stguid, onTemplateOptionClick);
        new_item_elms.push(item_elm);
    }
    getPasteTemplateSelectorOptionsElement().replaceChildren(...new_item_elms);

    let new_header_elm = createTemplateSelectorItem(getTemplateDefByGuid(stguid), paste_sel, onDocumenClickToClosePasteTemplateSelector);
    let old_header_elm = getPasteTemplateSelectorHeaderElement();
    let header_parent_elm = old_header_elm.parentNode;
    header_parent_elm.replaceChild(new_header_elm, old_header_elm);
    new_header_elm.id = old_header_elm.id;
}

function createTemplateSelectorItem(t, sel, ftguid, onClick) {
    let is_disabled = true;
    let is_unavailable = false;
    let is_selected = false;
    if (t) {
        is_disabled = !isTemplateAnInputType(t);
        is_unavailable =
            getAllTemplateDocIdxs(t.templateGuid).every(x => !isDocIdxInRange(x, sel));
        is_selected = t.templateGuid == ftguid;
    }

    let icon_color = t ? t.templateColor : 'black';
    let icon_svg_key = t ? getTemplateTypeSvgKey(t.templateType) : 'empty';
    let label_text = t ? t.templateName : '';
    let tguid = t ? t.templateGuid : '';

    let item_elm = document.createElement('DIV');
    let icon_elm = document.createElement('DIV');
    let label_elm = document.createElement('SPAN');

    // ICON SVG
    let icon_svg_elm = createSvgElement(icon_svg_key, 'svg-icon paste-toolbar-icon');
    icon_elm.appendChild(icon_svg_elm);

    // ICON CONTAINER
    icon_elm.classList.add('paste-template-option-icon');
    icon_elm.style.backgroundColor = icon_color;

    // LABEL
    label_elm.classList.add('paste-template-option-label');
    label_elm.innerText = label_text;


    // CONTAINER
    item_elm.appendChild(icon_elm);
    item_elm.appendChild(label_elm);
    if (is_disabled && globals.IS_SMART_TEMPLATE_NAV_ENABLED) {
        // NOTE only show 'disabled' if using the smart (ignore static nav is enabled)
        item_elm.classList.add('disabled');
    }
    if (is_unavailable) {
        item_elm.classList.add('unavailable-text');
    }
    if (is_selected) {
        item_elm.classList.add('selected-paste-option');
    }
    item_elm.classList.add('paste-template-option-div');
    item_elm.setAttribute('templateGuid', tguid);
    item_elm.addEventListener('click', onClick);

    return item_elm;
}

function applyTemplateToOptionDiv(opt_div, t) {
    let icon_color = t ? t.templateColor : 'black';
    let icon_svg_key = t ? getTemplateTypeSvgKey(t.templateType) : 'empty';
    let label_text = t ? t.templateName : '';

    let icon_elm = document.createElement('DIV');
    let label_elm = document.createElement('SPAN');
    opt_div.replaceChildren(icon_elm, label_elm);

    icon_elm.classList.add('paste-template-option-icon');
    icon_elm.style.backgroundColor = icon_color;

    let icon_svg_elm = document.createElement('SVG');
    icon_elm.appendChild(icon_svg_elm);
    icon_svg_elm.outerHTML = getSvgHtml(icon_svg_key, '');
    //setSvgElmColor(icon_svg_elm, icon_color);

    label_elm.classList.add('paste-template-option-label');
    label_elm.innerText = label_text;

    if (isTemplateAnInputType(t)) {
        opt_div.classList.remove('no-input-template');
    } else {
        opt_div.classList.add('no-input-template');
    }
}

function updatePasteTemplateOptionsBounds() {
    if (!isPasteTemplateHaveOptions()) {
        return;
	}
    let opts_div = getPasteTemplateSelectorOptionsElement();
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



function toggleShowPasteTemplateSelectorOptions() {
    let opts_div = getPasteTemplateSelectorOptionsElement();
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
    //if (e.currentTarget.classList.contains('no-input-template')) {
    //    return;
    //}

    let clicked_templateGuid = e.currentTarget.getAttribute('templateGuid');
    focusTemplate(clicked_templateGuid);
    hideAllPopups();
    log('templateGuid: ' + clicked_templateGuid);
}

function onTemplateSelectorClick(e) {
    if (isElementDisabled(getPasteFocusTemplateContainerElement())) {
        return;
    }
    toggleShowPasteTemplateSelectorOptions()
}
// #endregion Event Handlers