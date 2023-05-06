// #region Globals

// #endregion Globals

// #region Life Cycle

function loadTextContent(itemDataStr) {
	quill.enable(true);
	//setRootHtml(itemDataStr)
	//log('loading text content: ' + itemDataStr);

	// HTML LOAD NOTES
	// templates work
	// xml breaks (randomly missing)
	//setRootHtml('');
	//let decoded_html = decodeHtmlSpecialEntities(itemDataStr);
	//insertHtml(0, decoded_html, 'silent');

	// DELTA LOAD NOTES
	// xml works
	// templates lost
	let delta = convertHtmlToDelta(itemDataStr);
	delta = decodeHtmlEntitiesInDeltaInserts(delta);
	setContents(delta,'silent');

	loadTemplates();
	loadLinkHandlers();
	enableTableContextMenu();
	enableTableInteraction();
}

// #endregion Life Cycle

// #region Getters

function getTextContentData() {
	let qhtml = '';
	if (isContentATable()) {
		// NOTE delta-to-html will loose tables. This probably means this will loose templates
		qhtml = getHtml2();
	} else {
		qhtml = getHtml();
	}
	return qhtml;
}

function getEncodedTextContentText(range) {
	let t_idxs = getAllTemplateDocIdxs();
	let li_idxs = getAllListItemBulletDocIdxs();
	let text = getText(range, false);

	let out_text = '';
	for (var i = 0; i < range.length; i++) {
		let doc_idx = range.index + i;
		let doc_idx_char = getText({ index: doc_idx, length: 1 });
		if (t_idxs.includes(doc_idx)) {
			let t = getTemplateAtDocIdx(doc_idx);
			if (isTemplateAtDocIdxPrePadded(doc_idx)) {
				out_text = substringByLength(out_text, 0, Math.max(0, out_text.length - 1));
			}
			if (isTemplateAtDocIdxPostPadded(doc_idx)) {
				i++;
			}
			out_text += getEncodedTemplateStr(t);
		}
		if (li_idxs.includes(doc_idx)) {
			let li_elm = getListItemElementAtDocIdx(doc_idx);
			out_text += getEncodedListItemStr(li_elm);
		}

		out_text += doc_idx_char;
	}
	return out_text;
}

function getDecodedTextContentText(encoded_text) {
	let decoded_text = encoded_text;
	decoded_text = getDecodedTemplateText(decoded_text);
	decoded_text = getDecodedListItemText(decoded_text);
	return decoded_text;
}

function getTextContenLineCount() {
	return getLineCount();
}

function getTextContentCharCount() {
	return getText().length;
} 

function getContentRange() {
	return { index: 0, length: getDocLength() };
}

function getLineStartDocIdx(docIdx) {
	docIdx = docIdx < 0 ? 0 : docIdx >= getDocLength() ? getDocLength() - 1 : docIdx;
	while (!isDocIdxLineStart(docIdx)) {
		docIdx--;
	}
	return docIdx;
}

function getLineEndDocIdx(docIdx) {
	docIdx = docIdx < 0 ? 0 : docIdx >= getDocLength() ? getDocLength() - 1 : docIdx;
	while (!isDocIdxLineEnd(docIdx)) {
		docIdx++;
	}
	return docIdx;
}

function getLineIdx(docIdx) {
	// NOTE below doesn't work like doc's say it returns the same docIdx given for lineIdx (at least for 1 line content) maybe since this is dev 2.0 quill
	//let line_blot = quill.getLine(docIdx);
	//return line_blot ? line_blot[1] : -1;

	let docLength = getDocLength();
	let cur_line_idx = 0;
	for (var i = 0; i < docLength; i++) {
		let line_start_doc_idx = getLineStartDocIdx(i);
		let line_end_doc_idx = getLineEndDocIdx(i);
		if (docIdx >= line_start_doc_idx && docIdx <= line_end_doc_idx) {
			return cur_line_idx;
		}
		i = line_end_doc_idx + 1;
	}
	return -1;
}

