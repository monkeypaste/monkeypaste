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
    elm.addEventListener('mp_drop', onDrop);
    
}

function initDragDropOverrides() {
    // from https://stackoverflow.com/a/46986927/105028
    window.addEventListener('dragstart', function (event) {
        // (note: not cross-browser)
        var event2 = new CustomEvent('mp_dragstart', { detail: { original: event } });
        event.target.dispatchEvent(event2);
        event.stopPropagation();
    }, true);

    window.addEventListener('drop', function (event) {
        // (note: not cross-browser)
        var event2 = new CustomEvent('mp_drop', { detail: { original: event } });
        event.target.dispatchEvent(event2);
        event.stopPropagation();
    }, true);

    //document.getElementById('editor').addEventListener('drop', onDrop);
}


function onDragStart(e) {
    log('drag started yo');
}

function onDrop(e) {
    //e.detail.original.preventDefault();

    let itemData = '';
    let isHtml = false;
    let dt = e.detail.original.dataTransfer;
    if (dt.types.indexOf('text/html') > -1) {
        itemData = dt.getData('text/html');
        itemData = parseForHtmlContentStr(itemData);
        isHtml = true;
    }
    if (itemData == '' && dt.types.indexOf('text/plain') > -1) {
        itemData = dt.getData('text/plain');
        //itemData = retargetPlainTextClipboardData(itemData);
    }
    if (itemData != '') {
        let dropIdx = getEditorIndexFromPoint2({ x: e.detail.original.clientX, y: e.detail.original.clientY });
        quill.setSelection(dropIdx, 0);
        if (isHtml) {
            quill.clipboard.dangerouslyPasteHTML(dropIdx, itemData);
        } else {
            quill.insertText(dropIdx, itemData);
        }
    }

    log('drop dat shhhiiiit:');
    log(itemData);
}