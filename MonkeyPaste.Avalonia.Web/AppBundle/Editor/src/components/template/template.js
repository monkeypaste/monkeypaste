// #region Globals

const TEMPLATE_VALID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890#-_ ";
const MIN_TEMPLATE_DRAG_DIST = 5;

var availableTemplates = null;

const ENCODED_TEMPLATE_OPEN_TOKEN = "{t{";
const ENCODED_TEMPLATE_CLOSE_TOKEN = "}t}";
const ENCODED_TEMPLATE_REGEXP = new RegExp(ENCODED_TEMPLATE_OPEN_TOKEN + ".*?" + ENCODED_TEMPLATE_CLOSE_TOKEN, "");

var IsMovingTemplate = false;

var IsTemplatePaddingAfterTextChange = false;

var IsAnyTemplateBgTemporary = false;
var DragAnchorDocIdxWhenTemplateWithinSelection = -1;

// #endregion Globals

// #region Life Cycle

function initTemplates() {
    quill.on("text-change", onEditorTextChangedPadTemplates);
    initTemplateBlot();
    initTemplateMatcher();
}

function initTemplateMatcher() {
    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    let Delta = Quill.imports.delta;

    quill.clipboard.addMatcher('span', function (node, delta) {
        if (node.hasAttribute('templateguid')) {
            delta.ops[0].attributes = delta.ops[0].insert.template;
            //delete delta.ops[0].insert.template;
            //delta.ops[0].insert = '';
        }
        return delta;
    });
}
//#endregion Life Cycle

// #region Getters

function getTemplateElements(tguid, sortBy = 'docIdx', isDescending = false) {
    var all_telms = Array.from(document.getElementsByClassName(TemplateEmbedClass));
    
    var target_telms = [];
    for (var i = 0; i < all_telms.length; i++) {    
        let telm = all_telms[i];
        let t = getTemplateFromDomNode(telm);
        if (!tguid || t.templateGuid == tguid) {
            target_telms.push(telm);
        }
    }
    if (!sortBy || sortBy == 'docIdx') {
        return isDescending ? target_telms.reverse() : target_telms;
	}
    return target_telms.sort((a, b) => {
        if (isDescending) {
            return a.getAttribute(sortBy) < a.getAttribute(sortBy) ? 1 : -1
		}
        return a.getAttribute(sortBy) < a.getAttribute(sortBy) ? -1 : 1
	});
}

function getTemplateElementTextSpan(telm) {
    return telm.firstChild.nextSibling;
}

function getTemplateInstanceDefs() {
    var telms = getTemplateElements();
    var tdefs = [];
    for (var i = 0; i < telms.length; i++) {
        let telm = telms[i];
        let template = getTemplateFromDomNode(telm);
        template.domNode = telm;
  //      if (telm.hasAttribute('docIdx')) {
  //          template.docIdx = parseInt(telm.getAttribute('docIdx'));
  //      } else {
  //          template.docIdx = getTemplateDocIdx(template.templateInstanceGuid);
		//}
       
        tdefs.push(template);
    }

    return tdefs;
}

function getTemplateDefs() {
    let tidl = getTemplateInstanceDefs();
    let tdl = [];
    for (var i = 0; i < tidl.length; i++) {
        let uti = tidl[i];
        let isDefined = tdl.find(x => x.templateGuid == uti.templateGuid) != null;
        if (isDefined) {
            continue;
        }
        delete uti.domNode;
        delete uti.docIdx;
        //delete uti.templateInstanceGuid;
        tdl.push(uti);
    }
    return tdl;
}

function getTemplateDefByGuid(tguid) {
    let utdl = getTemplateDefs();
    for (var i = 0; i < utdl.length; i++) {
        let t = utdl[i];
        if (t.templateGuid == tguid) {
            return t;
		}
	}
    return null;
}

function getTemplateCountBeforeDocIdx(docIdx) {
    let t_docIdx_L = getAllTemplateDocIdxs().filter(x => x < docIdx);
    return t_docIdx_L.length;
}