function getLineDocRange(lineIdx) {
	lineIdx = lineIdx < 0 ? 0 : lineIdx >= getLineCount() ? getLineCount() - 1 : lineIdx;
	let docIdx = 0;
	let maxDocIdx = getDocLength() - 1;
	let curLineIdx = 0;
	while (docIdx <= maxDocIdx) {
		let lineStartIdx = getLineStartDocIdx(docIdx);
		let lineEndIdx = getLineEndDocIdx(docIdx);
		if (curLineIdx == lineIdx) {
			return [lineStartIdx, lineEndIdx];
		}
		curLineIdx++;
		docIdx = lineEndIdx + 1;
	}
	return null;
}

function getLineCount() {
	return quill.getLines().length;
}

function getDocLength(omitTrailingLineEnd = false) {
	if (!quill) {
		return 0;
	}

	let len = quill.getLength();
	if (omitTrailingLineEnd) {
		let pt = getAllText();
		if (!isNullOrEmpty(pt)) {
			if (pt.endsWith('\n')) {
				len--;
			}
		}
	} 
	return len;
}

function getCharacterRect(docIdx, isWindowOrigin = true, inflateToLineRect = true, inflateEmptyRange = true) {
	docIdx = parseInt(docIdx);
	if (isNaN(docIdx)) {
		return cleanRect();
	}

	//let doc_idx_t = getTemplateAtDocIdx(docIdx);

	//if (doc_idx_t) {
	//	let doc_idx_t_elm = getTemplateElementsInRange({ index: docIdx, length: 1 });
	//	if (doc_idx_t_elm) {
	//		return cleanRect(doc_idx_t_elm.getBoundingClientRect());
	//	}
	//}
	let len = inflateEmptyRange ? 1:0
	let docIdx_rect = quill.getBounds(docIdx, len);

	if (isWindowOrigin) {
		docIdx_rect = editorToScreenRect(docIdx_rect);
	} else {
		docIdx_rect = cleanRect(docIdx_rect);
	}
	if (inflateToLineRect) {
		let lh = getLineHeightAtDocIdx(docIdx);
		let inflate_y_amt = Math.ceil((lh - docIdx_rect.height) / 2);
		docIdx_rect = inflateRect(docIdx_rect, 0, -inflate_y_amt, 0, inflate_y_amt);

	}

	return docIdx_rect;
}

function getLineRect(lineIdx, snapToEditor = true) {
	let line_doc_range = getLineDocRange(lineIdx);
	let line_start_rect = getCharacterRect(line_doc_range[0]);
	let line_end_rect = getCharacterRect(line_doc_range[1]);
	let line_rect = rectUnion(line_start_rect, line_end_rect);
	if (!snapToEditor) {
		return line_rect;
	}
	let editor_rect = getEditorContainerRect();
	//union line with editor left/right edges
	line_rect.left = editor_rect.left;
	line_rect.right = editor_rect.right;

	//inflate TOP of line to previous line bottom so no empties
	// for 0 inflate to editor top
	// for last also inflate BOTTOM to editor bottom

	let inflated_top = 0;
	let lineCount = getLineCount();
	if (lineIdx == 0) {
		inflated_top = editor_rect.top;
	} else {
		let prev_line_rect = getLineRect(lineIdx - 1);
		inflated_top = prev_line_rect.bottom;
	}

	if (lineIdx == lineCount - 1) {
		line_rect.bottom = editor_rect.bottom;
	}
	line_rect.top = inflated_top;

	line_rect = cleanRect(line_rect);
	return line_rect;
}

