
// #region Life Cycle
function initDrop() {
    globals.DropItemElms = [getEditorContainerElement()/*, getDragOverlayElement()*/];

    for (var i = 0; i < globals.DropItemElms.length; i++) {
        let item = globals.DropItemElms[i];
        item.addEventListener('dragenter', onDragEnter, true);
        item.addEventListener('dragover', onDragOver, true);
        item.addEventListener('dragleave', onDragLeave, true);
        item.addEventListener('drop', onDrop, true);
    }
}

// #endregion Life Cycle

// #region Getters

function getDropBlockState(doc_idx, mp, isShiftDown) {    
    if (doc_idx < 0) {
        return false;
    }
    const can_inline = globals.ContentItemType == 'Text';
    if (isShiftDown && can_inline) {
        return 'split';
    }

    let caret_rect = getCharacterRect(doc_idx);
    let caret_line = getCaretLine(doc_idx);
    let block_threshold = Math.max(2, caret_line.height / 4);

    let doc_start_rect = getCharacterRect(0);

    let is_pre_block_drop =
        Math.abs(mp.y - doc_start_rect.top) < block_threshold ||
        mp.y < doc_start_rect.top;
    if (is_pre_block_drop) {
        return 'pre';
    }

    let is_post_block_drop =
        Math.abs(mp.y - caret_line.y2) < block_threshold ||
        mp.y > caret_line.y2;
    if (is_post_block_drop || !can_inline) {
        return 'post';
    }
    return 'inline';
}

// #endregion Getters

// #region Setters
// #endregion Setters

// #region State

function isDragCopy() {
    return globals.ModKeys.IsCtrlDown;
}

function isDragCut() {
    return !globals.ModKeys.IsCtrlDown;
}

function isDropHtml() {
    return globals.ModKeys.IsAltDown;
}

function isDropping() {
    return globals.CurDropTargetElm != null;
}



// #endregion State

// #region Actions

function rejectDrop(e) {
    globals.DropIdx = -1;
    if (!isNullOrUndefined(e.dataTransfer)) {
        e.dataTransfer.dropEffect = 'none';
        log('drop rejected (effect=none)');
    }
    drawOverlay();
    return false;
}
function resetDrop(fromHost, wasLeave, wasCancel) {
    if (wasCancel) {
        onDragEnd_ntf(fromHost, true);
    }
    resetDebounceDragOver();
    stopAutoScroll(wasLeave);

    globals.CurDropTargetElm = null;

    globals.ModKeys = cleanModKeys(null);
    globals.IsTableDragSelecting = false;

    for (var i = 0; i < globals.DropItemElms.length; i++) {
        globals.DropItemElms[i].classList.remove('drop');
    }

    globals.DropIdx = -1;
    if (wasLeave && !isDragging() && globals.WasNoSelectBeforeDragStart) {
        disableSubSelection();
    } else {
        if (!wasLeave && isDragging()) {
            // NOTE dragend isn't called on drag cancel (from escape key)
            onDragEnd();
        }
        enableSubSelection();
    }

    log('drop reset: ' + (fromHost ? "FROM HOST" : "INTERNALLY") + (wasLeave ? ' | WAS LEAVE' : ' | WAS DROP'));


    delay(500)
        .then(() => {
            // mod key msgs may still come through after reset so wipe em out
            globals.ModKeys = cleanModKeys(null);
        });
    updateAllElements();
}

function processEffectAllowed(e) {
    let effect_str = 'none';
    let from_host = true;
    if (e.fromHost === undefined && !isNullOrUndefined(e.dataTransfer)) {
        effect_str = e.dataTransfer.effectAllowed;
        from_host = false;
    } else if (!isNullOrUndefined(e.effectAllowed_override)) {
        effect_str = e.effectAllowed_override;
    } else {
        from_host = false;
    }

    if (!globals.AllowedEffects.includes(effect_str)) {
        effect_str = 'none'
    }
    if (isDragCopy() || effect_str == 'copy') {
        effect_str = 'copy';
    } else if (isDragCut()) {
        effect_str = 'move';
    } else {
        effect_str = 'none';
    }
    if (!isNullOrUndefined(e.dataTransfer)) {
        e.dataTransfer.dropEffect = effect_str;
    }
    log('drop effects: ' + effect_str + ' from host: ' + from_host);
    
    return effect_str;
}

// #endregion Actions

// #region Event Handlers
function onDragEnter(e) {
    updateWindowMouseState(e,'dragEnter');

    if (e.fromHost === undefined) {
        e.stopPropagation();
        e.preventDefault();
    }

    if (isDropping()) {
        // NOTE called on every element drag enters, only need once
        return false;
    }
    if (!isValidDataTransfer(e.dataTransfer)) {
        return rejectDrop(e); 
    }

    globals.CurDropTargetElm = e.target;
    resetDebounceDragOver();

    onDragEnter_ntf();
    log('drag enter');
    startAutoScroll();

    for (var i = 0; i < globals.DropItemElms.length; i++) {
        globals.DropItemElms[i].classList.add('drop');
    }
    // store state before drop starts so the right state is restored 
    if (!isDragging()) {
        if (isSubSelectionEnabled()) {
            globals.WasNoSelectBeforeDragStart = false;
        } else {
            globals.WasNoSelectBeforeDragStart = true;
            enableSubSelection();
        }
    }

    hidePasteToolbar();
    return false;
}

