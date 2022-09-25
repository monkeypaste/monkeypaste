var IsDragging = false;
var DragRange = null;

var SelIdxBeforeDrag = -1;
var DocLengthBeforeDrag = -1;

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
            // overlay drag is full content so select all
            SelIdxBeforeDrag = sel ? sel.index : -1;
            selectAll();
            sel = getEditorSelection();
        }

        if (!sel || sel.length == 0) {
            e.preventDefault();
            e.stopPropagation();
            SelIdxBeforeDrag = -1;
            return false;
        }

        log('drag start');
        DragRange = sel;
        IsDragging = true;
        DocLengthBeforeDrag = getDocLength();
        e.stopPropagation();

        e.dataTransfer.effectAllowed = 'copyMove';

        let textStr = getText(sel,true);
        e.dataTransfer.setData('text/plain', textStr);

        let htmlStr = getHtml(sel);
        e.dataTransfer.setData('text/html', htmlStr);

        let deltaJsonStr = getDeltaJson(sel, true);
        e.dataTransfer.setData('application/json', deltaJsonStr);

        onDragStartOrEnd_ntf(true);

        return true;
    }

    function handleDragEnd(e) {
        log('drag end');

        if (e && e.dataTransfer.dropEffect == 'move') {
            // 'move' should imply it was an internal drop
            if (DropIdx >= 0) {

                let drop_doc_length_delta = getDocLength() - DocLengthBeforeDrag;
                // this should only happen for internal drop
                if (DropIdx < DragRange.index) {
                    // when drop is before drag sel adjust drag range to clear the move
                    DragRange.index += drop_doc_length_delta;
                }
			}
            setTextInRange(DragRange, '','user');
        }

        resetDragDrop(false);

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