function getRangeRects(range, isWindowOrigin = true, inflateToLineHeight = true, inflateEmptyRange = true) {
	range = cleanDocRange(range);

	let range_rects = [];
	if (!range) {
		return range_rects;
	}
	if (range.length == 0) {
		let caret_rect = getCharacterRect(range.index, isWindowOrigin, inflateToLineHeight, inflateEmptyRange);
		range_rects.push(caret_rect);
		return range_rects;
	}

	let cur_line_rect = null;
	for (var i = range.index; i < range.index + range.length; i++) {
		let cur_idx_rect = getCharacterRect(i, isWindowOrigin, inflateToLineHeight, inflateEmptyRange);

		let is_cur_idx_wrapped = false;
		if (cur_line_rect == null) {
			//new line 
			cur_line_rect = cur_idx_rect
		} else {
			// check if idx rect is wrapped if its top is closer to the cur line rects bottom than the current line rect's top
			let top_dist = Math.abs(cur_idx_rect.top - cur_line_rect.top);
			let bottom_dist = Math.abs(cur_idx_rect.top - cur_line_rect.bottom);
			is_cur_idx_wrapped = bottom_dist < top_dist;
			if (is_cur_idx_wrapped) {
				let pre_wrap_text = getText({ index: range.index, length: i - range.index });
				//debugger;
				range_rects.push(cur_line_rect);
				cur_line_rect = cur_idx_rect;
			} else {
				cur_line_rect = rectUnion(cur_line_rect, cur_idx_rect);
			}
		}
		if (isDocIdxLineEnd(i)) {
			range_rects.push(cur_line_rect);
			cur_line_rect = null;
		}
	}
	if (cur_line_rect) {
		// end of range is before end of line
		range_rects.push(cur_line_rect);
	}
	return range_rects;
}

function getLineIdxAndRectFromPoint(p) {
	let lineCount = getLineCount();
	let blockIdx = getBlockIdxFromPoint(p);
	let start_line_idx = getLineIdx(blockIdx); //0;
	for (var i = start_line_idx; i < lineCount; i++) {
		let line_rect = getLineRect(i);
		if (isPointInRect(line_rect, p)) {
			return [i, line_rect];
		}
	}

	return null;
}

function getBlockIdxFromPoint(p) {
	let p_elm = document.elementFromPoint(p.x, p.y);
	let blot = quillFindBlot(p_elm);
	let block_idx = blot.offset(quill.scroll);
	if (!isNaN(parseInt(block_idx))) {
		debugger;
		const p_range = document.caretRangeFromPoint(p.x, p.y);
		log('block idx NaN, using caretRangeFromPoint which is ' + p_range.startOffset);
		return p_range.startOffset;
	}
	return block_idx;
}

function getElementBlot(elm) {
	let cur_elm = elm;
	while (true) {
		if (cur_elm == null) {
			return null;
		}

		let cur_blot = quillFindBlot(cur_elm);
		if (cur_blot && typeof cur_blot.offset === 'function') {
			return cur_blot;
		}
		cur_elm = cur_elm.parentNode;
	}
}

function getElementDocIdx(elm) {
	let elm_blot = getElementBlot(elm);
	if (elm_blot == null) {
		return 0;
	}
	return elm_blot.offset(quill.scroll);
}

