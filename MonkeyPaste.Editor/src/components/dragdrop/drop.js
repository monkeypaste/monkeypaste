// #region Globals

//var IsDropping = false;
var DropIdx = -1;

var WasEditableBeforeDragEnter = false;

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

    CurDropTargetElm = null;

    for (var i = 0; i < DropItemElms.length; i++) {
        DropItemElms[i].classList.remove('drop');
    }

    stopAutoScroll(wasLeave);
    if (isDragging()) {
        updateAllElements();
        log('(self) drop reset: ' + (fromHost ? "FROM HOST" : "INTERNALLY") + (wasLeave ? '| WAS LEAVE' : '| WAS DROP'));
        return;
    }

    //IsDropping = false;
    //if (!isDragging()) {
    // for dragend needs dropIdx if effect was move
    DropIdx = -1;
    //}

    IsCtrlDown = false;
    IsAltDown = false
    IsShiftDown = false;


    //updateAllElements();

    if (wasLeave && !WasEditableBeforeDragEnter) {
        disableSubSelection();
    }


    log('drop reset: ' + (fromHost ? "FROM HOST" : "INTERNALLY") + (wasLeave ? '| WAS LEAVE' : '| WAS DROP'));
}

//function dropData(docIdx, dt, source_range) {
//    performDataTransferOnContent(dt, { index: docIdx, length: 0 }, 'user');
//}

// #endregion Actions

// #region Event Handlers
function onDragEnter(e) {
    updateWindowMouseState(e);

    e.stopPropagation();
    e.preventDefault();

    if (isDropping()) {
        // NOTE called on every element drag enters, only need once
        return false;
    }
    if (ContentItemType != 'Text') {
        return false;
    }
    //if (e.target.id == 'dragOverlay') {
    //    // this should be able to happen when sub-selection is disabled
    //    if (isDragging()) {
    //        return false;
    //    }
    //    //return;
    //}
    CurDropTargetElm = e.target;

    onDragEnter_ntf();
    log('drag enter');
    //IsDropping = true;

    for (var i = 0; i < DropItemElms.length; i++) {
        DropItemElms[i].classList.add('drop');
    }
    startAutoScroll();

    // store state before drop starts so the right state is restored 
    WasEditableBeforeDragEnter = !isReadOnly();
    if (!isSubSelectionEnabled()) {
        enableSubSelection(false);
    }
    hidePasteToolbar();
    return false;
}

function onDragOver(e) {
    updateWindowMouseState(e);

    e.stopPropagation();
    e.preventDefault();

    if (!isDropping()) {
        // IsDropping won't be set to true when its dragOverlay ie. can't drop whole tile on itself.
        log('onDragOver called but not dropping, returning false');
        return false;
    }
    if (e.target.id == 'dragOverlay') {
        debugger;
    }
    //let emp = getClientMousePos(e);


    // VALIDATE (EXTERNAL)

    //if (SelIdxBeforeDrag >= 0) {
    //    // don't allow overlay drag to drop, needs to be sub-selectable to allow
    //    return false;
    //}

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

    if (!AllowedEffects.includes(e.dataTransfer.effectAllowed)) {
        return false;
    }

    if (isDragCopy() || e.dataTransfer.effectAllowed == 'copy') {
        e.dataTransfer.dropEffect = 'copy';
    } else if (isDragCut()) {
        e.dataTransfer.dropEffect = 'move';
    } else {
        e.dataTransfer.dropEffect = 'none';
        return false;
    }


    // DEBOUNCE (my own type but word comes from https://css-tricks.com/debouncing-throttling-explained-examples/)
    let can_proceed = canDebounceDragOverProceed();
    if (!can_proceed) {
        return false;
    }

    //let cur_date_time = Date.now();

    //LastDragOverDateTime = LastDragOverDateTime == null ? cur_date_time : LastDragOverDateTime;
    //let m_dt = LastDragOverDateTime - cur_date_time;

    //if (WindowMouseLoc == null) {
    //    // mouse was not over editor until drag was in progress
    //    WindowMouseLoc = emp;
    //}

    //let m_delta_dist = dist(emp, WindowMouseLoc);
    //let m_v = m_delta_dist / m_dt;

    //LastDragOverDateTime = cur_date_time;

    //WindowMouseLoc = emp;
    //let debounce = m_delta_dist != 0 || m_v != 0;
    //if (debounce) {
    //    return false;
    //}


    // DROP IDX

    DropIdx = getDocIdxFromPoint(WindowMouseLoc);

    // VALIDATE SELF DROP
    if (isDragging()) {
        let is_drop_over_drag_sel = isDocIdxInRange(DropIdx, getDocSelection());
        let is_drop_over_template = getAllTemplateDocIdxs().includes(DropIdx);
        if (is_drop_over_drag_sel || is_drop_over_template) {
            //log('invalidating self drop. over drag sel: ' + is_drop_over_drag_sel + ' over template: ' + is_drop_over_template);
            DropIdx = -1;
            e.dataTransfer.dropEffect = "none";
        }
    }

    //let drop_over_drag_range = isDragging() && isDocIdxInRange(DropIdx, DragSelectionRange);
    //if (drop_over_drag_range) {
    //    log('invalidating drop over drag range');
    //}
    //let drop_over_template = getAllTemplateDocIdxs().includes(DropIdx);
    //if (drop_over_template) {
    //    log('invalidating drop over template');
    //}
    //if (drop_over_drag_range || drop_over_template || DropIdx < 0) {
    //    DropIdx = -1;
    //    e.dataTransfer.dropEffect = 'none';
    //}
    drawOverlay();

    return false;
}

