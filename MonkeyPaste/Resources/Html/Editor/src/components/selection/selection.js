const DefaultSelectionBgColor = 'lightblue';
const DefaultSelectionFgColor = 'black';
const DefaultCaretColor = 'black';

var BlurredSelectionRange = null;
var BlurredSelectionRects = null;

function setTextSelectionBgColor(bgColor) {
    document.body.style.setProperty('--selbgcolor', bgColor);
}

function getCaretLine(forceDocIdx = -1) {
    let caret_doc_idx = forceDocIdx;
    if (caret_doc_idx < 0) {
        let sel = getEditorSelection();
        if (!sel) {
            log('no selection, cannot get caret line');
            return;
        }
        if (sel.length > 0) {
            log('warning should only get caret line when selection empty')
            debugger;
            return;
        }
        caret_doc_idx = sel.index;
	}
    
    let editor_rect = getEditorContainerRect();
    let caret_rect = getCharacterRect(caret_doc_idx);

    let caret_line = { x1: caret_rect.left, y1: caret_rect.top, x2: caret_rect.left, y2: caret_rect.bottom };
    let left_clamp = 0;
    let right_clamp = editor_rect.width;
    if (caret_line.x1 < 0) {
        caret_line.x1 = left_clamp;
        caret_line.x2 = left_clamp;
        log('caret_line was < editor_rect.left: ' + left_clamp);
    } else if (caret_line.x1 > right_clamp) {
        caret_line.x1 = right_clamp;
        caret_line.x2 = right_clamp;
        log('caret_line was > editor_rect.right: ' + right_clamp);
    }
    if (caret_line.x1 < 0 || caret_line.x2 < 0) {
        caret_line.x1 = 0;
        caret_line.x2 = 0;
    }
    caret_line = cleanLine(caret_line);
    return caret_line;
}

function getTextSelectionBgColor() {
    let bodyStyles = window.getComputedStyle(document.body);
    let bg_color = bodyStyles.getPropertyValue('--selbgcolor');
    return bg_color;
}

function setTextSelectionFgColor(fgColor) {
    document.body.style.setProperty('--selfgcolor', fgColor);
}

function getTextSelectionFgColor() {
    let bodyStyles = window.getComputedStyle(document.body);
    let fg_color = bodyStyles.getPropertyValue('--selfgcolor');
    return bg_color;
}

function setCaretColor(caretColor) {
    getEditorElement().style.caretColor = caretColor;
}

function getCaretColor() {
    return getEditorElement().style.caretColor;
}