function getDocIdxFromPoint(p, fallbackIdx) {
	if (!p || p.x === undefined || p.y === undefined) {
		debugger;
	}
	fallbackIdx = fallbackIdx ? parseInt(fallbackIdx) : -1;

	let textNode = null;
	let text_node_idx = -1;
	let parent_idx = 0;
	let doc_idx = fallbackIdx;
	let range = null;
	if (document.caretRangeFromPoint) {
		// see https://developer.mozilla.org/en-US/docs/Wt! How are yeb/API/Document/caretRangeFromPoint
		range = document.caretRangeFromPoint(p.x, p.y);
		if (range) {
			textNode = range.startContainer;
			text_node_idx = range.startOffset;
		}
	} else if (document.caretPositionFromPoint) {
		range = document.caretPositionFromPoint(p.x, p.y);
		textNode = range.offsetNode;
		text_node_idx = range.offset;
	}

	if (!isNaN(parseInt(text_node_idx)) && text_node_idx >= 0) {
		let text_blot = quillFindBlot(textNode);
		if (text_blot && typeof text_blot.offset === 'function') {
			//doc_idx = Quill.find(textNode).offset(quill.scroll) + text_node_idx;
			doc_idx = quillFindBlotOffset(textNode) + text_node_idx;
		} else {
			let parent_node = textNode.parentNode;
			while (parent_node != null) {
				if (typeof parent_node.offset === 'function') {
					break;
				}
				parent_node = parent_node.parentNode;
			}
			if (parent_node) {
				try {
					doc_idx = quillFindBlotOffset(textNode.parentNode, true);
				} catch (Ex) {
					debugger;
				}
			}

		}

		return doc_idx;

		if (textNode && textNode.parentElement) {
			let parent_blot = quillFindBlot(textNode.parentElement);
			if (parent_blot && typeof parent_blot.offset === 'function') {
				// get doc idx of block
				parent_idx = parent_blot.offset(quill.scroll);
				let prev_sib_node = textNode.previousSibling;
				if (prev_sib_node != null) {
					let prev_sib_total_offset = 0;
					while (prev_sib_node != null) {
						// get prev siblings offset from parent block to current node
						let prev_sib_offset = 0;
						if (prev_sib_node.nodeType == 3) {
							prev_sib_offset = prev_sib_node.textContent.length;;
						} else {
							let prev_sib_blot = quillFindBlot(prev_sib_node);
							if (prev_sib_blot && typeof prev_sib_blot.offset === 'function') {
								if (prev_sib_node.hasAttribute('templateGuid')) {
									//length of 0
								} else {
									prev_sib_offset = prev_sib_node.innerText.length;
								}
							}
						}
						if (isNaN(parseInt(prev_sib_offset))) {
							debugger;
						} else {
							prev_sib_total_offset += prev_sib_node.textContent.length;
						}
						prev_sib_node = prev_sib_node.previousSibling;
					}
					doc_idx = /*text_node_idx + */parent_idx + prev_sib_total_offset;
				} else {
					doc_idx = text_node_idx + parent_idx;
				}
				doc_idx += range.endOffset;


				if (doc_idx == 0) {
					// NOTE parentElement is editor if p is outside of actual editable space (after line break)
					// but parentElement will still be the enclosed block blot so find the blot offset
					let p_elm = document.elementFromPoint(p.x, p.y);
					if (p_elm) {
						let blot = quillFindBlot(p_elm);
						if (blot && typeof blot.offset === 'function') {
							let block_idx = blot.offset(quill.scroll);
							if (!isNaN(parseInt(block_idx))) {
								doc_idx = block_idx;
							} else {
								debugger;
							}
						}

					} else {
						debugger;
					}

				}

			}
		}
	}
	//log('doc_idx: ' + doc_idx + ' offset: ' + text_node_idx + ' parent_idx: ' + parent_idx);
	return doc_idx;
}

function getBlotAtDocIdx(docIdx) {
	if (!quill) {
		return null;
	}
	let leaf = quill.getLeaf(docIdx);
	if (leaf && leaf.length > 0) {
		return leaf[0];
	}
	return null;
}

function getElementAtDocIdx(docIdx, ignoreTextNode = false, ignoreColGroup = true) {
	if (!isNullOrUndefined(ignoreColGroup) &&
		ignoreColGroup &&
		isDocIdxInTable(docIdx)) {
		 //ignore colgroup elements
		return getTableCellElementAtDocIdx(docIdx);
	}
	let doc_idx_blot = getBlotAtDocIdx(docIdx);
	if (!doc_idx_blot) {
		return getEditorElement();
	}
	if (ignoreTextNode &&
		doc_idx_blot.domNode &&
		doc_idx_blot.domNode.nodeType !== undefined &&
		doc_idx_blot.domNode.nodeType === 3) {
		return doc_idx_blot.domNode.parentNode;
	}
	return doc_idx_blot.domNode;
}

function getBlockElementAtDocIdx(docIdx) {
	let cur_blot = getBlotAtDocIdx(docIdx);
	while (cur_blot != null) {
		if (cur_blot.domNode && cur_blot.domNode.tagName) {
			// is false for text nodes
			let cur_tag_name = cur_blot.domNode.tagName.toLowerCase();
			if (BlockTags.includes(cur_tag_name)) {
				return cur_blot.domNode;
			}
		}
		cur_blot = cur_blot.parent;
	}
	return null;
}

function getHtmlFromDocRange(docRange) {
	let old_sel = getDocSelection();

	IgnoreNextSelectionChange = true;

	setDocSelection(docRange.index, docRange.length, 'silent');
	let rangeHtml = getSelectedHtml();

	IgnoreNextSelectionChange = true;
	setDocSelection(old_sel.index, old_sel.length, 'silent');

	if (IgnoreNextSelectionChange) {
		log('Hey! setSelection by silent doesnt trigger sel change event');
		IgnoreNextSelectionChange = false;
	}
	return rangeHtml;
}

