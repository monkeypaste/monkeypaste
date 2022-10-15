
const TEMPLATE_VALID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890#-_ ";
const MIN_TEMPLATE_DRAG_DIST = 5;

var availableTemplates = null;
var userDeletedTemplateGuids = [];

var ENCODED_TEMPLATE_OPEN_TOKEN = "{t{";
var ENCODED_TEMPLATE_CLOSE_TOKEN = "}t}";
var ENCODED_TEMPLATE_REGEXP = new RegExp(ENCODED_TEMPLATE_OPEN_TOKEN + ".*?" + ENCODED_TEMPLATE_CLOSE_TOKEN, "");

var IsMovingTemplate = false;

var IsTemplateAtInsert = false;

var HasTemplates = false;

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


function loadTemplates() {
    let telml = getTemplateElements();
    for (var i = 0; i < telml.length; i++) {
        let t_elm = telml[i];
        let t_blot = getTemplateFromDomNode(t_elm);
        applyTemplateToDomNode(t_elm, t_blot);
    }

    HasTemplates = telml.length > 0;
}

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

        te.classList.remove(Template_FOCUSED_INSTANCE_Class);
        te.classList.remove(Template_FOCUSED_NOT_INSTANCE_Class);
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
        if (telm.classList.contains(Template_FOCUSED_INSTANCE_Class)) {
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
    let result = quill.getLeaf(docIdx + 1);
    if (!result || result.length < 2) {
        return null;
    }
    let blot = result[0];
    if (blot && blot.domNode && blot.domNode.hasAttribute != null && blot.domNode.hasAttribute('templateGuid')) {
        return getTemplateFromDomNode(blot.domNode);
    }
    return null;
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

function isTemplateAtDocIdx(docIdx) {
    return getTemplateAtDocIdx(docIdx) != null;
}

function getTemplateElementsWithDocIdx() {
    let tewdil = [];
    let t_elms = getTemplateElements();
    for (var i = 0; i < t_elms.length; i++) {
        let te = t_elms[i];
        let teDocIdx = getTemplateDocIdx(te.getAttribute('templateInstanceGuid'));
        tewdil.push({ teDocIdx, te });
	}
    return tewdil;
}

function getTemplateAsPlainText(t) {
    let template_text = '';
    if (IsPastingTemplate) {
        let t_elm = getTemplateElements(null, t['templateInstanceGuid']);
        template_text = t_elm.innerText;
    } else {
        template_text = ENCODED_TEMPLATE_OPEN_TOKEN + t['templateGuid'] + ',' + t['templateInstanceGuid'] + ENCODED_TEMPLATE_CLOSE_TOKEN;
    }
    return template_text;
}

function getTemplatePlainTextForDocRange(range) {
    let text = quill.getText(range.index, range.length);
    let out_text = '';
    for (var i = 0; i < range.length; i++) {
        let doc_idx = range.index + i;
        let doc_idx_char = getText({ index: doc_idx, length: 1 });
        let t_at_next_doc_idx = getTemplateAtDocIdx(doc_idx);
        if (t_at_next_doc_idx != null) {
            // check if pre template space is a pad space
            let pre_t_doc_idx = doc_idx - 1;
            let pre_t_doc_idx_char = getText({ index: pre_t_doc_idx, length: 1 });
            if (pre_t_doc_idx_char == ' ') {
                // previous is potentially pad since its a space
                let pre_pre_t_doc_idx_char = getText({ index: pre_t_doc_idx - 1, length: 1 });
                let is_pre_t_idx_pad_space = pre_t_doc_idx == 0 || pre_pre_t_doc_idx_char == '\n';
                if (is_pre_t_idx_pad_space) {
                    // occurs when previous pad space is beginning of doc or pad was start of new block
                    // remove prev pad space
                    out_text = substringByLength(out_text, 0, Math.max(0,out_text.length - 1));
                }                
            }
            // check if pre temppostlate space is a pad space
            let post_t_doc_idx = doc_idx + 1;
            let post_t_doc_idx_char = getText({ index: post_t_doc_idx, length: 1 });
            if (post_t_doc_idx_char == ' ') {
                // next is potentially pad since its a space
                let post_post_t_doc_idx_char = getText({ index: post_t_doc_idx + 1, length: 1 });
                let is_post_t_idx_pad_space = post_t_doc_idx == getDocLength() - 1 || post_post_t_doc_idx_char == '\n';
                if (is_post_t_idx_pad_space) {
                    // occurs when post pad space is end of doc or pad is end of current block
                    // skip next iteration so loop ignores the pad space and picks back up at end of block or terminates
                    i++;
                }
            }
            out_text += getTemplateAsPlainText(t_at_next_doc_idx);
        }
        out_text += doc_idx_char;
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

function getAllTemplateDocIdxs() {
    let tDocIdxs = [];
    for (var i = 0; i < getDocLength(); i++) {
        if (getTemplateAtDocIdx(i)) {
            tDocIdxs.push(i);
		}
    }
    return tDocIdxs;
}


function insertTemplate(range, t, fromDropDown, source = 'silent') {
    quill.deleteText(range.index, range.length, source);
    quill.insertEmbed(range.index, "template", t, source);
    //quill.insertText(range.index, ' ', 'templatePad_pre',true, 'silent');
    //quill.insertText(range.index+2, ' ', 'templatePad_post', true, 'silent');
    //focusTemplate(t.templateGuid, t.templateInstanceGuid, fromDropDown);
}

function moveTemplate(tiguid, nidx, isCopy) {
    IsMovingTemplate = true;

    let tidx = getTemplateDocIdx(tiguid);

    let t = getTemplateDefByInstanceGuid(tiguid);

    if (isCopy) {
        t.templateInstanceGuid = generateGuid();
        t = createTemplateFromDropDown(t);
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
    focusTemplate(t, null, true);
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

function focusTemplate(ftguid, ftiguid, fromDropDown, isNew) {
    if (ftguid == null) {
        return;
    }
    clearTemplateFocus();
    hideAllTemplateContextMenus();

    let wasFocusSet = false;
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

            let ft_instance_elm = null;

            if (ftiguid) {
                if (te.getAttribute('templateInstanceGuid') == ftiguid) {
                    ft_instance_elm = te;
				}                
            } else if (!wasFocusSet) {
                ft_instance_elm = te;
            }

            if (ft_instance_elm) {
                wasFocusSet = true;

                te.setAttribute('isFocus', true);
                te.classList.add(Template_FOCUSED_INSTANCE_Class);
                let teBlot = Quill.find(te);
                let teIdx = quill.getIndex(teBlot);
                //setEditorSelection(teIdx, 1);
			} else {
                te.classList.add(Template_FOCUSED_NOT_INSTANCE_Class);
                te.setAttribute('isFocus', false);
            }
           // te.setAttribute('isFocus', true);

        } else {
            te.setAttribute('isFocus', false);
            te.classList.remove(Template_FOCUSED_INSTANCE_Class);
            te.classList.remove(Template_FOCUSED_NOT_INSTANCE_Class);
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

function isTemplateElementHavePad_pre(ti) {
    if (!ti) {
        return false;
    }
    if (!ti.previousSibling) {
        return false;
    }
    if (ti.previousSibling.nodeType == 3) {
        return false;
    }
    return ti.previousSibling.hasAttribute('templatePad_pre');
}
function isTemplateElementHavePad_post(ti) {
    if (!ti) {
        return false;
    }
    if (!ti.nextSibling) {
        return false;
    }
    if (ti.nextSibling.nodeType == 3) {
        return false;
    }
    return ti.nextSibling.hasAttribute('templatePad_post');
}
function cleanTemplateElementPad_pre(ti) {
    let is_dirty = ti.previousSibling.innerText != ' ';

}

function isDocIdxTemplatePad_pre(docIdx) {
    let format = quill.getFormat(docIdx, 0);
    if (format.templatePad_pre !== undefined && format.templatePad_pre) {
        return true;
    } 
    return false;
}
function isDocIdxTemplatePad_post(docIdx) {
    let format = quill.getFormat(docIdx, 0);
    if (format.templatePad_post !== undefined && format.templatePad_post) {
        return true;
    }
    return false;
}

function decodeInsertedTemplates(insertIdx, plainText, source = 'silent') {
    // parse all text for encoded templates
    // parse inserted text for [tguid,tiguid] and replace w/ template blot and mark for update
    for (var i = 0; i < plainText.length; i++) {
        let cur_pt = substringByLength(plainText, i);
        if (cur_pt.startsWith(ENCODED_TEMPLATE_OPEN_TOKEN)) {
            let t_end_idx = cur_pt.indexOf(ENCODED_TEMPLATE_CLOSE_TOKEN);
            let encoded_template_str = substringByLength(cur_pt, ENCODED_TEMPLATE_OPEN_TOKEN.length, t_end_idx);
            let template_def = getTemplateDefByGuid(encoded_template_str.split(',')[0]);
            insertTemplate({ index: insertIdx, length: 0 }, template_def, false, source);
            i += t_end_idx + ENCODED_TEMPLATE_CLOSE_TOKEN.length - 1;
        } else {
            insertText(insertIdx, cur_pt[0], source, false);
        }
        insertIdx++;
    }
    if (source == 'silent') {
        updateTemplatesAfterTextChanged();
	}
}

function updateTemplatesAfterTextChanged(delta, oldDelta, source) {
    let cur_template_elms = getTemplateElements();
    let tiguids_to_update = [];
        

    if (delta && delta.ops) {
        // called from text change
        let idx = 0;
        for (var j = 0; j < delta.ops.length; j++) {
            let op = delta.ops[j];

            //if (op.retain) {
            //    idx += op.retain;
            //}

            //if (op.insert) {
            //    idx += op.insert.length;
            //}

            if (op.attributes && op.attributes.templateInstanceGuid) {
                tiguids_to_update.push(op.attributes.templateInstanceGuid);
            }
        }
    } else {
        // called from somewhere (currently decodeInsertedTemplate)
        tiguids_to_update = cur_template_elms.map((telm) => { return telm.getAttribute('templateInstanceGuid'); });
	}
    

    // HACK after text change scan all templates and pad any at head/tail of block with a space to avoid text nav issues
    let max_idx = getDocLength() - 1;
    for (var j = 0; j < cur_template_elms.length; j++) {
        let t_elm = cur_template_elms[j];
        let t_doc_idx = getTemplateDocIdx(t_elm.getAttribute('templateInstanceGuid'));
        let next_char = getText({ index: t_doc_idx + 1, length: 1 }, false);
        if (next_char == '\n' || t_doc_idx == max_idx) {
            insertText(t_doc_idx + 1, ' ', 'api');
            t_elm.style.marginRight = '-3px'
        } else {
            t_elm.style.marginRight = '0px';
		}
        let prev_char = getText({ index: t_doc_idx - 1, length: 1 }, false);
        if (prev_char == '\n' || t_doc_idx == 0) {
            insertText(t_doc_idx, ' ', 'api');
            t_elm.style.marginLeft = '0px';
        } else {
            t_elm.style.marginLeft = '0px';
		}
    }

    // HACK new templates (no other refs) added to list items (consistent example) 
    // are real tiny, applying fit content here(can't do in creation) fixes
    for (var j = 0; j < tiguids_to_update.length; j++) {        
        let t = getTemplateDefByInstanceGuid(tiguids_to_update[j]);
        let telm = getTemplateInstanceElement(t.templateInstanceGuid);
        applyTemplateToDomNode(telm, t);

        telm.style.width = 'fit-content';
        telm.style.height = 'fit-content';
        telm.innerText = t.templateName;
    }
}

function updateTemplatesAfterSelectionChange(sel_range, oldRange) {
    oldRange = !oldRange ? sel_range : oldRange;
    let sel_bg_color =  getTextSelectionBgColor();
    let template_elms_in_sel_range = sel_range ? getTemplateElementsInRange(sel_range) : [];
    let all_template_elms = getTemplateElements();
    let show_sel_bg_color = !isShowingEditTemplateToolbar() && IsSubSelectionEnabled;

    let old_closest_idx = sel_range.index > oldRange.index ? oldRange.index + oldRange.length : oldRange.index;
    let is_nav_right = sel_range.index > old_closest_idx && sel_range.length == 0;
    IsTemplateAtInsert = false;
    for (var i = 0; i < all_template_elms.length; i++) {
        let t_elm = all_template_elms[i];
        let tiguid = t_elm.getAttribute('templateInstanceGuid');
        let is_t_in_sel_range = template_elms_in_sel_range.includes(t_elm);
        let t_doc_idx = getTemplateDocIdx(tiguid);
        if (show_sel_bg_color) {
            if (is_t_in_sel_range) {
                if (is_nav_right &&
                    !t_elm.classList.contains(Template_IN_SEL_RANGE_Class) &&
                    !t_elm.classList.contains(Template_AT_INSERT_Class)) {
                    setTemplateNavState(t_elm, Template_BEFORE_INSERT_Class);
                    continue;
                }
                log('sel template: ' + t_elm.getAttribute('templateInstanceGuid'));
                if (sel_range.length > 0) {
                    let is_t_at_sel_bounds = t_doc_idx == sel_range.index || t_doc_idx == sel_range.index + sel_range.length;
                    if (is_t_at_sel_bounds) {
                        if (t_elm.classList.contains(Template_AT_INSERT_Class)) {
                            setTemplateNavState(t_elm, Template_IN_SEL_RANGE_Class);
                        } else {
                            clearTemplateNavState(t_elm);
						}
                    } else {
                        setTemplateNavState(t_elm, Template_IN_SEL_RANGE_Class);                        
					}
                } else {
                    setTemplateNavState(t_elm, Template_AT_INSERT_Class);
                    IsTemplateAtInsert = true;
				}
                
            } else {
                if (is_nav_right && sel_range.index == t_doc_idx + 1) {
                    if (t_elm.classList.contains(Template_AT_INSERT_Class)) {
                        setTemplateNavState(t_elm);
                    } else {
                        setEditorSelection(sel_range.index - 1, 0, 'silent');
                        setTemplateNavState(t_elm, Template_AT_INSERT_Class);
                        IsTemplateAtInsert = true;
                    } 

                } else if (!is_nav_right && sel_range.index == t_doc_idx - 1) {
                    if (t_elm.classList.contains(Template_BEFORE_INSERT_Class)) {
                        clearTemplateNavState(t_elm);
                    } else {
                        setEditorSelection(sel_range.index + 1, 0, 'silent');
                        setTemplateNavState(t_elm, Template_BEFORE_INSERT_Class);
					}
                    
                } else {
                    clearTemplateNavState(t_elm);
				}
                
            }
		}        
    }
}

function clearTemplateNavState(t_elm) {
    t_elm.classList.remove(Template_BEFORE_INSERT_Class);
    t_elm.classList.remove(Template_AT_INSERT_Class);
    t_elm.classList.remove(Template_AFTER_INSERT_Class);
    t_elm.classList.remove(Template_IN_SEL_RANGE_Class);
}

function isTemplateInNavState(t_elm) {
    let is_before = t_elm.classList.contains(Template_BEFORE_INSERT_Class);
    let is_at = t_elm.classList.contains(Template_AT_INSERT_Class);
    let is_after = t_elm.classList.contains(Template_AT_INSERT_Class);
    return is_before || is_at || is_after;
}

function setTemplateNavState(t_elm, navStateClass) {
    clearTemplateNavState(t_elm);
    if (!navStateClass) {
        return;
	}
    t_elm.classList.add(navStateClass);
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


