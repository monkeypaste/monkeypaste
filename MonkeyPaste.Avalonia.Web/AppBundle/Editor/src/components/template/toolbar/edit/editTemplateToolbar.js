﻿
var IsTemplateNameTextAreaFocused = false;
var IsTemplateDetailTextAreaFocused = false;

var TemplateBeforeEdit = null;

// #region Life Cycle

function initEditTemplateToolbar() {
    enableResize(getEditTemplateToolbarContainerElement());

    addClickOrKeyClickEventListener(getEditTemplateColorBoxElement(), onTemplateColorBoxContainerClick);

    addClickOrKeyClickEventListener(getDeleteTemplateButtonElement(), onDeleteTemplateButtonClick);

    getEditTemplateNameTextAreaElement().addEventListener('focus', onTemplateNameTextAreaGotFocus);
    getEditTemplateNameTextAreaElement().addEventListener('blur', onTemplateNameTextAreaLostFocus);
    getEditTemplateNameTextAreaElement().addEventListener('input', onTemplateNameChanged);
    initBouncyTextArea(getEditTemplateNameTextAreaElement());

    getEditTemplateDetailTextAreaElement().addEventListener('focus', onTemplateDetailTextAreaGotFocus);
    getEditTemplateDetailTextAreaElement().addEventListener('blur', onTemplateDetailTextAreaLostFocus);
    getEditTemplateDetailTextAreaElement().addEventListener('input', onTemplateDetailChanged);       
}

function showEditTemplateToolbar(isNew = false) {
    let ett = getEditTemplateToolbarContainerElement();
    ett.classList.remove('hidden');

    let t = getTemplateDefByGuid(getFocusTemplateGuid());
    if (t) {
        if (isNew) {
            // keep comprarer empty besides guid to ensure host is notified of add
            TemplateBeforeEdit = { templateGuid: t.templateGuid };
        } else {
            TemplateBeforeEdit = t;
        }
        createEditTemplateToolbarForTemplate(t);
    } else {
        log('no focus template found');
    }

    updateAllElements();
}

function hideEditTemplateToolbar(wasEscCancel = false, wasDelete = false) {
    if (!isShowingEditTemplateToolbar()) {
        TemplateBeforeEdit = null;
        return;
    }
    hideColorPaletteMenu();

    clearAllTemplateEditClasses();
    if (TemplateBeforeEdit != null && !wasDelete) {
        // get new or updated def
        let updated_t = getTemplateDefByGuid(TemplateBeforeEdit.templateGuid);
        if (isTemplateDefChanged(TemplateBeforeEdit, updated_t) && !wasEscCancel) {
            // t new or updated
            onAddOrUpdateTemplate_ntf(updated_t);
        }
        // reset comprarer template
        TemplateBeforeEdit = null;
    }

    var ett = getEditTemplateToolbarContainerElement();
    ett.classList.add('hidden');
}

// #endregion Life Cycle

// #region Getters

function getEditTemplateToolbarContainerElement() {
    return document.getElementById('editTemplateToolbar');
}

function getEditTemplateToolbarHeight() {
    return getEditTemplateToolbarContainerElement().getBoundingClientRect().height;
}

function getEditTemplateColorBoxElement() {
    return document.getElementById('templateColorBox');
}

function getEditTemplateNameTextAreaElement() {
    return document.getElementById('templateNameTextArea');
}

function getEditTemplateDetailTextAreaElement() {
    return document.getElementById('templateDetailTextArea');
}

function getDeleteTemplateButtonElement() {
    return document.getElementById('editTemplateDeleteButton');
}
// #endregion Getters

// #region Setters

function setTemplateName(tguid, name) {
    var telms = getTemplateElements(tguid);
    for (var i = 0; i < telms.length; i++) {
        var telm = telms[i];
        if (telm.getAttribute('templateGuid') == tguid) {
            telm.setAttribute('templateName', name);
            //changeInnerText(telm, telm.innerText, name);
            setTemplateElementText(telm, name);
        }
    }
}

function setTemplateDetailData(tguid, detailData) {
    let telms = getTemplateElements(tguid);
    for (var i = 0; i < telms.length; i++) {
        let telm = telms[i];
        telm.setAttribute('templateData', detailData);
        if (isShowingPasteToolbar()) {
            let t = getTemplateFromDomNode(telm);
            let t_text = getTemplatePasteValue(t);
            telm.setAttribute('templateText', t_text);
            setTemplateElementText(telm, t_text);
            //telm.innerText = t_text;
		}
	}
}

function setEditToolbarColorButtonColor(chex) {
    getEditTemplateColorBoxElement().style.backgroundColor = chex;
}
// #endregion Setters

// #region Actions

