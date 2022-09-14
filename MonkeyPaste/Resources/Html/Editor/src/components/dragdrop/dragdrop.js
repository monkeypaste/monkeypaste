var CefDragData;
var DropElm;

var IsSplitDrop = false;
var IsPreBlockDrop = false;
var IsPostBlockDrop = false;

var DropIdx = -1;

var IsCtrlDown = false; //duplicate
var IsShiftDown = false; //split 
var IsAltDown = false; // w/ formatting (as html)? ONLY formating? dunno

var IsDragCancel = false; // flagged from drag_end  evt resetDragDrop then unset in editorSelectionChange which restores selection

var IsDragging = false;
var WasNoneSelectedBeforeDrag = false;
var SelIdxBeforeDrag = -1;

var PreDropState = null;

var LastMousePos = null;
var LastMouseUpdateDateTime = null;

var DropProcessInvokeCount = 0;

function initDragDrop() {
    initDragDropOverrides();

    let allDocTags = [...InlineTags, ...BlockTags];
    let allDocTagsQueryStr = allDocTags.join(',');
    //let editorElms = document.getElementById('editor').querySelectorAll(allDocTagsQueryStr);

    enableDragDropOnElement(document.body);
    enableDragDropOnElement(window);
    enableDragDropOnElement(document.getElementById('editor'));

    //Array.from(editorElms).forEach(elm => {
    //    enableDragDropOnElement(elm);
    //});    
    window.addEventListener('mousemove', onMouseMove_dragOverFallback);
    window.addEventListener('mousedown', onMouseDown);

    //setInterval(onTick, 1000)
    window.addEventListener('keydown', onKeyDown_debug);
}

function onTick() {
    //let range = getSelection();
    //log("dragover idx " + range.index + ' length "' + range.length);

}

function initDragDropOverrides() {
    // from https://stackoverflow.com/a/46986927/105028
    window.addEventListener('dragstart', function (event) {
        event.dataTransfer.effectAllowed = 'all';

        var event2 = new CustomEvent('mp_dragstart', { detail: { original: event } });
        event.target.dispatchEvent(event2);

        event.stopPropagation();

        //onDragEnter(event);
    }, true);

    window.addEventListener('dragend', function (event) {
        //var event2 = new CustomEvent('mp_dragend', { detail: { original: event } });
        //event.target.dispatchEvent(event2);

        event.stopPropagation();

        resetDragDrop(true);
    }, true);

    window.addEventListener('drop', function (event) {
        //var event2 = new CustomEvent('mp_drop', { detail: { original: event } });
        //event.target.dispatchEvent(event2);

        event.stopPropagation();

        onDrop(event);
    }, true);

    //window.addEventListener('dragenter', function (event) {
    //    var event2 = new CustomEvent('mp_dragenter', { detail: { original: event } });
    //    event.target.dispatchEvent(event2);

    //    event.stopPropagation();
    //}, true);

    //window.addEventListener('dragover', function (/*event*/) {
    //    //var event2 = new CustomEvent('mp_dragover', { detail: { original: event } });
    //    //event.target.dispatchEvent(event2);
        

    //    event.stopPropagation();
    //}, true);


    //window.addEventListener('dragover', function (event) {
    //    var event2 = new CustomEvent('mp_dragover', { detail: { original: event } });
    //    event.target.dispatchEvent(event2);

    //    event.stopPropagation();
    //}, true);


}


