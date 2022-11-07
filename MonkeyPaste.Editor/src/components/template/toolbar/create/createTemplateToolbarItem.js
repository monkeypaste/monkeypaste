// #region Globals

var TemplateTypesMenuOptions = [
    {
        label: 'Dynamic',
        icon: 'fa-solid fa-keyboard'
    },
    {
        label: 'Static',
        icon: 'fa-solid fa-icicles'
    },
    /* {
         label: 'Content',
         icon: 'fa-solid fa-clipboard'
     },
     {
         label: 'Analyzer',
         icon: 'fa-solid fa-scale-balanced'
     },
     {
         label: 'Action',
         icon: 'fa-solid fa-bolt-lightning'
     },*/
    {
        label: 'Contact',
        icon: 'fa-solid fa-id-card'
    },
    {
        label: 'DateTime',
        icon: 'fa-solid fa-clock'
    }
];

// #endregion Globals

// #region Life Cycle

function initTemplateToolbarButton() {
    addClickOrKeyClickEventListener(getCreateTemplateToolbarButton(), onTemplateToolbarButtonClick);
    getCreateTemplateToolbarButton().innerHTML = getSvgHtml('createtemplate');
}

function showTemplateToolbarContextMenu() {
    if (!isCreateTemplateValid()) {
        log('user add template currently disabled (selection must be on/within template)');
        return;
    }
    var tb = getCreateTemplateToolbarButton();
    let tb_rect = tb.getBoundingClientRect();
    let x = tb_rect.left;
    let y = tb_rect.bottom;

    let allTemplateDefs = getAvailableTemplateDefinitions()
    var cm = [];

    for (var i = 0; i < TemplateTypesMenuOptions.length; i++) {
        let tmi = TemplateTypesMenuOptions[i];

        let allTemplateDefsForType = allTemplateDefs.filter(x => x.templateType.toLowerCase() == tmi.label.toLowerCase());

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

        if (allTemplateDefsForType.length > 0) {
            tmi.submenu.push({ separator: true });
        }
        tmi.submenu.push(
            {
                icon: 'fa-solid fa-plus',
                iconFgColor: 'lime',
                label: 'New...',
                action: function (option, contextMenuIndex, optionIndex) {
                    createTemplateFromDropDown(null, tmi.label.toLowerCase());
                },
            }
        )
        cm.push(tmi);
    }

    let result = superCm.createMenu(cm, { pageX: x, pageY: y });
    return;
}

function hideCreateTemplateToolbarContextMenu() {
    if (isShowingCreateTemplateToolbarMenu()) {
        superCm.destroyMenu();
    }
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

function createTemplateFromDropDown(templateObjOrId, newTemplateType) {
    var templateObj;
    if (templateObjOrId != null && typeof templateObjOrId === 'string') {
        templateObj = getTemplateDefByGuid(templateObjOrId);
    } else {
        templateObj = templateObjOrId;
    }

    var range = quill.getSelection(true);

    var isNew = templateObj == null;
    var newTemplateObj = templateObj;

    if (isNew) {
        //grab the selection head's html to set formatting of template div
        let sel_html_format = '';
        let sel_head_html_str = getHtml({ index: range.index, length: 1 });
        if (sel_head_html_str != null && sel_head_html_str.length > 0) {
            let sel_head_html_doc = DomParser.parseFromString(sel_head_html_str, 'text/html');
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
            newTemplateName = getLowestAnonTemplateName();
        } else {
            newTemplateName = getText(range).trim();
        }
        if (sel_html_format == '<br>') {
            //this occurs when selection.length == 0
            sel_html_format = '';// newTemplateName;
        }
        let formatInfo = quill.getFormat(range.index, 1);
        newTemplateObj = {
            templateGuid: generateGuid(),
            templateColor: getRandomColor(),
            templateName: newTemplateName,
            templateType: newTemplateType,
            templateData: '',
            templateDeltaFormat: JSON.stringify(formatInfo),
            templateHtmlFormat: sel_html_format
        };
    }

    hideCreateTemplateToolbarContextMenu();
    insertTemplate(range, newTemplateObj);
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






