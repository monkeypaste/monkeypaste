const TEMPLATE_VALID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890#-_ ";

var isShowingEditTemplateToolbar = false;
var isShowingPasteTemplateToolbar = false;

var selectedTemplateId = 0;
var wasLastClickOnTemplate = false;

function registerTemplateSpan(Quill) {
    const Parchment = Quill.imports.parchment;

    class TemplateSpanBlot extends Parchment.EmbedBlot {
        static blotName = 'templatespan';
        static tagName = 'SPAN';
        static className = 'template_btn';
        static create(value) {
            const node = super.create(value);
            let iId = getUniqueTemplateInstanceId(value.templateId);
            var textColor = isBright(value.templateColor) ? 'black' : 'white';
            node.contentEditable = 'false';
            node.innerText = value.templateName;
            node.setAttribute('templateName', value.templateName);
            node.setAttribute('templateType', value.templateType);
            node.setAttribute('templateColor', value.templateColor);
            node.setAttribute('templateId', value.templateId);
            node.setAttribute('templateData', value.templateData);
            node.setAttribute('templateText', value.templateText);
            node.setAttribute('instanceId', iId);
            node.setAttribute('docIdx', value.docIdx);
            node.setAttribute('isFocus', false);

            node.setAttribute("spellcheck", "false");
            node.setAttribute('class', 'template_btn');
            node.setAttribute('style', 'background-color: ' + value.templateColor + ';color:' + textColor + ';');

            node.addEventListener('click', function (e) {
                //console.log(e);
                focusTemplate(node);
            });

            //node.addEventListener('keydown', onLogKeyDown);
            //this._addRemovalButton(node);
            return node;
        }

        static value(domNode) {
            return {
                docIdx: domNode.getAttribute('docIdx'),
                templateId: domNode.getAttribute('templateId'),
                instanceId: domNode.getAttribute('instanceId'),
                isFocus: domNode.getAttribute('isFocus'),
                templateName: domNode.getAttribute('templateName'),
                templateColor: domNode.getAttribute('templateColor'),
                templateText: domNode.getAttribute('templateText'),
                templateType: domNode.getAttribute('templateType'),
                templateData: domNode.getAttribute('templateData')
            }
        }

        static _addRemovalButton(node) {
            const button = document.createElement('button');
            button.innerText = 'x';
            button.onclick = () => node.remove();
            button.contentEditable = 'false';
            node.appendChild(button);

            // Extra span forces the cursor to the end of the label, otherwise it appears inside the removal button
            const span = document.createElement('span');
            span.innerText = ' ';
            node.appendChild(span);
        }


    }

    Quill.register(TemplateSpanBlot);
}

function decodeTemplates(quill) {
    let regex = new RegExp("{{.*?}}", "");
    let qtext = quill.getText();
    let tcount = 0; 
    while (result = regex.exec(qtext)) {
        let encodedTemplateStr = result[0];
        let t = decodeTemplate(encodedTemplateStr);

        // NOTE embed blots have zero length in text (completely ignored) 
        // BUT take up a signle character in actual content so tcount keeps track of the
        // ignored character as they are added (SHEESH)
        let tsIdx = qtext.indexOf(encodedTemplateStr) + tcount;
        quill.deleteText(tsIdx, encodedTemplateStr.length);
        //delete tt.length;
        quill.insertEmbed(tsIdx, 'templatespan', t);
        qtext = quill.getText();
        tcount++;
    }
}
function decodeTemplate(encodedTemplateStr, sToken = '{{', eToken = '}}', sep = ',') {
    var tsIdx = encodedTemplateStr.indexOf(sToken);
    var teIdx = encodedTemplateStr.indexOf(eToken);

    if (tsIdx < 0 || teIdx < 0) {
        return null;
    }

    var templateSpan = encodedTemplateStr.substring(tsIdx + sToken.length, teIdx);
    var templateParts = templateSpan.split(sep);

    if (templateParts.length != 6) {
        return null;
    }
    return {
        templateId: parseInt(templateParts[0]),
        templateName: templateParts[1],
        templateColor: templateParts[2],
        templateText: templateParts[3],
        templateType: templateParts[4],
        templateData: templateParts[5],
        docIdx: tsIdx,
        length: teIdx - tsIdx + eToken.length
    };
}
function findNextEncodedTemplateToken(fIdx, sToken = '{{', eToken = '}}', sep = ',') {
    var text = getText().substring(fIdx);
    var tsIdx = text.indexOf(sToken);
    var teIdx = text.indexOf(eToken);

    if (tsIdx < 0 || teIdx < 0) {
        return null;
    }

    var templateSpan = text.substring(tsIdx + sToken.length, teIdx);
    var templateParts = templateSpan.split(sep);

    if (templateParts.length != 6) {
        return null;
    }
    return {
        templateId: parseInt(templateParts[0]),
        templateName: templateParts[1],
        templateColor: templateParts[2],
        templateText: templateParts[3],
        templateType: templateParts[4],
        templateData: templateParts[5],
        docIdx: tsIdx,
        length: teIdx - tsIdx + eToken.length
    };
}

