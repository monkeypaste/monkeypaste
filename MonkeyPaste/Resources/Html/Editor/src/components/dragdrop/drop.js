var IsDropping = false;
var DropIdx = -1;

var LastDragOverDateTime = null;

var IsSplitDrop = false;
var IsPreBlockDrop = false;
var IsPostBlockDrop = false;

const AllowedEffects = ['copy', 'copyLink', 'copyMove', 'link', 'linkMove', 'move'];

const AllowedDropTypes = ['text/plain', 'text/html', 'application/json', 'files'];

function initDrop() {

    function handleDragEnter(e) {
        if (IsDropping) {
            // NOTE called on every element drag enters, only need once
            return;
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

        items.forEach(function (item) {
            item.classList.add('drop');
        });

        enableSubSelection();
    }

    function handleDragOver(e) {
        if (!IsDropping) {
            // IsDropping won't be set to true when its dragOverlay ie. can't drop whole tile on itself.
            return;
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
        if (isRunningInHost()) {
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

        if (isDragCopy()) {
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

        items.forEach(function (item) {
            item.classList.remove('drop');
        });

        if (IsReadOnly) {
            disableSubSelection();
        }
        drawOverlay();
    }

    function handleDrop(e) {
        // OVERRIDE DEFAULT

        e.stopPropagation(); // stops the browser from redirecting.

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
            quill.insertText(0, '\n');
            dropData(0, e.dataTransfer);

            length_delta = quill.getLength() - pre_doc_length - 1;
            post_sel_start_idx = 0;
        } else if (IsPostBlockDrop) {
            cur_drop_idx = getLineEndDocIdx(cur_drop_idx);
            quill.insertText(cur_drop_idx, '\n');
            dropData(cur_drop_idx + 1, e.dataTransfer);

            length_delta = quill.getLength() - pre_doc_length - 1;
            post_sel_start_idx = cur_drop_idx + 1;
        } else if (IsSplitDrop) {
            quill.insertText(cur_drop_idx, '\n');
            quill.insertText(cur_drop_idx, '\n');
            dropData(cur_drop_idx + 1, e.dataTransfer);

            length_delta = quill.getLength() - pre_doc_length - 2;
            post_sel_start_idx = cur_drop_idx + 1;
        } else {
            dropData(cur_drop_idx, e.dataTransfer);

            length_delta = quill.getLength() - pre_doc_length;
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
        

        items.forEach(function (item) {
            item.classList.remove('drop');
        });

        if (IsReadOnly) {
            disableSubSelection();
        }
        drawOverlay();
        return false;
    }

    let items = [getEditorContainerElement(), getDragOverlayElement()];
        items.forEach(function (item) {
            item.addEventListener('dragenter', handleDragEnter,true);
            item.addEventListener('dragover', handleDragOver, true);
            item.addEventListener('dragleave', handleDragLeave, true);
            item.addEventListener('drop', handleDrop, true);
        });
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

