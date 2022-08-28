const TEMPLATE_VALID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890#-_ ";
const MIN_TEMPLATE_DRAG_DIST = 5;

var ENCODED_TEMPLATE_OPEN_TOKEN = "{t{";
var ENCODED_TEMPLATE_CLOSE_TOKEN = "}t}";
var ENCODED_TEMPLATE_REGEXP;

var MouseDownOnTemplatePos;
var IsMovingTemplate = false;

var templateTypesMenuOptions = [
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

var userDeletedTemplateGuids = [];

function registerTemplateSpan() {
    const Parchment = Quill.imports.parchment;

    class TemplateEmbedBlot extends Parchment.EmbedBlot {
        static blotName = 'template';
        static tagName = 'DIV';
        static className = 'ql-template-embed-blot';

        static create(value) {
            const node = super.create(value);

            if (value.domNode != null) {
                // creating existing instance
                value = getTemplateFromDomNode(value.domNode);
            }

            if (!IsMovingTemplate) {
                //ensure new template has unique instance guid
                value.templateInstanceGuid = generateGuid();
            }

            applyTemplateToDomNode(node, value);

            return node;
        }

        static formats(node) {
            return getTemplateFromDomNode(node); 
        }

        format(name, value) {
            super.format(name, value);
        }

        static value(domNode) {
            return getTemplateFromDomNode(domNode);
        }
    }

    Quill.register(TemplateEmbedBlot, true);
}

//#region Init

function initTemplates(usedTemplates, isPasting) {
    ENCODED_TEMPLATE_REGEXP = new RegExp(ENCODED_TEMPLATE_OPEN_TOKEN + ".*?" + ENCODED_TEMPLATE_CLOSE_TOKEN, "");

    if (usedTemplates != null) {
        decodeTemplates(usedTemplates);
    }

    Array
        .from(document.getElementsByClassName('resizable-textarea'))
        .forEach((rta) => {
            function updateToolbarSize() {
                updateEditTemplateToolbarPosition();
            }
            new ResizeObserver(updateToolbarSize).observe(rta);
        });
    

    initTemplateToolbarButton();

    document.getElementById('templateNameTextInput').onfocus = onTemplateNameTextAreaGotFocus;
    document.getElementById('templateNameTextInput').onblur = onTemplateNameTextAreaLostFocus;
    document.getElementById('templateDetailTextInput').onfocus = onTemplateDetailTextAreaGotFocus;
    document.getElementById('templateDetailTextInput').onblur = onTemplateDetailTextAreaLostFocus;

    if (isPasting) {
        // this SHOULD only happen when there are templates 
        // since browser extension will just return data...
        if (usedTemplates != null && usedTemplates.length > 0) {
            // TODO need set templateText by template type/data here
            showPasteTemplateToolbar();
        } else {
        }
    }
}

function initTemplateToolbarButton() {
    // Toolbar Template Button
    const templateToolbarButton = new QuillToolbarButton({
        //icon: '<img id="templateToolbarButton" style="height:20px" src="data:image/png;base64,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"></img>'//`<svg viewBox="0 0 18 18"> <path class="ql-stroke" d="M5,3V9a4.012,4.012,0,0,0,4,4H9a4.012,4.012,0,0,0,4-4V3"></path></svg>`
        icon: '<i id="templateToolbarButton" class="fa-solid fa-masks-theater"></i>'
    })

    templateToolbarButton.onClick = function (e) {
        var templateButton = document.getElementById('templateToolbarButton');
        showTemplateToolbarContextMenu(templateButton);

        //var tl = getAvailableTemplateDefinitions();
        //if (tl.length > 0) {
        //    showTemplateToolbarContextMenu(templateButton);
        //} else {
        //    createTemplate();
        //}

        event.stopPropagation(e);
    }
    templateToolbarButton.attach(quill);
}
//#endregion

//#region Convert To/From Blot/DomNode

function getTemplateFromDomNode(domNode) {
    if (domNode == null) {
        return null;
    }
    return {
        templateGuid: domNode.getAttribute('templateGuid'),
        templateInstanceGuid: domNode.getAttribute('templateInstanceGuid'),
        isFocus: domNode.getAttribute('isFocus'),
        templateName: domNode.getAttribute('templateName'),
        templateColor: domNode.getAttribute('templateColor'),
        templateText: domNode.getAttribute('templateText'),
        templateType: domNode.getAttribute('templateType'),
        templateData: domNode.getAttribute('templateData'),
        templateDeltaFormat: domNode.getAttribute('templateDeltaFormat'),
        templateHtmlFormat: domNode.getAttribute('templateHtmlFormat')
    }
}

function applyTemplateToDomNode(node, value) {
    if (node == null || value == null) {
        return node;
    }

    node.setAttribute('templateGuid', value.templateGuid);
    node.setAttribute('templateInstanceGuid', value.templateInstanceGuid);
    node.setAttribute('templateName', value.templateName);
    node.setAttribute('templateType', value.templateType);
    node.setAttribute('templateColor', value.templateColor);
    node.setAttribute('templateData', value.templateData);
    node.setAttribute('templateText', value.templateText);
    node.setAttribute('templateDeltaFormat', value.templateDeltaFormat);
    node.setAttribute('templateHtmlFormat', value.templateHtmlFormat);

    var textColor = isBright(value.templateColor) ? 'black' : 'white';
    node.setAttribute('style', 'background-color: ' + value.templateColor + ';color:' + textColor + ';');

    node.innerHTML = value.templateHtmlFormat;
    changeInnerText(node, node.innerText, value.templateName);

    // TODO instead of rejecting mouse down, template should be draggable

    //disable text selection
    //node.setAttribute('unselectable', 'on');
    //node.setAttribute('onselectstart', 'return false;');
    //node.setAttribute('onmousedown', 'return false;');

    node.contentEditable = 'false';
    node.setAttribute('isFocus', false);
    node.setAttribute("spellcheck", "false");
    node.setAttribute('class', 'ql-template-embed-blot');

    
    node.setAttribute('contenteditable', false);

    var templateDocIdxCache;

    function pointerDown(e) {
        node.onpointermove = pointerMove;
        node.setPointerCapture(e.pointerId);
        MouseDownOnTemplatePos = { x: e.clientX, y: e.clientY };
    }

    function pointerUp(e) {
        templateDocIdxCache = null;

        node.onpointermove = null;
        node.releasePointerCapture(e.pointerId);
        let curMousePos = { x: e.clientX, y: e.clientY };
        if (dist(MouseDownOnTemplatePos, curMousePos) < MIN_TEMPLATE_DRAG_DIST) {
            return;
        }

        let targetDocIdx = getEditorIndexFromPoint({ x: e.clientX, y: e.clientY }, false);

        if (targetDocIdx < 0) {
            return;
        }
        
        moveTemplate(node.getAttribute('templateGuid'), node.getAttribute('templateInstanceGuid'), targetDocIdx, e.ctrlKey);
    }

    function pointerMove(e) {
        let curMousePos = { x: e.clientX, y: e.clientY };
        if (dist(MouseDownOnTemplatePos, curMousePos) < MIN_TEMPLATE_DRAG_DIST) {
            return;
        }

        if (templateDocIdxCache == null) {
            templateDocIdxCache = getTemplateElementsWithDocIdx();
        }

        let docIdx = getEditorIndexFromPoint({ x: e.clientX, y: e.clientY },false, templateDocIdxCache);
        log('docIdx: ' + docIdx);

        if (docIdx < 0) {
            return;
        }
        if (!quill.hasFocus()) {
            quill.focus();
        }
        quill.setSelection(docIdx, 0);
    }

    node.onpointerdown = pointerDown;
    node.onpointerup = pointerUp;

    node.addEventListener('click', function (e) {
        focusTemplate(node.getAttribute('templateGuid'),false, node.getAttribute('templateInstanceGuid'));
    });

    //node.addEventListener('dragstart', function (e) {
    //    log('dragstart template');
    //});

    //initDragDrop();

    let observer = new MutationObserver(function (mutationsList, observer) {
        console.log(mutationsList);
    });

    observer.observe(node, { characterData: false, childList: true, attributes: false });
    return node;
}


//#endregion
function setTemplateProperty(tguid, propertyName, propertyValue) {
    getUsedTemplateInstances().forEach(function (ti) {
        if (ti.domNode.getAttribute('templateGuid') == tguid) {
            ti.domNode.setAttribute(propertyName, propertyValue);
        }
    });
}

function getTemplateProperty(tguid, propertyName) {
    let t = getTemplateDefByGuid(tguid);
    if (t == null) {
        return null;
    }
    return t.domNode.getAttribute(propertyName);
}

function isTemplateNode(node) {
    return node.getAttribute('templateGuid') != null;
}

//#endregion

//#region Encode/Decode

function getEncodedTemplateGuids() {
    // this returns all parsed templates to html extension on load BEFORE init
    let etgl = [];

    let tcount = 0;
    while (result = ENCODED_TEMPLATE_REGEXP.exec(itemData)) {
        let encodedTemplateStr = result[0];
        let tguid = parseEncodedTemplateGuid(encodedTemplateStr);

        let isDefined = etgl.some(function (etg) {
            return etg == tguid
        });
        if (!isDefined) {
            etgl.push(tguid);
        }
    }
    return etgl;
}

function getDecodedTemplateGuids() {
    //this returns all load template blots distinct guid's
    let dtgl = [];

    getUsedTemplateInstances().forEach(function (cit) {
        if (!dtgl.includes(cit.templateGuid)) {
            dtgl.push(cit.templateGuid);
        }        
    });
    //setComOutput(JSON.stringify(dtgl));

    return dtgl;
}

function removeTemplatesByGuid(tguid) {
    getUsedTemplateInstances().forEach(function (cit) {
        if (cit.templateGuid == tguid) {
            let docIdx = getTemplateDocIdx(cit.templateInstanceGuid);
            if (docIdx >= 0) {
                quill.deleteText(docIdx, 1);
			}
		}
    });
}

function decodeTemplates(templateDefs) {
    //templateDefs is all FOUND template defs in db from pre init getEncodedTempalteGuids
    //when def is not found that means user clicked delete template which removes it from db
    //and deletes all current instances in active document so when not provided replace 
    //encoded template w/ empty character

    let qtext = quill.getText();
    let tcount = 0; 
    while (result = ENCODED_TEMPLATE_REGEXP.exec(qtext)) {
        let encodedTemplateStr = result[0];
        let tguid = parseEncodedTemplateGuid(encodedTemplateStr);

        // NOTE embed blots have zero length in text (completely ignored) 
        // BUT take up a signle character in actual content so tcount keeps track of the
        // ignored character as they are added (SHEESH)
        let tsIdx = qtext.indexOf(encodedTemplateStr) + tcount;

        // remove ' ' padding from both ends of template or extra ' ' will be added time it loads
        quill.deleteText(tsIdx-1, encodedTemplateStr.length + 1);

        let t = templateDefs.forEach(function (td) {
            if (td.templateGuid == tguid) {
                return td;
            }
        });

        if (t != null) {
            quill.insertEmbed(tsIdx, 'template', t);
            tcount++;
        } else {
            log('template def \'' + tguid + '\' not found so omitting from editor');
        }
        qtext = quill.getText();
    }
}

function parseEncodedTemplateGuid(encodedTemplateStr, sToken = ENCODED_TEMPLATE_OPEN_TOKEN, eToken = ENCODED_TEMPLATE_CLOSE_TOKEN) {
    var tsIdx = encodedTemplateStr.indexOf(sToken);
    var teIdx = encodedTemplateStr.indexOf(eToken);

    if (tsIdx < 0 || teIdx < 0) {
        return null;
    }

    return encodedTemplateStr.substring(tsIdx + sToken.length, teIdx);
}

function getTemplateEmbedStr(t, sToken = ENCODED_TEMPLATE_OPEN_TOKEN, eToken = ENCODED_TEMPLATE_CLOSE_TOKEN) {
    var result = sToken + t.domNode.getAttribute('templateGuid') + eToken;
    return result;
}

function encodeTemplates() {
    // NOTE template text should be cleared from html before calling this
    var html = getHtml();
    var til = getUsedTemplateInstances();
    for (var i = 0; i < til.length; i++) {
        var ti = til[i];
        var tin = ti.domNode;
        var tihtml = tin.outerHTML;
        var ties = getTemplateEmbedStr(ti);
        html = html.replaceAll(tihtml, ties);
    }
    return html;
}

//#endregion

function clearTemplateFocus() {
    let tel = getTemplateElements();
    tel.forEach(te => {
        te.setAttribute('isFocus', false);

        te.classList.remove('ql-template-embed-blot-focus');
        te.classList.remove('ql-template-embed-blot-focus-not-instance');
    });
}

function isTemplateFocused() {
    return getFocusTemplate() != null;
}

function getFocusTemplate() {
    let til = getUsedTemplateInstances();
    let result = til.find(x => x.domNode.getAttribute('isFocus') == "true");
    return result;
}

function getFocusTemplateGuid() {
    let ft = getFocusTemplate();
    if (ft == null) {
        return null;
    }
    return ft.domNode.getAttribute('templateGuid');
}

function getTemplatesFromRange(range) {
    if (range == null || range.index == null) {
        log('invalid range: ' + range);
    }
    let tl = [];
    let tel = getTemplateElements();
    tel.forEach(function (te) {
        let t = getTemplateFromDomNode(te);
        let docIdx = getTemplateDocIdx(t.templateInstanceGuid);
        if (docIdx == range.index) {
            tl.push(te);
        } else if (docIdx > range.index && docIdx < range.index + range.length) {
            tl.push(te);
        }
    });
    return tl;
}


//#region Paste 

function getTextWithEmbedTokens() {
    // for pasting NOT encoding for db
    var text = getText().split('');
    var otl = getUsedTemplateInstances();
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

//#endregion

function getUsedTemplateDefinitions() {
    let tdl = [];
    getUsedTemplateInstances().forEach(function (ti) {
        let isDefined = tdl.find(x => x.domNode.getAttribute('templateGuid') == ti.domNode.getAttribute('templateGuid')) != null;
        if (!isDefined) {
            tdl.push(ti);
        }
    });
    return tdl;
}

function getAvailableTemplateDefinitions() {
    if (availableTemplates == null || availableTemplates.length == 0) {
        if (typeof getAllTemplatesFromDb === 'function') {
            let allTemplatesJsonStr = getAllTemplatesFromDb();
            log('templates from db:');
            log(allTemplatesJsonStr);
            availableTemplates = JSON.parse(allTemplatesJsonStr);
		}

        if (availableTemplates == null || availableTemplates.length == 0) {
            return getUsedTemplateDefinitions().map(x => getTemplateFromDomNode(x.domNode));
		}
    }

    let utdl = getUsedTemplateDefinitions();
    let allMergedTemplates = [];
    for (var i = 0; i < availableTemplates.length; i++) {
        let atd = availableTemplates[i];
        let isUsed = false;
        //loop through all templates from master collection
        for (var j = 0; j < utdl.length; j++) {
            //check if this editor is using one available in master
            let utd = utdl[j];
            if (utd.domNode.getAttribute('templateGuid') == atd.templateGuid) {
                //if this editor is using a template already known from master use this editor's version
                allMergedTemplates.push(getTemplateFromDomNode(utd.domNode));
                isUsed = true;
                break;
            }
        }
        if (!isUsed) {
            allMergedTemplates.push(atd);
        }
    }

    // now add any new templates in this editor that weren't in master list
    for (var i = 0; i < utdl.length; i++) {
        let utd = utdl[i];
        let wasFound = allMergedTemplates.find(x => x.templateGuid == utd.domNode.getAttribute('templateGuid')) != null;
        if (!wasFound) {
            allMergedTemplates.push(getTemplateFromDomNode(utd.domNode));
        }
    }
    return allMergedTemplates;
}

function getTemplateDocIdx(tiguid) {
    let docLength = quill.getLength();
    for (var i = 0; i < docLength; i++) {
        let curDelta = quill.getContents(i, 1);
        if (curDelta.ops.length > 0 &&
            curDelta.ops[0].hasOwnProperty('insert') &&
            curDelta.ops[0].insert.hasOwnProperty('template')) {
            let curTemplate = curDelta.ops[0].insert.template;
            if (curTemplate.templateInstanceGuid == tiguid) {
                return i;
            }
        }
    }
    return -1;
}

function getTemplateElementsWithDocIdx() {
    let tewdil = [];
    getTemplateElements().forEach(te => {
        let teDocIdx = getTemplateDocIdx(te.getAttribute('templateInstanceGuid'));
        tewdil.push({ teDocIdx, te });
    });
    return tewdil;
}

function getUsedTemplateInstances() {
    var domTemplates = document.querySelectorAll('.ql-template-embed-blot');
    var templates = [];
    for (var i = 0; i < domTemplates.length; i++) {
        var domTemplate = domTemplates[i];
        var templateBlot = Quill.find(domTemplate);
        if (templateBlot != null) {
            templates.push(templateBlot);
        }
    }
    return templates.sort((a, b) => (
        getTemplateDocIdx(a.domNode.getAttribute('templateInstanceGuid')) < 
        getTemplateDocIdx(b.domNode.getAttribute('templateInstanceGuid')) ? -1 : 1));
}

function createTemplate(templateObjOrId,newTemplateType) {
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
        let selectionInnerHtml = '';
        let shtmlStr = getSelectedHtml(1);
        if (shtmlStr != null && shtmlStr.length > 0) {
            let shtml = domParser.parseFromString(shtmlStr, 'text/html');
            let pc = shtml.getElementsByTagName('p');
            if (pc != null && pc.length > 0) {
                let p = pc[0];
                //clear text from selection
                selectionInnerHtml = p.innerHTML;
            }
        }

        let newTemplateName = '';
        if (range.length == 0) {
            newTemplateName = getLowestAnonTemplateName();
        } else {
            newTemplateName = quill.getText(range.index, range.length).trim();
        }
        if (selectionInnerHtml == '<br>') {
            //this occurs when selection.length == 0
            selectionInnerHtml = newTemplateName;
        }
        let formatInfo = quill.getFormat(range.index, 1);
        newTemplateObj = {
            templateGuid: generateGuid(),
            templateColor: getRandomColor(),
            templateName: newTemplateName,
            templateType: newTemplateType,
            templateData: '',
            templateDeltaFormat: JSON.stringify(formatInfo),
            templateHtmlFormat: selectionInnerHtml
        };
    }

    insertTemplate(range, newTemplateObj);

    if (isNew) {
        showEditTemplateToolbar();
    }
    

    hideTemplateToolbarContextMenu();

    return newTemplateObj;
}

function insertTemplate(range, t) {
    quill.deleteText(range.index, range.length);
    quill.insertEmbed(range.index, "template", t, Quill.sources.USER);
    focusTemplate(t.templateGuid, true);
}

function moveTemplate(tguid, tiguid, nidx, isCopy) {
    IsMovingTemplate = true;

    let tidx = getTemplateDocIdx(tiguid);

    let t = getTemplateInstance(tguid, tiguid);

    if (isCopy) {
        t.templateInstanceGuid = generateGuid();
        t = createTemplate(t);
    } else {
        if (tidx < nidx) {
            //removing template decreases doc size by 3 characters
            nidx -= 3;
        }
        //set tidx to space behind and delete template and spaces from that index
        quill.deleteText(tidx - 1, 3);

        insertTemplate({ index: nidx, length: 0 }, t);
    }

    IsMovingTemplate = false;
    focusTemplate(t, true);
}

function refreshTemplatesAfterSelectionChange() {
    let range = quill.getSelection();

    getTemplateElements().forEach(telm => { telm.classList.remove('ql-template-embed-blot-at-insert') });

    let tl = getTemplatesFromRange(range);
    if (tl.length > 0) {

        tl.forEach(telm => { telm.classList.add('ql-template-embed-blot-at-insert') });
    }
}

function getLowestAnonTemplateName(anonPrefix = 'Template #') {
    var tl = getUsedTemplateDefinitions();
    var maxNum = 0;
    tl.forEach(function (t) {
        if (t.domNode.getAttribute('templateName').startsWith(anonPrefix)) {
            var anonNum = parseInt(t.domNode.getAttribute('templateName').substring(anonPrefix.length));
            maxNum = Math.max(maxNum, anonNum);
        }
    });
    return anonPrefix + (parseInt(maxNum) + 1);
}

function focusTemplate(tguid, fromDropDown, tiguid) {
    if (tguid == null) {
        return;
    }
    clearTemplateFocus();
    hideAllTemplateContextMenus();

    var tel = getTemplateElements();
    for (var i = 0; i < tel.length; i++) {
        var te = tel[i];
        if (te.getAttribute('templateGuid') == tguid) {
            if (isPastingTemplate) {
                $('#templateTextArea').placeholder = "Enter text for " + te.innerText;
                if (te.innerText != getTemplateDefByGuid(tguid)['templateName']) {
                    $('#templateTextArea').val(te.innerText);
                } else {
                    $('#templateTextArea').val('');
                }
            }
            te.setAttribute('isFocus', true);

            if (tiguid != null && te.getAttribute('templateInstanceGuid') == tiguid) {
                te.classList.add('ql-template-embed-blot-focus');
                let teBlot = Quill.find(te);
                let teIdx = quill.getIndex(teBlot);
                quill.setSelection(teIdx,1);
            } else {
                te.classList.add('ql-template-embed-blot-focus-not-instance');
            }
        } else {
            te.setAttribute('isFocus', false);
            te.classList.remove('ql-template-embed-blot-focus');
            te.classList.remove('ql-template-embed-blot-focus-not-instance');
        }
    }

    if (fromDropDown == null || !fromDropDown) {
        if (isPastingTemplate) {
            //when user clicks a template this will adjust to drop dwon to the clicked element
            var items = document.getElementById('paste-template-custom-select').getElementsByTagName("div");
            for (var i = 0; i < items.length; i++) {
                if (items[i].getAttribute('optionId') != null && items[i].getAttribute('optionId') == getFocusTemplateGuid()) {
                    eventFire(items[i], 'click');
                    break;
                }
            }
        }
        //moved from quill embed constructor maybe causing selection issue on android
        
        hideAllTemplateContextMenus();
        showEditTemplateToolbar();
    }

}

function getTemplateElements(tguid, iguid) {
    var tel = [];
    var stl = document.getElementsByClassName("ql-template-embed-blot");
    if (!tguid && !iguid) {
        return Array.from(stl);
    }
    for (var i = 0; i < stl.length; i++) {
        let t = stl[i];
        let ctguid = t.getAttribute('templateGuid');
        let ctiguid = t.getAttribute('templateInstanceGuid');
        if (ctguid == tguid || !tguid) {
            if (iguid == null) {
                tel.push(t);
            } else if (ctiguid == iguid) {
                return t;
            }
        }
    }
    return iguid == null ? tel : null;
}

function onColorPaletteItemClick(chex) {
    let tel = getTemplateElements(getFocusTemplateGuid());

    for (var i = 0; i < tel.length; i++) {
        var te = tel[i];
        te.style.backgroundColor = chex;
        te.setAttribute('templateColor', chex);

        te.style.color = isBright(chex) ? 'black' : 'white';
    }
    document.getElementById('templateColorBox').style.backgroundColor = chex;
    hideAllTemplateContextMenus();
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

function getTemplateDefByGuid(tguid) {
    return getUsedTemplateDefinitions().find(x=>x.domNode.getAttribute('templateGuid') == tguid);
}

function getTemplateInstance(tguid, tiguid) {
    let telm = document.querySelector('[templateGuid="' + tguid + '"],[templateInstanceGuid="' + tiguid + '"]');
    if (telm == null) {
        return null;
    }
    return getTemplateFromDomNode(telm);
}

function hideAllTemplateContextMenus() {
    hideTemplateColorPaletteMenu();
    hideTemplateToolbarContextMenu();
}

function resetTemplates() {
    var til = getUsedTemplateInstances();
    for (var i = 0; i < til.length; i++) {
        var ti = til[i];

        ti.domNode.setAttribute('templateText', '');
        ti.domNode.setAttribute('isFocus', 'false');
        ti.domNode.innerHTML = ti.domNode.getAttribute('templateName');
    }
}

function padTemplate(tiguid,delta) {
    let teDocIdx = getTemplateDocIdx(tiguid);
    if (teDocIdx < 0) {
        throw 'tiguid: ' + tiguid + ' cannot have docIdx: ' + teDocIdx;
    }
    let needsPre = false;
    let needsPost = false;

    if (isDocIdxLineStart(teDocIdx)) {
        needsPre = true;
    } else {
        let preText = quill.getText(teDocIdx - 1, 1);
        needsPre = preText != ' ';
    }
    if (isDocIdxLineEnd(teDocIdx)) {
        needsPost = true;
    } else {
        let postText = quill.getText(teDocIdx + 1, 1);
        needsPost = postText != ' ';
    }

    if (needsPre) {
        //IgnoreNextTextChange = true;
        //IgnoreNextSelectionChange = true;
        quill.insertText(teDocIdx, ' ');
        teDocIdx++;
    }
    if (needsPost) {
        //IgnoreNextTextChange = true;
        //IgnoreNextSelectionChange = true;
        quill.insertText(teDocIdx + 1, ' ');
    }
}

function updateTemplatesAfterTextChanged(delta, oldDelta, source) {
    let til = getTemplateElements();
    let idx = 0;
    for (var i = 0; i < delta.ops.length; i++) {
        let op = delta.ops[i];

        if (op.retain) {
            idx += op.retain;
        }
        if (op.insert) {
            idx += op.insert.length;
        }
        if (op.delete) {

            for (var j = 0; j < til.length; j++) {
                let ti = til[j];
                let tiDocIdx = getTemplateDocIdx(ti.getAttribute('templateInstanceGuid'));
                if (idx - op.delete == tiDocIdx) {
                    // deleting post pad so delete template and pre pad
                    IgnoreNextTextChange = true;
                    quill.deleteText(tiDocIdx - 1, 2);
                }
            }
        }
    }
    til = getTemplateElements();
    for (var i = 0; i < til.length; i++) {
        let ti = til[i];
        padTemplate(ti.getAttribute('templateInstanceGuid'));
    }
}

function updateTemplatesAfterSelectionChanged(range, oldRange, source) {
    if (!range) {
        return;
    }
    /*
    // cases
    // 1. user clicks on template idx
    // 2. user clicks on pre pad idx
    // 3. user clicks on post pad idx
    // 4. user arrows right onto pre pad idx
    // 5. user arrows left onto pre pad idx
    // 6. user arrows right onto post pad idx
    // 7. user arrows left onto post pad idx
    // 8. user dragging selection from right onto post pad idx
    // 9. user dragging selection from left onto pre pad idx


    // results
    // 1. do nothing
    // 2. selection moved +1 to template idx
    // 3. selection is moved -1 to template idx
    // 4. #2
    // 5. do nothing
    // 6. do nothing
    // 7.
    */
    let tel = getTemplateElements();

    for (var i = 0; i < tel.length; i++) {
        let nrange = range;
        let te = tel[i];
        let tDocIdx = getTemplateDocIdx(te.getAttribute('templateInstanceGuid'));
        if (range.index == tDocIdx + 1) {
            //if start/caret idx is at post pad space
            if (oldRange.index == range.index + 1) {
                //caret moving left
                nrange.index = tDocIdx;
            } else if (oldRange.index == tDocIdx) {
                //caret moving right
                nrange.index++;
            } else {
                nrange = null;
            }
            //return;
        } else if (range.index == tDocIdx - 1) {
            //if start/caret idx is at pre pad space
            if (oldRange.index == tDocIdx) {
                //caret moving left
                nrange.index--;
            } else if (oldRange.index + 1 == range.index) {
                //caret moving right
                nrange.index++;
            } else {
                nrange = null;
            }
            //return;
        } else if (range.index == tDocIdx) {
            nrange = range;
        }
        if (nrange) {
            //IgnoreNextSelectionChange = true;
            quill.setSelection(nrange);
            //range = nrange;
            refreshTemplatesAfterSelectionChange();
            break;
        }
    }

}

function getTemplateToolbarHeight() {
    if (!isShowingEditTemplateToolbar() && !isShowingPasteTemplateToolbar()) {
        return 0;
    }
    if (isShowingEditTemplateToolbar()) {
        return parseInt($("#editTemplateToolbar").outerHeight());
    } else if (isShowingPasteTemplateToolbar()) {
        return parseInt($("#pasteTemplateToolbar").outerHeight());
    }
    return 0;
}