function getTemplateEmbedStr(t, sToken = '{{', eToken = '}}', sep = ',') {
    var templateStr = [parseInt(t.templateId), t.templateName, t.templateColor, t.templateText, t.templateType, t.templateData].join(sep);
    var result = sToken + templateStr + eToken;
    return result;
}

function encodeTemplates() {
    // NOTE template text should be cleared from html before calling this
    var html = getHtml();
    var til = getTemplateInstances();
    for (var i = 0; i < til.length; i++) {
        var ti = til[i];
        var tin = ti.domNode;
        var tihtml = tin.outerHTML;
        var ties = getTemplateEmbedStr(ti);
        var newHtml = html.replace(tihtml, ties);
        if (newHtml == html) {
            console.log("Template not replaced");
            console.log("Template HTML: ");
            console.log(tihtml);
            console.log("Current Encoded HTML: ");
            console.log(html);
        }
        html = newHtml;
    }
    return html;
}


function getTemplates() {
    var domTemplates = document.getElementsByClassName("template_btn");
    var templates = [];
    for (var i = 0; i < domTemplates.length; i++) {
        var domTemplate = domTemplates[i];
        var template = {
            templateId: domTemplate.getAttribute('templateId'),
            templateName: domTemplate.getAttribute('templateName'),
            templateColor: domTemplate.getAttribute('templateColor'),
            templateType: domTemplate.getAttribute('templateType'),
            templateData: domTemplate.getAttribute('templateData'),
            docIdx: domTemplate.getAttribute('docIdx'),
            isFocus: domTemplate.getAttribute('isFocus'),
            instanceId: domTemplate.getAttribute('instanceId'),
            templateText: domTemplate.innerText,
            domNode: domTemplate
        }
        //templates.push(template);
        var curTemplateIdx = -1;
        for (var j = 0; j < templates.length; j++) {
            if (templates[j]['templateId'] == template['templateId']) {
                curTemplateIdx = j;
                break;
            }
        }
        if (curTemplateIdx >= 0) {
            if (Array.isArray(templates[curTemplateIdx].docIdx)) {
                templates[curTemplateIdx].docIdx.push(template.docIdx);
            } else {
                templates[curTemplateIdx].docIdx = [templates[curTemplateIdx].docIdx, template.docIdx];
            }
        } else {
            templates.push(template);
        }
    }
    return templates;
}

function shiftTemplates(fromDocIdx, byVal) {
    var tl = getTemplates();
    tl.forEach(function (t) {
        if (Array.isArray(t.docIdx)) {
            t.docIdx.forEach(function (tDocIdx) {
                if (tDocIdx >= fromDocIdx) {
                    setTemplateDocIdx(t, tDocIdx, parseInt(parseInt(tDocIdx) + parseInt(byVal)));
                }
            });
        } else {
            if (parseInt(t.docIdx) >= fromDocIdx) {
                setTemplateDocIdx(t, t.docIdx, parseInt(parseInt(t.docIdx) + parseInt(byVal)));
            }
        }
    });
}

function getTemplatesFromRange(range) {
    if (range == null || range.index == null) {
        console.log('invalid range: ' + range);
    }
    let tl = [];
    getTemplatesByDocOrder().forEach(function (tn) {
        let docIdx = tn.docIdx;
        if (docIdx >= range.index && docIdx <= range.index + range.length) {
            tl.push(tn);
        }
    });
    return tl;
}



function getTextWithEmbedTokens() {
    // for pasting NOT encoding for db
    var text = getText().split('');
    var otl = getTemplatesByDocOrder();
    var outText = '';
    var offset = 0;
    otl.forEach(function (ot) {
        offset += parseInt(ot.docIdx);
        var embedStr = getTemplateEmbedStr(ot);
        //text.splice(offset, 1);
        for (var i = 0; i < embedStr.length; i++) {
            text.splice(offset + i, 0, embedStr[i]);
        }
        offset += (embedStr.length);
    });
    return text.join('');
}


function getTemplatesByDocOrder() {
    var til = getTemplateInstances();
    til.sort((a, b) => (parseInt(a.docIdx) > parseInt(b.docIdx)) ? 1 : -1);
    return til;
}


function getUniqueTemplateInstanceId(tId) {
    let newInstanceId = 1;
    let isDup = true;
    while (isDup) {
        isDup = false;
        getTemplateInstances(tId).forEach(function (ti) {
            if (ti.instanceId == newInstanceId) {
                isDup = true;
            }
        });
        if (!isDup) {
            return newInstanceId;
        }
        newInstanceId++;
    }
    return newInstanceId;
}

