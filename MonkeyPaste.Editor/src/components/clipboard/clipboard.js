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
];

function initClipboard() {
    startClipboardHandler();
}

function addPlainHtmlClipboardMatchers() {
    // NOTE I think under the hood, quill handles html tags somehow but for xml tags it just
    // omits them completely so if content is xml or xml fragment, the entire content may just
    // become omitted

    // This is attached to converter and called during dangerousPaste and escapes non-quill nodes
    // I think this is called after quill does its thing so there won't be html tags in here.
    // may need more testing and/or new tags added to some kinda group or something to ignore this i don't know
    const Delta = Quill.imports.delta;

    quill.clipboard.addMatcher(Node.ELEMENT_NODE, function (node, delta) {
        let tag_name = node.tagName.toLowerCase();
        if (AllDocumentTags.includes(tag_name)) {
            // for normal tags use default behavior
            return delta;
        }
        // for any unrecognized tags treat its html as plain text
        if (delta && delta.ops !== undefined && delta.ops.length > 0) {
            for (var i = 0; i < delta.ops.length; i++) {
                if (delta.ops[i].insert === undefined) {
                    continue;
                }
                //delta.ops[i].insert = escapeHtml(node.outerHTML);
                //return delta;
                return new Delta().insert(escapeHtml(node.outerHTML));
			}
        }
        return delta;
    });
}

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
        elm.addEventListener('paste', onPaste, true);
        elm.addEventListener('cut', onCut,true);
        elm.addEventListener('copy', onCopy, true);
        //elm.addEventListener('keydown', onManualClipboardKeyDown);
        return;
    }
    elm.removeEventListener('paste', onPaste);
    elm.removeEventListener('cut', onCut);
    elm.removeEventListener('copy', onCopy);
    //elm.removeEventListener('keydown', onManualClipboardKeyDown);
}

function onCut(e) {
    let sel = getDocSelection();
    if (!sel) {
        return;
    }
    let selHtml = getHtml(sel);
    let selText = getText(sel, true);
    e.clipboardData.setData('text/html', selHtml);
    e.clipboardData.setData('text/plain', selText);

    setTextInRange(sel, '', 'silent');

    //let dt = getDataTransferObject(e);
    //let cut_action = {
    //    cut: getDataTransferDeltaJson(dt),
    //    range: sel
    //};
    //ReadOnlyCutPasteHistory_undo = [cut_action, ...ReadOnlyCutPasteHistory_undo];
    log('cut plaintext: ' + selText);
    log('cut html: ' + selHtml);


    e.preventDefault();
    return true;
    //e.stopPropagation();
    //setEditorContentEditable(false);
}
function onCopy(e) {
    let sel = getDocSelection();
    if (!sel) {
        return;
    }
    let selHtml = getHtml(sel);
    let selText = getText(sel, true);
    e.clipboardData.setData('text/html', selHtml);
    e.clipboardData.setData('text/plain', selText);

    e.preventDefault();
    //e.stopPropagation();
    //e = setDataTransferObjectForSelection(e, 'copy');
    //let dt = getDataTransferObject(e);
    //log('copy plaintext: ' + getDataTransferPlainText(dt));
    //log('copy html: ' + getDataTransferHtml(dt));
}

