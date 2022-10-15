
var IsTemplateNameTextAreaFocused = false;
var IsTemplateDetailTextAreaFocused = false;

function initEditTemplateToolbar() {
    let resizers = Array.from(document.getElementsByClassName('resizable-textarea'));
    for (var i = 0; i < resizers.length; i++) {
        let rta = resizers[i];

        new ResizeObserver(() => {
            updateEditTemplateToolbarPosition();
        }).observe(rta);
    }

    enableResize(document.getElementById('editTemplateToolbar'));

    document.getElementById('templateNameTextInput').addEventListener('focus', onTemplateNameTextAreaGotFocus);
    document.getElementById('templateNameTextInput').addEventListener('blur', onTemplateNameTextAreaLostFocus);
    document.getElementById('templateNameTextInput').addEventListener('keydown', onTemplateNameTextArea_keydown);
    document.getElementById('templateNameTextInput').addEventListener('keyup', onTemplateNameTextArea_keyup);

    document.getElementById('templateDetailTextInput').addEventListener('focus', onTemplateDetailTextAreaGotFocus);
    document.getElementById('templateDetailTextInput').addEventListener('blur', onTemplateDetailTextAreaLostFocus);

    document.getElementById('templateDetailTextInput').addEventListener('input', onTemplateDetailChanged);

    document.getElementById('templateColorBox').addEventListener('click', onTemplateColorBoxContainerClick);

    document.getElementById('templateNameTextInput').addEventListener('input', onTemplateNameChanged);
}

function showEditTemplateToolbar() {
    var ett = document.getElementById('editTemplateToolbar');
    ett.style.display = 'flex';


    updateAllSizeAndPositions();

    var t = getTemplateDefByGuid(getFocusTemplateGuid());
    if (t && t.domNode) {
        setTemplateType(t.domNode.getAttribute('templateGuid'), t.domNode.getAttribute('templateType'));
    }

    //document.getElementById('editTemplateTypeMenuSelector').addEventListener('change', onTemplateTypeChanged);
}

function hideEditTemplateToolbar() {
    clearAllTemplateEditClasses();

    var ett = document.getElementById('editTemplateToolbar');
    ett.style.display = 'none';

    //document.getElementById('editTemplateTypeMenuSelector').removeEventListener('change', onTemplateTypeChanged);
}

function updateEditTemplateToolbarPosition() {
    let wh = window.visualViewport.height;
    let etth = $("#editTemplateToolbar").outerHeight();
    $("#editTemplateToolbar").css("top", wh - etth);
}

