
// #region Life Cycle

function initEditTemplateToolbar() {
    addClickOrKeyClickEventListener(getEditTemplateColorBoxElement(), onTemplateColorBoxContainerClick);
    addClickOrKeyClickEventListener(getDeleteTemplateButtonElement(), onDeleteTemplateButtonClick);

    getEditTemplateNameTextAreaElement().addEventListener('focus', onTemplateNameTextAreaGotFocus);
    getEditTemplateNameTextAreaElement().addEventListener('blur', onTemplateNameTextAreaLostFocus);
    getEditTemplateNameTextAreaElement().addEventListener('input', onTemplateNameChanged);
    initBouncyTextArea(getEditTemplateNameTextAreaElement());     
}

function showEditTemplateToolbar(isNew = false) {
    const needs_update = isShowingEditTemplateToolbar() == false;
    let ett = getEditTemplateToolbarContainerElement();
    ett.classList.remove('hidden');
    getPasteEditFocusTemplateButtonElement().classList.add('checked');

    let t = getTemplateDefByGuid(getFocusTemplateGuid(true));
    if (t) {
        createEditTemplateToolbarForTemplate(t);
    } else {
        log('no focus template found');
    }
    if (needs_update) {
        updateAllElements();
    }
}

function hideEditTemplateToolbar(wasEscCancel = false, wasDelete = false) {
    if (isShowingEditTemplateToolbar()) {
        hideColorPaletteMenu();
        clearAllTemplateEditClasses();
        getEditTemplateToolbarContainerElement().classList.add('hidden');
        getPasteEditFocusTemplateButtonElement().classList.remove('checked');
    }
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

function getDeleteTemplateButtonElement() {
    return document.getElementById('editTemplateDeleteButton');
}

// #endregion Getters

// #region Setters


function setEditToolbarColorButtonColor(chex) {
    getEditTemplateColorBoxElement().style.backgroundColor = chex;
}
// #endregion Setters

// #region Actions

function showTemplateColorPaletteMenu() {
    hideColorPaletteMenu();

    let color_box_elm = getEditTemplateColorBoxElement();
    let ft = getFocusTemplate(true);
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
    if (globals.ColorPaletteAnchorElement != getEditTemplateColorBoxElement()) {
        // only hide if color palette is for template
        return;
    }
    hideColorPaletteMenu();

}

function createEditTemplateToolbarForTemplate(t) {
    if (!t) {
        return;
    }
    log('Editing Template: ' + t.templateGuid + " selected type: " + t.templateType);
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

// #endregion State



//#region Model Changes

function deleteFocusTemplate() {
    let ftguid = getFocusTemplateGuid(true);
    if (!ftguid) {
        return;
    }

    let ft = getTemplateDefByGuid(ftguid);
    if (!ft) {
        return;
    }
    if (!isRunningOnHost()) {
        removeTemplatesByGuid(ftguid);
        hideEditTemplateToolbar(false, true);
        return;
    }

    getMessageBoxResultAsync_get(
        'confirm', 'Are you sure you want to delete ALL usages of \'' + ft.templateName + '\'? This cannot be undone.',
        'okcancel',
        'WarningImage')
        .then(result => {
            if (isNullOrUndefined(result) || result == false || result == 'false') {
                log('delete canceled');
                return;
            }

            removeTemplatesByGuid(ftguid);

            // NOTE may need to force (notify) content write to db here so MasterTemplateCollection doesn't pick this item up
            onUserDeletedTemplate_ntf(ftguid);

            hideEditTemplateToolbar(false, true);
    //log('Template \'' + ftguid + '\' \'' + td.templateName + '\' was DELETED');
        });
}

//#endregion

//#region Event Handlers

function onTemplateColorBoxContainerClick(e) {
    showTemplateColorPaletteMenu();
    event.stopPropagation(e);
}

function onTemplateNameChanged(e) {
    let newTemplateName = getEditTemplateNameTextAreaElement().value;
    setTemplateName(getFocusTemplateGuid(), newTemplateName);
}


function onTemplateNameTextAreaGotFocus() {
    // NOTE noticable performance lag when editing template names so only ntf when complete
    globals.SuppressContentChangedNtf = true;
    getEditTemplateNameTextAreaElement().addEventListener('keydown', onTemplateNameTextAreaKeyDown, true);
}

function onTemplateNameTextAreaLostFocus() {
    globals.SuppressContentChangedNtf = false;
    getEditTemplateNameTextAreaElement().removeEventListener('keydown', onTemplateNameTextAreaKeyDown);
}

function onTemplateNameTextAreaKeyDown(e) {
    if (e.key == 'Enter' || e.key == 'Escape') {
        e.preventDefault();
        hideEditTemplateToolbar();
    }
}

function onDeleteTemplateButtonClick(e) {
    deleteFocusTemplate();
}

function onColorPaletteItemClick(chex) {
    setTemplateBgColor(getFocusTemplateGuid(true), chex, false);
    
    hideAllTemplateContextMenus();
}
//#endregion Event Handlers