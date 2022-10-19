
const TEMPLATE_VALID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890#-_ ";
const MIN_TEMPLATE_DRAG_DIST = 5;

var availableTemplates = null;

var ENCODED_TEMPLATE_OPEN_TOKEN = "{t{";
var ENCODED_TEMPLATE_CLOSE_TOKEN = "}t}";
var ENCODED_TEMPLATE_REGEXP = new RegExp(ENCODED_TEMPLATE_OPEN_TOKEN + ".*?" + ENCODED_TEMPLATE_CLOSE_TOKEN, "");

var IsMovingTemplate = false;

var IsTemplateAtInsert = false;

var HasTemplates = false;

var IsAnyTemplateBgTemporary = false;
var DragAnchorDocIdxWhenTemplateWithinSelection = -1;

// #region Life Cycle

function loadTemplates(isPasteRequest) {
    loadEditTemplateToolbar();

    let telml = getTemplateElements();
    HasTemplates = telml.length > 0;

    if (isPasteRequest && HasTemplates) {
        IsPastingTemplate = true;
    }

    for (var i = 0; i < telml.length; i++) {
        let t_elm = telml[i];
        //resetTemplateElement(t_elm);
        let t = getTemplateFromDomNode(t_elm);
        applyTemplateToDomNode(t_elm, t);
    }

    if (HasTemplates) {
        // since data is already set, manually trigger template update (for list item bug)
        updateTemplatesAfterTextChanged();
	}
    
    if (IsPastingTemplate) {
        if (isAnyTemplateRequireInput()) {
            showPasteTemplateToolbar();
        } else {
            finishTemplatePaste();
		}
        
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

//#endregion Life Cycle

// #region Getters

function getTemplateElements(tguid, sortBy = 'docIdx', isDescending = false) {
    var all_telms = Array.from(document.getElementsByClassName(TemplateEmbedClass));
    
    var target_telms = [];
    for (var i = 0; i < all_telms.length; i++) {    
        let telm = all_telms[i];
        let t = getTemplateFromDomNode(telm);
        if (!tguid || t.templateGuid == tguid) {
   //         if (sortBy) {
   //             telm.setAttribute('docIdx', getTemplateDocIdx(t.templateInstanceGuid));
			//}
            target_telms.push(telm);
        }
    }
    if (!sortBy || sortBy == 'docIdx') {
        return target_telms;
	}
    return target_telms.sort((a, b) => {
        if (isDescending) {
            return a.getAttribute(sortBy) < a.getAttribute(sortBy) ? 1 : -1
		}
        return a.getAttribute(sortBy) < a.getAttribute(sortBy) ? -1 : 1
	});
}

function getTemplateInstanceDefs() {
    var telms = getTemplateElements();
    var tdefs = [];
    for (var i = 0; i < telms.length; i++) {
        let telm = telms[i];
        let template = getTemplateFromDomNode(telm);
        template.domNode = telm;
        if (telm.hasAttribute('docIdx')) {
            template.docIdx = parseInt(telm.getAttribute('docIdx'));
        } else {
            template.docIdx = getTemplateDocIdx(template.templateInstanceGuid);
		}
       
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

function getAvailableTemplateDefinitions() {
    if (availableTemplates == null || availableTemplates.length == 0) {
        availableTemplates = getAllTemplatesFromDb_get();

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
    if (IsPastingTemplate) {
        //let t_elm = getTemplateInstanceElement(t.templateInstanceGuid);
        //template_text = t_elm.innerText;
        return t.templateText;
    }
    let template_text = `${ENCODED_TEMPLATE_OPEN_TOKEN}${t.templateGuid},${t.templateInstanceGuid}${ENCODED_TEMPLATE_CLOSE_TOKEN}`;
    return template_text;
}

function getTemplateDisplayValue(t) {
    if (!t) {
        return '';
    }
    if (IsPastingTemplate && !isNullOrEmpty(t.templateText)) {
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
                    out_text = substringByLength(out_text, 0, Math.max(0, out_text.length - 1));
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

function getAllTemplateDocIdxs() {
    let tDocIdxs = [];
    for (var i = 0; i < getDocLength(); i++) {
        if (getTemplateAtDocIdx(i)) {
            tDocIdxs.push(i);
        }
    }
    return tDocIdxs;
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

function getTemplateTypeIcon(ttype) {
    ttype = ttype.toLowerCase();
    for (var i = 0; i < TemplateTypesMenuOptions.length; i++) {
        if (TemplateTypesMenuOptions[i].label.toLowerCase() == ttype) {
            return TemplateTypesMenuOptions[i].icon.replace('fa-solid ','');
		}
    }
    return 'fa-solid';
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
        tel[i].style.color = isBright(color_name_or_hex) ? 'black' : 'white';
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

// #endregion Setters

// #region State

function isTemplateElementFocused(telm) {
    if (!telm) {
        return false;
    }
    if (telm.classList.contains(Template_FOCUSED_INSTANCE_Class) ||
        telm.classList.contains(Template_FOCUSED_NOT_INSTANCE_Class)) {
        // is this ok time to remove this?
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

// #endregion State

// #region Actions

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
    hideColorPaletteMenu();
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
    let all_template_elms = getTemplateElements(null,null);

    // HACK after text change scan all templates and pad any at head/tail of block with a space to avoid text nav issues
    let max_idx = getDocLength() - 1;
    for (var j = 0; j < all_template_elms.length; j++) {
        let telm = all_template_elms[j];

        // ensure templates along block edges are padded w/ a space
        let t_doc_idx = getTemplateDocIdx(telm.getAttribute('templateInstanceGuid'));
        let next_char = getText({ index: t_doc_idx + 1, length: 1 }, false);
        if (next_char == '\n' || t_doc_idx == max_idx) {
            insertText(t_doc_idx + 1, ' ', 'api');
            telm.style.marginRight = '-3px'
        } else {
            telm.style.marginRight = '0px';
        }
        let prev_char = getText({ index: t_doc_idx - 1, length: 1 }, false);
        if (prev_char == '\n' || t_doc_idx == 0) {
            insertText(t_doc_idx, ' ', 'api');
            telm.style.marginLeft = '0px';
        } else {
            telm.style.marginLeft = '0px';
        }

        // set css size to 'fit-contnet' for templates or templates in lists will be tiny or unformatted

        let t = getTemplateFromDomNode(telm);
        applyTemplateToDomNode(telm, t);

        //telm.style.width = 'fit-content';
        //telm.style.height = 'fit-content';
        //telm.innerText = getTemplateDisplayValue(t);
        debabyTemplateElement(telm);
    }
}

function updateTemplatesAfterSelectionChange(sel_range, oldRange) {
    if (WasTextChanged) {
        // selection timer and input can throw off sel_range here
        // probably bugs w/ the nav classes but hard to tell..
        // but keeping a one-or-the-other approach (when sel changes)
        // is appearing less problematic for now
        updateTemplatesAfterTextChanged();
        WasTextChanged = false;
        return;
    }
    if (WindowMouseDownLoc) {
        return;
    }
    if (IsPastingTemplate) {
        updatePasteTemplateToolbarToSelection();
	}
    oldRange = !oldRange ? sel_range : oldRange;
    let sel_bg_color = getTextSelectionBgColor();
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

function insertTemplate(range, t, fromDropDown, source = 'silent') {
    quill.deleteText(range.index, range.length, source);
    quill.insertEmbed(range.index, "template", t, source);

    let telm = getTemplateInstanceElement(t.templateInstanceGuid);
    debabyTemplateElement(telm);
}

function debabyTemplateElement(telm) {
    let t = getTemplateFromDomNode(telm);
    telm.style.width = 'fit-content';
    telm.style.height = 'fit-content';
    telm.innerText = getTemplateDisplayValue(t);
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
    focusTemplate(t, true);
}

function focusTemplate(ftguid, fromDropDown = false, isNew = false, fromClickOnTemplate = false) {
    if (IsPastingTemplate) {
        // only mark template as visited after it loses focus
        let old_ftguid = getFocusTemplateGuid();
        if (old_ftguid) {
            let ft = getTemplateDefByGuid(old_ftguid);
            let telms = getTemplateElements(old_ftguid);
            for (var i = 0; i < telms.length; i++) {
                telms[i].setAttribute('wasVisited', true);
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

    if (IsPastingTemplate) {
        updatePasteTemplateToolbarToSelection(ftguid);
    } else {
        //showEditTemplateToolbar(isNew);
    }
    if (isNew || fromClickOnTemplate) {
        showEditTemplateToolbar(isNew);
	}
}
// #endregion Actions