function getElementDocRange(elm) {
	const ignore_text_node = elm.nodeType != 3;
	let elm_blot = quillFindBlot(elm);
	if (!elm_blot) {
		return null;
	}
	let cur_elm_doc_idx = quill.getIndex(elm_blot);
	let elm_length = 0;

	// BUG quill returns bad offset for links, need to expand left/right to find real stuff

	// expand back
	while (cur_elm_doc_idx >= 0) {
		if (cur_elm_doc_idx == 0) {
			break;
		}
		let prev_elm = getElementAtDocIdx(cur_elm_doc_idx - 1, ignore_text_node);
		if (prev_elm == elm || isChildOfElement(prev_elm,elm)) {
			cur_elm_doc_idx--;
		} else {
			break;
		}
	}

	// expand forward
	let max_index = getDocLength() - 1;
	while (cur_elm_doc_idx + elm_length <= max_index) {
		let forward_idx = cur_elm_doc_idx + elm_length;
		if (forward_idx == max_index) {
			break;
		}
		let next_elm = getElementAtDocIdx(forward_idx + 1, ignore_text_node);
		if (next_elm == elm || isChildOfElement(next_elm,elm)) {
			elm_length++;
		} else {
			break;
		}
	}
	return {
		index: cur_elm_doc_idx,
		length: elm_length
	};
}

function getDefaultLineHeight() {
	let editor_height = getElementLineHeight(getEditorElement());
	return editor_height;
}

function getLineHeightAtDocIdx(docIdx) {
	let doc_idx_elm = getElementAtDocIdx(docIdx);
	if (!doc_idx_elm) {
		return getDefaultLineHeight();
	}
	let line_height = getElementLineHeight(doc_idx_elm);
	if (isNaN(line_height)) {
		return getDefaultLineHeight();
	}
	return line_height;
}

function getElementLineHeight(elm) {
	if (isNullOrUndefined(elm)) {
		elm = getEditorElement();
	}
	if (elm.nodeType == 3) {
		elm = elm.parentNode;
	}
	let attrb_val = getElementComputedStyleProp(elm,'line-height');
	let float_val = parseFloat(attrb_val);
	return float_val;
}

function getDocRangeLineRanges(docRange) {
	let line_ranges = [];
	let cur_line_doc_range = null;
	for (var i = 0; i < docRange.length; i++) {
		let doc_idx = docRange.index + i;
		if (cur_line_doc_range == null) {
			// new line
			cur_line_doc_range = { index: doc_idx, length: 1 };
		} else {
			// continue along line
			cur_line_doc_range.length++;
		}
		if (isDocIdxLineEnd(doc_idx) || i == docRange.length - 1) {
			line_ranges.push(cur_line_doc_range);
			cur_line_doc_range = null;
		}
	}
	return line_ranges;
}

function getDocRangeLineIntersectRanges(docRange) {
	// get pre range
	let pre_range_start_doc_idx = getLineStartDocIdx(docRange.index);
	let post_range_end_doc_idx = getLineEndDocIdx(docRange.index + docRange.length);
	return [
		//pre
		{
			index: pre_range_start_doc_idx,
			length: docRange.index - pre_range_start_doc_idx
		},
		//post
		{
			index: docRange.index + docRange.length,
			length: post_range_end_doc_idx - (docRange.index + docRange.length)
		}
	]
}


// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isDocEndInRange(doc_range) {
	let dl = getDocLength();
	if (!doc_range) {
		return false;
	}
	return doc_range.index + doc_range.length >= dl;
}

function isBlockElement(elm) {
	if (elm == null || !elm instanceof HTMLElement) {
		return false;
	}
	let tn = elm.tagName.toLowerCase();
	return BlockTags.includes(tn);
}

function isInlineElement(elm) {
	if (elm == null || !elm instanceof HTMLElement) {
		return false;
	}
	let tn = elm.tagName.toLowerCase();
	return InlineTags.includes(tn);
}

