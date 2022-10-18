// #region Globals

const MIN_DRAG_DIST = 10;

var IsDragging = false;

var WasDragCanceled = false;

var DragSelectionRange = null;

var DragStartScrollOffset = null;

var SelIdxBeforeDrag = -1;
var DocLengthBeforeDrag = -1;


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

// #endregion State

// #region Actions

function resetDrag(fromHost = false, wasDragCanceled = false) {

    // NOTE only flagging drag cancel in drag source to avoid confuse other state changes
    // when true, window keyup handler ignores decrease focus 
    WasDragCanceled = wasDragCanceled;
    //if (!WasDragCanceled) {
    // NOTE don't know why but selection is reset when drag cancels
    // and document sel change will null this after it restores sel
    DragSelectionRange = null;
    //}
    IsDragging = false
    SelIdxBeforeDrag = -1;
    DocLengthBeforeDrag = -1;

    log('drag reset: ' + (fromHost ? "FROM HOST" : "INTERNALLY"));
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
// #endregion Actions

// #region Event Handlers

function onDragStart(e) {
    WasDragCanceled = false;

    if (IsDragging) {
        return;
    }
    if (!WindowMouseDownLoc || !SelectionOnMouseDown) {
        return;
    }
    let is_valid_drag_gesture = isPointInRange(WindowMouseDownLoc, SelectionOnMouseDown);
    if (!is_valid_drag_gesture) {
        log('drag rejected, mouse down' + JSON.stringify(WindowMouseDownLoc) + ' not in range' + JSON.stringify(SelectionOnMouseDown));
        e.preventDefault();
        //e.stopPropagation();
        return false;
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

    DragSelectionRange = sel;

    log('drag start sel: ', DragSelectionRange);

    IsDragging = true;
    DocLengthBeforeDrag = getDocLength();
    e.stopPropagation();

    if (ContentItemType == 'Text') {
        e.dataTransfer.effectAllowed = 'copyMove';

        let textStr = getText(sel, true);
        e.dataTransfer.setData('text/plain', textStr);

        e.dataTransfer.setData('text/html', getTextContentData());
    } else if (ContentItemType == 'FileList') {
        e.dataTransfer.effectAllowed = 'copy';

        e.dataTransfer.setData('text\plain', getFileListContentData());
    } else if (ContentItemType == 'Image') {
        e.dataTransfer.effectAllowed = 'copy';

        e.dataTransfer.setData('text\plain', getImageContentData());
    }



    return true;
}

function onDragEnd(e) {
    let fromHost = e.fromHost ? e.fromHost : false;
    log('drag end fromHost: ', fromHost);
    if (IsDropping && fromHost) {
        log('ignoring host drag end');
        return;
    }
    log('drag end not rejected and finishing..');
    let selfDrop = DropIdx >= 0;
    if (selfDrop && e && e.dataTransfer.dropEffect.toLowerCase().includes('move')) {
        // 'move' should imply it was an internal drop
        let drop_doc_length_delta = getDocLength() - DocLengthBeforeDrag;
        // this should only happen for internal drop
        if (DropIdx < DragSelectionRange.index) {
            // when drop is before drag sel adjust drag range to clear the move
            DragSelectionRange.index += drop_doc_length_delta;
        }
        setTextInRange(DragSelectionRange, '', 'user');
        DropIdx = -1;
    }

    //deselectAll(SelIdxBeforeDrag >= 0 ? SelIdxBeforeDrag : DragSelectionRange ? DragSelectionRange.index : 0);
    let wasCanceled = e.dataTransfer.dropEffect == 'none';
    if (!wasCanceled && fromHost) {
        // just to be sure since drop target maybe external check msg from host for cancel
        wasCanceled = e.wasCancel;
    }

    resetDrag(fromHost, wasCanceled);
}
// #endregion Event Handlers