function enableDragDropOnElement(elm) {
    if (elm.tagName) {
        if (InlineTags.includes(elm.tagName.toLowerCase())) {
            elm.setAttribute('draggable', true);

            elm.addEventListener('mp_dragstart', onDragStart);
        } else if (BlockTags.includes(elm.tagName.toLowerCase())) {

            elm.addEventListener('dragenter', onDragEnter)
            elm.addEventListener('dragover', onDragOver);
            elm.addEventListener('dragleave', onDragLeave);
            elm.addEventListener('drop', onDrop);

            //elm.addEventListener('mp_drop', onDrop);
        } else if (elm.nodeName == 'BODY') {
            // only used to capture mouse when outside editor
            //elm.addEventListener('dragenter', onDragEnter);
            //elm.addEventListener('dragleave', onDragLeave);
            elm.addEventListener('dragover', onDragOver);
            elm.addEventListener('drop', onDrop);
        } else {

            log('dragdrop warning! attempting to enable unknown element: ' + elm.id);
		}
	}
     else {

        elm.addEventListener('dragenter', onDragEnter)
        elm.addEventListener('dragover', onDragOver);
        elm.addEventListener('dragleave', onDragLeave);
        elm.addEventListener('drop', onDrop);
	}
    //elm.addEventListener('drop', onDrop);
}

