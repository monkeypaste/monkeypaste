var CefDragData;
var DropElm;

var IsSplitDrop = false;
var IsPreBlockDrop = false;
var IsPostBlockDrop = false;

var DropIdx = -1;

var IsCtrlDown = false; //duplicate
var IsShiftDown = false; //split 
var IsAltDown = false; // w/ formatting (as html)? ONLY formating? dunno

var MousePos = { x: -1, y: -1 };

function initDragDrop() {
    initDragDropOverrides();

    let allDocTags = [...InlineTags, ...BlockTags];
    let allDocTagsQueryStr = allDocTags.join(',');
    let editorElms = document.getElementById('editor').querySelectorAll(allDocTagsQueryStr);

    enableDragDropOnElement(document.getElementById('editor'));
    Array.from(editorElms).forEach(elm => {
       // enableDragDropOnElement(elm);
    });    

    window.addEventListener('mousemove', onMouseMove);
}


function initDragDropOverrides() {
    // from https://stackoverflow.com/a/46986927/105028
    window.addEventListener('dragstart', function (event) {
        event.dataTransfer.effectAllowed = 'all';

        var event2 = new CustomEvent('mp_dragstart', { detail: { original: event } });
        event.target.dispatchEvent(event2);
        event.stopPropagation();
    }, true);

    window.addEventListener('dragend', function (event) {
        //var event2 = new CustomEvent('mp_dragstart', { detail: { original: event } });
        //event.target.dispatchEvent(event2);
        event.stopPropagation();

        resetDragDrop();
    }, true);

    window.addEventListener('drop', function (event) {
        //var event2 = new CustomEvent('mp_drop', { detail: { original: event } });
        //event.target.dispatchEvent(event2);
        event.stopPropagation();

        onDrop(event);
    }, true);
}

function enableDragDropOnElement(elm) {
    if (InlineTags.includes(elm.tagName.toLowerCase())) {
        elm.setAttribute('draggable', true);

        elm.addEventListener('mp_dragstart', onDragStart);
    } else if (BlockTags.includes(elm.tagName.toLowerCase())) {

        elm.addEventListener('dragenter', onDragEnter)
        elm.addEventListener('dragover', onDragOver);
        elm.addEventListener('dragleave', onDragLeave);
        elm.addEventListener('drop', onDrop);

        elm.addEventListener('mp_drop', onDrop);
    }
    //elm.addEventListener('drop', onDrop);
}

function isDropping() {
    return DropElm != null;
}

function isDuplicate() {
    return IsCtrlDown;
}

function isFormatted() {
    return IsAltDown;
}

function isDragSource() {
    let selection = quill.getSelection();
    return selection.length > 0;
}


function resetDragDrop() {
    DropElm = null;
    setTextSelectionBgColor('lightblue');
    setTextSelectionFgColor('black');

    IsPreBlockDrop = false;
    IsPostBlockDrop = false;
    IsSplitDrop = false;

    MousePos = { x: -1, y: -1 };

    DropIdx = -1;

    drawOverlay();
}

function getEditorMousePos(e) {
    if (!e || !e.clientX || !e.clientY) {
        return { x: -1, y: -1 };
    }

    let mp = { x: parseFloat(e.clientX), y: parseFloat(e.clientY) };

    let editor_rect = getEditorRect(false);

    mp.x = mp.x - editor_rect.left;
    mp.y = mp.y - editor_rect.top;

    return mp;
}

function updateModKeys(e) {
    IsCtrlDown = e.ctrlKey;
    IsAltDown = e.altKey;
    IsShiftDown = e.shiftKey;
    if (IsCtrlDown || IsAltDown || IsShiftDown) {
        return;
	}
}

function onMouseMove(e) {
    if (isDropping() && parseInt(e.buttons) == 0) {
        resetDragDrop();
	}
}

function onDragEnter(e) {
    //log('onDragEnter: ' + e);
    DropElm = e.currentTarget;
    updateModKeys(e);

    drawOverlay();
}
function onDragOver(e) {
    //log('onDragOver: ' + e);
    let mp = getEditorMousePos(e);
    updateModKeys(e);
        
    if (isDropping()) {
        e.preventDefault();

        if (dist(MousePos, mp) > 1) {
            MousePos = mp;
            DropIdx = getEditorIndexFromPoint(MousePos);
            log('Mouse DocIdx: ' + DropIdx + 'Mouse Pos: x: ' + MousePos.x + ' y: ' + MousePos.y); 
        }
    } 
    drawOverlay();
}
function onDragLeave(e) {
    drawOverlay();
}


//function onDrop(e) {
//    //e.detail.original.preventDefault();
//    log('onDrop: '+e);

//    let itemHtml = '';

//    if (CefDragData) {
//        //e.preventDefault();
//        //itemHtml = retargetPlainTextClipboardData(CefDragData);
//        //CefDragData = null;
//        let dropIdx = 0;//getEditorIndexFromPoint_ByLine({ x: e.clientX, y: e.clientY });
//        quill.setSelection(dropIdx, 0);
//        quill.insertText(0, CefDragData);
//        //quill.clipboard.dangerouslyPasteHTML(dropIdx, CefDragData);
//    } else {
//        let mp = null;
//        if (e.detail) {
//            itemHtml = convertDataTransferToHtml(e.detail.original.dataTransfer);
//            mp = { x: e.detail.original.clientX, y: e.detail.original.clientY };
//        } else {
//            itemHtml = convertDataTransferToHtml(e.dataTransfer);
//            mp = { x: e.clientX, y: e.clientY }
//        }