function getTemplateInstances(tId, iId) {
    //var til = [];
    //getTemplates().forEach(function (t) {
    //    if (tId != null) {
    //        if (t.templateId != tId) {
    //            return;
    //        }
    //        if (iId != null && t.instanceId != iId) {
    //            return;
    //        }
    //    }

    //    if (Array.isArray(t.docIdx)) {
    //        t.docIdx.forEach(function (tDocIdx) {
    //            var ti = new Object();
    //            var ti = Object.assign(ti, t);
    //            ti.docIdx = tDocIdx;
    //            til.push(ti);
    //        });
    //    } else {
    //        til.push(t);
    //    }
    //});
    //if (tId != null && iId != null && til.length == 1) {
    //    return til[0];
    //}
    //return til;

    var domTemplates = document.getElementsByClassName("template_btn");
    var templates = [];
    for (var i = 0; i < domTemplates.length; i++) {
        var domTemplate = domTemplates[i];
        var template = {
            templateId: domTemplate.getAttribute('templateId'),
            templateName: domTemplate.getAttribute('templateName'),
            templateColor: domTemplate.getAttribute('templateColor'),
            templateType: domTemplate.getAttribute('templateType'),
            templateData: domTemplate.getAttribute('templateData'),
            docIdx: domTemplate.getAttribute('docIdx'),
            isFocus: domTemplate.getAttribute('isFocus'),
            instanceId: domTemplate.getAttribute('instanceId'),
            templateText: domTemplate.innerText,
            domNode: domTemplate
        }
        templates.push(template);
    } 
    return templates.sort((a, b) => (parseInt(a.docIdx) > parseInt(b.docIdx)) ? 1 : -1);
}

//function getTemplateOffset(_t, idx) {
//    var _tDocIdx = Array.isArray(_t.docIdx) ? _t.docIdx[idx] : _t.docIdx;
//    var text = getText();
//    var tl = getTemplates();
//    var offset = 0;
//    tl.forEach(function (t) {
//        var embedStr = '{{' + t.templateId + '}}';
//        if (Array.isArray(t.docIdx)) {
//            t.docIdx.forEach(function (tDocIdx) {
//                if (tDocIdx < _tDocIdx) {
//                    offset += getTemplateEmbedStr(t).length;
//                }
//            });
//        } else {
//            if (t.docIdx < _tDocIdx) {
//                offset += getTemplateEmbedStr(t).length;
//            }
//        }
//    });
//    return offset;
//}

//function getTemplateEmbedStr(t) {
//    var templateStr = '{{' + t.templateId + '}}';
//    return templateStr;
//}

function getTemplatesJson() {
    var val = JSON.stringify(getTemplates());
    return val;
}

function createTemplate(templateObjOrId, idx, len) {
    var templateObj = templateObjOrId;
    if (templateObj === parseInt(templateObj, 10)) {
        templateObj = getTemplateById(templateObjOrId);
    }
    var range = quill.getSelection(true);
    if (idx != null && length != null) {
        range = { index: idx, length: len };
    }

    var isNew = templateObj == null;
    var newTemplateObj = templateObj;
    if (isNew) {
        newTemplateObj = {
            templateId: getNewTemplateId(),
            templateColor: getRandomColor(),
            templateName: quill.getText(range.index, range.length).trim(),
            templateType: 'dynamic',
            templateData: ''
        };
    }
    newTemplateObj.docIdx = range.index;

    if (range.length == 0 && isNew) {
        newTemplateObj['templateName'] = getLowestAnonTemplateName();
    }

    quill.deleteText(range.index, range.length);//, Quill.sources.SILENT);
    if (Math.abs(parseInt(range.length)) > 0) {
        //shiftTemplates(range.index, -range.length);
    }
    //shiftTemplates(range.index, 1);
    quill.insertEmbed(range.index, "templatespan", newTemplateObj);//, Quill.sources.SILENT);
    var eofIdx = quill.getLength();
    if (range.index + newTemplateObj['templateName'].length >= eofIdx) {
        quill.insertText(range.index + 1, ' ');//, Quill.sources.SILENT);
    }
    quill.setSelection(range.index + 1, Quill.sources.API);

    selectTemplate(newTemplateObj['templateId'], true);

    console.log(getTemplates());

    showEditTemplateToolbar();

    hideTemplateToolbarContextMenu();

    return range.index;
}


function getLowestAnonTemplateName(anonPrefix = 'Template #') {
    var tl = getTemplates();
    var maxNum = 0;
    tl.forEach(function (t) {
        if (t.templateName.startsWith(anonPrefix)) {
            var anonNum = parseInt(t.templateName.substring(anonPrefix.length));
            maxNum = Math.max(maxNum, anonNum);
        }
    });
    return anonPrefix + (parseInt(maxNum) + 1);
}

function getNewTemplateId() {
    var tl = getTemplates();
    var minId = 0;
    tl.forEach(function (t) {
        minId = Math.min(minId, t.templateId);
    });
    return (parseInt(minId) - 1);
}

function clearTemplateSelection() {
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        t.style.border = "";
    }
}

