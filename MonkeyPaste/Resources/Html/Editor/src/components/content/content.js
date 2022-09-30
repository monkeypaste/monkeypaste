var ContentHandle = null;
var ContentItemType = 'Text';

function loadContent(contentHandle, contentType, contentData, isPasteRequest) {
	resetDragDrop();

	ContentHandle = contentHandle;
	ContentItemType = contentType;

	if (ContentItemType.includes('.')) {
		log('hey item type is ' + ContentItemType);
		ContentItemType = ContentItemType.split('.')[1];
		log('now item type is ' + ContentItemType);
	}

	// enusre IsLoaded is false so msg'ing doesn't get clogged up
	IsLoaded = false;

	enableReadOnly();

	log('Editor loaded');

	if (ContentItemType == 'Image') {
		initImageContent(contentData);
	} else if (ContentItemType == 'FileList') {
		initFileListContent(contentData);
	} else if (ContentItemType == 'Text') {
		loadTextContent(contentData, isPasteRequest);
	}

	updateAllSizeAndPositions();
	IsLoaded = true;
	// initial load content length ntf
	onContentLengthChanged_ntf();
}

function getContentWidth() {
	var bounds = quill.getBounds(0, quill.getLength());
	bounds = cleanRect(bounds);
    return parseFloat(bounds.width);
}

function getContentHeight() {
	var bounds = quill.getBounds(0, quill.getLength());
	bounds = cleanRect(bounds);
    return parseFloat(bounds.height);
}

