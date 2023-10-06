// #region Life Cycle

function initDrag() {
    globals.DragItemElms = [getEditorContainerElement()/*, getDragOverlayElement()*/];
    for (var i = 0; i < globals.DragItemElms.length; i++) {
        let item = globals.DragItemElms[i];
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
    return globals.CurDragTargetElm != null;
}

// #endregion State

// #region Actions

function enableDragOverlay() {
    //getDragOverlayElement().classList.remove('no-hit-test');
    //getDragOverlayElement().classList.add('hit-testable');
    //getDragOverlayElement().setAttribute('draggable', true);

    //getEditorContainerElement().setAttribute('draggable', false);
}

function disableDragOverlay() {
    //getDragOverlayElement().classList.add('no-hit-test');
    //getDragOverlayElement().classList.remove('hit-testable');
    //getDragOverlayElement().setAttribute('draggable', false);

    //getEditorContainerElement().setAttribute('draggable', true);
}

// #endregion Actions

// #region Event Handlersconsidered 

function onDragStart(e) {
    // dragstart doesn't have buttons set so updateState'll screw up
    updateWindowMouseState(e,'dragStart');

    if (isDragging()) {
        return;
    }
    
    if (!isSubSelectionEnabled()) {
        globals.WasNoSelectBeforeDragStart = true;
        //enableSubSelection();
        selectAll();
    } else {
        globals.WasNoSelectBeforeDragStart = false;
    }

    let sel = getDocSelection();

    if (globals.ContentItemType == 'Text' &&
        isSubSelectionEnabled() &&
        //isClassInElementPath(e.currentTarget, globals.TABLE_WRAPPER_CLASS_NAME)
        (getTableSelectedCells().length == 0 || globals.IsTableDragSelecting) &&
        (!sel || sel.length == 0)) {
        log('drag start rejected by selection state. selectable but w/o range');
        globals.WasNoSelectBeforeDragStart = false;
        e.preventDefault();
        e.stopPropagation();
        return false;
    }

    globals.CurDragTargetElm = e.target;

    log('drag start. sel: ',sel);
    e.stopPropagation();

    if (globals.ContentItemType == 'Text') {
        e.dataTransfer.effectAllowed = 'copy';// 'copyMove';
    } else if (globals.ContentItemType == 'FileList') {
        e.dataTransfer.effectAllowed = 'copy';
    } else if (globals.ContentItemType == 'Image') {
        e.dataTransfer.effectAllowed = 'copy';
    }
    return true;
}

function onDragEnd(e) {
    log('drag end');
    updateWindowMouseState(e,'dragEnd');
    globals.CurDragTargetElm = null;
    globals.ModKeys.IsShiftDown = false;
    globals.ModKeys.IsCtrlDown = false;
    globals.ModKeys.IsAltDown = false;
    
    if (globals.WasNoSelectBeforeDragStart) {
        resetSelection();
        disableSubSelection();
        globals.WasNoSelectBeforeDragStart = false;
    }
    drawOverlay();
}
// #endregion Event Handlers