function setTemplateType(templateId, templateTypeValue) {
    console.log('Template: ' + templateId + " selected type: " + templateTypeValue);

    var t = getTemplateById(templateId);
    t['templateType'] = templateTypeValue;
    document.getElementById("editTemplateTypeMenuSelector").value = t["templateType"];

    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var te = stl[i];
        let ctid = parseInt(te.getAttribute('templateid'));
        if (ctid == templateId) {
            te.setAttribute('templateType', templateTypeValue);
        }
    }

    var t = getTemplateById(selectedTemplateId);
    if (t['templateType'] == 'dynamic') {
        document.getElementById('templateDetailTextInputContainer').style.display = 'none';
    } else {
        document.getElementById('templateDetailTextInputContainer').style.display = 'inline-block';
        document.getElementById('templateDetailTextInput').value = t['templateData'];
        document.getElementById('templateDetailTextInput').addEventListener('input', onTemplateDetailChanged);
    }

    document.getElementById('templateColorBox').style.backgroundColor = t['templateColor'];
    document.getElementById('templateColorBox').addEventListener('click', onTemplateColorBoxContainerClick);

    //document.getElementById('templateNameAndColorContainer').style.display = 'inline-block';
    document.getElementById('templateNameTextInput').value = t['templateName'];
    document.getElementById('templateNameTextInput').addEventListener('input', onTemplateNameChanged);
}

function selectTemplate(templateId, fromDropDown, iId) {
    if (templateId == null || isRenamingTemplate()) {
        return;
    }

    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        if (t.getAttribute('templateid') == templateId) {
            if (isPastingTemplate) {
                $('#templateTextArea').placeholder = "Enter text for " + t.innerText;
                if (t.innerText != getTemplateById(templateId)['templateName']) {
                    $('#templateTextArea').val(t.innerText);
                } else {
                    $('#templateTextArea').val('');
                }
            }
            selectedTemplateId = templateId;
            t.style.border = "2px solid red";
            if (iId != null && t.getAttribute('instanceId') == iId) {
                t.style.border = "2px solid lightseagreen";
            }
        } else {
            t.style.border = "";
        }
    }
    if (fromDropDown == null || !fromDropDown) {
        if (isPastingTemplate) {
            //when user clicks a template this will adjust to drop dwon to the clicked element
            var items = document.getElementById('paste-template-custom-select').getElementsByTagName("div");
            for (var i = 0; i < items.length; i++) {
                if (items[i].getAttribute('optionId') != null && items[i].getAttribute('optionId') == selectedTemplateId) {
                    eventFire(items[i], 'click');
                    break;
                }
            }
        }
        //moved from quill embed constructor maybe causing selection issue on android
        wasLastClickOnTemplate = true;

        hideAllContextMenus();
        //showTemplateContextMenu(getFocusTemplateElement());
        showEditTemplateToolbar();
    }
}

function isTemplateSelected() {
    var selectionHtml = getSelectedHtml();
    return selectionHtml.includes('template_btn');
}

function getTemplateElements(tId, iId) {
    var tel = [];
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        let t = stl[i];
        let ctid = parseInt(t.getAttribute('templateId'));
        let ciid = parseInt(t.getAttribute('instanceId'));
        if (ctid == tId) {
            if (iId == null) {
                tel.push(t);
            } else if (ciid == iId) {
                return t;
            }
        }
    }
    return iId == null ? tel : null;
}

function focusTemplate(tn) {
    clearTemplateFocus();
    hideAllContextMenus();
    let tId = tn.getAttribute('templateId');
    let iId = parseInt(tn.getAttribute('instanceId'));
    let te = getTemplateElements(tId, iId);
    te.isFocus = true;
    te.setAttribute('isFocus', true);
    selectTemplate(tId, false, iId);
}

function clearTemplateFocus() {
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        t.isFocus = false;
    }
}

function getFocusTemplateElement() {
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        if (t.isFocus == true) {
            return t;
        }
    }
    return null;
}

function renameTemplateClick(tId, iId) {
    startSetTemplateName(tId, iId);
}

function changeTemplateColorClick(tId, iId) {
    hideTemplateContextMenu();

    if (isShowingTemplateColorPaletteMenu) {
        hideTemplateColorPaletteMenu();
    }
    let te = getTemplateElements(tId, iId);
    showTemplateColorPaletteMenu(te);
}

function onColorPaletteItemClick(tId, chex) {
    let tel = getTemplateElements(tId);

    for (var i = 0; i < tel.length; i++) {
        var te = tel[i];
        te.style.backgroundColor = chex;
        te.setAttribute('templateColor', chex);
    }
    document.getElementById('templateColorBox').style.backgroundColor = chex;
}

function deleteAllTemplateClick(tId) {
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        if (t.getAttribute('templateId') == tId) {
            t.parentNode.removeChild(t);
        }
    }
}

