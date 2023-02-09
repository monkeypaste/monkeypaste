// #region Globals

//var IsDropping = false;
var DropIdx = -1;

//var IsSplitDrop = false;
//var IsPreBlockDrop = false;
//var IsPostBlockDrop = false;


const AllowedEffects = ['copy', 'copyLink', 'copyMove', 'link', 'linkMove', 'move', 'all'];

const AllowedDropTypes = ['text/plain', 'text/html', 'application/json', 'files'];

var CurDropTargetElm = null;

var DropItemElms = [];

// #endregion Globals

// #region Life Cycle
function initDrop() {
    DropItemElms = [getEditorContainerElement(), getDragOverlayElement()];

    for (var i = 0; i < DropItemElms.length; i++) {
        let item = DropItemElms[i];
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
    if (isShiftDown) {
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
    if (is_post_block_drop) {
        return 'post';
    }
    return 'inline';
}

// #endregion Getters

// #region Setters
// #endregion Setters

// #region State

function isDragCopy() {
    return IsCtrlDown;
}

function isDragCut() {
    return !IsCtrlDown;
}

function isDropHtml() {
    return IsAltDown;
}

function isDropping() {
    return CurDropTargetElm != null;
}



// #endregion State

// #region Actions

function resetDrop(fromHost, wasLeave) {
    resetDebounceDragOver();
    stopAutoScroll(wasLeave);

    CurDropTargetElm = null;

    IsCtrlDown = false;
    IsAltDown = false
    IsShiftDown = false;

    for (var i = 0; i < DropItemElms.length; i++) {
        DropItemElms[i].classList.remove('drop');
    }

    DropIdx = -1;
    if (wasLeave && !isDragging() && WasNoSelectBeforeDragStart) {
        disableSubSelection();
    } else {
        if (wasLeave && isDragging()) {// && DragDomRange) {
            //setDomSelection(DragDomRange);
            // NOTE dragend isn't called on drag cancel (from escape key)
            onDragEnd();
        }
        updateAllElements();
    }

    log('drop reset: ' + (fromHost ? "FROM HOST" : "INTERNALLY") + (wasLeave ? '| WAS LEAVE' : '| WAS DROP'));
}

function processEffectAllowed(e) {
    let effect_str = 'none';
    if (e.fromHost === undefined) {
        effect_str = e.dataTransfer.effectAllowed;
    } else {
        effect_str = e.effectAllowed_override;
    }
    if (!AllowedEffects.includes(effect_str)) {
        effect_str = 'none'
    }
    if (isDragCopy() || effect_str == 'copy') {
        effect_str = 'copy';
    } else if (isDragCut()) {
        effect_str = 'move';
    } else {
        effect_str = 'none';
    }
    e.dataTransfer.dropEffect = effect_str;
    return effect_str;
}

// #endregion Actions

// #region Event Handlers
function onDragEnter(e) {
    updateWindowMouseState(e);

    if (e.fromHost === undefined) {
        e.stopPropagation();
        e.preventDefault();
    }

    if (isDropping()) {
        // NOTE called on every element drag enters, only need once
        return false;
    }
    if (ContentItemType != 'Text') {
        return false; 
    }

    CurDropTargetElm = e.target;
    resetDebounceDragOver();

    onDragEnter_ntf();
    log('drag enter');
    for (var i = 0; i < DropItemElms.length; i++) {
        DropItemElms[i].classList.add('drop');
    }
    startAutoScroll();

    // store state before drop starts so the right state is restored 
    if (DragDomRange) {
        // update blur ranges due to drop class margins
        //BlurredSelectionRects = getRangeRects(convertDomRangeToDocRange(DragDomRange));
    }
    if (!isDragging()) {
        if (isSubSelectionEnabled()) {
            WasNoSelectBeforeDragStart = false;
        } else {
            WasNoSelectBeforeDragStart = true;
            enableSubSelection();
        }
    }

    hidePasteToolbar();
    return false;
}

function onDragOver(e) {
    //log('drag over called');

    updateWindowMouseState(e);

    if (e.fromHost === undefined) {
        e.stopPropagation();
        e.preventDefault();
    }
    

    if (!isDropping()) {
        // IsDropping won't be set to true when its dragOverlay ie. can't drop whole tile on itself.
        log('onDragOver called but not dropping, returning false');
        return false;
    }
    if (e.target.id == 'dragOverlay') {
        debugger;
    }
    // DEBOUNCE (my own type but word comes from https://css-tricks.com/debouncing-throttling-explained-examples/)
    let can_proceed = canDebounceDragOverProceed();
    if (!can_proceed) {
        return false;
    }

    // VALIDATE (EXTERNAL)

    let is_valid = false;
    for (var i = 0; i < e.dataTransfer.types.length; i++) {
        let dt_type = e.dataTransfer.types[i];
        if (AllowedDropTypes.includes(dt_type)) {
            is_valid = true;
            break;
        }
    }

    if (!is_valid) {
        return false;
    }


    // DROP EFFECT
    if (isRunningInHost() && isDragging()) {
        // mod keys updated from host msg in updateModKeys
    } else {
        updateModKeys(e);
    }

    if (processEffectAllowed(e) == 'none') {
        return false;
    }  

    // DROP IDX

    DropIdx = getDocIdxFromPoint(WindowMouseLoc);

    // VALIDATE SELF DROP
    if (isDragging()) {
        let sel = getDocSelection();
        let is_drop_over_drag_sel = isDocIdxInRange(DropIdx, sel);
        let is_drop_over_template = getAllTemplateDocIdxs().includes(DropIdx);
        if (!is_drop_over_drag_sel && !is_drop_over_template) {
            is_drop_over_drag_sel = isPointInRange(WindowMouseLoc, sel);
        }
        if (is_drop_over_drag_sel ||
            is_drop_over_template) {
            DropIdx = -1;
            e.dataTransfer.dropEffect = "none";
            log('invalidating self drop. DROP EFFECT SHOULD BE NONE IS: ' + e.dataTransfer.dropEffect + ' over drag sel: ' + is_drop_over_drag_sel + ' over template: ' + is_drop_over_template);
        }
    }
    drawOverlay();

    return false;
}

function onDragLeave(e) {
    log('drag leave called');

    updateWindowMouseState(e);

    //if (e.target.id == 'dragOverlay') {
    //    return;
    //}

    //let emp = getClientMousePos(e);
    let editor_rect = getEditorContainerRect();
    if (isPointInRect(editor_rect, WindowMouseLoc)) {
        resetDrop(e.fromHost, false);
        log('drag canceled (pointer still in window)');
        onDragLeave_ntf();
        return;
    }

    log('drag leave');

    resetDrop(e.fromHost, true);

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
    log('drop attempting. mp ' + pointStr(WindowMouseLoc) + ' mdp ' + pointStr(WindowMouseDownLoc));

    let drag_dist =
        isDragging() && isPoint(WindowMouseLoc) && isPoint(WindowMouseDownLoc) ?
            dist(WindowMouseLoc, WindowMouseDownLoc) : null;

    updateWindowMouseState(e);

    

    // VALIDATE

    if (DropIdx < 0) {
        log('Drop rejected, dropIdx ' + DropIdx);
        resetDrop(e.fromHost, false);
        return false;
    }
    if (!isDropping()) {
        log('onDrop called but not dropping, ignoring and returning false');
        resetDrop(e.fromHost, false);
        return false;
    }

    let dropEffect = processEffectAllowed(e);
    if (dropEffect == 'none') {
        log('onDrop called but dropEffect was none, ignoring and returning false');
        resetDrop(e.fromHost, false);
        return false;
    }

    if (isDragging() && isDocIdxInRange(DropIdx, getDocSelection())) {
        log('onDrop called but drop within drag range, ignoring and returning false');
        resetDrop(e.fromHost, false);
        return false;
    }

    if (isDragging() && (!drag_dist || drag_dist < MIN_DRAG_DIST)) {
        log('Drop rejected, dist was ' + drag_dist + ' minimum is ' + MIN_DRAG_DIST);
        resetDrop(e.fromHost, false);
        return false;
    }


    log('drop');


    // PREPARE SOURCE/DEST DOC RANGES

    var drop_insert_source = 'api';
    var source_range = null;
    if (isDragging() &&
        dropEffect.toLowerCase().includes('move')) {
        source_range = getDocSelection();
    }
    var drop_range = {
        index: DropIdx,
        length: 0,
        mode: getDropBlockState(DropIdx, WindowMouseLoc, IsShiftDown)
    };

    logDataTransfer(e.dataTransfer, 'Drop DataTransfer (unprocessed):');
    // REQUEST DT PROCESSING FROM HOST
    getDragDataTransferObjectAsync_get(e.dataTransfer)
        .then((processed_dt) => {
            logDataTransfer(processed_dt, 'Drop DataTransfer (processed):');

            // PERFORM DROP TRANSACTION    

            performDataTransferOnContent(processed_dt, drop_range, source_range, drop_insert_source, 'Dropped');

            // RESET

            resetDrop(e.fromHost, false);

            onDropCompleted_ntf();
            drawOverlay();
        });


    return false;
}

// #endregion Event Handlers