function isDropping() {
    return DropElm != null;
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

function isDropPlainText() {
    return !IsAltDown;
}

function isBlockDrop() {
    return IsPreBlockDrop || IsPostBlockDrop || IsSplitDrop;
}

function isDragSource() {
    let selection = quill.getSelection();
    return selection.length > 0;
}

function isDragDataValid(dt) {
    if (isDataTransferValid(dt)) {
        return true;
    }
    return false;
}

function isDragValid(dt,emp) {
    if (isDragDataValid(dt)) {
        let sel_range = getSelection();
        if (isPointInRange(emp, sel_range)) {
            return false;
        }
        return true;
    }
    return false;
}

function isDropValid() {
    return DropIdx >= 0;
}

function resetDragDrop(isEscCancel = false) {
    IsDragCancel = isEscCancel;

    DropElm = null;
    //setTextSelectionBgColor('lightblue');
    //setTextSelectionFgColor('black');
    CefDragData = null;
    IsPreBlockDrop = false;
    IsPostBlockDrop = false;
    IsSplitDrop = false;

    LastMousePos = null;
    LastMouseUpdateDateTime = null;

    DropIdx = -1;

    DropProcessInvokeCount = 0;


    enableTextWrapping();
    hideScrollbars();

    quill.update();
 //   if (PreDropState) {
 //       log('dragDrop reset to initial state: ' + PreDropState);
 //       if (PreDropState == 'readonly') {
 //           // do nothing
 //       }
 //   } else {
 //       log('error reseting dragDrop, no pre drop state set');
	//}
    log('dragDrop reset. ignoring initial state: ' + PreDropState + ' and reseting to readonly w/ text wrapping');
    PreDropState = null;
    onDropEffectChanged_ntf('none');

    drawOverlay();
    IsDragCancel = false;
}

function startDrag() {
    IsDragging = true;
    let sel = getSelection();
    if (sel.length == 0) {
        WasNoneSelectedBeforeDrag = true;
        SelIdxBeforeDrag = sel.index;
    }
    drawOverlay();
}

function endDrag(isUserCancel = false) {
    IsDragging = false;
    if (WasNoneSelectedBeforeDrag) {
        setEditorSelection(SelIdxBeforeDrag, 0);
        WasNoneSelectedBeforeDrag = false;
        SelIdxBeforeDrag = -1;
    }
    resetDragDrop(isUserCancel);
}

function startDrop(e) {
    if (!e.currentTarget || e.currentTarget.id == 'editor') {
        // currentTarget null when passed from host so force target to editor
        e.currentTarget = getEditorContainerElement();
        let dt = getDataTransferObject(e);
        if (isDragDataValid(dt)) {
            DropElm = e.currentTarget;
        }
    }
    
    updateModKeys(e);
    if (isDropping()) {
        if (isEditorToolbarVisible()) {
            PreDropState = 'editable';
        } else if (IsSubSelectionEnabled) {
            PreDropState = 'subselectable';
        } else {
            PreDropState = 'readonly';
        }

        log('drop started from state: ' + PreDropState);

        if (typeof e.preventDefault === 'function') {
            e.preventDefault();
		}
        //
        e = setDropEffect(e);

        disableTextWrapping();
        showScrollbars();
    }
    return e;
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

    if (e.escKey) {
        resetDragDrop();
	}
}

function getDragData(dt) {
    // NOTE CefDragData should be pre-processed by host and ready from drop

    // TODO should deal w/ CefDragData here or it shouldn't matter

    //let item_data = isFormatted() ?
    //    convertDataTransferToHtml(dt) :
    //    convertDataTransferToPlainText(dt);
    //let drag_data = convertDataTransferToHtml(dt);
    if (isDragSource()) {
        let sel = getSelection();
        return getText(sel);
	}
    let drag_data = convertDataTransferToPlainText(dt);
    return drag_data;
}

function dropData(docIdx, data) {
    //quill.clipboard.dangerouslyPasteHTML(DropIdx + 1, drop_content_data);
    insertContent(docIdx, data, true);
}

function getDropEffect() {
    if (isDropping()) {
        if (isDragCut()) {
            return 'move';
        }
        if (isDragCopy()) {
            return 'copy';
        }
	}
    return 'none';
}

function setDropEffect(e) {
    let dropEffects = getDropEffect();
    onDropEffectChanged_ntf(dropEffects);
    e.dataTransfer.effectAllowed = dropEffects;
    return e;
}

function onMouseDown(e) {
    if (!IsSubSelectionEnabled) {
        onContentDraggableChanged_ntf(true);
        return;
    }

    let sel = getSelection();
    let is_none_selected = sel.length == 0;
    if (is_none_selected) {
        onContentDraggableChanged_ntf(false);
        return;
    }

    let emp = getEditorMousePos(e);
    let is_down_on_range = isPointInRange(emp, sel);
    let is_all_selected = isAllSelected();

    is_draggable = is_down_on_range || is_all_selected;
    onContentDraggableChanged_ntf(is_draggable);
}

function onMouseMove_dragOverFallback(e) {
    //let offset = getDocIdxFromPoint({ x: parseFloat(e.clientX), y: parseFloat(e.clientY) });
    //log('offset: ' + offset);

    if (!isDropping()) {
        if (parseInt(e.buttons) != 0) {
            // mouse button is down but not dragging

            
            //showTemplateUserSelection();
		}
        
        return;
    }
    log('window.mousemove dragover fallback resetting drag drop');
    resetDragDrop();
    return;

    if (parseInt(e.buttons) == 0) {
        resetDragDrop();
        return;
    }

    // dragging must be outside editor (otherwise this event is suppressed), likely in a toolbar but still within window
    if (DropIdx < 0) {
        return;
	}
    LastMousePos = getEditorMousePos(e);
    LastMouseUpdateDateTime = Date.now();

    DropIdx = -1;
    drawOverlay();
}

function onKeyDown_debug(e) {
    if (IsDragging && e.code == 'Escape') {
        endDrag(true);
	}
}

function onDragEnter(e) {
    e = startDrop(e);
    drawOverlay();
}

function onDragOver(e) {
    let mp = getEditorMousePos(e);
    if (mp.x < 0 || mp.y < 0) {
        // drag leave should pick this up i think
        return;
	}
    updateModKeys(e);
        
    if (isDropping()) {
        let min_drag_mouse_delta_dist = 1;
        let cur_date_time = Date.now();

        LastMouseUpdateDateTime = LastMouseUpdateDateTime == null ? cur_date_time : LastMouseUpdateDateTime;
        let m_dt = LastMouseUpdateDateTime - cur_date_time;

        LastMousePos = LastMousePos == null ? mp : LastMousePos;
        let m_delta_dist = dist(mp, LastMousePos);

        let m_v = m_delta_dist / m_dt;

        let do_update = m_delta_dist == 0 && m_v == 0;//dist(LastMousePos, mp) > min_drag_mouse_delta_dist
        //log('m_v: ' + m_v + ' m_delta_dist: ' + m_delta_dist);
        // NOTE to optimize only up
        //let do_drop_update = m_delta_dist > min_drag_mouse_delta_dist &&
        LastMousePos = mp;
        LastMouseUpdateDateTime = cur_date_time;
        if (do_update) {
            
            let dt = getDataTransferObject(e);
            let is_valid = isDragValid(dt, LastMousePos);
            if (is_valid) {
                if (typeof e.preventDefault === 'function') {
                    e.preventDefault();
                }

                DropProcessInvokeCount++;
                DropIdx = getDocIdxFromPoint(LastMousePos, true, DropProcessInvokeCount);

                //let mp_elm = document.elementFromPoint(LastMousePos.x, LastMousePos.y);
                //let blot = Quill.find(mp_elm);
                //let test_index = blot.offset(quill.scroll);

                //let caret_obj = Document.caretPositionFromPoint(LastMousePos.x, LastMousePos.y);
                //let caret_elm = caret_obj.offsetNode;
                //let caret_blot = Quill.find(caret_elm);
                //let test_index2 = blot.offset(quill.scroll);

                //log('DropIdx: ' + DropIdx);

            } else {
                DropIdx = -1;
			}            

            // NOTE to optimize only updating overlay when mouse move is significant enough
            drawOverlay();
            //log('Mouse DocIdx: ' + DropIdx + ' Mouse Pos: x: ' + LastMousePos.x + ' y: ' + LastMousePos.y + ' call count: ' + DropProcessInvokeCount); 
        }
    } 
    e = setDropEffect(e);
}

function onDragLeave(e) {
    drawOverlay();
}

function onDrop(e) {
    log('onDrop called  ');
    if (!isDropValid()) {
        log('drop error! drop attempted w/ DropIdx -1 it should be canceled before getting here')
        resetDragDrop();
        return;
    }

    let cur_drop_idx = DropIdx;

    let is_internal_drop = isDragSource();
    let is_host_drop = !is_internal_drop && CefDragData == null;

    if (!is_internal_drop && !is_host_drop) {
        log('drop error! drag is not internal and CefDragData == null host should be canceling drag events');
        resetDragDrop();
        return;
	}        

    let dt = getDataTransferObject(e);
    let drop_content_data = getDragData(dt);

    if (!drop_content_data || drop_content_data == '') {
        log('drop warning! drop data null or empty, resetting...');
        resetDragDrop();
        return;
	}


    if (isDragCut()) {

        if (is_internal_drop) {
            let cur_sel = quill.getSelection();
            if (cur_drop_idx > cur_sel.index) {
                // when dropidx is after cut remove cut length from drop idx
                log('drop cut adjusted from: ' + cur_drop_idx);
                cur_drop_idx -= cur_sel.length;
                log('drop cut adjusted to: ' + cur_drop_idx);
			}
            setTextInRange(cur_sel, '');
        } else {
            // NOTE host should be notified of cut here or just handles if drop is success

        }
    }
    // to retain selection store doc length before paste to know drop data length (diff'd w/ length after)
    let pre_doc_length = quill.getLength();

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
        dropData(0, drop_content_data);

        length_delta = quill.getLength() - pre_doc_length - 1;
        post_sel_start_idx = 0;
    } else if (IsPostBlockDrop) {
        cur_drop_idx = getLineEndDocIdx(cur_drop_idx);
        quill.insertText(cur_drop_idx, '\n');
        dropData(cur_drop_idx + 1, drop_content_data);

        length_delta = quill.getLength() - pre_doc_length - 1;
        post_sel_start_idx = cur_drop_idx + 1;
    } else if (IsSplitDrop) {
        quill.insertText(cur_drop_idx, '\n');
        quill.insertText(cur_drop_idx, '\n');
        dropData(cur_drop_idx + 1, drop_content_data);

        length_delta = quill.getLength() - pre_doc_length - 2;
        post_sel_start_idx = cur_drop_idx + 1;
    } else {
        dropData(cur_drop_idx, drop_content_data);

        length_delta = quill.getLength() - pre_doc_length;
    }

    setEditorSelection(post_sel_start_idx, length_delta);

    resetDragDrop();
}

function onDragStart(e) {
    log('drag started yo');
}