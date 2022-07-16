var InlineTags = ['span', 'a', 'em', 'strong', 'u', 's', 'sub', 'sup', 'img'];
var BlockTags = ['p', 'ol', 'ul', 'li', 'div', 'table', 'colgroup', 'col', 'tbody', 'tr', 'td', 'iframe','blockquote']

function initContent(itemHtml) {
    setHtml(itemHtml);
}

function getContentWidth() {
    var bounds = quill.getBounds(0, quill.getLength());
    return bounds.width;
}

function getContentHeight() {
    var bounds = quill.getBounds(0, quill.getLength());
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