function isShowingEditTemplateToolbar() {
    return $("#editTemplateToolbar").css("display") != 'none';
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

function isShowingTemplateColorPaletteMenu() {
    return $('templateColorPaletteMenu').css('display') != 'none';
}

//#region Model Changes

function deleteFocusTemplate() {
    let tguid_to_delete = getFocusTemplateGuid();
    if (tguid_to_delete == null) {
        log('error, no focus template to delete, ignoring...');
        return;
    }

    let td = getTemplateFromDomNode(getTemplateDefByGuid(tguid_to_delete).domNode);
    let result = window.confirm('Are you sure you want to delete ALL usages of \'' + td.templateName + '\'?')

    if (!result) {
        return;
    }

    userDeletedTemplateGuids.push(tguid_to_delete);

    let availTemplateRef = availableTemplates.filter(x => x.templateGuid == tguid_to_delete);
    if (availTemplateRef != null && availTemplateRef.length > 0) {
        let availIdx = availableTemplates.indexOf(availTemplateRef[0]);
        availableTemplates.splice(availIdx, 1);
    }

    getUsedTemplateInstances()
        .filter(x => x.domNode.getAttribute('templateGuid') == tguid_to_delete)
        .forEach((ti) => {
            let t = getTemplateFromDomNode(ti.domNode);
            let docIdx = getTemplateDocIdx(t.templateInstanceGuid);
            quill.deleteText(docIdx, 1, Quill.sources.USER);
        });

    hideEditTemplateToolbar();
    log('Template \'' + tguid_to_delete + '\' \'' + td.templateName + '\' was DELETED');
}

function setTemplateType(tguid, ttype) {
    log('Template: ' + tguid + " selected type: " + ttype);

    //setTemplateProperty(tguid, 'templateType', ttype);

    var t = getTemplateDefByGuid(tguid);
    //t.domNode.setAttribute('templateType', templateTypeValue);
    //document.getElementById("editTemplateTypeMenuSelector").value = ttype;

    if (ttype == 'datetime' && t.domNode.getAttribute('templateData') == '') {
        //setTemplateProperty(tguid, 'templateData', 'MM/dd/yyy HH:mm:ss');
    }

    if (ttype == 'dynamic') {
        document.getElementById('templateDetailTextInputContainer').style.display = 'none';
    } else {
        document.getElementById('templateDetailTextInputContainer').style.display = 'inline-block';
        document.getElementById('templateDetailTextInput').value = getTemplateProperty(tguid, 'templateData');
        //document.getElementById('templateDetailTextInput').addEventListener('input', onTemplateDetailChanged);
    }

    document.getElementById('templateColorBox').style.backgroundColor = getTemplateProperty(tguid, 'templateColor');
    //document.getElementById('templateColorBox').addEventListener('click', onTemplateColorBoxContainerClick);

    document.getElementById('templateNameTextInput').value = getTemplateProperty(tguid, 'templateName');
    //document.getElementById('templateNameTextInput').addEventListener('input', onTemplateNameChanged);
}

function templateTextAreaChange() {
    var curText = $('#templateTextArea').val();
    setTemplateText(getFocusTemplateGuid(), curText);
}

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

function isEditTemplateTextAreaFocused() {
    return IsTemplateNameTextAreaFocused || IsTemplateDetailTextAreaFocused;
}

function clearAllTemplateEditClasses() {
    getTemplateElements().forEach((telm) => {
        telm.classList.remove('ql-template-embed-blot-display-key-up');
        telm.classList.remove('ql-template-embed-blot-display-key-down');
    });
}

async function scaleFocusTemplates(scaleType, tguid) {
    if (!tguid) {
        tguid = getFocusTemplateGuid();
        if (!tguid) {
            return;
		}
	}

    let f_cit_elm_l = getTemplateElements(tguid);
    for (var i = 0; i < f_cit_elm_l.length; i++) {
        let f_cit_elm = f_cit_elm_l[i];
        if (scaleType == 'bigger') {
            f_cit_elm.classList.remove('ql-template-embed-blot-display-key-up');
            f_cit_elm.classList.add('ql-template-embed-blot-display-key-down');
        } else if (scaleType == 'default') {
            f_cit_elm.classList.remove('ql-template-embed-blot-display-key-down');
            f_cit_elm.classList.add('ql-template-embed-blot-display-key-up');
        } else {

            f_cit_elm.classList.remove('ql-template-embed-blot-display-key-down');
            f_cit_elm.classList.remove('ql-template-embed-blot-display-key-up');
        }
	}
}

async function jiggleFocusTemplates(resetOnComplete = false) {
    return;
    //let f_cit = getFocusTemplateElement();
    let tguid = getFocusTemplateGuid();
    if (!tguid) {
        return;
	}
    let scale_ms = 100;
    for (var i = 0; i < 2; i++) {
        await scaleFocusTemplates('bigger', tguid);
        await delay(scale_ms);
        await scaleFocusTemplates('default', tguid);
        await delay(scale_ms);
    }
    if (resetOnComplete) {
        scaleFocusTemplates('reset', tguid);
	}
}

//#endregion

//#region Event Callbacks

function onTemplateTypeChanged(e) {
    setTemplateType(getFocusTemplateGuid(), this.value);
}

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

    jiggleFocusTemplates();
}

function onTemplateNameTextAreaLostFocus() {
    IsTemplateNameTextAreaFocused = false;

    jiggleFocusTemplates(true);
}

function onTemplateDetailTextAreaGotFocus() {
    IsTemplateDetailTextAreaFocused = true;
}


function onTemplateDetailTextAreaLostFocus() {
    IsTemplateDetailTextAreaFocused = false;
}

async function onTemplateNameTextArea_keydown() {
    await scaleFocusTemplates('bigger');
}

async function onTemplateNameTextArea_keyup() {
    await scaleFocusTemplates('default');
}

//#endregion