//        if (itemHtml != '') {
//            let dropIdx = getEditorIndexFromPoint_ByLine(mp);
//            quill.setSelection(dropIdx, 0);
//            quill.clipboard.dangerouslyPasteHTML(dropIdx, itemHtml);
//            //if (isHtml) {
//            //    quill.clipboard.dangerouslyPasteHTML(dropIdx, itemData);
//            //} else {
//            //    quill.insertText(dropIdx, itemData);
//            //}
//        }

//    //let isHtml = false;
//    //let dt = e.detail.original.dataTransfer;
//    //if (dt.types.indexOf('text/html') > -1) {
//    //    itemData = dt.getData('text/html');
//    //    itemData = parseForHtmlContentStr(itemData);
//    //    isHtml = true;
//    //}
//    }
//    log('drop dat shhhiiiit:');
//    log(CefDragData);

//    resetDragDrop();
//}

function onDrop(e) {
    log('onDrop: ' + e);

    
    let drop_data = '';
    if (isFormatted()) {
        drop_data = convertDataTransferToHtml(e.detail.original.dataTransfer);
	}
    if (isDragSource()) {

        
	}

    if (CefDragData) {
        //e.preventDefault();
        //itemHtml = retargetPlainTextClipboardData(CefDragData);
        //CefDragData = null;
        //let dropIdx = 0;//getEditorIndexFromPoint_ByLine({ x: e.clientX, y: e.clientY });
        quill.setSelection(DropIdx, 0);
        quill.insertText(0, CefDragData);
        //quill.clipboard.dangerouslyPasteHTML(dropIdx, CefDragData);
    } else {
        let dt = e.detail ? e.detail.original.dataTransfer : e.dataTransfer;

        //let item_data = isFormatted() ?
        //    convertDataTransferToHtml(dt) :
        //    convertDataTransferToPlainText(dt);
        let item_data = convertDataTransferToHtml(dt);

        if (item_data != '') {
            if (!isDuplicate()) {
                setTextInRange(quill.getSelection(), '');
            }

            //store doc length before paste to know drop data length (diff'd w/ length after)
            let pre_doc_length = quill.getLength();
            let length_delta = 0;
            let post_sel_start_idx = DropIdx;

            if (IsPreBlockDrop) {
                let isFirstLine = getLineIdx(DropIdx) == 0;
                if (!isFirstLine) {
                    log('WARNING! drop is flagged as pre block but not 1st line line is ' + getLineIdx(DropIdx));
                } else {
                    DropIdx = 0;
                }
                quill.insertText(0, '\n');
                quill.clipboard.dangerouslyPasteHTML(0, item_data);

                length_delta = quill.getLength() - pre_doc_length - 1;
                post_sel_start_idx = 0;
            } else if (IsPostBlockDrop) {
                DropIdx = getLineEndDocIdx(DropIdx);
                quill.insertText(DropIdx, '\n');
                quill.clipboard.dangerouslyPasteHTML(DropIdx + 1, item_data);

                length_delta = quill.getLength() - pre_doc_length - 1;
                post_sel_start_idx = DropIdx + 1;
            } else if (IsSplitDrop) {
                quill.insertText(DropIdx, '\n');
                quill.insertText(DropIdx, '\n');
                quill.clipboard.dangerouslyPasteHTML(DropIdx + 1, item_data);

                length_delta = quill.getLength() - pre_doc_length - 2;
                post_sel_start_idx = DropIdx + 1;
            } else {
                quill.clipboard.dangerouslyPasteHTML(DropIdx, item_data);

                length_delta = quill.getLength() - pre_doc_length;
            }
            quill.setSelection(post_sel_start_idx, length_delta);


            //if (IsSplitDrop || IsPostBlockDrop) {
            //    setTextInRange({ index: DropIdx, length: 1 }, '\n');
            //    DropIdx++;
            //}
            //// SECURITY paste data should be sanitized here when html
            ////quill.clipboard.dangerouslyPasteHTML(DropIdx, item_data);

            //if (IsPreBlockDrop || IsPostBlockDrop) {
            //    let length_delta = quill.getLength() - pre_doc_length;
            //    DropIdx += length_delta + 1;
            //}
            //if (IsPreBlockDrop || IsPostBlockDrop || IsSplitDrop) {
            //    setTextInRange({ index: DropIdx, length: 1 }, '\n');
            //}
            
            //if (isHtml) {
            //    quill.clipboard.dangerouslyPasteHTML(dropIdx, itemData);
            //} else {
            //    quill.insertText(dropIdx, itemData);
            //}
        }
    }
    resetDragDrop();
}

function onDragStart(e) {
    log('drag started yo');
}

function onCefDragEnter(text) {
    let decodedText = atob(text);
    CefDragData = decodedText;
    log('onCefDragEnter called with text:');
    log(decodedText);

}


