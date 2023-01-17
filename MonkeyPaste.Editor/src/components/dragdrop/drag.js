// #region Globals

const MIN_DRAG_DIST = 10;

var WasNoSelectBeforeDragStart = false;
var DragDomRange = null;
var CurDragTargetElm = null;
var DragItemElms = [];

// #endregion Globals

// #region Life Cycle

function initDrag() {
    DragItemElms = [getEditorContainerElement(), getDragOverlayElement()];
    for (var i = 0; i < DragItemElms.length; i++) {
        let item = DragItemElms[i];
        item.addEventListener('dragstart', onDragStart, true);
        item.addEventListener('drag', onDrag, true);
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

function enableDragOverlay() {
    getDragOverlayElement().classList.remove('no-hit-test');
    getDragOverlayElement().classList.add('hit-testable');
    getDragOverlayElement().setAttribute('draggable', true);

    getEditorContainerElement().setAttribute('draggable', false);
}

function disableDragOverlay() {
    getDragOverlayElement().classList.add('no-hit-test');
    getDragOverlayElement().classList.remove('hit-testable');
    getDragOverlayElement().setAttribute('draggable', false);

    getEditorContainerElement().setAttribute('draggable', true);
}

// #endregion Actions

// #region Event Handlersconsidered 

function onDragStart(e) {
    // dragstart doesn't have buttons set so updateState'll screw up
    updateWindowMouseState(e);

    if (isDragging()) {
        return;
    }
    
    if (!isSubSelectionEnabled()) {
        WasNoSelectBeforeDragStart = true;
        //enableSubSelection();
        selectAll();
    } else {
        WasNoSelectBeforeDragStart = false;
    }

    let sel = getDocSelection();

    if (!sel || sel.length == 0 && ContentItemType == 'Text') {
        log('drag start rejected by selection state. selectable but w/o range');
        WasNoSelectBeforeDragStart = false;
        e.preventDefault();
        e.stopPropagation();
        return false;
    }
    //DragDomRange = convertDocRangeToDomRange(sel);
    //BlurredSelectionRects = getRangeRects(sel);

    CurDragTargetElm = e.target;

    log('drag start. sel: ' + sel);
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
function onDrag(e) {

}
function onDragEnd(e) {
    log('drag end');
    updateWindowMouseState(e);
    CurDragTargetElm = null;
    IsShiftDown = false;
    IsCtrlDown = false;
    IsAltDown = false;
    DragDomRange = null;
    BlurredSelectionRects = null;
    
    if (WasNoSelectBeforeDragStart) {
        resetSelection();
        disableSubSelection();
        WasNoSelectBeforeDragStart = false;
    }
    drawOverlay();
}
// #endregion Event Handlers