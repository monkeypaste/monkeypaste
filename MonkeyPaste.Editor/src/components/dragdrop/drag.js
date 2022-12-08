// #region Globals

const MIN_DRAG_DIST = 10;

//var IsDragging = false;

//var WasDragCanceled = false;

//var DragSelectionRange = null;

//var DragStartScrollOffset = null;

//var SelIdxBeforeDrag = -1;
//var DocLengthBeforeDrag = -1;

var CurDragTargetElm = null;


var DragItemElms = [];
// #endregion Globals

// #region Life Cycle

function initDrag() {
    DragItemElms = [getEditorContainerElement(), getDragOverlayElement()];
    for (var i = 0; i < DragItemElms.length; i++) {
        let item = DragItemElms[i];
        item.addEventListener('dragstart', onDragStart, true);
        item.addEventListener('dragend', onDragEnd);
    }
}
// #endregion Life Cycle

// #region Getters

function getDragOverlayElement() {
    return document.getElementById('dragOverlay');
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isDragging() {
    return CurDragTargetElm != null;
}

// #endregion State

// #region Actions

function resetDrag(fromHost = false, wasDragCanceled = false) {

    // NOTE only flagging drag cancel in drag source to avoid confuse other state changes
    // when true, window keyup handler ignores decrease focus 
    CurDragTargetElm = null;

    //WasDragCanceled = wasDragCanceled;
    //if (!WasDragCanceled) {
    // NOTE don't know why but selection is reset when drag cancels
    // and document sel change will null this after it restores sel
    //DragSelectionRange = null;
    //resetSelection();
    //}
    //IsDragging = false
    //SelIdxBeforeDrag = -1;
    //DocLengthBeforeDrag = -1;

    log(`drag reset. fromHost: ${fromHost} wasCancel: ${wasDragCanceled}`);
    //drawOverlay();
}

function enableDragOverlay() {
    getDragOverlayElement().classList.remove('drag-overlay-disabled');
    getDragOverlayElement().classList.add('drag-overlay-enabled');
    getDragOverlayElement().setAttribute('draggable', true);

    getEditorContainerElement().setAttribute('draggable', false);
}

function disableDragOverlay() {
    getDragOverlayElement().classList.add('drag-overlay-disabled');
    getDragOverlayElement().classList.remove('drag-overlay-enabled');
    getDragOverlayElement().setAttribute('draggable', false);

    getEditorContainerElement().setAttribute('draggable', true);
}
// #endregion Actions

// #region Event Handlers

function onDragStart(e) {
    log('has focus: ' + quill.hasFocus());

    updateWindowMouseState(e);

    //WasDragCanceled = false;

    if (isDragging()) {
        return;
    }

    let sel = getDocSelection();

    if (e.target.id == 'dragOverlay') {
        // overlay drag is full content so select all
        //SelIdxBeforeDrag = sel ? sel.index : -1;
        selectAll();
        sel = getDocSelection();
    }

    if (!sel || sel.length == 0) {
        e.preventDefault();
        e.stopPropagation();
        //SelIdxBeforeDrag = -1;
        return false;
    }
    CurDragTargetElm = e.target;

    //DragSelectionRange = sel;

    log('drag start sel: ', sel);

    //IsDragging = true;
    //DocLengthBeforeDrag = getDocLength();
    e.stopPropagation();

    if (ContentItemType == 'Text') {
        e.dataTransfer.effectAllowed = 'copyMove';
    } else if (ContentItemType == 'FileList') {
        e.dataTransfer.effectAllowed = 'copy';
    } else if (ContentItemType == 'Image') {
        e.dataTransfer.effectAllowed = 'copy';
    }
    return true;
}

function onDragEnd(e) {
    updateWindowMouseState(e);
    CurDragTargetElm = null;
    IsShiftDown = false;
    IsCtrlDown = false;
    IsAltDown = false;

    //let fromHost = e.fromHost ? e.fromHost : false;
    //log('drag end fromHost: ', fromHost);
    //if (isDropping() && fromHost) {
    //    log('ignoring host drag end');
    //    return;
    //}
    ////let selfDrop = DropIdx >= 0;
    ////log('drag end not rejected and finishing...Self drop: ' + selfDrop);
    ////if (selfDrop && e && e.dataTransfer.dropEffect.toLowerCase().includes('move')) {
    ////    // 'move' should imply it was an internal drop
    ////    let drop_doc_length_delta = getDocLength() - DocLengthBeforeDrag;
    ////    // this should only happen for internal drop
    ////    if (DropIdx < DragSelectionRange.index) {
    ////        // when drop is before drag sel adjust drag range to clear the move
    ////        DragSelectionRange.index += drop_doc_length_delta;
    ////    }
    ////    setTextInRange(DragSelectionRange, '', 'user');
    ////    DropIdx = -1;
    ////}

    ////deselectAll(SelIdxBeforeDrag >= 0 ? SelIdxBeforeDrag : DragSelectionRange ? DragSelectionRange.index : 0);
    //let wasCanceled = e.dataTransfer.dropEffect == 'none';
    //if (!wasCanceled && fromHost) {
    //    // just to be sure since drop target maybe external check msg from host for cancel
    //    wasCanceled = e.wasCancel;
    //}

    //resetDrag(fromHost, wasCanceled);
}
// #endregion Event Handlers