function onPaste(e) {
    let sel = getDocSelection();
    if (!sel) {
        log('no selection, cannot paste');
        return;
    }
    e.preventDefault();
    e.stopPropagation();

    if (e.clipboardData.types.includes('text/html')) {
        setHtmlInRange(sel, e.clipboardData.getData('text/html'), 'user', true);
        return;
    }
    if (e.clipboardData.types.includes('text/plain')) {
        setTextInRange(sel, e.clipboardData.getData('text/plain'), 'user', true);
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

    // setEditorContentEditable(true);

    if (undo_action.cut) {
        insertDelta({ index: undo_action.range.index, length: 0 }, undo_action.cut);
    } else if (undo_action.paste) {
        insertDelta(undo_action.range, undo_action.paste);
    } else {
        log('unknown undo action: ' + JSON.stringify(undo_action));
    }
    //quill.history.undo();
    //setEditorContentEditable(false);
}

function redoManualClipboardAction() {
    if (ReadOnlyCutPasteHistory_redo.length == 0) {
        return;
    }
    let redo_action = ReadOnlyCutPasteHistory_redo[0];
    ReadOnlyCutPasteHistory_redo.shift();
    ReadOnlyCutPasteHistory_undo = [redo_action, ...ReadOnlyCutPasteHistory_undo];

    //setEditorContentEditable(true);

    if (redo_action.cut) {
        setTextInRange(redo_action.range, '');
    } else if (redo_action.paste) {
        setTextInRange(redo_action.range, '');
        insertDelta(redo_action.range, redo_action.removed);
    } else {
        log('unknown undo action: ' + JSON.stringify(redo_action));
    }
    //quill.history.undo();
    //setEditorContentEditable(false);
}


function retargetHtmlClipboardData(htmlDataStr) {
    // TODO maybe wise to use requestRecentClipboardData here
    log('Paste Input: ');
    log(htmlDataStr);

    let newContentGuid = generateGuid();
    let cb_html = DomParser.parseFromString(htmlDataStr, 'text/html');
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
    let newContentGuid = getContentGuidByIdx(quill.getDocSelection().index);
    let ptHtmlStr = '<span copyItemInlineGuid="' + newContentGuid + '">' + pt + '"</span>';
    return ptHtmlStr;
}

async function requestRecentClipboardData(fromDateTime) {
    // fromDateTime should be null initially and will respond 
    // with all cb data since disableReadOnly but subsequent
    // will be since last request

    //maybe this can give c# info from database
}

function createHtmlClipboardFragment(htmlStr,range) {
    // NOTE not sure if this varies by OS, assuming no
    /*
Version:0.9
StartHTML:0000000165
EndHTML:0000001132
StartFragment:0000000201
EndFragment:0000001096
SourceURL:https://github.com/loagit/Quill-Examples-and-FAQ
<html>
<body>
<!--StartFragment--><ol dir="auto" style="box-sizing: border-box; padding-left: 2em; margin-top: 0px; margin-bottom: 16px; color: rgb(36, 41, 47); font-family: -apple-system, BlinkMacSystemFont, &quot;Segoe UI&quot;, Helvetica, Arial, sans-serif, &quot;Apple Color Emoji&quot;, &quot;Segoe UI Emoji&quot;; font-size: 16px; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: start; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(255, 255, 255); text-decoration-thickness: initial; text-decoration-style: initial; text-decoration-color: initial;"><li style="box-sizing: border-box;">ntry point. Open it to see the editor.</li><li style="box-sizing: border-box; margin-top: 0.25em;">app.js - The JavaScript source co</li></ol><!--EndFragment-->
</body>
</html>
    */

    let num_str = '0000000000';
    let pre_fragment_str = '<!--StartFragment-->';
    let sourceUrl = 'https://abc.com';
    let join_str = envNewLine();

    let fragment_parts = [
        'Version:0.9',
        'StartHTML:' + num_str, //[1]
        'EndHTML:' + num_str,     //[2]
        'StartFragment:' + num_str,
        'EndFragment:' + num_str,
        'SourceURL:' + sourceUrl,
        '<html>',                   //[6]
        '<body>',
        pre_fragment_str,     //[8]
        '</body>',
        '</html>'
    ];

    fragment_parts[8] += htmlStr;


    let start_html_idx = fragment_parts.slice(0, 6).join('').length + (6 * join_str.length);
    let end_html_idx = fragment_parts.join(join_str).length;// + (fragment_parts.length * join_str.length);

    let start_fragment_idx = fragment_parts.slice(0, 8).join('').length + (8 * join_str.length);
    start_fragment_idx += pre_fragment_str.length;
    let end_fragment_idx = fragment_parts.slice(0, 9).join('').length + (9 * join_str.length);

    fragment_parts[1] = fragment_parts[1].replace(num_str, numToPaddedStr(start_html_idx, '0', 10));
    fragment_parts[2] = fragment_parts[2].replace(num_str, numToPaddedStr(end_html_idx, '0', 10));

    fragment_parts[3] = fragment_parts[3].replace(num_str, numToPaddedStr(start_fragment_idx, '0', 10));
    fragment_parts[4] = fragment_parts[4].replace(num_str, numToPaddedStr(end_fragment_idx, '0', 10));

    return fragment_parts.join(join_str);
}