function isDocIdxBlockStart(docIdx) {
	if (isDocIdxAtListItemStart(docIdx)) {
		return true;
	}
	if (isNaN(parseFloat(docIdx))) {
		return false;
	}
	if (docIdx == 0) {
		return true;
	}
	if (docIdx >= getDocLength()) {
		return false;
	}
	let prev_char = getText({ index: docIdx - 1, length: 1 });
	return prev_char == '\n';
}

function isDocIdxBlockEnd(docIdx) {
	if (isNaN(parseFloat(docIdx))) {
		return false;
	}
	if (docIdx == getDocLength()) {
		return true;
	}
	if (docIdx < 0) {
		return false;
	}
	let next_char = getText({ index: docIdx + 1, length: 1 });
	return next_char == '\n';
}

function isDocIdxLineStart(docIdx) {
	if (isNaN(parseFloat(docIdx))) {
		return false;
	}
	if (docIdx == 0) {
		return true;
	}
	if (docIdx >= getDocLength()) {
		return false;
	}
	let idxLine = quill.getLine(docIdx);
	let prevIdxLine = quill.getLine(docIdx - 1);
	return idxLine[0] != prevIdxLine[0];
}

function isDocIdxLineEnd(docIdx) {
	if (isNaN(parseFloat(docIdx))) {
		return false;
	}
	if (docIdx == getDocLength()) {
		return true;
	}
	if (docIdx < 0) {
		return false;
	}
	let idxLine = quill.getLine(docIdx);
	let nextIdxLine = quill.getLine(docIdx + 1);
	return idxLine[0] != nextIdxLine[0];
}

function isDocIdxInRange(docIdx, range) {
	return docIdx >= range.index && docIdx <= range.index + range.length;
}

function isPointOnLine(p, lineIdx) {
	let line_rect = getLineRect(lineIdx);
	return isPointInRect(p);
}

function isPointInRange(p, range) {
	let is_in_range = false;
	let range_rects = getRangeRects(range);
	for (var i = 0; i < range_rects.length; i++) {
		let range_rect = range_rects[i];
		if (isPointInRect(range_rect, p)) {
			is_in_range = true;
		}
	}
	return is_in_range;

	//let p_doc_idx = getDocIdxFromPoint(p, false);
	//let is_in_range = isDocIdxInRange(p_doc_idx,range);
	//return is_in_range;
}
// #endregion State

// #region Actions

function convertTextContentToFormats(isForOle, formats) {
	let sel = isForOle ? getDocSelection(isForOle) : { index: 0, length: getDocLength() };
	let items = [];
	for (var i = 0; i < formats.length; i++) {
		let lwc_format = formats[i].toLowerCase();
		let data = null;
		if (isHtmlFormat(lwc_format)) {
			data = getHtml(sel);
			if (lwc_format == 'html format') {
				// NOTE web html doesn't use fragment format
				data = createHtmlClipboardFragment(data);
			}
		} else if (isPlainTextFormat(lwc_format)) {
			if (isContentATable()) {
				data = getTableCsv('Text', null, isForOle);
			} else {
				data = getText(sel, isForOle);
			}
			if (isForOle && isDocEndInRange(sel)) {
				data = trimQuillTrailingLineEndFromText(data);
			}
		} else if (isCsvFormat(lwc_format) && isContentATable()) {
			// TODO figure out handling table selectinn logic and check here 
			data = getTableCsv('Text', null, isForOle);
		} else if (isImageFormat(lwc_format)) {
			// trigger async screenshot notification where host needs 
			// to null and wait for value to avoid async issues
			getDocRangeAsImageAsync(sel)
				.then((result) => {
					onCreateContentScreenShot_ntf(result);
				});
			data = PLACEHOLDER_DATAOBJECT_TEXT;
		} 
		if (!data || data == '') {
			continue;
		} 
		let item = {
			format: formats[i],
			data: data
		};
		items.push(item);
	}
	return items;
}


function appendTextContentData(data) {
	data = data == null ? '' : data;
	const append_range = getAppendDocRange();
	let dt = new DataTransfer();
	// NOTE since data is result of ci builder it will always be html
	dt.setData('text/html', data);	
	performDataTransferOnContent(dt, append_range, null, 'api', 'Appended');
	scrollToAppendIdx();

	onContentChanged_ntf();
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers