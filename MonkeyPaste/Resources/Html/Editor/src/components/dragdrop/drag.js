var IsDragging = false;
var DragRange = null;

var SelIdxBeforeDrag = -1;

var IsCtrlDown = false; //duplicate
var IsShiftDown = false; //split 
var IsAltDown = false; // w/ formatting (as html)? ONLY formating? dunno

function initDrag() {

    function handleDragStart(e) {
        if (IsDragging) {
            return;
        }
        let sel = getEditorSelection();

        if (e.target.id == 'dragOverlay') {
            SelIdxBeforeDrag = sel ? sel.index : -1;
            selectAll();
            sel = getEditorSelection();
        }

        if (!sel || sel.length == 0) {
            e.preventDefault();
            e.stopPropagation();
            return false;
        }

        log('drag start');
        DragRange = sel;
        IsDragging = true;
        e.stopPropagation();

        e.dataTransfer.effectAllowed = 'copyMove';

        let textStr = getText(sel,true);
        e.dataTransfer.setData('text/plain', textStr);

        let htmlStr = getHtml(sel);
        e.dataTransfer.setData('text/html', htmlStr);

        let deltaJsonStr = getDeltaJson(sel, true);
        e.dataTransfer.setData('application/json', deltaJsonStr);

        return true;
    }

    function handleDragEnd(e) {
        log('drag end');
        if (e.target.id == 'dragOverlay') {
            let desel = { index: 0, length: 0 };
            if (SelIdxBeforeDrag >= 0) {
                desel.index = SelIdxBeforeDrag;
            }
            SelIdxBeforeDrag = -1;
            setEditorSelection(desel);
        }
        DragRange = null;
        IsDragging = false;
        IsCtrlDown = false;
        IsAltDown = false
        IsShiftDown = false;


        drawOverlay();
    }

    let items = [getEditorContainerElement(), getDragOverlayElement()];
    items.forEach(function (item) {
        item.addEventListener('dragstart', handleDragStart, true);
        item.addEventListener('dragend', handleDragEnd);
    });
}

function isDragging() {
    return DragRange != null;
}

function enableDragOverlay() {
    getDragOverlayElement().classList.remove('drag-overlay-disabled');
    getDragOverlayElement().classList.add('drag-overlay-enabled');
    getDragOverlayElement().setAttribute('draggable', true);
}

function disableDragOverlay() {
    getDragOverlayElement().classList.add('drag-overlay-disabled');
    getDragOverlayElement().classList.remove('drag-overlay-enabled');
    getDragOverlayElement().setAttribute('draggable', false);
}

function getDragOverlayElement() {
    return document.getElementById('dragOverlay');
}