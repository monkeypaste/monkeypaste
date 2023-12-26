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

function resetDrag(e) {
    let from_host = isNullOrUndefined(e);
    log('drag reset. from_host: ' + from_host);
    updateWindowMouseState(e, 'dragEnd');

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
// #endregion Actions

// #region Event Handlersconsidered 

function onDragStart(e) {
    //if (isRunningOnHost() && e.fromHost != true) {
    //    // drag start handled from host control
    //    log('dragStart rejected not from host');
    //    e.preventDefault();
    //    e.stopPropagation();
    //    return false;
    //}
    log('dragstart');
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
    resetDrag(e);
}
// #endregion Event Handlers