const TEMPLATE_VALID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890#-_ ";
const MIN_TEMPLATE_DRAG_DIST = 5;

var availableTemplates = null;

var ENCODED_TEMPLATE_OPEN_TOKEN = "{t{";
var ENCODED_TEMPLATE_CLOSE_TOKEN = "}t}";
var ENCODED_TEMPLATE_REGEXP;

var IsMovingTemplate = false;

var IsAnyTemplateBgTemporary = false;
var DragAnchorDocIdxWhenTemplateWithinSelection = -1;

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



//#region Init

function initTemplates(usedTemplates, isPasting) {
    ENCODED_TEMPLATE_REGEXP = new RegExp(ENCODED_TEMPLATE_OPEN_TOKEN + ".*?" + ENCODED_TEMPLATE_CLOSE_TOKEN, "");
    // scan doc for templates even if none provided

    if (usedTemplates != null) {
        decodeTemplates(usedTemplates);
    }
    //let telml = getTemplateElements();
    //for (var i = 0; i < telml.length; i++) {
    //    let t_elm = telml[i];
    //    let t_blot = getTemplateFromDomNode(t_elm);
    //    applyTemplateToDomNode(t_elm, t_blot);
    //    //t_blot.domNode = t_elm;
    //    //TemplateEmbedBlot.create(t_blot);
    //}

    let resizers = Array.from(document.getElementsByClassName('resizable-textarea'));
    for (var i = 0; i < resizers.length; i++) {
        let rta = resizers[i];

        new ResizeObserver(() => {
            updateEditTemplateToolbarPosition();
		}).observe(rta);
	}
    

    initTemplateToolbarButton();

    document.getElementById('templateNameTextInput').addEventListener('focus',onTemplateNameTextAreaGotFocus);
    document.getElementById('templateNameTextInput').addEventListener('blur',onTemplateNameTextAreaLostFocus);
    document.getElementById('templateNameTextInput').addEventListener('keydown',onTemplateNameTextArea_keydown);
    document.getElementById('templateNameTextInput').addEventListener('keyup',onTemplateNameTextArea_keyup);

    document.getElementById('templateDetailTextInput').addEventListener('focus',onTemplateDetailTextAreaGotFocus);
    document.getElementById('templateDetailTextInput').addEventListener('blur',onTemplateDetailTextAreaLostFocus);


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

//#endregion

//#region Convert To/From Blot/DomNode




//#endregion
function setTemplateProperty(tguid, propertyName, propertyValue) {
    let til = getUsedTemplateInstances();
	for (var i = 0; i < til.length; i++) {
        let ti = til[i];
        if (ti.domNode.getAttribute('templateGuid') == tguid) {
            ti.domNode.setAttribute(propertyName, propertyValue);
        }
	}
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

    let util = getUsedTemplateInstances();
    for (var i = 0; i < util.length; i++) {
        let cit = util[i];
        if (!dtgl.includes(cit.templateGuid)) {
            dtgl.push(cit.templateGuid);
        }        
    }

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

    let qtext = getText();
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
        qtext = getText();
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


function getEncodedHtml() {
    resetTemplates();
    var result = encodeTemplates();
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
	for (var i = 0; i < tel.length; i++) {
        let te = tel[i];
        te.setAttribute('isFocus', false);

        te.classList.remove(TemplateFocusInstanceClass);
        te.classList.remove(TemplateFocusNotInstanceClass);
    }
}

function isTemplateFocused() {
    return getFocusTemplateElement() != null;
}

function getFocusTemplateElement() {
    let fallback_telm = null;
    let telml = getTemplateElements();
    for (var i = 0; i < telml.length; i++) {
        let telm = telml[i];
        if (telm.classList.contains(TemplateFocusInstanceClass)) {
            return telm;
        }
        if (parseBool(telm.getAttribute('isFocus')) == true) {
            fallback_telm = telm;
		}
    }
    return fallback_telm;

    //let til = getUsedTemplateInstances();
    //let result = til.find(x => x.domNode.getAttribute('isFocus') == true);
    //return result;
}

function getFocusTemplateGuid() {
    let ft = getFocusTemplateElement();
    if (ft == null) {
        return null;
    }
    return ft.getAttribute('templateGuid');
}

function getTemplateElementsInRange(range) {
    if (range == null || range.index == null) {
        log('invalid range: ' + range);
    }
    let tl = [];
    let tel = getTemplateElements();
    tel.forEach(function (te) {
        let te_blot = Quill.find(te);
        if (!te_blot) {
            return;
		}
        let te_doc_idx = te_blot.offset(quill.scroll);
        if (te_doc_idx >= range.index && te_doc_idx <= range.index + range.length) {
            // template is range
            tl.push(te);
		}
        //let t = getTemplateFromDomNode(te);
        //let docIdx = getTemplateDocIdx(t.templateInstanceGuid);
        //if (docIdx == range.index) {
        //    tl.push(te);
        //} else if (docIdx > range.index && docIdx < range.index + range.length) {
        //    tl.push(te);
        //}
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
        availableTemplates = getAllTemplatesFromDb_get();

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

function getTemplateAtDocIdx(docIdx) {
    let result = quill.getLeaf(docIdx);
    if (!result || result.length < 2) {
        return null;
    }
    let blot = result[0];
    if (blot && blot.domNode && blot.domNode.hasAttribute != null && blot.domNode.hasAttribute('templateGuid')) {
        return getTemplateFromDomNode(blot.domNode);
    }
    return null;
}

function isTemplateAtDocIdx(docIdx) {
    return getTemplateAtDocIdx(docIdx) != null;
}

function getTemplateElementsWithDocIdx() {
    let tewdil = [];
    getTemplateElements().forEach(te => {
        let teDocIdx = getTemplateDocIdx(te.getAttribute('templateInstanceGuid'));
        tewdil.push({ teDocIdx, te });
    });
    return tewdil;
}

function getTemplatePlainText(t) {
    let template_text = '';
    if (IsPastingTemplate) {
        let t_elm = getTemplateElements(null, t['templateInstanceGuid']);
        template_text = t_elm.innerText;
    } else {
        template_text = '{t{' + t['templateGuid'] + ',' + t['templateInstanceGuid'] + '}t}';
    }
    return template_text;
}

function getRangeTextWithTemplateText(range) {
    let text = quill.getText(range.index, range.length);
    let out_text = text;
    let out_idx = 0;
    // to seamlessly use templates check range and insert tguid or or display value for templates
    for (var i = 0; i <= range.length; i++) {
        let doc_idx = range.index + i;
        let t_at_doc_idx = getTemplateAtDocIdx(doc_idx);
        if (t_at_doc_idx == null) {
            out_idx++;
            continue;
        }

        let pre_text = substringByLength(out_text, 0, out_idx);
        // remove leading pad
        pre_text = substringByLength(pre_text, 0, pre_text.length - 1);
        let post_text = substringByLength(text, i, text.length - i);
        // remove trailing pad
        post_text = substringByLength(post_text, 0, post_text.length - 1);
        let template_text = getTemplatePlainText(t_at_doc_idx);
        out_text = pre_text + template_text + post_text;
        // offset out by template and adjust for 2 removed pad spaces
        out_idx += template_text.length - 2;
    }
    return out_text;
}
function getUsedTemplateInstances() {
    //var domTemplates = document.querySelectorAll('.ql-template-embed-blot');
    var domTemplates = document.querySelectorAll('.' + TemplateEmbedClass);
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
            newTemplateName = getText(range).trim();
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

    insertTemplate(range, newTemplateObj,isNew);

    if (isNew) {
        showEditTemplateToolbar();
    }
    

    hideTemplateToolbarContextMenu();

    return newTemplateObj;
}

function insertTemplate(range, t, isNew) {
    IgnoreNextTextChange = true;
    quill.deleteText(range.index, range.length);
    quill.insertEmbed(range.index, "template", t, Quill.sources.USER);

    focusTemplate(t.templateGuid, true);
}

function moveTemplate(tiguid, nidx, isCopy) {
    IsMovingTemplate = true;

    let tidx = getTemplateDocIdx(tiguid);

    let t = getTemplateDefByInstanceGuid(tiguid);

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


function coereceSelectionWithTemplatePadding(range, oldRange, source) {
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
    // 1. do nothing (disable add template button)
    // 2. selection moved +1 to template idx
    // 3. selection is moved -1 to template idx
    // 4. #2
    // 5. do nothing
    // 6. do nothing
    // 7.
    */

    let isAddTemplateValid = true;
    let tel = getTemplateElements();

    for (var i = 0; i < tel.length; i++) {
        let new_range = range;
        let te = tel[i];
        let tDocIdx = getTemplateDocIdx(te.getAttribute('templateInstanceGuid'));
        if (range.index == tDocIdx + 1) {
            //if start/caret idx is at post pad space
            if (oldRange.index == range.index + 1) {
                //caret moving left
                new_range.index = tDocIdx;
            } else if (oldRange.index == tDocIdx) {
                //caret moving right
                new_range.index++;
            } else {
                new_range = null;
            }
            //return;
        } else if (range.index == tDocIdx - 1) {
            //if start/caret idx is at pre pad space
            if (oldRange.index == tDocIdx) {
                //caret moving left
                new_range.index--;
            } else if (oldRange.index + 1 == range.index) {
                //caret moving right
                new_range.index++;
            } else {
                new_range = null;
            }
            //return;
        } else if (range.index == tDocIdx) {
            new_range = range;
            isAddTemplateValid = false;
        }
        if (new_range && (new_range.index != range.index || new_range.length != range.length)) {
            //IgnoreNextSelectionChange = true;


            setEditorSelection(new_range.index, new_range.length);

            //range = nrange;
            refreshTemplatesAfterSelectionChange();
            break;
        }
    }


    getTemplateElements().forEach(telm => { telm.classList.remove('ql-template-embed-blot-at-insert') });

    if (range) {
        let tl = getTemplateElementsInRange(range);
        if (tl.length > 0) {
            tl.forEach(telm => { telm.classList.add('ql-template-embed-blot-at-insert') });
        }
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

function focusTemplate(ftguid, fromDropDown, ftiguid) {
    if (ftguid == null) {
        return;
    }
    clearTemplateFocus();
    hideAllTemplateContextMenus();

    var tel = getTemplateElements();
    for (var i = 0; i < tel.length; i++) {
        var te = tel[i];
        if (te.getAttribute('templateGuid') == ftguid) {
            if (IsPastingTemplate) {
                $('#templateTextArea').placeholder = "Enter text for " + te.innerText;
                if (te.innerText != getTemplateDefByGuid(ftguid)['templateName']) {
                    $('#templateTextArea').val(te.innerText);
                } else {
                    $('#templateTextArea').val('');
                }
            }
            te.setAttribute('isFocus', true);

            if (ftiguid != null && te.getAttribute('templateInstanceGuid') == ftiguid) {
                te.classList.add(TemplateFocusInstanceClass);
                let teBlot = Quill.find(te);
                let teIdx = quill.getIndex(teBlot);
                setEditorSelection(teIdx,1,'silent');
            } else {
                te.classList.add(TemplateFocusNotInstanceClass);
            }
        } else {
            te.setAttribute('isFocus', false);
            te.classList.remove(TemplateFocusInstanceClass);
            te.classList.remove(TemplateFocusNotInstanceClass);
        }
    }

    if (fromDropDown == null || !fromDropDown) {
        if (IsPastingTemplate) {
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
    //var stl = document.getElementsByClassName("ql-template-embed-blot");
    var stl = document.getElementsByClassName(TemplateEmbedClass);
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
    let tguid = getFocusTemplateGuid();
    setTemplateBgColor(tguid, null,chex, false);

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

function setTemplateBgColor(tguid, tiguid, color_name_or_hex, isTemporary) {
    let tel = [];
    if (tguid && !tiguid) {
        tel = getTemplateElements(tguid);
    } else {
        let te = getTemplateInstanceElement(tiguid);
        if (te) {
            tel.push(te);
		}
	}

    for (var i = 0; i < tel.length; i++) {
        tel[i].style.backgroundColor = color_name_or_hex;
        tel[i].style.color = isBright(color_name_or_hex) ? 'black' : 'white';
        if (isTemporary) {
            tel[i].classList.add('temporary-bg-color');
            continue;
        }

        tel[i].setAttribute('templateColor', color_name_or_hex);
    }
}

function resetAllTemporaryTemplateBgColors() {
    let temp_template_bg_elms = document.querySelectorAll('.temporary-bg-color');
    Array.from(temp_template_bg_elms)
        .forEach((te) => {
            te.classList.remove('temporary-bg-color');
            let template_color = te.getAttribute('templateColor');
            te.style.backgroundColor = template_color;
            te.style.color = isBright(template_color) ? 'black' : 'white';
        });
    IsAnyTemplateBgTemporary = false;
}


function getTemplateDefByGuid(tguid) {
    return getUsedTemplateDefinitions().find(x=>x.domNode.getAttribute('templateGuid') == tguid);
}

function getTemplateDefByInstanceGuid(tiguid) {
    let telm = getTemplateElements(null, tiguid);
    if (!telm) {
        debugger;
    }
    return getTemplateFromDomNode(telm);
}

function getTemplateInstanceElement(tiguid) {
    let telm = document.querySelector('[templateInstanceGuid="' + tiguid + '"]');
    return telm;
 //   if (telm == null) {
 //       return null;
 //   }
 //   let telm_l = Array.from(telm);
 //   if (telm_l.length == 0) {
 //       return null;
	//}
 //   return telm_l[0];
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

function padTemplate(tiguid) {
    let teDocIdx = getTemplateDocIdx(tiguid);
    if (teDocIdx < 0) {
        throw 'tiguid: ' + tiguid + ' cannot have docIdx: ' + teDocIdx;
    }
    let needsPre = false;
    let needsPost = false;

    if (isDocIdxLineStart(teDocIdx)) {
        needsPre = true;
    } else {
        let preText = getText({ index: teDocIdx - 1, length: 1 });
        needsPre = preText != ' ';
    }
    if (isDocIdxLineEnd(teDocIdx)) {
        needsPost = true;
    } else {
        let postText = getText({ index: teDocIdx + 1, length: 1 });
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
    let cur_template_elms = getTemplateElements();
    let tguids_to_content_fit = [];
    let idx = 0;
    for (var i = 0; i < delta.ops.length; i++) {
        let op = delta.ops[i];

        if (op.retain) {
            idx += op.retain;
        }
        if (op.insert) {
            idx += op.insert.length;
        }
        if (op.attributes) {
            if (op.attributes.templateInstanceGuid) {
                tguids_to_content_fit.push(op.attributes.templateInstanceGuid);
			}
		}
        if (op.delete) {
            for (var j = 0; j < cur_template_elms.length; j++) {
                let ti = cur_template_elms[j];
                let tiDocIdx = getTemplateDocIdx(ti.getAttribute('templateInstanceGuid'));
                if (idx - op.delete == tiDocIdx) {
                    // deleting post pad so delete template and pre pad
                    IgnoreNextTextChange = true;
                    quill.deleteText(tiDocIdx - 1, 2);
                }
            }
        }
    }
    cur_template_elms = getTemplateElements();
    for (var i = 0; i < cur_template_elms.length; i++) {
        let ti = cur_template_elms[i];
        padTemplate(ti.getAttribute('templateInstanceGuid'));
    }
    for (var i = 0; i < tguids_to_content_fit.length; i++) {        
        let t = getTemplateDefByInstanceGuid(tguids_to_content_fit[i]);
        let telm = getTemplateInstanceElement(t.templateInstanceGuid);
        applyTemplateToDomNode(telm, t);

        telm.style.width = 'fit-content';
        telm.style.height = 'fit-content';
        telm.innerText = t.templateName;
	}
}
function updateTemplatesAfterSelectionChange(sel_range) {    
    let sel_bg_color =  getTextSelectionBgColor();
    let template_elms_in_sel_range = sel_range ? getTemplateElementsInRange(sel_range) : [];
    let all_template_elms = getTemplateElements();
    let show_sel_bg_color = !isShowingEditTemplateToolbar() && IsSubSelectionEnabled;

    for (var i = 0; i < all_template_elms.length; i++) {
        let te = all_template_elms[i];
        let updated_bg_color = null;
        let isTemporary = false;
        let tiguid = te.getAttribute('templateInstanceGuid');
        if (show_sel_bg_color && template_elms_in_sel_range.includes(te)) {
            log('sel template: ' + te.getAttribute('templateInstanceGuid'));

            updated_bg_color = sel_bg_color;
            isTemporary = true;
        } else {
            updated_bg_color = te.getAttribute('templateColor');
        }
        te.style.backgroundColor = updated_bg_color;
        te.style.color = isBright(updated_bg_color) ? 'black' : 'white';
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