function eventFire(el, etype) {
    if (el.fireEvent) {
        el.fireEvent('on' + etype);
    } else {
        var evObj = document.createEvent('Events');
        evObj.initEvent(etype, true, false);
        el.dispatchEvent(evObj);
    }
}

function templateTextAreaChange() {
    var curText = $('#templateTextArea').val();
    setTemplateText(selectedTemplateId, curText);
}


function setTemplateName(tid, name) {
    var tl = getTemplateElements(tid);
    for (var i = 0; i < tl.length; i++) {
        var te = tl[i];
        if (parseInt(te.getAttribute('templateid')) == tid) {
            te.setAttribute('templateName', name);
            te.innerHTML = name;
        }
    }
}

function setTemplateDetailData(tid, detailData) {
    var tl = getTemplateElements(tid);
    for (var i = 0; i < tl.length; i++) {
        var te = tl[i];
        if (parseInt(te.getAttribute('templateid')) == tid) {
            te.setAttribute('templateData', detailData);
        }
    }
}

var origTemplateName = null;

function startSetTemplateName(tId, iId) {
    enterKeyBindings = disableKey('Enter');
    document.addEventListener('keypress', onLogKeyPress);
    document.addEventListener('keydown', onLogKeyDown);

    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        let te = stl[i];
        te.setAttribute('contentEditable', false);
        te.style.cursor = "pointer";
        te.style.caretColor = "transparent";

        let ctId = te.getAttribute('templateId');
        let ciId = te.getAttribute('instanceId');
        if (ctId != tId && ciId != iId) {
            continue;
        }

        te.setAttribute('contentEditable', true);
        te.style.cursor = "text";
        te.style.caretColor = "black";

        origTemplateName = te.innerText;
        selectText(te);
    }
    hideTemplateContextMenu();
}

function endSetTemplateName(wasCancel) {
    let fte = getFocusTemplateElement();
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        t.contentEditable = false;
        t.style.cursor = "pointer";
        t.style.caretColor = "transparent";
        if (t.getAttribute('templateId') == fte.getAttribute('templateId') && wasCancel == true) {
            t.setAttribute('templateName', origTemplateName);
            t.innerText = origTemplateName;
        }
    }

    reenableKey('Enter', enterKeyBindings);
    enterKeyBindings = null;

    origTemplateName = null;

    document.removeEventListener('keypress', onLogKeyPress);
    document.removeEventListener('keydown', onLogKeyDown);
}

function onTemplateTypeChanged(e) {
    setTemplateType(selectedTemplateId, this.value);
}

function onTemplateColorBoxContainerClick(e) {
    showTemplateColorPaletteMenu(selectedTemplateId);
}

function onTemplateNameChanged(e) {
    let newTemplateName = document.getElementById('templateNameTextInput').value;
    setTemplateName(selectedTemplateId, newTemplateName);
}

function onTemplateDetailChanged(e) {
    let newDetailData = document.getElementById('templateDetailTextInput').value;
    setTemplateName(selectedTemplateId, newDetailData);
}

function onLogKeyPress(e) {
    let fte = getFocusTemplateElement();
    setTemplateName(parseInt(fte.getAttribute('templateId')), parseInt(fte.getAttribute('instanceId')), fte.innerText);
}

function onLogKeyDown(e) {
    if (!isRenamingTemplate) {
        return;
    }

    if (e.code == "Escape") {
        endSetTemplateName(true);
        return;
    }
    if (e.code == "Enter") {
        e.preventDefault();
        endSetTemplateName(false);
        return;
    }

    //if(!TEMPLATE_VALID_CHARS.includes(String.fromCharCode(e.keyCode))) {
    //    e.preventDefault();
    //}
}

function disableKey(keyName) {
    var tempBindings = null;

    var keyboard = quill.getModule('keyboard');
    tempBindings = keyboard.bindings[keyName];
    keyboard.bindings[keyName] = null;

    return tempBindings;
}

function reenableKey(keyName, bindings) {
    var keyboard = quill.getModule('keyboard');
    keyboard.bindings[keyName] = bindings;
}

function selectText(elm) {
    if (document.body.createTextRange) {
        const range = document.body.createTextRange();
        range.moveToElementText(elm);
        range.select();
    } else if (window.getSelection) {
        const selection = window.getSelection();
        const range = document.createRange();
        range.selectNodeContents(elm);
        selection.removeAllRanges();
        selection.addRange(range);
    } else {
        console.warn("Could not select text in node: Unsupported browser.");
    }
}

function isRenamingTemplate() {
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        if (t.contentEditable == "true") {
            return true;
        }
    }
    return false;
}

