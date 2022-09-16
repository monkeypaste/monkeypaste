var BlurredSelectionRange = null;
var BlurredSelectionRects = null;

function setTextSelectionBgColor(bgColor) {
    document.body.style.setProperty('--selbgcolor', bgColor);
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