function getFocusTemplateElement() {
    //let fallback_telm = null;
    //let ftelm = null;
    //let telml = getTemplateElements();
    //if (telml.length == 0) {
    //    return null;
    //}
    //for (var i = 0; i < telml.length; i++) {
    //    let telm = telml[i];
    //    if (!fallback_telm && isTemplateElementFocusNotInstance(telm)) {
    //        fallback_telm = telm;
    //    }
    //    if (isTemplateElementFocusInstance(telm)) {
    //        if (ftelm) {
    //            // should only be 1
    //            debugger;
    //        }
    //        ftelm = telm;
    //        break;
    //    }
    //}
    //let selected_tguid = getSelectedOptionTemplateGuid();
    //if (ftelm) {
    //    if (isNullOrWhiteSpace(selected_tguid)) {
    //        // selector should have updated
    //        debugger;
    //    }
    //    if (ftelm.getAttribute('templateguid') != selected_tguid) {
    //        // selector should have updated or vice versa
    //        debugger;
    //    }
    //    return ftelm;
    //}

    //if (fallback_telm) {
    //    // this should be focus (maybe set here)
    //    debugger;
    //}
    //return fallback_telm;
    let ftelms = getFocusTemplateElements();
    if (ftelms.length > 0) {
        return ftelms[0];
    }
    return null;
}

function getFocusTemplate() {
    let ftguid = getFocusTemplateGuid();
    if (!ftguid) {
        return null;
    }
    return getTemplateDefByGuid(ftguid);
}

function getFocusTemplateGuid() {
    let ft = getFocusTemplateElement();
    if (ft == null) {
        return null;
    }
    return ft.getAttribute('templateGuid');
}

function getFocusTemplateElements() {
    return getTemplateElements().filter(x =>
        x.classList.contains('focused'));
}