function setTemplateDocIdx(t, oldIdx, newIdx) {
    var domTemplates = document.getElementsByClassName("template_btn");
    for (var i = 0; i < domTemplates.length; i++) {
        var domTemplate = domTemplates[i];
        var template = {
            templateId: domTemplate.getAttribute('templateId'),
            templateName: domTemplate.getAttribute('templateName'),
            templateColor: domTemplate.getAttribute('templateColor'),
            docIdx: domTemplate.getAttribute('docIdx'),
            templateText: domTemplate.innerText,

            //templateType: domTemplate.getAttribute('templateType'),
            //templateData: domTemplate.getAttribute('templateData'),
            //isFocus: domTemplate.getAttribute('isFocus'),
            //instanceId: domTemplate.getAttribute('instanceId'),
            //domNode: domTemplate
        }
        if (template.templateId == t.templateId && template.docIdx == oldIdx) {
            domTemplate.setAttribute('docIdx', newIdx);
            return;
        }
    }
}

function getTemplatesJson() {
    return JSON.stringify(getTemplates());
}

function getTemplateById(templateId) {
    var tl = getTemplates();
    for (var i = 0; i < tl.length; i++) {
        var t = tl[i];
        if (t['templateId'] == templateId) {
            return t;
        }
    }
    return null;
}

function showTemplateToolbarContextMenu(tb) {
    clickCount = 0;

    var rgtClickContextMenu = document.getElementById('templateToolbarMenu');
    var rgtClickContextMenuList = document.getElementById('templateToolbarMenuList');
    rgtClickContextMenuList.innerHTML = '';

    var tl = getTemplates();
    var listItemPrefix = 'templateListItem_';
    for (var i = 0; i < tl.length; i++) {
        var t = tl[i];
        var itemId = listItemPrefix + t.templateId;
        var templateItem = '<li><a onclick="createTemplate(' + t['templateId'] + ');">' +
            '<div style="display:inline-block;margin: 0 5px 0 0;width: 15px; height: 15px;background-color:' + t['templateColor'] + '"></div>' +
            '<span style="font-family:arial;">' + t['templateName'] + '</span></a></li>';
        rgtClickContextMenuList.innerHTML += templateItem;
    }

    rgtClickContextMenuList.innerHTML += '<li><a href="javascript:void(0);" onclick="createTemplate();">' +
        '<span style="margin: 0 5px 0 0;color:lightseagreen;font-size:20px">+</span>' +
        '<span style="font-family:arial;">Add Template</span></a></li>';
    const rect = tb.getBoundingClientRect();
    const x = rect.left + 20;
    const y = rect.bottom;
    rgtClickContextMenu.style.left = `${x}px`;
    rgtClickContextMenu.style.top = `${y}px`;
    rgtClickContextMenu.style.display = 'block';

    isShowingTemplateToolbarMenu = true;
}

function hideTemplateToolbarContextMenu() {
    var rgtClickContextMenu = document.getElementById('templateToolbarMenu');
    rgtClickContextMenu.style.display = 'none';

    isShowingTemplateToolbarMenu = false;
}

function showTemplateContextMenu(te) {
    if (te == null) {
        return;
    }
    if (isShowingTemplateContextMenu) {
        hideTemplateContextMenu();
    }
    clickCount = 0;

    var rgtClickContextMenu = document.getElementById('templateContextMenu');
    var rgtClickContextMenuList = document.getElementById('templateContextMenuList');
    rgtClickContextMenuList.innerHTML = '';

    let tId = te.getAttribute('templateId');
    let iId = te.getAttribute('instanceId');
    var context_items = [
        { icon_class: 'template_rename_icon', label: '   Rename', func: 'renameTemplateClick' },
        { icon_class: 'template_color_icon', label: '   Change Color', func: 'changeTemplateColorClick' },
        { icon_class: 'template_delete_icon', label: '   Delete All', func: 'deleteAllTemplateClick' }];

    context_items.forEach(function (ci) {
        var context_item = '<li><a href="javascript:void(0);" onclick="' + ci.func + '(' + tId + ',' + iId + ');">' +
            '<div class="template_context_item ' + ci.icon_class + '"></div> ' +
            '<span style="font-family:arial;">' + ci.label + '</span></a></li>';
        rgtClickContextMenuList.innerHTML += context_item;
    });

    const rect = te.getBoundingClientRect();
    const x = rect.left + 20;
    const y = rect.bottom;
    rgtClickContextMenu.style.left = `${x}px`;
    rgtClickContextMenu.style.top = `${y}px`;
    rgtClickContextMenu.style.display = 'block';

    isShowingTemplateContextMenu = true;
}

function hideTemplateContextMenu() {
    var rgtClickContextMenu = document.getElementById('templateContextMenu');
    rgtClickContextMenu.style.display = 'none';

    isShowingTemplateContextMenu = false;
}

