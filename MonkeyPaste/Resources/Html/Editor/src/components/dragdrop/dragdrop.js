function initDragDrop() {
    initDragDropOverrides();

    let editorElms = document.getElementById('editor').querySelectorAll('p,span,div');

    Array.from(editorElms).forEach(elm => {
        enableDragDrop(elm);
    });    
}

function enableDragDrop(elm) {
    elm.addEventListener('mp_dragstart', onDragStart);
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
}



function onDragStart(e) {
    log('drag started yo');
}

function onDrop(e) {
    log('drop dat shhhiiiit');
}