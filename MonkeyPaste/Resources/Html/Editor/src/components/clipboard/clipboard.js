var LastCutDocRange = { index: 0, length: 0 };
var LastCutOrCopyUpdatedHtml = '';

var PasteNode;

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

function initClipboard() {
    let allDocTags = [...InlineTags, ...BlockTags];
    let allDocTagsQueryStr = allDocTags.join(',');
    let editorElms = document.getElementById('editor').querySelectorAll(allDocTagsQueryStr);

    Array.from(editorElms).forEach(elm => {
        // WAIT!! don't enable unless necessary quill handles this better than manually (internally at least)

        //enableClipboardHandlers(elm);
    });
}

function enableClipboardHandlers(elm) {
    elm.addEventListener('paste', onPaste);
    elm.addEventListener('cut', onCut);
    elm.addEventListener('copy', onCopy);
}

function onCut(e) {
    e = setDataTransferObject(e, 'cut');
    let dt = getDataTransferObject(e);
    log('cut plaintext: ' + getDataTransferPlainText(dt));
    log('cut html: ' + getDataTransferHtml(dt));
}
function onCopy(e) {
    e = setDataTransferObject(e, 'copy');
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
    let sel = getSelection();
    if (!sel) {
        log('no selection, cannot paste');
        return;
    }
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
    let newContentGuid = getContentGuidByIdx(quill.getSelection().index);
    let ptHtmlStr = '<span copyItemInlineGuid="' + newContentGuid + '">' + pt + '"</span>';
    return ptHtmlStr;
}

async function requestRecentClipboardData(fromDateTime) {
    // fromDateTime should be null initially and will respond 
    // with all cb data since disableReadOnly but subsequent
    // will be since last request

    //maybe this can give c# info from database
}