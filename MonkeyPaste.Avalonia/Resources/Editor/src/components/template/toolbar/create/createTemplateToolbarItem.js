// #region Globals

// #endregion Globals

// #region Life Cycle

function initTemplateToolbarButton() {
    addClickOrKeyClickEventListener(getCreateTemplateToolbarButton(), onTemplateToolbarButtonClick);
    //getCreateTemplateToolbarButton().innerHTML = getSvgHtml('createtemplate');
}


// #endregion Life Cycle

// #region Getters

function getCreateTemplateToolbarButton() {
    return document.getElementById('createTemplateToolbarButton');
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isShowingCreateTemplateToolbarMenu() {
    return superCm.isOpen();
}

function isCreateTemplateValid() {
    let sel = getDocSelection();
    if (!sel) {
        return false;
    }
    if (getTemplateAtDocIdx(sel.index) != null ||
        getTemplateAtDocIdx(sel.index + sel.length) != null) {
        return false;
    }
    return true;
}
// #endregion State

// #region Actions

function createTemplateFromDropDown(templateObjOrId, newTemplateType, allDefs) {
    let templateObj;
    if (templateObjOrId != null && typeof templateObjOrId === 'string') {
        templateObj = getTemplateDefByGuid(templateObjOrId);
    } else {
        templateObj = templateObjOrId;
    }

    let range = globals.quill.getSelection(true);

    let isNew = templateObj == null;
    let newTemplateObj = templateObj;

    if (isNew) {
        //grab the selection head's html to set formatting of template div
        let sel_html_format = '';
        let sel_head_html_str = getHtml({ index: range.index, length: 1 });
        if (sel_head_html_str != null && sel_head_html_str.length > 0) {
            let sel_head_html_doc = globals.DomParser.parseFromString(sel_head_html_str, 'text/html');
            let sel_head_p_elms = sel_head_html_doc.getElementsByTagName('p');
            if (sel_head_p_elms != null && sel_head_p_elms.length > 0) {
                let sel_head_p_elm = sel_head_p_elms[0];
                //clear text from selection
                sel_head_p_elm.innerText = '';
                sel_html_format = sel_head_p_elm.innerHTML;
            }
        }

        let newTemplateName = '';
        if (range.length == 0) {
            newTemplateName = getLowestAnonTemplateName(newTemplateType, allDefs);
        } else {
            newTemplateName = getText(range).trim();
        }
        if (sel_html_format == '<br>') {
            //this occurs when selection.length == 0
            sel_html_format = '';// newTemplateName;
        }
        let formatInfo = globals.quill.getFormat(range.index, 1);
        newTemplateObj = {
            templateGuid: generateGuid(),
            templateColor: getRandomPaletteColor(),
            templateName: newTemplateName,
            templateType: newTemplateType,
            templateData: '',
            templateDeltaFormat: JSON.stringify(formatInfo),
            templateHtmlFormat: sel_html_format
        };

        onAddOrUpdateTemplate_ntf(newTemplateObj);
    }

    hideCreateTemplateToolbarContextMenu();
    insertTemplate(range, newTemplateObj,'user');
    focusTemplate(newTemplateObj.templateGuid, true, isNew);
    return newTemplateObj;
}

function updateCreateTemplateToolbarButtonToSelection() {
    if (isCreateTemplateValid()) {
        getCreateTemplateToolbarButton().classList.remove('disabled');
    } else {
        getCreateTemplateToolbarButton().classList.add('disabled');
	}
}


function showTemplateToolbarContextMenu() {
    if (!isCreateTemplateValid()) {
        log('user add template currently disabled (selection must be on/within template)');
        return;
    }

    var tb_elm = getCreateTemplateToolbarButton();
    let anchor_corner = 'bottom-left';
    let menu_anchor_corner = 'top-left';
    let offset = { x: 0, y: 5 };
    showContextMenu(tb_elm, getBusySpinnerMenuItem(), anchor_corner, menu_anchor_corner, offset.x, offset.y);

    getAllSharedTemplatesFromDbAsync_get()
        .then((result) => {
            result = result ? result : [];

            let all_non_input_defs = result;
            let all_local_defs = getTemplateDefs();
            let db_defs_to_add = all_non_input_defs.filter(x => all_local_defs.every(y => y.templateGuid != x.templateGuid));

            let allTemplateDefs = [...all_local_defs, ...db_defs_to_add];

            let cm = [];

            for (var i = 0; i < globals.TemplateTypesMenuOptions.length; i++) {
                // get predefined template type
                let tmi = globals.TemplateTypesMenuOptions[i];
                if (tmi.separator) {
                    cm.push(tmi);
                    continue;
                }
                if (tmi.id == 'MoreLink') {
                    tmi.action = function () {
                        onNavigateUriRequested_ntf(tmi.url, 'uri', -1, '', null, true);
                    }
                    cm.push(tmi);
                    continue;
                }
                // get all templates for type
                let allTemplateDefsForType = allTemplateDefs.filter(x => x.templateType.toLowerCase() == tmi.label.toLowerCase());

                tmi.submenu = [];
                // bind existing template creates to menu items
                tmi.submenu = allTemplateDefsForType.map(function (ttd) {
                    return {
                        icon: ' ',
                        iconBgColor: ttd.templateColor,
                        label: ttd.templateName,
                        action: function (option, contextMenuIndex, optionIndex) {
                            createTemplateFromDropDown(ttd);
                        },
                    }
                });
                // append add new to bottom of template type sub menu
                if (allTemplateDefsForType.length > 0) {
                    tmi.submenu.push({ separator: true });
                }
                tmi.submenu.push(
                    {
                        icon: 'plus',
                        iconFgColor: 'lime',
                        iconClasses: 'svg-no-defaults',
                        label: 'New...',
                        action: function (option, contextMenuIndex, optionIndex) {
                            createTemplateFromDropDown(null, tmi.templateType, allTemplateDefs);
                        },
                    }
                );
                cm.push(tmi);
            }

            showContextMenu(tb_elm, cm, anchor_corner, menu_anchor_corner, offset.x, offset.y);
        });
}

function hideCreateTemplateToolbarContextMenu() {
    if (isShowingCreateTemplateToolbarMenu()) {
        superCm.destroyMenu();
    }
}

// #endregion Actions

// #region Event Handlers

function onTemplateToolbarButtonClick(e) {
    if (isShowingCreateTemplateToolbarMenu()) {
        hideCreateTemplateToolbarContextMenu();
    } else {
        showTemplateToolbarContextMenu();
	}
    
    //event.stopPropagation(e);
}
// #endregion Event Handlers