function onDragLeave(e) {
    updateWindowMouseState(e);

    if (e.target.id == 'dragOverlay') {
        return;
    }

    //let emp = getClientMousePos(e);
    let editor_rect = getEditorContainerRect();
    if (isPointInRect(editor_rect, WindowMouseLoc)) {
        return;
    }

    log('drag leave');

    resetDrop(e.fromHost, true);

    onDragLeave_ntf();
}

function onDrop(e) {
    updateWindowMouseState(e);

    // OVERRIDE DEFAULT

    // stops the browser from redirecting.
    e.stopPropagation();
    e.preventDefault();

    // VALIDATE

    if (!isDropping()) {
        log('onDrop called but not dropping, ignoring and returning false');
        return false;
    }

    if (isDragCopy() || e.dataTransfer.effectAllowed == 'copy') {
        e.dataTransfer.dropEffect = 'copy';
    } else if (isDragCut()) {
        e.dataTransfer.dropEffect = 'move';
    } else {
        e.dataTransfer.dropEffect = 'none';
        return false;
    }

    log('drop');

    // DROP DATA
    let drop_range = { index: DropIdx, length: 0 };
    let source_range = null;
    if (isDragging() &&
        e.dataTransfer.dropEffect.toLowerCase().includes('move')) {
        source_range = getDocSelection();
    }
    let block_state = getDropBlockState(DropIdx, WindowMouseLoc, IsShiftDown);
    switch (block_state) {
        case 'split':
            insertText(drop_range.index, '\n');
            insertText(drop_range.index, '\n');
            drop_range.index += 1;
        //dropData(cur_drop_idx + 1, e.dataTransfer);
            break;
        case 'pre':
            //let isFirstLine = getLineIdx(drop_range.index) == 0;
            //if (!isFirstLine) {
            //    log('WARNING! drop is flagged as pre block but not 1st line line is ' + getLineIdx(drop_range.index));
            //} else {
            //    //cur_drop_idx = 0;
            //    drop_range.index = 0;
            //}
            drop_range.index = 0;
            insertText(drop_range.index, '\n');
        //dropData(0, e.dataTransfer);
            break;
        case 'post':
            drop_range.index = getLineEndDocIdx(drop_range.index);
            if (drop_range.index < getDocLength() - 1) {
                // ignore new line for last line since it already is a new line
                insertText(drop_range.index, '\n');
                drop_range.index += 1;
            }
        //dropData(cur_drop_idx, e.dataTransfer);
            break;
        case 'inline':
        default:
        //dropData(cur_drop_idx, e.dataTransfer);
            break;
    }

    performDataTransferOnContent(e.dataTransfer, drop_range, source_range, 'api');

    // RESET

    resetDrop(e.fromHost, false);

    onDropCompleted_ntf();
    drawOverlay();
    return false;
    }

// #endregion Event Handlers