function onDragOver(e) {
    updateWindowMouseState(e,'dragOver');

    if (e.fromHost === undefined) {
        e.stopPropagation();
        e.preventDefault();
    }
    

    if (!isDropping()) {
        onDragEnter(e);
    }

    // VALIDATE 

    if (!e || !isValidDataTransfer(e.dataTransfer)) {
        return rejectDrop(e);
    }    

    // DROP EFFECT
    if (isRunningInHost() && isDragging()) {
        // mod keys updated from host msg in updateGlobalModKeys
    } else {
        updateGlobalModKeys(e);
    }
    // DEBOUNCE (my own type but word comes from https://css-tricks.com/debouncing-throttling-explained-examples/)
    let can_proceed = canDebounceDragOverProceed();
    if (!can_proceed) {
        return false;
    }

    if (processEffectAllowed(e) == 'none') {
        return rejectDrop(e);
    }  

    if (globals.ContentItemType == 'FileList' &&
        isDragging() &&
        isDragCopy()) {
        // reject file item copy self drop
        return rejectDrop(e);
    }

    // DROP IDX

    globals.DropIdx = getDocIdxFromPoint(globals.WindowMouseLoc);

    // INVALIDATE DROP ONTO SELECTION
    //if (isDragging()) {
        let sel = getDocSelection();
        let is_drop_over_drag_sel = isDocIdxInRange(globals.DropIdx, sel);
        let is_drop_over_template = getAllTemplateDocIdxs().includes(globals.DropIdx);
        if (!is_drop_over_drag_sel && !is_drop_over_template) {
            is_drop_over_drag_sel = isPointInRange(globals.WindowMouseLoc, sel);
        }
        if (is_drop_over_drag_sel ||
            is_drop_over_template) {
            log('invalidating self drop. DROP EFFECT SHOULD BE NONE IS: ' + e.dataTransfer.dropEffect + ' over drag sel: ' + is_drop_over_drag_sel + ' over template: ' + is_drop_over_template);
            return rejectDrop(e);
        }
    //}
    drawOverlay();

    return false;
}

function onDragLeave(e) {
    let window_rect = getWindowRect();
    let mp_dist = getPointDistanceToRect(globals.WindowMouseLoc, window_rect);
    let is_mp_in_win = isPointInRect(window_rect, globals.WindowMouseLoc);
    let from_host = e.fromHost === undefined || e.fromHost == false ? false : true;
    log('drag leave called. mp dist: ' + mp_dist + ' in editor: ' + is_mp_in_win);

    if (is_mp_in_win && !from_host) {
        // NOTE host only triggers in webview drag leave
        // since catching actual drag leave is so problematic
        updateWindowMouseState(e, 'dragOver');
        return;
    }
    updateWindowMouseState(e, 'dragLeave');

    log('drag leave confirmed. from host: ' + from_host);

    resetDrop(from_host, true, false);

    onDragLeave_ntf();
}

function onDrop(e) {
    // OVERRIDE DEFAULT

    // stops the browser from redirecting.
    if (e.fromHost === undefined) {
        e.stopPropagation();
        e.preventDefault();
    }

    // get drag dist before down loc is cleared
    log('drop attempting. mp ' + pointStr(globals.WindowMouseLoc) + ' mdp ' + pointStr(globals.WindowMouseDownLoc));

    let drag_dist =
        isDragging() && isPoint(globals.WindowMouseLoc) && isPoint(globals.WindowMouseDownLoc) ?
            dist(globals.WindowMouseLoc, globals.WindowMouseDownLoc) : null;

    updateWindowMouseState(e,'drop');    

    // VALIDATE

    if (globals.DropIdx < 0) {
        log('Drop rejected, dropIdx ' + globals.DropIdx);
        resetDrop(e.fromHost, false, true);
        return false;
    }
    if (!isDropping()) {
        log('onDrop called but not dropping, ignoring and returning false');
        resetDrop(e.fromHost, false, true);
        return false;
    }

    let dropEffect = processEffectAllowed(e);

    if (dropEffect == 'none') {
        log('onDrop called but dropEffect was none, ignoring and returning false');
        resetDrop(e.fromHost, false, true);
        return rejectDrop(e);
    }

    if (isDragging() && isDocIdxInRange(globals.DropIdx, getDocSelection())) {
        log('onDrop called but drop within drag range, ignoring and returning false');
        resetDrop(e.fromHost, false, true);
        return rejectDrop(e);
    }

    //if (isDragging() && (!drag_dist || drag_dist < globals.MIN_DRAG_DIST)) {
    //    log('Drop rejected, dist was ' + drag_dist + ' minimum is ' + globals.MIN_DRAG_DIST);
    //    resetDrop(e.fromHost, false, true);
    //    return rejectDrop(e);
    //}


    log('drop');


    // PREPARE SOURCE/DEST DOC RANGES

    var drop_insert_source = 'api';
    var source_range = null;
    if (isDragging() &&
        dropEffect.toLowerCase().includes('move')) {
        if (globals.ContentItemType == 'FileList') {
            source_range = getSelectedFileItemIdxs();
        } else {
            source_range = getDocSelection();
        }
    }
    var drop_range = {
        index: globals.DropIdx,
        length: 0,
        mode: getDropBlockState(globals.DropIdx, globals.WindowMouseLoc, globals.ModKeys.IsShiftDown)
    };

    logDataTransfer(e.dataTransfer, 'Drop DataTransfer (unprocessed):');
    // REQUEST DT PROCESSING FROM HOST
    getDragDataTransferObjectAsync_get(e.dataTransfer)
        .then((processed_dt) => {
            logDataTransfer(processed_dt, 'Drop DataTransfer (processed):');

            // PERFORM DROP TRANSACTION    
            try {
                performDataTransferOnContent(processed_dt, source_range, drop_range, drop_insert_source, 'Dropped');
            } catch (ex) {
                log('Exception performing drop: ');
                log(ex);
            }

            // RESET

            onDropCompleted_ntf();
            resetDrop(e.fromHost, false, false);
            drawOverlay();
        });


    return false;
}

// #endregion Event Handlers