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

const CB_TEXT_DATA_TYPES = [CB_DATA_TYPES[0]];
const CB_CSV_DATA_TYPES = [CB_DATA_TYPES[2]];
const CB_HTML_DATA_TYPES = [CB_DATA_TYPES[4]];

const CB_IMAGE_DATA_TYPES = [CB_DATA_TYPES[6], CB_DATA_TYPES[7], CB_DATA_TYPES[8], CB_DATA_TYPES[9]];

const CB_URI_DATA_TYPES = [CB_DATA_TYPES[1]];

const CB_SUPPORTED_FORMATS = [
    CB_CSV_DATA_TYPES,
    CB_TEXT_DATA_TYPES,
    CB_HTML_DATA_TYPES
];

function initClipboard() {
    const Delta = Quill.imports.delta;
    const clipboardItems = [];

    // Cases
    // 1. Cb data was cut or copied from THIS document 
    //    - this is known if the clipboard html data equals Last var and can be pasted as is
    // 2. Cb data was copied externally and only has text (NOTE 1)
    // 3. Cb data was copied externally and has html (NOTE 1)
    // 4. Cb data was copied from another tile within app


    // Cases
    // 1. A selection is CUT
    //      -If remaining document does NOT contain a cut contentGuid then 
    //       full item was removed so do not alter its contentBlot
    // 2. A selection is COPIED
    //      -Since content will be duplicated if pasted always give new guid

    // Notes
    // 1. When data is brought in externally then clipboard watcher will add it to db
    //    so in enableReadOnly back in c# new guids should be cross-checked by data
    //    if found in db use its source info when creating it (may be problematic)
    quill.clipboard.addMatcher(Node.ELEMENT_NODE, function (node, delta) {
        //PasteNode = retargetContentItemDomNode(node);
        
        PasteNode = node;
        return delta;
    });
    quill.clipboard.addMatcher('[templateGuid]', function (node, delta) {
        //PasteNode = retargetContentItemDomNode(node);
        debugger;
        PasteNode = node;
        return delta;
    });


    //document.addEventListener('paste', function (e) {
    //    if (!quill.hasFocus()) {
    //        return;
    //    }
    //    let srange = quill.getSelection();
    //    let cb = e.clipboardData;
    //    if (!cb || !cb.items || cb.items.length == 0) {
    //        return;
    //    }

    //    let itemData = ''
    //    if (cb.types.indexOf('text/html') > -1) {
    //        //itemData = cb.getData('text/html');
    //        //itemData = parseForHtmlContentStr(itemData);
    //    } else if (cb.types.indexOf('text/plain') > -1) {
    //        itemData = cb.getData('text/plain');
    //        itemData = retargetPlainTextClipboardData(itemData);
    //    } 

    //    if (itemData != '') {
    //        e.preventDefault();
            
    //        if (srange.length > 0) {
    //            quill.deleteText(srange.index,srange.length);
    //        }
    //        quill.clipboard.dangerouslyPasteHTML(srange.index, itemData);

    //        //quill.insertText(
    //        //    srange.index,
    //        //    itemData,
    //        //    {
    //        //        copyItemGuid: generateGuid()
    //        //    });
    //    }
    //});

}

function parseForHtmlContentStr(htmlStr) {
    if (!htmlStr) {
        return '';
    }
    let preTag = '<!--StartFragment-->';
    let preIdx = htmlStr.indexOf(preTag);
    if (preIdx < 0) {
        return htmlStr;
    }
    preIdx += preTag.length;

    let postTag = '<!--EndFragment-->';
    let postIdx = htmlStr.indexOf(postTag);
    if (postIdx < 0) {
        postIdx = htmlStr.length - 1;
    }
    let result = htmlStr.substring(preIdx, postIdx);
    return result;
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
    let ptHtmlStr = '<span copyItemGuid="' + newContentGuid + '">' + pt + '"</span>';
    return ptHtmlStr;
}

async function requestRecentClipboardData(fromDateTime) {
    // fromDateTime should be null initially and will respond 
    // with all cb data since disableReadOnly but subsequent
    // will be since last request

    //maybe this can give c# info from database
}