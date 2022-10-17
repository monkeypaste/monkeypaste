
var IsTemplateNameTextAreaFocused = false;
var IsTemplateDetailTextAreaFocused = false;

var TemplateBeforeEdit = null;

// #region Life Cycle

function initEditTemplateToolbar() {
    //let resizers = Array.from(document.getElementsByClassName('resizable-textarea'));
    //for (var i = 0; i < resizers.length; i++) {
    //    let rta = resizers[i];

    //    new ResizeObserver(() => {
    //        updateEditTemplateToolbarPosition();
    //    }).observe(rta);
    //}

    enableResize(getEditTemplateToolbarContainerElement());

    document.getElementById('templateColorBox').addEventListener('click', onTemplateColorBoxContainerClick);
    
    document.getElementById('templateNameTextInput').addEventListener('focus', onTemplateNameTextAreaGotFocus);
    document.getElementById('templateNameTextInput').addEventListener('blur', onTemplateNameTextAreaLostFocus);
    document.getElementById('templateNameTextInput').addEventListener('input', onTemplateNameChanged);
    initBouncyTextArea('templateNameTextInput');

    document.getElementById('templateDetailTextInput').addEventListener('focus', onTemplateDetailTextAreaGotFocus);
    document.getElementById('templateDetailTextInput').addEventListener('blur', onTemplateDetailTextAreaLostFocus);

    document.getElementById('templateDetailTextInput').addEventListener('input', onTemplateDetailChanged);
       
    document.getElementById('editTemplateDeleteButton').addEventListener('click', onDeleteTemplateButtonClick);
}

function loadEditTemplateToolbar() {
    IsTemplateDetailTextAreaFocused = false;
    IsTemplateNameTextAreaFocused = false;
    TemplateBeforeEdit = null;
}

function createEditTemplateToolbarForTemplate(t) {
    log('Editing Template: ' + t.templateGuid + " selected type: " + t.templateType);
    let ttype = t.templateType.toLowerCase();
    if (ttype == 'dynamic') {
        document.getElementById('templateDetailTextInputContainer').classList.add('hidden');
        document.getElementById('editTemplateToolbar').classList.remove('template-with-detail-layout');
    } else {
        document.getElementById('templateDetailTextInputContainer').classList.remove('hidden');
        document.getElementById('editTemplateToolbar').classList.add('template-with-detail-layout');

        document.getElementById('templateDetailTextInput').value = t.templateData;
        document.getElementById('templateDetailTextInput').addEventListener('input', onTemplateDetailChanged);
    }

    document.getElementById('templateColorBox').style.backgroundColor = t.templateColor;

    document.getElementById('templateNameTextInput').value = t.templateName;
}

function showEditTemplateToolbar(isNew = false) {
    var ett = getEditTemplateToolbarContainerElement();
    ett.classList.remove('hidden');

    updateAllSizeAndPositions();

    var t = getTemplateDefByGuid(getFocusTemplateGuid());
    if (t) {
        if (isNew) {
            // keep comprarer empty besides guid to ensure host is notified of add
            TemplateBeforeEdit = { templateGuid: t.templateGuid };
        } else {
            TemplateBeforeEdit = t;
		}
        createEditTemplateToolbarForTemplate(t);
    }
}

function hideEditTemplateToolbar() {    
    clearAllTemplateEditClasses();
    if (TemplateBeforeEdit != null) {
        // get new or updated def
        let updated_t = getTemplateDefByGuid(TemplateBeforeEdit.templateGuid);
        if (isTemplateDefChanged(TemplateBeforeEdit, updated_t)) {
            // t new or updated
            onAddOrUpdateTemplate_ntf(updated_t);
        }
        // reset comprarer template
        TemplateBeforeEdit = null;
    }
    

    var ett = getEditTemplateToolbarContainerElement();
    ett.classList.add('hidden');
}
function showTemplateColorPaletteMenu() {
    if (isShowingTemplateColorPaletteMenu()) {
        hideTemplateColorPaletteMenu();
    }

    var palette_item = {
        style_class: 'template_color_palette_item',
        func: 'onColorPaletteItemClick'
    };

    let paletteHtml = '<table>';
    for (var r = 0; r < 10; r++) {
        paletteHtml += '<tr>';
        for (var c = 0; c < 10; c++) {
            let c = getRandomColor().trim();
            let item = '<td><a href="javascript:void(0);" onclick="' + palette_item.func + '(\'' + c + '\'); event.stopPropagation();">' +
                '<div class="' + palette_item.style_class + '" style="background-color: ' + c + '" ></div></a></td > ';
            paletteHtml += item;
        }
        paletteHtml += '</tr>';
    }

    var paletteMenuElm = document.getElementById('templateColorPaletteMenu');
    paletteMenuElm.innerHTML = paletteHtml;

    paletteMenuElm.style.display = 'block';
    var paletteMenuRect = paletteMenuElm.getBoundingClientRect();

    const editToolbarRect = document.getElementById('editTemplateToolbar').getBoundingClientRect();
    const colorBoxRect = document.getElementById('templateColorBox').getBoundingClientRect();
    const x = colorBoxRect.left;
    const y = editToolbarRect.top - paletteMenuRect.height;
    paletteMenuElm.style.left = `${x}px`;
    paletteMenuElm.style.top = `${y}px`;
}

function hideTemplateColorPaletteMenu() {
    var paletteMenuElm = document.getElementById('templateColorPaletteMenu');
    paletteMenuElm.style.display = 'none';
}

// #endregion Life Cycle

// #region Getters

function getEditTemplateToolbarContainerElement() {
    return document.getElementById('editTemplateToolbar');
}
// #endregion Getters

// #region Setters

function setTemplateName(tguid, name) {
    var tl = getTemplateElements(tguid);
    for (var i = 0; i < tl.length; i++) {
        var te = tl[i];
        if (te.getAttribute('templateGuid') == tguid) {
            te.setAttribute('templateName', name);
            changeInnerText(te, te.innerText, name);
        }
    }
}

function setTemplateDetailData(tguid, detailData) {
    var tl = getTemplateElements(tguid);
    for (var i = 0; i < tl.length; i++) {
        var te = tl[i];
        if (te.getAttribute('templateGuid') == tguid) {
            te.setAttribute('templateData', detailData);
            if (te.getAttribute('templateType').toLowerCase() == 'datetime') {
                log(jQuery.format.date(new Date(), detailData));
            }
        }
    }
}
// #endregion Setters

// #region Actions

// #endregion Actions

// #region State

function isShowingEditTemplateToolbar() {
    return $("#editTemplateToolbar").css("display") != 'none';
}

function isShowingTemplateColorPaletteMenu() {
    return $('templateColorPaletteMenu').css('display') != 'none';
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
    onUserDeletedTemplate_ntf(ftguid);

    hideEditTemplateToolbar();
    log('Template \'' + ftguid + '\' \'' + td.templateName + '\' was DELETED');
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
    let newTemplateName = document.getElementById('templateNameTextInput').value;
    setTemplateName(getFocusTemplateGuid(), newTemplateName);
}

function onTemplateDetailChanged(e) {
    let newDetailData = document.getElementById('templateDetailTextInput').value;
    setTemplateDetailData(getFocusTemplateGuid(), newDetailData);
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
    let tguid = getFocusTemplateGuid();
    let t = getTemplateDefByGuid(tguid);

    setTemplateBgColor(tguid, chex, false);
    document.getElementById('templateColorBox').style.backgroundColor = chex;

    hideAllTemplateContextMenus();
}
//#endregion Event Handlers