async function getAvailableTemplateDefinitions() {
    if (availableTemplates == null || availableTemplates.length == 0) {
        availableTemplates = await getAllNonInputTemplatesFromDbAsync_get();

        if (availableTemplates == null || availableTemplates.length == 0) {
            return getTemplateDefs();
        }
    }

    let utdl = getTemplateDefs();
    let allMergedTemplates = [];
    for (var i = 0; i < availableTemplates.length; i++) {
        let atd = availableTemplates[i];
        let isUsed = false;
        //loop through all templates from master collection
        for (var j = 0; j < utdl.length; j++) {
            //check if this editor is using one available in master
            let utd = utdl[j];
            if (utd.templateGuid == atd.templateGuid) {
                //if this editor is using a template already known from master use this editor's version
                allMergedTemplates.push(utd);
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
        let wasFound = allMergedTemplates.find(x => x.templateGuid == utd.templateGuid) != null;
        if (!wasFound) {
            allMergedTemplates.push(getTemplateFromDomNode(utd.domNode));
        }
    }
    return allMergedTemplates;
}


function getTemplateElementsInRange(range) {
    if (range == null || range.index == null) {
        log('invalid range: ' + range);
    }
    let target_telms = [];
    let telms = getTemplateElements();
    for (var i = 0; i < telms.length; i++) {
        let telm = telms[i];
        let te_doc_idx = getElementDocIdx(telm);
        if (te_doc_idx >= range.index && te_doc_idx <= range.index + range.length) {
            // template is range
            target_telms.push(telm);
        }
	}
    return target_telms;
}

function getTemplateDefsInRange(range) {
    let telms = getTemplateElementsInRange(range);
    let tl = [];
    for (var i = 0; i < telms.length; i++) {
        let telm = telms[i];
        let t = getTemplateFromDomNode(telm);
        let exists = tl.find(x => x.templateGuid == t.templateGuid) != null;
        if (exists) {
            continue;
        }
        tl.push(t);
    }
    return tl;
}

function getTemplateAsPlainText(t) {
    if (isShowingPasteToolbar()) {
        return getTemplatePasteValue(t);
    }
    return getEncodedTemplateStr(t);
}

function getEncodedTemplateStr(t) {
    let encoded_template_str = `${ENCODED_TEMPLATE_OPEN_TOKEN}${t.templateGuid}${ENCODED_TEMPLATE_CLOSE_TOKEN}`;
    return encoded_template_str;
}

function getDecodedTemplateText(encoded_text) {
    if (!encoded_text) {
        return encoded_text;
    }
    let decoded_text = encoded_text;
    var result = ENCODED_TEMPLATE_REGEXP.exec(decoded_text);
    while (result) {
        let encoded_template_text = decoded_text.substr(result.index, result[0].length);
        let tguid = encoded_template_text.replace(ENCODED_TEMPLATE_OPEN_TOKEN, '').replace(ENCODED_TEMPLATE_CLOSE_TOKEN, '');
        let t = getTemplateDefByGuid(tguid);
        let tpv = getTemplatePasteValue(t);
        decoded_text = decoded_text.replaceAll(encoded_template_text, tpv);
        result = ENCODED_TEMPLATE_REGEXP.exec(decoded_text);
    }
    return decoded_text;
}

function getTemplateDisplayValue(t) {
    if (!t) {
        return '';
    }
    let dv = getTemplatePasteValue(t);
    if (isNullOrEmpty(dv)) {
        dv = t.templateName;
    }
    //if (isShowingPasteToolbar() && !isNullOrEmpty(t.templateText)) {
    //    return t.templateText;
    //}
    //return t.templateName;
    return dv;
}

function getTemplatePlainTextForDocRange(range) {
    let text = quill.getText(range.index, range.length);
    let out_text = '';
    for (var i = 0; i < range.length; i++) {
        let doc_idx = range.index + i;
        let doc_idx_char = getText({ index: doc_idx, length: 1 });
        let t_at_next_doc_idx = getTemplateAtDocIdx(doc_idx);
        if (t_at_next_doc_idx != null) {
            if (isTemplateAtDocIdxPrePadded(doc_idx)) {
                out_text = substringByLength(out_text, 0, Math.max(0, out_text.length - 1));
            }
            if (isTemplateAtDocIdxPostPadded(doc_idx)) {
                i++;
            }
            out_text += getTemplateAsPlainText(t_at_next_doc_idx);
        }
        out_text += doc_idx_char;
    }
    return out_text;
}

function getAllTemplateDocIdxs(tguid = null) {
    let telms = [];
    if (tguid) {
        telms = Array.from(
            document.querySelectorAll(`span[templateGuid="${tguid}"]`));
    } else {
        telms = Array.from(
            document.querySelectorAll(`span[templateGuid]`));
    }
    // NOTE these are always off by 1 i thinks a zero length thing
    return telms.map(x => getElementDocIdx(x) + 1);
}

//function getTemplateDocIdx(tiguid) {
//    let docLength = getDocLength();
//    for (var i = 0; i < docLength; i++) {
//        let curDelta = getDelta({ index: i, length: 1 });
//        if (curDelta.ops.length > 0 &&
//            curDelta.ops[0].hasOwnProperty('insert') &&
//            curDelta.ops[0].insert.hasOwnProperty('template')) {
//            let curTemplate = curDelta.ops[0].insert.template;
//            if (curTemplate.templateInstanceGuid == tiguid) {
//                return i;
//            }
//        }
//    }
//    return -1;
//}

//function getTemplateAtDocIdx(docIdx) {
//    let result = quill.getLeaf(docIdx + 1);
//    if (!result || result.length < 2) {
//        return null;
//    }
//    let blot = result[0];
//    if (blot && blot.domNode && blot.domNode.hasAttribute != null && blot.domNode.hasAttribute('templateGuid')) {
//        return getTemplateFromDomNode(blot.domNode);
//    }
//    return
function getTemplateAtDocIdx(docIdx) {
    //
    let telm = getElementAtDocIdx(docIdx);
    if (!telm || telm.nodeType === 3 || !telm.hasAttribute('templateGuid')) {
        return null;
    }
    return getTemplateFromDomNode(telm);
}
function getTemplateEmbedStr(t, sToken = ENCODED_TEMPLATE_OPEN_TOKEN, eToken = ENCODED_TEMPLATE_CLOSE_TOKEN) {
    var result = sToken + t.domNode.getAttribute('templateGuid') + eToken;
    return result;
}

function getLowestAnonTemplateName(anonPrefix = 'Template #') {
    let utdl = getTemplateDefs();
    let maxNum = 0;
    for (var i = 0; i < utdl.length; i++) {
        let t = utdl[i];
        if (t.templateName.startsWith(anonPrefix)) {
            var anonNum = parseInt(t.templateName.substring(anonPrefix.length));
            maxNum = Math.max(maxNum, anonNum);
        }
    }
    return anonPrefix + (parseInt(maxNum) + 1);
}

function getTemplateTypeSvgKey(ttype) {
    for (var i = 0; i < TemplateTypesMenuOptions.length; i++) {
        if (TemplateTypesMenuOptions[i].label.toLowerCase() == ttype.toLowerCase()) {
            return TemplateTypesMenuOptions[i].icon;
		}
    }
    return 'empty';
}

function getTemplatePadAdjustedDocIdx(doc_idx) {
    let adj_idx = doc_idx;
    if (isDocIdxTemplatePreOrPostPad(doc_idx)) {
        if (isDocIdxTemplatePrePad(doc_idx) && isDocIdxTemplatePostPad(doc_idx)) {
            // i don't think this should happen, good test would be 2 docidx between two adjacent templates 
            debugger;
        }
        if (isNavJump()) {
            adj_idx++;
        } else if (isNavRight()) {
            adj_idx++;
        } else {
            // nav left
            adj_idx--;
        }
    }
    return adj_idx;
}

function getTemplatePadAdjustedRange(sel) {
    let adj_start_idx = getTemplatePadAdjustedDocIdx(sel.index);
    let adj_end_idx = getTemplatePadAdjustedDocIdx(sel.index + sel.length);
    let min_idx = Math.min(adj_start_idx, adj_end_idx);
    let max_idx = Math.max(adj_start_idx, adj_end_idx);
    return { index: min_idx, length: max_idx - min_idx };
}
// #endregion Getters

// #region Setters

function setTemplateBgColor(tguid, color_name_or_hex) {
    let tel = getTemplateElements(tguid);
    for (var i = 0; i < tel.length; i++) {
        tel[i].style.backgroundColor = color_name_or_hex;
        tel[i].firstChild.nextSibling.style.color = getContrastHexColor(color_name_or_hex);
        //setSvgElmColor(tel[i].firstChild, tel[i].style.color);

        tel[i].setAttribute('templateColor', color_name_or_hex);
    }
    setEditToolbarColorButtonColor(color_name_or_hex);
    createTemplateSelector(tguid, getDocSelection(true));
}

function setTemplateElementText(telm, text) {

    getTemplateElementTextSpan(telm).innerText = text;
}
// #endregion Setters

// #region State

function hasTemplates() {
    let telml = getTemplateElements();
    return telml.length > 0;
}

function hasAnyInputRequredTemplate() {
    return getTemplateDefs().some(x => isTemplateAnInputType(x));
}

function isTemplateElementFocused(telm) {
    if (!telm) {
        return false;
    }
    return telm.classList.contains('focused');
}


function validateTemplate(t) {
    if (!t) {
        return 'no data';
    }
    if (isNullOrWhiteSpace(t.templateName)) {
        return 'name must be non-empty or whitespace';
    }
    return null;
}

function isTemplateDefChanged(orig_t, new_t) {
    if ((!orig_t && new_t) || (orig_t && !new_t)) {
        return true;
    }
    if (!orig_t && !new_t) {
        return false;
    }
    if (orig_t.templateGuid != new_t.templateGuid) {
        log('error! comparing two different templates');
        debugger;
        return true;
    }
    if (orig_t.templateName != new_t.templateName) {
        log('template ' + new_t.templateName + ' name changed');
        return true;
    }
    if (orig_t.templateColor != new_t.templateColor) {
        log('template ' + new_t.templateName + ' color changed');
        return true;
    }
    if (orig_t.templateData != new_t.templateData) {
        log('template ' + new_t.templateName + ' data changed');
        return true;
    }
    if (orig_t.templateHtmlFormat != new_t.templateHtmlFormat) {
        log('template ' + new_t.templateName + ' html format changed');
        return true;
    }
    if (orig_t.templateDeltaFormat != new_t.templateDeltaFormat) {
        log('template ' + new_t.templateName + ' delta format changed');
        return true;
    }
    return false;
}

function isTemplateFocused() {
    return getFocusTemplateElement() != null;
}

function isTemplateAtDocIdx(docIdx) {
    return getTemplateAtDocIdx(docIdx) != null;
}

function isAnyTemplateRequireInput() {
    let tl = getTemplateDefs();
    for (var i = 0; i < tl.length; i++) {
        if (isTemplateAnInputType(tl[i])) {
            return true;
		}
    }
    return false;
}

function isTemplateAnInputType(t) {
    if (!t) {
        return false;
    }
    let ttype = t.templateType.toLowerCase();
    if (ttype == 'dynamic' || ttype == 'contact') {
        return true;
    }
    return false;
}

function isSelAtFocusTemplateInsert() {
    //let sel = getDocSelection();
    //if (sel.length > 0) {
    //    return false;
    //}
    //return isTemplateAtDocIdx(sel.index);
    return document.getElementsByClassName(Template_AT_INSERT_Class).length > 0;
}

function isTemplateAtDocIdxPrePadded(t_docIdx) {
    let pre_text = getText({ index: t_docIdx - 1, length: 1 });
    if (pre_text == ' ') {
        return isDocIdxBlockStart(t_docIdx - 1);
    }
    return false;
}

function isTemplateAtDocIdxPostPadded(t_docIdx) {
    let post_text = getText({ index: t_docIdx + 1, length: 1 });
    if (post_text == ' ') {
        return isDocIdxBlockEnd(t_docIdx + 1);
    }
    return false;
}

function isDocIdxTemplatePrePad(doc_idx) {
    return isTemplateAtDocIdx(doc_idx + 1) && isTemplateAtDocIdxPrePadded(doc_idx + 1);
}

function isDocIdxTemplatePostPad(doc_idx) {
    return isTemplateAtDocIdx(doc_idx - 1) && isTemplateAtDocIdxPostPadded(doc_idx - 1);
}

function isDocIdxTemplatePreOrPostPad(doc_idx) {
    return isDocIdxTemplatePrePad(doc_idx) || isDocIdxTemplatePostPad(doc_idx);
}

function isHtmlStrContainTemplate(htmlStr) {
    if (isNullOrEmpty(htmlStr)) {
        return false;
    }
    return htmlStr.toLowerCase().indexOf('templateguid') >= 0;
}

function isPlainTextStrContainTemplate(ptStr) {
    if (isNullOrEmpty(ptStr)) {
        return false;
    }
    return
    ptStr.toLowerCase().indexOf(ENCODED_TEMPLATE_OPEN_TOKEN) >= 0 &&
        ptStr.toLowerCase().indexOf(ENCODED_TEMPLATE_CLOSE_TOKEN) >= 0;
}

function isDocIdxAtTemplateInsert(doc_idx, telm) {

}

// #endregion State

// #region Actions

function loadTemplates() {
    quill.update();
    resetEditTemplateToolbar();
    let telml = getTemplateElements();
    for (var i = 0; i < telml.length; i++) {
        let t_elm = telml[i];
        //resetTemplateElement(t_elm);
        let t = getTemplateFromDomNode(t_elm);
        applyTemplateToDomNode(t_elm, t);
    }

    if (hasTemplates()) {
        // since data is already set, manually trigger template update (for list item bug)
        updateTemplatesAfterTextChanged();
    }

    if (isShowingPasteToolbar()) {
        if (isAnyTemplateRequireInput()) {
            showPasteToolbar();
        } else {
            finishTemplatePaste();
        }
    }
}

function unparentTemplatesAfterHtmlInsert() {
    if (!hasTemplates()) {
        return;
    }
    let telms = getTemplateElements();
    for (var i = 0; i < telms.length; i++) {
        let telm = telms[i];
        let t_parent_elm = telm.parentNode;
        if (t_parent_elm.tagName.toLowerCase() == 'span') {
            // needs unparenting
            t_parent_elm.replaceWith(t_parent_elm.firstChild);
        }
	}
}

function enableTemplateSubSelection() {
    let telms = getTemplateElements();
    for (var i = 0; i < telms.length; i++) {
        let telm = telms[i];
        telm.classList.remove('no-select');
    }
}

function disableTemplateSubSelection() {
    let telms = getTemplateElements();
    for (var i = 0; i < telms.length; i++) {
        let telm = telms[i];
        telm.classList.add('no-select');
    }
}

function resetTemplateElement(telm) {
    let ti = getTemplateFromDomNode(telm);
    ti.templateText = '';
    ti.isFocus = false;
    ti.wasVisited = false;
    applyTemplateToDomNode(telm, ti);
}

function resetTemplates() {
    var telms = getTemplateElements();
    for (var i = 0; i < telms.length; i++) {
        resetTemplateElement(telms[i]);
    }
}

function finishTemplatePaste() {

}

function removeTemplatesByGuid(tguid) {    
    let telms_to_remove = getTemplateElements(tguid);
    for (var i = 0; i < telms_to_remove.length; i++) {
        removeTemplateElement(telms_to_remove[i]);
    }
}

function removeTemplateElement(telm) {
    let telm_doc_idx = getElementDocIdx(telm);
    let range_to_del = { index: telm_doc_idx, length: 1 };

    if (isTemplateAtDocIdxPrePadded(telm_doc_idx)) {
        range_to_del.index--;
        range_to_del.length++;
    }
    if (isTemplateAtDocIdxPostPadded(telm_doc_idx)) {
        range_to_del.length++;
    }
    deleteText(range_to_del);
}

function clearTemplateFocus() {
    let telms = getTemplateElements(null,null);
    for (var i = 0; i < telms.length; i++) {
        let telm = telms[i];
        //setTemplateElementFocus(telm, false, false);
        telm.classList.remove('focused');
    }
}

function hideAllTemplateContextMenus() {
    hideTemplateColorPaletteMenu()
    hideCreateTemplateToolbarContextMenu();
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

function updateTemplatesAfterTextChanged() {
    if (IsTemplatePaddingAfterTextChange) {
        debugger;
	}
    IsTemplatePaddingAfterTextChange = true;

    let all_template_elms = getTemplateElements(null,null);

    // HACK after text change scan all templates and pad any at head/tail of block with a space to avoid text nav issues
    let max_idx = getDocLength() - 1;
    for (var j = 0; j < all_template_elms.length; j++) {
        let telm = all_template_elms[j];

        // IMPORTANT!!! source MUST be silent or the change compounds itself
        // ensure templates along block edges are padded w/ a space
        //let t_doc_idx = getTemplateDocIdx(telm.getAttribute('templateInstanceGuid'));
        let t_doc_idx = getElementDocIdx(telm);
        if (isDocIdxBlockStart(t_doc_idx)) {
            insertText(t_doc_idx, ' ', 'silent');
            let pre_space_width = getCharacterRect(t_doc_idx).width;
            t_doc_idx++;
            telm.style.marginLeft = -pre_space_width + 'px';
        } else if (!isTemplateAtDocIdxPrePadded(t_doc_idx)) {
            telm.style.marginLeft = '0px';
		}
        if (isDocIdxBlockEnd(t_doc_idx)) {
            insertText(t_doc_idx + 1, ' ', 'silent');
            //let post_space_width = getCharacterRect(t_doc_idx + 1).width;
            //telm.style.marginRight = -post_space_width + 'px';
        } else if (!isTemplateAtDocIdxPostPadded(t_doc_idx)) {
            let hr = parseFloat(telm.style.marginRight) / 2;
            //telm.style.marginRight = hr+'px';
		}
        let t = getTemplateFromDomNode(telm);
        applyTemplateToDomNode(telm, t);

        //debabyTemplateElement(telm);
    }
    IsTemplatePaddingAfterTextChange = false;
}

function updateTemplatesAfterSelectionChange() {
    // this alters selection SILENTLY when template has pre/post padding
    // so range never falls on padding indexes.
    // when an extent is on a pad it then checks navigation to decide where to move it to
    // cases:
        // nav jump (last sel is not +/- 1 of current and length == 0)
        // -move pad extent to template
        // nav right pre/post pad
        // -move caret + 1
        // nav left pre/post pad
        // - move caret -1
    // then clear at insert class from all templates
    // and if updated extent is at template idx (must be done here since its silent) 
    if (!IsLoaded || isDragging()) {
        return;
    }
    if (isShowingPasteToolbar()) {
        updatePasteTemplateToolbarToSelection();
    }
    //return;
    let sel_range = getDocSelection();
    let adj_range = getTemplatePadAdjustedRange(sel_range);
    if (didSelectionChange(sel_range, adj_range)) {
        // needs adjustment
        setDocSelection(adj_range.index, adj_range.length, 'silent');
        sel_range = adj_range;
        quill.update();
    }
    let all_t_doc_idxs = getAllTemplateDocIdxs();
    for (var i = 0; i < all_t_doc_idxs.length; i++) {
        let t_doc_idx = all_t_doc_idxs[i];
        if (t_doc_idx >= sel_range.index && t_doc_idx <= sel_range.index + sel_range.length) {
            getElementAtDocIdx(t_doc_idx).classList.add(Template_AT_INSERT_Class);
        } else {
            getElementAtDocIdx(t_doc_idx).classList.remove(Template_AT_INSERT_Class);
        }
    }
    drawOverlay();
}


function insertTemplate(range, t, fromDropDown, source = 'api') {
    quill.deleteText(range.index, range.length, source);
    quill.insertEmbed(range.index, "template", t, source);

    // NOTE update must be called because text change hasn't picked up template yet (when none exist yet)
    updateTemplatesAfterTextChanged();
}

function focusTemplate(ftguid) {
    if (isShowingPasteToolbar()) {
        // only mark template as visited after it loses focus
        let old_ftguid = getFocusTemplateGuid();
        if (old_ftguid) {
            let ft = getTemplateDefByGuid(old_ftguid);
            let telms = getTemplateElements(old_ftguid);
            for (var i = 0; i < telms.length; i++) {
                if (parseBool(telms[i].getAttribute('wasVisited')) == false) {
                    telms[i].setAttribute('wasVisited', true);
                }
                
            }
		}
        
    }

    if (ftguid == null) {
        return;
    }
    clearTemplateFocus();
    hideAllTemplateContextMenus();
    
    var telms = getTemplateElements();
    for (var i = 0; i < telms.length; i++) {
        var telm = telms[i];
        let t = getTemplateFromDomNode(telm);
        if (t.templateGuid == ftguid) {
            telm.classList.add('focused');
        } else {
            telm.classList.remove('focused');
		}
    }

    if (isShowingPasteToolbar()) {
        updatePasteTemplateToolbarToSelection(ftguid);
    } else {
        if (isNew || fromClickOnTemplate) {
            showEditTemplateToolbar(isNew);
        }
    }
   
}
// #endregion Actions

//#region Events

function onEditorTextChangedPadTemplates(delta, oldDelta, source) {

}

//#endregion