// #region Globals

var IsDropping = false;
var DropIdx = -1;

var LastDragOverDateTime = null;

var IsSplitDrop = false;
var IsPreBlockDrop = false;
var IsPostBlockDrop = false;

const AllowedEffects = ['copy', 'copyLink', 'copyMove', 'link', 'linkMove', 'move'];

const AllowedDropTypes = ['text/plain', 'text/html', 'application/json', 'files'];

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

        //item.addEventListener('scroll', onScrollChange, true);
    }
}
// #endregion Life Cycle

// #region Getters

function getEditorMousePos(e) {
    if (!e || !e.clientX || !e.clientY) {
        return { x: -1, y: -1 };
    }

    let mp = { x: parseFloat(e.clientX), y: parseFloat(e.clientY) };

    //let editor_rect = getEditorRect(false);

    //mp.x = mp.x - editor_rect.left;
    //mp.y = mp.y - editor_rect.top;

    return mp;
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

// #endregion State

// #region Actions

function resetDrop(fromHost, wasLeave) {
    IsDropping = false;
    if (!IsDragging) {
        // drag end needs dropIdx if effect was move
        DropIdx = -1;
    }

    IsCtrlDown = false;
    IsAltDown = false
    IsShiftDown = false;

    for (var i = 0; i < DropItemElms.length; i++) {
        DropItemElms[i].classList.remove('drop');
    }

    updateAllSizeAndPositions();
    stopAutoScroll(wasLeave);

    if (IsReadOnly && !IsDragging) {
        disableSubSelection();
    }

    drawOverlay();

    log('drop reset: ' + (fromHost ? "FROM HOST" : "INTERNALLY"));
}

function dropData(docIdx, dt) {
    let drop_content_data = '';
    let drop_content_data_type = '';

    if (isDropHtml() && dt.types.includes('text/html')) {
        let drop_html_str = dt.getData('text/html');
        insertHtml(docIdx, drop_html_str, 'user');
        return;
    }
    let drop_pt = dt.getData('text/plain');

    insertText(docIdx, drop_pt, 'silent', true);
}

// #endregion Actions

// #region Event Handlers
function onDragEnter(e) {
    if (IsDropping) {
        // NOTE called on every element drag enters, only need once
        return;
    }
    if (ContentItemType != 'Text') {
        return true;
    }
    if (e.target.id == 'dragOverlay') {
        // this should be able to happen when sub-selection is disabled
        if (IsDragging) {
            return;
        }
        if (!IsSubSelectionEnabled) {
            enableSubSelection();
		}
        //return;
    }
    log('drag enter');
    IsDropping = true;

    for (var i = 0; i < DropItemElms.length; i++) {
        DropItemElms[i].classList.add('drop');
    }
    startAutoScroll();
    if (!IsSubSelectionEnabled) {
        enableSubSelection();
    }
    onDragEnter_ntf();
}

function onDragOver(e) {
    if (!IsDropping) {
        // IsDropping won't be set to true when its dragOverlay ie. can't drop whole tile on itself.
        return true;
    }
    //if (e.target.id == 'dragOverlay') {
    //    debugger;
    //}
    let emp = getEditorMousePos(e);

    e.stopPropagation();
    e.preventDefault();

    // VALIDATE (EXTERNAL)

    if (SelIdxBeforeDrag >= 0) {
        // don't allow overlay drag to drop, needs to be sub-selectable to allow
        return false;
    }

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
    if (isRunningInHost() && IsDragging) {
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
    let min_drag_mouse_delta_dist = 1;
    let cur_date_time = Date.now();

    LastDragOverDateTime = LastDragOverDateTime == null ? cur_date_time : LastDragOverDateTime;
    let m_dt = LastDragOverDateTime - cur_date_time;

    if (WindowMouseLoc == null) {
        // mouse was not over editor until drag was in progress
        WindowMouseLoc = emp;
    }

    let m_delta_dist = dist(emp, WindowMouseLoc);
    let m_v = m_delta_dist / m_dt;

    LastDragOverDateTime = cur_date_time;

    WindowMouseLoc = emp;
    let debounce = m_delta_dist != 0 || m_v != 0;
    if (debounce) {
        return false;
    }
    // DROP IDX

    DropIdx = getDocIdxFromPoint(emp);

    // VALIDATE (INTERNAL)

    let drop_over_drag_range = IsDragging && isDocIdxInRange(DropIdx, DragSelectionRange);
    if (drop_over_drag_range) {
        log('invalidating drop over drag range');
    }
    let drop_over_template = getAllTemplateDocIdxs().includes(DropIdx);
    if (drop_over_template) {
        log('invalidating drop over template');
    }
    if (drop_over_drag_range || drop_over_template || DropIdx < 0) {
        DropIdx = -1;
        e.dataTransfer.dropEffect = 'none';
    }
    drawOverlay();

    return false;
}

function onDragLeave(e) {
    if (e.target.id == 'dragOverlay') {
        return;
    }

    let emp = getEditorMousePos(e);
    let editor_rect = getEditorContainerRect();
    if (isPointInRect(editor_rect, emp)) {
        return;
    }

    log('drag leave');

    resetDrop(e.fromHost, true);

    onDragLeave_ntf();
}

function onDrop(e) {
    // OVERRIDE DEFAULT

    // stops the browser from redirecting.
    e.stopPropagation();
    e.preventDefault();
    // VALIDATE

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

    let cur_drop_idx = DropIdx;
    let pre_doc_length = getDocLength();

    let length_delta = 0;
    let post_sel_start_idx = cur_drop_idx;

    if (IsPreBlockDrop) {
        let isFirstLine = getLineIdx(cur_drop_idx) == 0;
        if (!isFirstLine) {
            log('WARNING! drop is flagged as pre block but not 1st line line is ' + getLineIdx(cur_drop_idx));
        } else {
            cur_drop_idx = 0;
        }
        insertText(0, '\n');
        dropData(0, e.dataTransfer);

        length_delta = getDocLength() - pre_doc_length - 1;
        post_sel_start_idx = 0;
    } else if (IsPostBlockDrop) {
        cur_drop_idx = getLineEndDocIdx(cur_drop_idx);
        if (cur_drop_idx < getDocLength() - 1) {
            // ignore new line for last line since it already is a new line
            insertText(cur_drop_idx, '\n');
            cur_drop_idx += 1;
        }
        dropData(cur_drop_idx, e.dataTransfer);

        length_delta = getDocLength() - pre_doc_length - 1;
        post_sel_start_idx = cur_drop_idx;
    } else if (IsSplitDrop) {
        insertText(cur_drop_idx, '\n');
        insertText(cur_drop_idx, '\n');
        dropData(cur_drop_idx + 1, e.dataTransfer);

        length_delta = getDocLength() - pre_doc_length - 2;
        post_sel_start_idx = cur_drop_idx + 1;
    } else {
        dropData(cur_drop_idx, e.dataTransfer);

        length_delta = getDocLength() - pre_doc_length;
    }

    // SELECT DROP CONTENT

    setEditorSelection(post_sel_start_idx, length_delta);

    // RESET

    resetDrop(e.fromHost, false);

    onDropCompleted_ntf();
    drawOverlay();
    return false;
}

function onScrollChange(e) {

    if (IsDragging) {
        //debugger;
        e.currentTarget.scrollLeft = DragStartScrollOffset.x;
        e.currentTarget.scrollTop = DragStartScrollOffset.y;
        log(e.currentTarget.id, ' scroll RESET FOR DRAG  top ', e.currentTarget.scrollTop, ' left ', e.currentTarget.scrollLeft, ' IsDragging: ', IsDragging, ' IsDropping: ', IsDropping);
    } else {
        log(e.currentTarget.id, ' scroll changed top ', e.currentTarget.scrollTop, ' left ', e.currentTarget.scrollLeft, ' IsDragging: ', IsDragging, ' IsDropping: ', IsDropping);

        DragStartScrollOffset = {
            x: e.currentTarget.scrollLeft,
            y: e.currentTarget.scrollTop
        };
    }
}
// #endregion Event Handlers