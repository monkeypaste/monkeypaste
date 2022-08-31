var InlineTags = ['span', 'a', 'em', 'strong', 'u', 's', 'sub', 'sup', 'img'];
var BlockTags = ['p', 'ol', 'ul', 'li', 'div', 'table', 'colgroup', 'col', 'tbody', 'tr', 'td', 'iframe','blockquote']
var CopyItemId = -1;
var CopyItemType = 'text';

function initContent(itemHtml) {
    setHtml(itemHtml);
}

function getContentWidth() {
	var bounds = quill.getBounds(0, quill.getLength());
	bounds = cleanRect(bounds);
    return bounds.width;
}

function getContentHeight() {
	var bounds = quill.getBounds(0, quill.getLength());
	bounds = cleanRect(bounds);
    return bounds.height;
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

function isDocIdxLineStart(docIdx) {
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
	let lineIdx = quill.getLine(docIdx);
	return lineIdx;
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

function getCharacterRect(docIdx) {
	let docIdx_rect = quill.getBounds(docIdx, 1);

	docIdx_rect = cleanRect(docIdx_rect);
	return docIdx_rect;
}

function getLineRect(lineIdx) {
	let line_doc_range = getLineDocRange(lineIdx);
	let line_start_rect = getCharacterRect(line_doc_range[0]);
	let line_end_rect = getCharacterRect(line_doc_range[1]);
	let line_rect = rectUnion(line_start_rect, line_end_rect);

	let editor_rect = getEditorRect();
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


var GetPointIdxInvokeCount = 0;

function isPointOnLine(p, lineIdx) {
	let line_rect = getLineRect(lineIdx);
	return isPointInRect(p);
}

function getPointDocLine(p) {
	let lineCount = getLineCount();
	for (var i = 0; i < lineCount; i++) {
		let line_rect = getLineRect(i);
		if (isPointInRect(line_rect, p)) {
			return [i,line_rect];
		}
	}

	return null;
}

function getEditorIndexFromPoint(p, snapToLine = true) {
	if (!p) {
		return -1;
	}
	
	let editor_rect = getEditorRect();

	if (!isPointInRect(editor_rect,p)) {
		return -1;
	}

	let point_line = getPointDocLine(p);
	if (!point_line || point_line[0] < 0 || !point_line[1]) {
		return -1;
	}
	let line_idx = point_line[0];
	let line_rect = point_line[1];
	let line_doc_range = getLineDocRange(line_idx);
	for (var i = line_doc_range[0]; i <= line_doc_range[1]; i++) {
		let i_rect = getCharacterRect(i);
		i_rect.top = line_rect.top;
		i_rect.bottom = line_rect.bottom;

		if (snapToLine) {
			//inflate edge rects to editor bounds
			if (i == line_doc_range[0]) {
				i_rect.left = editor_rect.left;
			}
			if (i == line_doc_range[1]) {
				i_rect.right = editor_rect.right;
			}
		}
		i_rect = cleanRect(i_rect);
		if (isPointInRect(i_rect, p)) {
			return i;
		}
	}
	return -1;
}

function getEditorIndexFromPoint_ByLine(p, fallbackIdx) {
	// this version first checks for closest line to p.y
	fallbackIdx = !fallbackIdx ? -1 : fallbackIdx;

	if (!p) {
		return fallbackIdx;
	}

	let editorRect = document.getElementById("editor").getBoundingClientRect();
	let erect = { x: 0, y: 0, w: editorRect.width, h: editorRect.height };

	let ex = p.x - editorRect.left; //x position within the element.
	let ey = p.y - editorRect.top; //y position within the element.
	let ep = { x: ex, y: ey };
	//log('editor pos: ' + ep.x + ' '+ep.y);
	if (!isPointInRect(erect, ep)) {
		return fallbackIdx;
	}

	let closestLineIdx = -1;
	let closestLineDist = Number.MAX_SAFE_INTEGER;
	let docLines = quill.getLines(0, quill.getLength());

	for (var i = 0; i < docLines.length; i++) {
		let l = docLines[i];
		let lrect = quill.getBounds(quill.getIndex(l));
		let lineY = lrect.top + lrect.height / 2;
		let curYDist = Math.abs(lineY - ey);
		if (curYDist < closestLineDist) {
			closestLineIdx = i;
			closestLineDist = curYDist;
		}
	}
	if (closestLineIdx < 0) {
		return fallbackIdx;
	}

	//log("closest line idx: " + closestLineIdx);

	let lineMinDocIdx = quill.getIndex(docLines[closestLineIdx]);
	let nextLineMinDocIdx = quill.getLength();
	if (closestLineIdx < docLines.length - 1) {
		nextLineMinDocIdx = quill.getIndex(docLines[closestLineIdx + 1]);
	}

	let closestIdx = -1;
	let closestDist = Number.MAX_SAFE_INTEGER;
	for (var i = lineMinDocIdx; i < nextLineMinDocIdx; i++) {
		let irect = quill.getBounds(i, 1);
		let ix = irect.left;
		let idist = Math.abs(ix - ex);
		if (idist < closestDist) {
			closestDist = idist;
			closestIdx = i;
		}
	}

	if (closestIdx < 0) {
		return fallbackIdx;
	}

	return closestIdx;
}

function getElementAtIdx(docIdx) {
	let leafNode = quill.getLeaf(docIdx)[0].domNode;
	let leafElementNode =
		leafNode.nodeType == 3 ? leafNode.parentElement : leafNode;
	return leafElementNode;
}
