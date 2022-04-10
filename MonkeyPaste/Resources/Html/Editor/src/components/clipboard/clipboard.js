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
    'application/octet-stream'
];

function initClipboard() {
    let allDocTags = [...InlineTags, ...BlockTags];
    let allDocTagsQueryStr = allDocTags.join(',');
    let editorElms = document.getElementById('editor').querySelectorAll(allDocTagsQueryStr);

    Array.from(editorElms).forEach(elm => {
        enablePasteHandler(elm);
    });
}
function enablePasteHandler(elm) {
    elm.onpaste = onPaste;
}

function onPaste(e) {
    var pastedText = undefined;
    if (e.clipboardData && e.clipboardData.getData) {
        pastedText = e.clipboardData.getData('text/plain');
    }
    alert(pastedText); // Process and handle text...
    return false; // Prevent the default handler from running.
}


function retargetHtmlClipboardData(htmlDataStr) {
    // TODO maybe wise to use requestRecentClipboardData here
    log('Paste Input: ');
    log(htmlDataStr);

    let newContentGuid = generateGuid();
    let cb_html = domParser.parseFromString(htmlDataStr, 'text/html');
    let cb_elms = cb_html.querySelectorAll('*');

    Array.from(cb_elms).forEach(elm => {
        retargetContentItemDomNode(elm, newContentGuid);
    });
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