function showTemplateColorPaletteMenu(tid) {
    var te = null;
    if (Array.isArray(getTemplateElements(tid))) {
        te = getTemplateElements(tid)[0];
    } else {
        te = getTemplateElements(tid);
    }

    if (isShowingTemplateColorPaletteMenu) {
        hideTemplateColorPaletteMenu();
    }
    clickCount = 0;

    let tId = te.getAttribute('templateId');
    var palette_item = {
        style_class: 'template_color_palette_item',
        func: 'onColorPaletteItemClick'
    };

    let paletteHtml = '<table>';
    for (var r = 0; r < 10; r++) {
        paletteHtml += '<tr>';
        for (var c = 0; c < 10; c++) {
            let c = getRandomColor().trim();
            let item = '<td><a href="javascript:void(0);" onclick="' + palette_item.func + '(' + tId + ',\'' + c + '\');">' +
                '<div class="' + palette_item.style_class + '" style="background-color: ' + c + '" ></div></a></td > ';
            paletteHtml += item;
        }
        paletteHtml += '</tr>';
    }

    var rgtClickColorPaletteMenu = document.getElementById('templateColorPaletteMenu');
    rgtClickColorPaletteMenu.innerHTML = paletteHtml;

    rgtClickColorPaletteMenu.style.display = 'block';
    var paletteMenuRect = rgtClickColorPaletteMenu.getBoundingClientRect();

    const editToolbarRect = document.getElementById('editTemplateToolbar').getBoundingClientRect(); //te.getBoundingClientRect();
    const colorBoxRect = document.getElementById('templateColorBox').getBoundingClientRect();
    const x = colorBoxRect.left;
    const y = editToolbarRect.top - paletteMenuRect.height;
    rgtClickColorPaletteMenu.style.left = `${x}px`;
    rgtClickColorPaletteMenu.style.top = `${y}px`;

    isShowingTemplateColorPaletteMenu = true;
}

function hideTemplateColorPaletteMenu() {
    var rgtClickColorPaletteMenu = document.getElementById('templateColorPaletteMenu');
    rgtClickColorPaletteMenu.style.display = 'none';

    isShowingTemplateColorPaletteMenu = false;
}

function hideAllContextMenus() {
    hideTemplateColorPaletteMenu();
    hideTemplateContextMenu();
    hideTemplateToolbarContextMenu();
}

function showEditTemplateToolbar() {
    clickCount = 0;
    isShowingEditTemplateToolbar = true;
    var ett = document.getElementById('editTemplateToolbar');
    ett.style.display = 'flex';

    $("#editTemplateToolbar").css("position", "absolute");
    $("#editTemplateToolbar").css("top", $(window).height() - $("#editTemplateToolbar").outerHeight());

    var t = getTemplateById(selectedTemplateId);

    setTemplateType(selectedTemplateId, t["templateType"]);

    document.getElementById('editTemplateTypeMenuSelector').addEventListener('change', onTemplateTypeChanged);
}

function hideEditTemplateToolbar() {
    isShowingEditTemplateToolbar = false;
    var ett = document.getElementById('editTemplateToolbar');
    ett.style.display = 'none';

    document.getElementById('editTemplateTypeMenuSelector').removeEventListener('change', onTemplateTypeChanged);
}

function selectedTemplateTypeChanged() {
    console.log("changed");
}

function showPasteTemplateToolbar() {
    isShowingPasteTemplateToolbar = true;
    var ptt = document.getElementById('pasteTemplateToolbar');
    ptt.style.display = 'inline-block';

    document.getElementById('paste-template-custom-select').innerHTML = '<select id="pasteTemplateToolbarMenuSelector"></select >';

    var templateMenuSelector = document.getElementById('pasteTemplateToolbarMenuSelector');
    templateMenuSelector.innerHTML = '';

    var tl = getTemplates();
    for (var i = 0; i < tl.length; i++) {
        var t = tl[i];
        var templateItem = '<option class="templateOption" value="' + t['templateId'] + '" onchange="selectTemplate(' + t['templateId'] + ');">' +
            t['templateName'] + '</option>';
        templateMenuSelector.innerHTML += templateItem;
    }

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

    createTemplateSelectorStyling(templateMenuSelector);

    //let tbb = isShowingToolbar ? $(".ql-toolbar").outerHeight() : 0;
    //moveToolbarTop(0);
    let wb = $(window).height();
    movePasteTemplateToolbarTop($(window).height() - $("#pasteTemplateToolbar").outerHeight());
    //moveEditorTop($("#pasteTemplateToolbar").outerHeight() + tbb);

    $('#templateTextArea').focus();
}

function hidePasteTemplateToolbar() {
    isShowingPasteTemplateToolbar = false;
    var ptt = document.getElementById('pasteTemplateToolbar');
    ptt.style.display = 'none';
    if (isShowingEditorToolbar) {
        moveEditorTop($(".ql-toolbar").outerHeight());
    } else {
        moveEditorTop(0);
    }
}

function gotoNextTemplate() {
    var curIdx = 0;
    for (var i = 0; i < tl.length; i++) {
        if (tl[i]['templateId'] == selectedTemplateId) {
            curIdx = i;
            break;
        }
    }
    var nextIdx = curIdx + 1;
    if (nextIdx >= tl.length) {
        nextIdx = 0;
    }
    selectTemplate(tl[nextIdx]['templateId']);
}

