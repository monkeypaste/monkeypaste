
const MIN_DRAG_DIST = 10;

function initDragDrop() {
    initDrop();
    initDrag();
}

function resetDragDrop(fromHost = false) {
    if (IsDragging) {
        deselectAll(SelIdxBeforeDrag >= 0 ? SelIdxBeforeDrag : DragRange ? DragRange.index : 0);
    }

    DragRange = null;
    IsDragging = false
    SelIdxBeforeDrag = -1;
    DocLengthBeforeDrag = -1;

    IsDropping = false;
    DropIdx = -1;

    IsCtrlDown = false;
    IsAltDown = false
    IsShiftDown = false;

    for (var i = 0; i < DropItemElms.length; i++) {
        DropItemElms[i].classList.remove('drop');
	}

    updateAllSizeAndPositions();

    
    drawOverlay();

    log('dragDrop reset: ' + (fromHost ? "FROM HOST" : "INTERNALLY"));
}
