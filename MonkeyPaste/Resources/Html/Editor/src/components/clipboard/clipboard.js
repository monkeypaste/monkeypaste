var LastCutDocRange = { index: 0, length: 0 };
var LastCutOrCopyUpdatedHtml = '';

var PasteNode;

var ReadOnlyCutPasteHistory_undo = [];
var ReadOnlyCutPasteHistory_redo = [];

const CB_DATA_TYPES = [
    'text/plain',
    'text/uri-list',
    'text/csv',
    'text/css',
    'text/html', //4
    'application/xhtml+xml',
    'image/png', //6
    'image/jpg',
    'image/jpeg',
    'image/gif',
    'image/svg+xml',
    'application/xml',
    'text/xml',
    'application/javascript',
    'application/json',
    'application/octet-stream',
    'application/json/quill-delta' //custom for quill delta
];

function startClipboardHandler() {
    ReadOnlyCutPasteHistory_undo = [];
    ReadOnlyCutPasteHistory_redo = [];
    updateClipboardHandlers(window, true);

    //let allDocTags = [...InlineTags, ...BlockTags];
    //let allDocTagsQueryStr = allDocTags.join(',');
    //let editorElms = document.getElementById('editor').querySelectorAll(allDocTagsQueryStr);

    //Array.from(editorElms).forEach(elm => {
    //    // WAIT!! don't enable unless necessary quill handles this better than manually (internally at least)

    //    //enableClipboardHandlers(elm);
    //});
}

function stopClipboardHandler() {
    ReadOnlyCutPasteHistory_undo = [];
    ReadOnlyCutPasteHistory_redo = [];
    updateClipboardHandlers(window, false);
}

function updateClipboardHandlers(elm, isEnabled) {
    if (isEnabled) {
        elm.addEventListener('paste', onPaste);
        elm.addEventListener('cut', onCut);
        elm.addEventListener('copy', onCopy);
        elm.addEventListener('keydown', onManualClipboardKeyDown);
        return;
    }
    elm.removeEventListener('paste', onPaste);
    elm.removeEventListener('cut', onCut);
    elm.removeEventListener('copy', onCopy);
    elm.removeEventListener('keydown', onManualClipboardKeyDown);
}

function onCut(e) {
    if (isEditorToolbarVisible() || !isContentEditable()) {
        // these events shouldn't be enabled in edit mode
        debugger;
        return;
	}
    let sel = getEditorSelection();
    if (!sel) {
        return;
    }
    setEditorContentEditable(true);

    e = setDataTransferObjectForSelection(e, 'cut');
    setTextInRange(sel, '', 'user');


    let dt = getDataTransferObject(e);
    let cut_action = {
        cut: getDataTransferDeltaJson(dt),
        range: sel
    };
    ReadOnlyCutPasteHistory_undo = [cut_action, ...ReadOnlyCutPasteHistory_undo];
    log('cut plaintext: ' + getDataTransferPlainText(dt));
    log('cut html: ' + getDataTransferHtml(dt));


    setEditorContentEditable(false);
}
function onCopy(e) {
    e = setDataTransferObjectForSelection(e, 'copy');
    let dt = getDataTransferObject(e);
    log('copy plaintext: ' + getDataTransferPlainText(dt));
    log('copy html: ' + getDataTransferHtml(dt));
}

