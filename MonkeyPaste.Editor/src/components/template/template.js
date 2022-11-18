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
        delete uti.templateInstanceGuid;
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

function getTemplateDefByInstanceGuid(tiguid) {
    let telm = getTemplateInstanceElement(tiguid);
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

function getFocusTemplateElement() {
    let fallback_telm = null;
    let telml = getTemplateElements();
    for (var i = 0; i < telml.length; i++) {
        let telm = telml[i];
        if (isTemplateElementFocused(telm)) {
            return telm;
        }
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

function getSelectedTemplateElements() {
    return getTemplateElements().filter(x => x.classList.contains(Template_FOCUSED_INSTANCE_Class) || x.classList.contains(Template_FOCUSED_NOT_INSTANCE_Class));
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

function getTemplateDocIdx(tiguid) {
    let docLength = getDocLength();
    for (var i = 0; i < docLength; i++) {
        let curDelta = getDelta({ index: i, length: 1 });
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
    if (isShowingPasteToolbar() && !isNullOrEmpty(t.templateText)) {
        return t.templateText;
    }
    return t.templateName;
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

function getAllTemplateDocIdxs() {
    let tDocIdxs = [];
    for (var i = 0; i < getDocLength(); i++) {
        if (getTemplateAtDocIdx(i)) {
            tDocIdxs.push(i);
        }
    }
    return tDocIdxs;
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


// #endregion Getters

// #region Setters

function setTemplateElementFocus(telm, isFocusInstance, isFocusNotInstance) {
    if (!telm) {
        return;
    }
    if (isFocusInstance) {
        telm.classList.add(Template_FOCUSED_INSTANCE_Class);
    } else {
        telm.classList.remove(Template_FOCUSED_INSTANCE_Class);
    }
    if (isFocusNotInstance) {
        telm.classList.add(Template_FOCUSED_NOT_INSTANCE_Class);
    } else {
        telm.classList.remove(Template_FOCUSED_NOT_INSTANCE_Class);
    }
}
function setTemplateBgColor(tguid, color_name_or_hex, isTemporary) {
    let tel = getTemplateElements(tguid);
    for (var i = 0; i < tel.length; i++) {
        tel[i].style.backgroundColor = color_name_or_hex;
        tel[i].style.color = getContrastHexColor(color_name_or_hex);
        setSvgElmColor(tel[i].firstChild, tel[i].style.color);
        if (isTemporary) {
            tel[i].classList.add('temporary-bg-color');
            continue;
        }

        tel[i].setAttribute('templateColor', color_name_or_hex);
    }
}

function setTemplateNavState(t_elm, navStateClass) {
    clearTemplateNavState(t_elm);
    if (!navStateClass) {
        return;
    }
    t_elm.classList.add(navStateClass);
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
    if (telm.classList.contains(Template_FOCUSED_INSTANCE_Class) ||
        telm.classList.contains(Template_FOCUSED_NOT_INSTANCE_Class)) {
        return true;
    }
    return false;
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

function isTemplateInNavState(t_elm) {
    let is_before = t_elm.classList.contains(Template_BEFORE_INSERT_Class);
    let is_at = t_elm.classList.contains(Template_AT_INSERT_Class);
    let is_after = t_elm.classList.contains(Template_AT_INSERT_Class);
    return is_before || is_at || is_after;
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
    let ftelm = getFocusTemplateElement();
    if (!ftelm) {
        return false;
    }
    return ftelm.classList.contains(Template_IN_SEL_RANGE_Class);
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

// #endregion State

// #region Actions

function loadTemplates(isPasteRequest) {
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
    let util = getTemplateInstanceDefs();
    for (var i = 0; i < util.length; i++) {
        let cit = util[i];
        if (cit.templateGuid == tguid) {
            let docIdx = getTemplateDocIdx(cit.templateInstanceGuid);
            if (docIdx >= 0) {
                quill.deleteText(docIdx, 1);
            }
        }
    }
}

function clearTemplateFocus() {
    let telms = getTemplateElements(null,null);
    for (var i = 0; i < telms.length; i++) {
        let telm = telms[i];
        setTemplateElementFocus(telm, false, false);
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
        let t_doc_idx = getTemplateDocIdx(telm.getAttribute('templateInstanceGuid'));
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

        debabyTemplateElement(telm);
    }
    IsTemplatePaddingAfterTextChange = false;
}

function updateTemplatesAfterSelectionChange() {
    //if (WasTextChanged) {
    //    // selection timer and input can throw off sel_range here
    //    // probably bugs w/ the nav classes but hard to tell..
    //    // but keeping a one-or-the-other approach (when sel changes)
    //    // is appearing less problematic for now
    //    updateTemplatesAfterTextChanged();
    //    WasTextChanged = false;
    //    return;
    //}
    if (WindowMouseDownLoc) {
        clearAllTemplateNavStates();
        return;
    }
    if (isShowingPasteToolbar()) {
        updatePasteTemplateToolbarToSelection();
    }
    let sel_range = getDocSelection();
    let last_sel_range = LastSelRange;

    last_sel_range = last_sel_range ? last_sel_range : sel_range;
    let sel_bg_color = getTextSelectionBgColor();
    let template_elms_in_sel_range = sel_range ? getTemplateElementsInRange(sel_range) : [];
    let all_template_elms = getTemplateElements();
    let show_sel_bg_color = !isShowingEditTemplateToolbar() && isSubSelectionEnabled();

    let old_closest_idx = sel_range.index > last_sel_range.index ? last_sel_range.index + last_sel_range.length : last_sel_range.index;
    let is_nav_right = sel_range.index > old_closest_idx && sel_range.length == 0;

    for (var i = 0; i < all_template_elms.length; i++) {
        let t_elm = all_template_elms[i];
        let tiguid = t_elm.getAttribute('templateInstanceGuid');
        let is_t_in_sel_range = template_elms_in_sel_range.includes(t_elm);
        //let t_doc_idx = getTemplateDocIdx(tiguid);
        let t_doc_idx = getElementDocIdx(t_elm);
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
                }

            } else {
                if (is_nav_right && sel_range.index == t_doc_idx + 1) {
                    if (t_elm.classList.contains(Template_AT_INSERT_Class)) {
                        setTemplateNavState(t_elm);
                    } else {
                        setDocSelection(sel_range.index - 1, 0, 'silent');
                        setTemplateNavState(t_elm, Template_AT_INSERT_Class);
                    }

                } else if (!is_nav_right && sel_range.index == t_doc_idx - 1) {
                    if (t_elm.classList.contains(Template_BEFORE_INSERT_Class)) {
                        clearTemplateNavState(t_elm);
                    } else {
                        setDocSelection(sel_range.index + 1, 0, 'silent');
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

function clearAllTemplateNavStates() {
    getTemplateElements().forEach(x => clearTemplateNavState(x));
}

function insertTemplate(range, t, fromDropDown, source = 'api') {
    quill.deleteText(range.index, range.length, source);
    quill.insertEmbed(range.index, "template", t, source);

    //quill.insertText(range.index, t.templateName, source);
    //range.length = t.templateName.length;
    //quill.formatText(range.index, range.length, 'template', t);

    //debugger;
    let telm = getTemplateInstanceElement(t.templateInstanceGuid);
    debabyTemplateElement(telm);

    // NOTE update must be called because text change hasn't picked up template yet (when none exist yet)
    updateTemplatesAfterTextChanged();
}

function debabyTemplateElement(telm) {
    return;
    let t = getTemplateFromDomNode(telm);
    telm.style.width = 'fit-content';
    telm.style.height = 'fit-content';
    telm.innerText = getTemplateDisplayValue(t);
}

function focusTemplate(ftguid, fromDropDown = false, isNew = false, fromClickOnTemplate = false) {
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
            setTemplateElementFocus(telm, true, true);
        } else {
            setTemplateElementFocus(telm, false, false);
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