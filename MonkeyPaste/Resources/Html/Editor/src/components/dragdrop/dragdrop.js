var CefDragData;

var IsCtrlDown = false; //duplicate
var IsShiftDown = false; //split 
var IsAltDown = false; // w/ formatting (as html)? ONLY formating? dunno


var DropElm;
var DropIdx = -1;
var IsDropCancel = false; // flagged from drag_end  evt resetDragDrop then unset in editorSelectionChange which restores selection

var IsSplitDrop = false;
var IsPreBlockDrop = false;
var IsPostBlockDrop = false;

var IsDragging = false;
var WasNoneSelectedBeforeDrag = false;
var SelIdxBeforeDrag = -1;

var PreDropState = null;

var LastMousePos = null;
var LastMouseUpdateDateTime = null;

const MIN_DRAG_DIST = 10;

function initDragDrop() {
    initWindowDragDrop();

    let allDocTags = [...InlineTags, ...BlockTags];
    let allDocTagsQueryStr = allDocTags.join(',');
    //let editorElms = document.getElementById('editor').querySelectorAll(allDocTagsQueryStr);

    //enableDragDropOnElement(document.body);
    //enableDragDropOnElement(window);
    enableDragDropOnElement(getEditorContainerElement());

    //Array.from(editorElms).forEach(elm => {
    //    enableDragDropOnElement(elm);
    //});    


    //setInterval(onTick, 1000)
}

function onTick() {
    //let range = getSelection();
    //log("dragover idx " + range.index + ' length "' + range.length);

}


function enableDragDropOnElement(elm) {
    elm.addEventListener('dragenter', onDragEnter);
    elm.addEventListener('dragover', onDragOver);
    elm.addEventListener('dragleave', onDragLeave);
    elm.addEventListener('drop', onDrop);
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
    let selection = getSelection();
    if (!selection) {
        // occurs when drag started from host w/o sub-selection
        return IsDragging;
	}
    return IsDragging || selection.length > 0;
}

function isDragValid(dt, emp) {
    if (isDataTransferDataValid(dt) && dt.effectAllowed !== undefined && dt.effectAllowed != 'none') {
        let sel_range = getSelection();

        if (sel_range && sel_range.length > 0 && isPointInRange(emp, sel_range)) {
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
    IsDropCancel = isEscCancel;

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
    IsDropCancel = false;
}

function startDrag(isFromHost) {
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
        resetDragDrop(true);
	}
}

function checkCanDrag(emp) {
    //if (!IsSubSelectionEnabled) {
    //	onContentDraggableChanged_ntf(true);
    //	return true;
    //}

    let sel = getSelection();
    let is_none_selected = sel == null || sel.length == 0;
    if (is_none_selected) {
        //onContentDraggableChanged_ntf(false);
        return false;
    }
    let is_down_on_range = isPointInRange(emp, sel);
    let is_all_selected = isAllSelected();

    is_draggable = is_down_on_range || is_all_selected;
    //onContentDraggableChanged_ntf(is_draggable);
    return is_draggable;
}

function getDragData(dt) {
    // NOTE CefDragData should be pre-processed by host and ready from drop

    // TODO should deal w/ CefDragData here or it shouldn't matter

    //let item_data = isFormatted() ?
    //    getDataTransferHtml(dt) :
    //    getDataTransferPlainText(dt);
    //let drag_data = getDataTransferHtml(dt);
 //   if (isDragSource()) {
 //       let sel = getSelection();
 //       if (sel) {
 //           return getText(sel);
	//	}
        
	//}
    let drag_data = getDataTransferPlainText(dt);
    return drag_data;
}

function dropData(docIdx, data) {
    if (data.includes('{t{') && data.includes('}t}')) {
        // single template drop
        let tguid = data.split(',')[0].replace('{t{', '');
        let t = getTemplateDefByGuid(tguid);
        insertTemplate({ index: docIdx, length: 1 }, t,true);
        return;
	}
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


function onDragEnter(e) {
    if (!e.currentTarget || e.currentTarget.id == 'editor') {
        // currentTarget null when passed from host so force target to editor
        e.currentTarget = getEditorContainerElement();
        let dt = getDataTransferObject(e);
        let emp = getEditorMousePos(e);
        if (isDragValid(dt,emp)) {
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

    //onDragOver(e);
    //drawOverlay();
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

                DropIdx = getDocIdxFromPoint(LastMousePos);


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
            //log('Mouse DocIdx: ' + DropIdx + ' Mouse Pos: x: ' + LastMousePos.x + ' y: ' + LastMousePos.y + ' call count: '); 
        }
    } 
    e = setDropEffect(e);
}

function onDragLeave(e) {
    let mp = getEditorMousePos(e);
    let editor_rect = getEditorContainerRect();
    if (!isPointInRect(editor_rect,mp)) {
        e.stopPropagation();
        e.preventDefault();
        resetDragDrop();
	}
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
            let cur_sel = getSelection();
            if (!cur_sel) {
                // should only happen for single template drag
                cur_sel = {};
                let tdata = getDataTransferPlainText(dt);
                let tguid = tdata.split(',')[0].replace('{t{', '');
                let tiguid = tdata.split(',')[1].replace('}t}','');
                cur_sel.index = getTemplateDocIdx(tiguid);
                cur_sel.length = 0;
                let t = getTemplateDefByGuid(tguid);
                if (cur_sel.index < cur_drop_idx) {
                    cur_drop_idx -= 3;
				}
                quill.deleteText(cur_sel.index - 1, 3);
                insertTemplate({ index: cur_drop_idx, length: 0 }, t,true);
                //moveTemplate(tiguid, cur_drop_idx, false);
                //focusTemplate(tguid, false, tiguid);
                resetDragDrop();
                return;
			}
            if (cur_sel) {
                if (cur_drop_idx > cur_sel.index) {
                    // when dropidx is after cut remove cut length from drop idx
                    log('drop cut adjusted from: ' + cur_drop_idx);
                    cur_drop_idx -= cur_sel.length;
                    log('drop cut adjusted to: ' + cur_drop_idx);
                }
            } 
            setTextInRange(cur_sel, '');
        } else {
            // NOTE host should be notified of cut here or just handles if drop is success

        }
    }
    // to retain selection store doc length before paste to know drop data length (diff'd w/ length after)
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
    startDrag(false);
    let htmlStr = getSelectedHtml();
    e.dataTransfer.setData('text/html', htmlStr);
    let pt = getSelectedText();
    e.dataTransfer.setData('text/plain', pt);
    e.dataTransfer.setData('application/json/quill-delta', getSelectedDeltaJson());
    //e = setDataTransferObjectForSelection(e, 'drag');
}