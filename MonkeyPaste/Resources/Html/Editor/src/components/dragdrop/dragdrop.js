var CefDragData;
var DropElm;
var WasReadOnly;

function initDragDrop() {
    initDragDropOverrides();

    let allDocTags = [...InlineTags, ...BlockTags];
    let allDocTagsQueryStr = allDocTags.join(',');
    let editorElms = document.getElementById('editor').querySelectorAll(allDocTagsQueryStr);

    enableDragDrop(document.getElementById('editor'));
    Array.from(editorElms).forEach(elm => {
        enableDragDrop(elm);
    });    
}

function enableDragDrop(elm) {
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


function initDragDropOverrides() {
    var wasReadOnly = false;
    // from https://stackoverflow.com/a/46986927/105028
    window.addEventListener('dragstart', function (event) {
        var event2 = new CustomEvent('mp_dragstart', { detail: { original: event } });
        event.target.dispatchEvent(event2);
        event.stopPropagation();
    }, true);

    window.addEventListener('dragend', function (event) {
        //var event2 = new CustomEvent('mp_dragstart', { detail: { original: event } });
        //event.target.dispatchEvent(event2);
        event.stopPropagation();
        if (wasReadOnly) {
            enableReadOnly();
        }
        wasReadOnly = false;
    }, true);

    window.addEventListener('drop', function (event) {
        //var event2 = new CustomEvent('mp_drop', { detail: { original: event } });
        //event.target.dispatchEvent(event2);
        event.stopPropagation();

        onDrop(event);
    }, true);
}

function IsDropping() {
    return DropElm != null;
}

function onDragEnter(e) {
    log('onDragEnter: ' + e);
    if (!IsDropping()) {
        WasReadOnly = IsReadOnly();
        if (WasReadOnly) {
            disableReadOnly({ isSilent: true });
        }
	}
}
function onDragOver(e) {
    log('onDragOver: ' + e);
    DropElm = e.currentTarget;
}
function onDragLeave(e) {
    log('onDragLeave: ' + e);    
}


function onDrop(e) {
    //e.detail.original.preventDefault();
    log('onDrop: '+e);

    let itemHtml = '';

    if (CefDragData) {
        //e.preventDefault();
        //itemHtml = retargetPlainTextClipboardData(CefDragData);
        //CefDragData = null;
        let dropIdx = 0;//getEditorIndexFromPoint2({ x: e.clientX, y: e.clientY });
        quill.setSelection(dropIdx, 0);
        quill.insertText(0, CefDragData);
        //quill.clipboard.dangerouslyPasteHTML(dropIdx, CefDragData);
    } else {
        let mp = null;
        if (e.detail) {
            itemHtml = convertDataTransferToHtml(e.detail.original.dataTransfer);
            mp = { x: e.detail.original.clientX, y: e.detail.original.clientY };
        } else {
            itemHtml = convertDataTransferToHtml(e.dataTransfer);
            mp = { x: e.clientX, y: e.clientY }
        }

        if (itemHtml != '') {
            let dropIdx = getEditorIndexFromPoint2(mp);
            quill.setSelection(dropIdx, 0);
            quill.clipboard.dangerouslyPasteHTML(dropIdx, itemHtml);
            //if (isHtml) {
            //    quill.clipboard.dangerouslyPasteHTML(dropIdx, itemData);
            //} else {
            //    quill.insertText(dropIdx, itemData);
            //}
        }

    //let isHtml = false;
    //let dt = e.detail.original.dataTransfer;
    //if (dt.types.indexOf('text/html') > -1) {
    //    itemData = dt.getData('text/html');
    //    itemData = parseForHtmlContentStr(itemData);
    //    isHtml = true;
    //}
    }
    log('drop dat shhhiiiit:');
    log(CefDragData);

    ResetDragDrop();
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

function ResetDragDrop() {
    if (WasReadOnly) {
        enableReadOnly();
    }
    WasReadOnly = null;
    DropElm = null;
}