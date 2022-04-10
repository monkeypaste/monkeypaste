var CefDragData;

function initDragDrop() {
    initDragDropOverrides();

    let allDocTags = [...InlineTags, ...BlockTags];
    let allDocTagsQueryStr = allDocTags.join(',');
    let editorElms = document.getElementById('editor').querySelectorAll(allDocTagsQueryStr);

    Array.from(editorElms).forEach(elm => {
        enableDragDrop(elm);
    });    
}
function enableDragDrop(elm) {
    if (InlineTags.includes(elm.tagName.toLowerCase())) {
        elm.setAttribute('draggable', true);
        elm.addEventListener('mp_dragstart', onDragStart);
    }
    //elm.addEventListener('mp_drop', onDrop);
    elm.addEventListener('drop', onDrop);
}


function initDragDropOverrides() {
    // from https://stackoverflow.com/a/46986927/105028
    window.addEventListener('dragstart', function (event) {
        var event2 = new CustomEvent('mp_dragstart', { detail: { original: event } });
        event.target.dispatchEvent(event2);
        event.stopPropagation();
    }, true);

    //window.addEventListener('drop', function (event) {
    //    var event2 = new CustomEvent('mp_drop', { detail: { original: event } });
    //    event.target.dispatchEvent(event2);
    //    event.stopPropagation();
    //}, true);
}

function onDragEnter(e) {
    debugger;
}

function onDragStart(e) {
    log('drag started yo');
}

function onDrop(e) {
    //e.detail.original.preventDefault();

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
}

function onCefDragEnter(text) {
    let decodedText = atob(text);
    CefDragData = decodedText;
    log('onCefDragEnter called with text:');
    log(decodedText);

}