function showTemplateColorPaletteMenu() {
    hideColorPaletteMenu();

    let color_box_elm = getEditTemplateColorBoxElement();
    let ft = getFocusTemplate();
    if (!ft) {
        debugger;
    }

    const colorBoxRect = color_box_elm.getBoundingClientRect();
    const x = colorBoxRect.left;
    const y = getEditTemplateToolbarContainerElement().getBoundingClientRect().top;

    showColorPaletteMenu(
        color_box_elm,
        'top|left',
        'above',
        ft.templateColor,
        onColorPaletteItemClick);

}

function hideTemplateColorPaletteMenu() {
    if (ColorPaletteAnchorElement != getEditTemplateColorBoxElement()) {
        // only hide if color palette is for template
        return;
    }
    hideColorPaletteMenu();

}

function createEditTemplateToolbarForTemplate(t) {
    log('Editing Template: ' + t.templateGuid + " selected type: " + t.templateType);
    let ttype = t.templateType.toLowerCase();
    if (ttype == 'dynamic') {
        getEditTemplateToolbarContainerElement().classList.remove('template-with-detail-layout');

        getEditTemplateDetailTextAreaElement().classList.add('hidden');
    } else {
        getEditTemplateToolbarContainerElement().classList.add('template-with-detail-layout');

        getEditTemplateDetailTextAreaElement().classList.remove('hidden');
        getEditTemplateDetailTextAreaElement().value = t.templateData;
        getEditTemplateDetailTextAreaElement().addEventListener('input', onTemplateDetailChanged);
    }

    getEditTemplateColorBoxElement().style.backgroundColor = t.templateColor;

    getEditTemplateNameTextAreaElement().value = t.templateName;
}

function updateEditTemplateToolbarSizesAndPositions() {
    if (!isShowingEditTemplateToolbar()) {
        return;
    }
    let ett = getEditTemplateToolbarContainerElement();
    if (isShowingPasteToolbar()) {
        //ett.classList.remove('bottom-align');
        let pttb_h = getPasteToolbarContainerElement().getBoundingClientRect().height;
        ett.style.bottom = pttb_h + 'px';
    } else {
        //ett.classList.add('bottom-align');
        ett.style.bottom = '0px';
	}
}

// #endregion Actions

// #region State

function isShowingEditTemplateToolbar() {
    return !getEditTemplateToolbarContainerElement().classList.contains('hidden');
}

function resetEditTemplateToolbar() {
    IsTemplateDetailTextAreaFocused = false;
    IsTemplateNameTextAreaFocused = false;
    TemplateBeforeEdit = null;

    hideEditTemplateToolbar();
}

// #endregion State



//#region Model Changes

function deleteFocusTemplate() {
    let ftguid = getFocusTemplateGuid();
    if (!ftguid) {
        debugger;
    }

    let ft = getTemplateDefByGuid(ftguid);
    let result = window.confirm('Are you sure you want to delete ALL usages of \'' + ft.templateName + '\'? This cannot be undone.')

    if (!result) {
        return;
    }

    removeTemplatesByGuid(ftguid);

    // NOTE may need to force (notify) content write to db here so MasterTemplateCollection doesn't pick this item up
    onUserDeletedTemplate_ntf(ftguid);

    hideEditTemplateToolbar(false, true);
    //log('Template \'' + ftguid + '\' \'' + td.templateName + '\' was DELETED');
}

function isEditTemplateTextAreaFocused() {
    return IsTemplateNameTextAreaFocused || IsTemplateDetailTextAreaFocused;
}

//#endregion

//#region Event Handlers

function onTemplateColorBoxContainerClick(e) {
    showTemplateColorPaletteMenu();
    event.stopPropagation(e);
}

function onTemplateNameChanged(e) {
    let newTemplateName = getEditTemplateNameTextAreaElement().value;
    setTemplateName(TemplateBeforeEdit.templateGuid, newTemplateName);
}

function onTemplateDetailChanged(e) {
    let newDetailData = getEditTemplateDetailTextAreaElement().value;
    setTemplateDetailData(TemplateBeforeEdit.templateGuid, newDetailData);
}

function onTemplateNameTextAreaGotFocus() {
    IsTemplateNameTextAreaFocused = true;
}

function onTemplateNameTextAreaLostFocus() {
    IsTemplateNameTextAreaFocused = false;
}

function onTemplateDetailTextAreaGotFocus() {
    IsTemplateDetailTextAreaFocused = true;
}

function onTemplateDetailTextAreaLostFocus() {
    IsTemplateDetailTextAreaFocused = false;
}

function onDeleteTemplateButtonClick(e) {
    deleteFocusTemplate();
}

function onColorPaletteItemClick(chex) {
    let tguid = TemplateBeforeEdit.templateGuid;
    let t = getTemplateDefByGuid(tguid);

    setTemplateBgColor(tguid, chex, false);
    
    hideAllTemplateContextMenus();
}
//#endregion Event Handlers