var IsDropping = false;
var DropIdx = -1;

var LastDragOverDateTime = null;

var IsSplitDrop = false;
var IsPreBlockDrop = false;
var IsPostBlockDrop = false;

var AutoScrollVelX = 0;
var AutoScrollVelY = 0;

var AutoScrollAccumlator = 5;
var AutoScrollBaseVelocity = 25;

const MIN_AUTO_SCROLL_DIST = 30;

var AutoScrollInterval = null;

const AllowedEffects = ['copy', 'copyLink', 'copyMove', 'link', 'linkMove', 'move'];

const AllowedDropTypes = ['text/plain', 'text/html', 'application/json', 'files'];

var DropItemElms = [];

function initDrop() {

    function handleDragEnter(e) {
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
            enableSubSelection();
            //return;
		}
        log('drag enter');
        IsDropping = true;

        for (var i = 0; i < DropItemElms.length; i++) {
            DropItemElms[i].classList.add('drop');
        }
        startAutoScroll();

        enableSubSelection();
        onDragEnter_ntf();
    }

    function handleDragOver(e) {
        if (!IsDropping) {
            // IsDropping won't be set to true when its dragOverlay ie. can't drop whole tile on itself.
            return true;
		}
        //if (e.target.id == 'dragOverlay') {
        //    debugger;
        //}
        let emp = getEditorMousePos(e);

        e.preventDefault();
        // VALIDATE

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
            let isModChanged =
                IsCtrlDown != e.ctrlKey ||
                IsAltDown != e.altKey ||
                IsShiftDown != e.shiftKey;

            IsCtrlDown = e.ctrlKey;
            IsAltDown = e.altKey;
            IsShiftDown = e.shiftKey;

            if (isModChanged) {
                log('mod changed: Ctrl: ' + (IsCtrlDown ? "YES" : "NO"));
                drawOverlay();
            }
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
        drawOverlay();

        return false;
    }

    function handleDragLeave(e) {
        if (e.target.id == 'dragOverlay') {
            return;
        }

        let emp = getEditorMousePos(e);
        let editor_rect = getEditorContainerRect();
        if (isPointInRect(editor_rect, emp)) {
            return;
		}

        log('drag leave');
        IsDropping = false;
        DropIdx = -1;

        for (var i = 0; i < DropItemElms.length; i++) {
            DropItemElms[i].classList.remove('drop');
        }

        if (IsReadOnly && !IsDragging) {
            disableSubSelection();
        }
        onDragLeave_ntf()
        drawOverlay();

        stopAutoScroll(true);
    }

    function handleDrop(e) {
        // OVERRIDE DEFAULT

        e.stopPropagation(); // stops the browser from redirecting.

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

        IsDropping = false;
        
        if (IsDragging) {
            // for internal drop do nothing, let dragEnd handler reset DropIdx
        } else {            
            DropIdx = -1;
		}
        

        for (var i = 0; i < DropItemElms.length; i++) {
            DropItemElms[i].classList.remove('drop');
        }
        updateAllSizeAndPositions();
        if (IsReadOnly) {
            disableSubSelection();
        }
        stopAutoScroll(false);
        onDropCompleted_ntf();
        drawOverlay();
        return false;
    }

    DropItemElms = [getEditorContainerElement(), getDragOverlayElement()];

    for (var i = 0; i < DropItemElms.length; i++) {
        let item = DropItemElms[i];
        item.addEventListener('dragenter', handleDragEnter, true);
        item.addEventListener('dragover', handleDragOver, true);
        item.addEventListener('dragleave', handleDragLeave, true);
        item.addEventListener('drop', handleDrop, true);
	}
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

function updateModKeys(e) {
    //if (e.fromHost === undefined && isRunningInHost()) {
    //    // ignore internal mod key updates when running from host
    //    return;
    //}

    let isModChanged =
        IsCtrlDown != e.ctrlKey ||
        IsAltDown != e.altKey ||
        IsShiftDown != e.shiftKey;

    IsCtrlDown = e.ctrlKey;
    IsAltDown = e.altKey;
    IsShiftDown = e.shiftKey;

    if (isModChanged) {
        log('mod changed: Ctrl: ' + (IsCtrlDown ? "YES" : "NO"));
        drawOverlay();
    }

    //if (e.escKey) {
    //    resetDragDrop(true);
    //}
}


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

function isDragCopy() {
    return IsCtrlDown;
}

function isDragCut() {
    return !IsCtrlDown;
}

function isDropHtml() {
    return IsAltDown;
}

function startAutoScroll() {
    // drop class makes .ql-editor huge so no wrapping this finds actual max width and sets so won't overscroll...
    let max_x = 0;
    //let max_y = 0;
    let lines = getLineCount();
    for (var i = 0; i < lines; i++) {
        let line_rect = getLineRect(i, false);
        max_x = Math.max(max_x, line_rect.width);
        //max_y = Math.max(max_y, line_rect.height);
    }
    // add 100 in case template at the end ( i think its from extra spaces or somethign...)
    max_x += 100;
    getEditorElement().style.width = max_x + 'px';
    AutoScrollInterval = setInterval(onAutoScrollTick, 300);
}

function stopAutoScroll(isLeave) {
    if (AutoScrollInterval == null) {
        return;
    }
    getEditorElement().style.width = '';
    clearInterval(AutoScrollInterval);
    AutoScrollInterval = null;
}

function onAutoScrollTick() {
    if (WindowMouseLoc == null) {
        return;
    }
    let window_rect = getWindowRect();
    if (!isPointInRect(window_rect, WindowMouseLoc)) {
        return;
    }

    let orig_scroll_x = document.body.scrollLeft;
    let orig_scroll_y = document.body.scrollTop;

    if (Math.abs(window_rect.right - WindowMouseLoc.x) <= MIN_AUTO_SCROLL_DIST) {
        document.body.scrollLeft += AutoScrollVelX;
    } else if (Math.abs(window_rect.left - WindowMouseLoc.x) <= MIN_AUTO_SCROLL_DIST) {
        document.body.scrollLeft -= AutoScrollVelX;
    }

    if (orig_scroll_x != document.body.scrollLeft) {
        AutoScrollVelX += AutoScrollAccumlator;
	}
}