function onPaste(e) {
    let dt = getDataTransferObject(e);
    if (!isDataTransferValid(dt)) {
        log('cannot paste, dt is invalid');
        return;
    }
    let sel = getEditorSelection();
    if (!sel) {
        log('no selection, cannot paste');
        return;
    }

    let paste_action = {
        paste: getDataTransferDeltaJson(dt),
        range: sel,
        removed: JSON.stringify(getDelta(sel))
    };

    ReadOnlyCutPasteHistory_undo = [paste_action, ...ReadOnlyCutPasteHistory_undo];

    if (hasQuillDeltaJson(dt)) {
        setTextInRange(sel, '');
        let deltaObj = JSON.parse(getDataTransferDeltaJson(dt));
        deltaObj = [{ retain: sel.index }, ...deltaObj.ops];
        quill.updateContents(deltaObj);
        return;
	}
    if (hasHtml(dt)) {
        setTextInRange(sel, '');
        insertHtml(sel.index, getDataTransferHtml(dt));
        return;
    }
    if (hasPlainText(dt)) {
        setTextInRange(sel, '');
        insertText(sel.index, getDataTransferPlainText(dt));
        return;
	}
    log('unknown paste format');
}

function onManualClipboardKeyDown(e) {
    if (isEditorToolbarVisible() || !isContentEditable()) {
        // these events shouldn't be enabled in edit mode
        //debugger;
        return;
    }
    if (e.ctrlKey && e.key === 'z') {
        // undo
        undoManualClipboardAction();
        return;
    }
    if (e.ctrlKey && e.key === 'y') {
        // redo
        redoManualClipboardAction();
        return;
    }
}

function undoManualClipboardAction() {
    if (ReadOnlyCutPasteHistory_undo.length == 0) {
        return;
    }
    let undo_action = ReadOnlyCutPasteHistory_undo[0];
    ReadOnlyCutPasteHistory_undo.shift();
    ReadOnlyCutPasteHistory_redo = [undo_action, ...ReadOnlyCutPasteHistory_redo];

    setEditorContentEditable(true);

    if (undo_action.cut) {
        insertDelta({ index: undo_action.range.index, length: 0 }, undo_action.cut);
    } else if (undo_action.paste) {
        insertDelta(undo_action.range, undo_action.paste);
    } else {
        log('unknown undo action: ' + JSON.stringify(undo_action));
    }
    //quill.history.undo();
    setEditorContentEditable(false);
}

function redoManualClipboardAction() {
    if (ReadOnlyCutPasteHistory_redo.length == 0) {
        return;
    }
    let redo_action = ReadOnlyCutPasteHistory_redo[0];
    ReadOnlyCutPasteHistory_redo.shift();
    ReadOnlyCutPasteHistory_undo = [redo_action, ...ReadOnlyCutPasteHistory_undo];

    setEditorContentEditable(true);

    if (redo_action.cut) {
        setTextInRange(redo_action.range, '');
    } else if (redo_action.paste) {
        setTextInRange(redo_action.range, '');
        insertDelta(redo_action.range, redo_action.removed);
    } else {
        log('unknown undo action: ' + JSON.stringify(redo_action));
    }
    //quill.history.undo();
    setEditorContentEditable(false);
}


function retargetHtmlClipboardData(htmlDataStr) {
    // TODO maybe wise to use requestRecentClipboardData here
    log('Paste Input: ');
    log(htmlDataStr);

    let newContentGuid = generateGuid();
    let cb_html = domParser.parseFromString(htmlDataStr, 'text/html');
    let cb_elms = cb_html.querySelectorAll('*');

    for (var i = 0; i < cb_elms.length; i++) {
        retargetContentItemDomNode(cb_elms[i], newContentGuid);
	}
    let cb_elms_str = cb_elms.toString();

    log('Paste Output:');
    log(cb_elms_str);

    return cb_elms_str;
}

function retargetPlainTextClipboardData(pt) {
    // TODO maybe wise to use requestRecentClipboardData here 
    let newContentGuid = getContentGuidByIdx(quill.getEditorSelection().index);
    let ptHtmlStr = '<span copyItemInlineGuid="' + newContentGuid + '">' + pt + '"</span>';
    return ptHtmlStr;
}

async function requestRecentClipboardData(fromDateTime) {
    // fromDateTime should be null initially and will respond 
    // with all cb data since disableReadOnly but subsequent
    // will be since last request

    //maybe this can give c# info from database
}