function gotoPrevTemplate() {
    var curIdx = 0;
    for (var i = 0; i < tl.length; i++) {
        if (tl[i]['templateId'] == selectedTemplateId) {
            curIdx = i;
            break;
        }
    }
    var prevIdx = curIdx - 1;
    if (prevIdx < 0) {
        prevIdx = tl.length - 1;
    }
    selectTemplate(tl[prevIdx]['templateId']);
}

function clearAllTemplateText() {
    var tl = getTemplates();
    for (var i = 0; i < tl.length; i++) {
        setTemplateText(tl[i]['templateId'], '');
    }
}

function setTemplateText(templateId, text) {
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        if (t.getAttribute('templateid') == templateId) {
            t.innerText = text;
            t.templateText = text;
        }
    }
}

function resetTemplates() {
    var tl = getTemplates();
    for (var i = 0; i < tl.length; i++) {
        var t = tl[i];

        t.templateText = '';
        t.isFocus = false;

        var tel = getTemplateElements(t.templateId);
        for (var j = 0; j < tel.length; j++) {
            let te = tel[j];
            te.innerHTML = t.templateName;
            te.setAttribute('templateText', '');
            te.setAttribute('isFocus', 'false');
        }
    }
}

function createTemplateSelectorStyling() {
    var tl = getTemplates();
    var x, i, j, selElmnt, a, b, c;
    x = document.getElementsByClassName("paste-template-custom-select");
    for (i = 0; i < x.length; i++) {
        selElmnt = x[i].getElementsByTagName("select")[0];
        if (selElmnt.options.length == 0) {
            continue;
        }
        var st = getTemplateById(selElmnt.options[selElmnt.selectedIndex].getAttribute('value'));
        a = document.createElement("DIV");
        a.setAttribute("class", "select-selected");
        a2 = document.createElement("SPAN");
        a2.setAttribute("class", "square-box");
        a2.setAttribute("style", "background-color: " + st['templateColor']);
        a.appendChild(a2);
        a.innerHTML += '<span style="margin: 0 10px 0 50px;">' + selElmnt.options[selElmnt.selectedIndex].innerHTML + '</span>';
        x[i].appendChild(a);
        b = document.createElement("DIV");
        b.setAttribute("class", "select-items select-hide");
        for (j = 0; j < selElmnt.length; j++) {
            var t = getTemplateById(selElmnt.options[j].getAttribute('value'));
            c = document.createElement("DIV");
            c.setAttribute('optionId', parseInt(t['templateId']));
            //c.innerHTML = selElmnt.options[j].innerHTML;
            d = document.createElement("SPAN");
            d.setAttribute("class", "square-box");
            d.setAttribute("style", "background-color: " + t['templateColor']);
            c.appendChild(d);
            c.innerHTML += selElmnt.options[j].innerHTML;
            c.addEventListener("click", onTemplateOptionClick);
            b.appendChild(c);
            if (j == selElmnt.selectedIndex) {
                c.classList.add("class", "same-as-selected");
            }
        }
        x[i].appendChild(b);
        a.addEventListener("click", function (e) {
            e.stopPropagation();
            closeAllSelect(this);
            this.nextSibling.classList.toggle("select-hide");
            this.classList.toggle("select-arrow-active");
        });
    }

    function closeAllSelect(elmnt) {
        var x, y, i, arrNo = [];
        x = document.getElementsByClassName("select-items");
        y = document.getElementsByClassName("select-selected");
        for (i = 0; i < y.length; i++) {
            if (elmnt == y[i]) {
                arrNo.push(i)
            } else {
                y[i].classList.remove("select-arrow-active");
            }
        }
        for (i = 0; i < x.length; i++) {
            if (arrNo.indexOf(i)) {
                x[i].classList.add("select-hide");
            }
        }
    }
    document.addEventListener("click", closeAllSelect);
}

function onTemplateOptionClick(e, target) {
    var targetDiv = e == null ? target : e.currentTarget;
    var tVal = parseInt(targetDiv.getAttribute('optionId'));
    var t = getTemplateById(tVal);
    var y, i, k, s, h;
    s = targetDiv.parentNode.parentNode.getElementsByTagName("select")[0];
    h = targetDiv.parentNode.previousSibling;
    for (i = 0; i < s.length; i++) {
        var sVal = parseInt(s.options[i].value);
        if (sVal == tVal) {
            s.selectedIndex = i;
            h.innerHTML = targetDiv.innerHTML.replace(t['templateName'], '<span style="margin: 0 10px 0 50px;">' + t['templateName'] + '</span>');
            y = targetDiv.parentNode.getElementsByClassName("same-as-selected");
            for (k = 0; k < y.length; k++) {
                y[k].removeAttribute("class");
            }
            targetDiv.setAttribute("class", "same-as-selected");
            break;
        }
    }

    selectTemplate(s.options[s.selectedIndex].value, true);
    h.click();
}

function isEmptyOrSpaces(str) {
    return str === null || str.match(/^ *$/) !== null;
}

// end templates