function getContentRange() {
	return { index: 0, length: getDocLength() };
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

function isDocIdxInListItem(docIdx) {
	let doc_idx_elm = getElementAtDocIdx(docIdx);
	while (doc_idx_elm != null) {
		if (doc_idx_elm && doc_idx_elm.tagName == 'LI') {
			return true;
		}
		doc_idx_elm = doc_idx_elm.parentNode;
	}
	return false;
}
function isDocIdxAtEmptyListItem(docIdx) {
	let block_elm = getBlockElementAtDocIdx(docIdx);
	if (block_elm.tagName == 'LI') {
		let doc_idx_elm = getElementAtDocIdx(docIdx);
		return doc_idx_elm && doc_idx_elm.tagName == 'BR';
	}
	return false;
}

function isDocIdxLineStart(docIdx) {
	if (isNaN(parseFloat(docIdx))) {
		return false;
	}
    if (docIdx == 0) {
        return true;
    }
    if (docIdx >= quill.getLength()) {
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
    if (docIdx == quill.getLength()) {
        return true;
    }
    if (docIdx < 0) {
        return false;
    }
    let idxLine = quill.getLine(docIdx);
    let nextIdxLine = quill.getLine(docIdx + 1);
    return idxLine[0] != nextIdxLine[0];
}


function getLineStartDocIdx(docIdx) {
    //let lineStartDocIdx = 0;
    //let pt = getText();
    //for (var i = 0; i < docIdx; i++) {
    //    if (pt[i] == '\n') {
    //        lineStartDocIdx = Math.min(i + 1, pt.length);
    //    }
    //}
	//return lineStartDocIdx;
	docIdx = docIdx < 0 ? 0 : docIdx >= quill.getLength() ? quill.getLength() - 1 : docIdx;
	while (!isDocIdxLineStart(docIdx)) {
		docIdx--;
	}
	return docIdx;
}

function getLineEndDocIdx(docIdx) {
    //let pt = getText();
    //let lineEndDocIdx = pt.length - 1;
    //for (var i = pt.length - 1; i >= docIdx; i--) {
    //    if (pt[i] == '\n') {
    //        lineEndDocIdx = i;
    //    }
    //}
    //return lineEndDocIdx;
	docIdx = docIdx < 0 ? 0 : docIdx >= quill.getLength() ? quill.getLength() - 1 : docIdx;
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
	let maxDocIdx = quill.getLength() - 1;
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

function getDocLength() {
	return quill.getLength();
}

function getCharacterRect(docIdx, inflateX = false, inflateY = false) {
	if (isNaN(parseFloat(docIdx))) {
		return cleanRect();
	}
	

	let docIdx_rect = quill.getBounds(docIdx, 1);
	docIdx_rect = editorToScreenRect(docIdx_rect);


	if (inflateX || inflateY) {
		inflateCharacterRect(docIdx, docIdx_rect, inflateX, inflateY);
	} else {
		docIdx_rect = cleanRect(docIdx_rect);
	}
	return docIdx_rect;
}

function isDocIdxInRange(docIdx, range) {
	return docIdx >= range.index && docIdx <= range.index + range.length;
}

function inflateCharacterRect(docIdx, docIdx_rect, inflateX, inflateY) {
	docIdx_rect = cleanRect(docIdx_rect);

	if (inflateX || inflateY) {
		let editor_rect = getEditorContainerRect();
		if (inflateX) {
			if (isDocIdxLineStart(docIdx)) {
				//inflate first line char to editor left
				docIdx_rect.left = editor_rect.left;
			} else if (docIdx > 0) {
				// inflate  char to previous uninflated rect left
				let prev_uninflated_docIdx_rect = getCharacterRect(docIdx - 1);
				docIdx_rect.left = prev_uninflated_docIdx_rect.right;
			}

			if (isDocIdxLineEnd(docIdx)) {
				docIdx_rect.right = editor_rect.right;
			} else if (docIdx < getDocLength() - 1) {
				let next_uninflated_docIdx_rect = getCharacterRect(docIdx + 1);
				docIdx_rect.right = next_uninflated_docIdx_rect.right;
			}
		}
		if (inflateY) {
			let line_idx = getLineIdx(docIdx);
			let line_count = getLineCount();
			
			if (line_idx == 0) {
				docIdx_rect.top = editor_rect.top;
			} else {
				let prev_line_rect = getLineRect(line_idx - 1);
				docIdx_rect.top = prev_line_rect.bottom;
			}

			if (line_idx == line_count - 1) {
				docIdx_rect.bottom = editor_rect.bottom;
			}
		}
	}
	docIdx_rect = cleanRect(docIdx_rect);
	return docIdx_rect;
}

function getLineRect(lineIdx,snapToEditor = true) {
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

function getRangeRects(range,inflateX = null, inflateY = null) {
	let range_rects = [];
	if (!range || range.length == 0) {
		return range_rects;
	}
	let cur_line_rect = null;
	for (var i = range.index; i < range.index + range.length; i++) {
		let cur_idx_rect = getCharacterRect(i,inflateX,inflateY);
		if (cur_line_rect == null) {
			//new line
			cur_line_rect = cur_idx_rect
		} else {
			cur_line_rect = rectUnion(cur_line_rect, cur_idx_rect);
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

function isPointOnLine(p, lineIdx) {
	let line_rect = getLineRect(lineIdx);
	return isPointInRect(p);
}

function getLineIdxAndRectFromPoint(p) {
	let lineCount = getLineCount();
	let blockIdx = getBlockIdxFromPoint(p);
	let start_line_idx = getLineIdx(blockIdx); //0;
	for (var i = start_line_idx; i < lineCount; i++) {
		let line_rect = getLineRect(i);
		if (isPointInRect(line_rect, p)) {
			return [i,line_rect];
		}
	}

	return null;
}

function getBlockIdxFromPoint(p) {
	let p_elm = document.elementFromPoint(p.x, p.y);
	let blot = Quill.find(p_elm);
	let block_idx = blot.offset(quill.scroll);
	if (!isNaN(parseInt(block_idx))) {
		debugger;
		const p_range = document.caretRangeFromPoint(p.x, p.y);
		log('block idx NaN, using caretRangeFromPoint which is ' + p_range.startOffset);
		return p_range.startOffset;
	}
	return block_idx;
}

function isPointInRange(p, range, snapToBlock) {
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
		let text_blot = Quill.find(textNode);
		if (text_blot && typeof text_blot.offset === 'function') {
			doc_idx = Quill.find(textNode).offset(quill.scroll) + text_node_idx;
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
					doc_idx = Quill.find(textNode.parentNode, true).offset(quill.scroll);
				} catch (Ex) {
					debugger;
				}
			}		

		}

		return doc_idx;
		if (textNode && textNode.parentElement) {
			let parent_blot = Quill.find(textNode.parentElement);
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
							let prev_sib_blot = Quill.find(prev_sib_node);
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
						let blot = Quill.find(p_elm);
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
	let leaf = quill.getLeaf(docIdx);
	if (leaf && leaf.length > 0) {
		return leaf[0];
	}
	return null;
}

function getElementAtDocIdx(docIdx) {
	let leafNode = getNodeAtDocIdx(docIdx);
	let leafElementNode =
		leafNode.nodeType == 3 ? leafNode.parentElement : leafNode;
	return leafElementNode;
}
function getNodeAtDocIdx(docIdx) {
	let leafNode = quill.getLeaf(docIdx)[0].domNode;
	return leafNode;
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
	let old_sel = getEditorSelection();

	IgnoreNextSelectionChange = true;

	setEditorSelection(docRange.index, docRange.length, 'silent');
	let rangeHtml = getSelectedHtml();

	IgnoreNextSelectionChange = true;
	setEditorSelection(old_sel.index, old_sel.length, 'silent');

	if (IgnoreNextSelectionChange) {
		log('Hey! setSelection by silent doesnt trigger sel change event');
		IgnoreNextSelectionChange = false;
	}
	return rangeHtml;
}



