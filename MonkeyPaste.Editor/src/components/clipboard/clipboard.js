// #region Globals

const CEF_CB_DATA_FORMATS = [
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

// #endregion Globals

// #region Life Cycle

function initClipboard() {
    startClipboardHandler();
}

// #endregion Life Cycle

// #region Getters

function getContentDataTransfer(ignoreSelection = false, formats = null, encodeTemplates = false) {
    let is_for_ole = !ignoreSelection;
    if (!formats) {
        formats = [
            'text/html',
            'text/plain'
        ];
    }

    var dt = new DataTransfer();

    e.clipboardData.setData('text/html', selHtml);
    e.clipboardData.setData('text/plain', selText);
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isHtmlClipboardFragment(dataStr) {
    // TODO need to check common browser html clipboard formats this is only for Chrome on Windows
    if (!dataStr.startsWith("Version:") || !dataStr.includes("StartHTML:") || !dataStr.includes("EndHTML:")) {
        return false;
    }
    return true;
}

function isHtmlFormat(format) {
    return format.toLowerCase() == 'html format' || format == 'text/html';
}

function isUri(format) {
    return format.toLowerCase() == 'html format' || format == 'text/html';
}

function isPlainTextFormat(format) {
    return format.toLowerCase == 'text' ||
        format.toLowerCase == 'unicode' ||
        format.toLowerCase == 'oemtext' ||
            format == 'text/plain';
}
function isCsvFormat(format) {
    return format.toLowerCase() == 'csv' || format == 'text/csv';
}

function isImageFormat(format) {
    return
        format.toLowerCase == 'png' ||
        format.toLowerCase() == 'bitmap' ||
        format.toLowerCase() == 'deviceindependentbitmap' ||
        format.startsWith('image/');
}

function isFileListFormat(format) {
    // NOTE files aren't in dataTransfer.items so no mime type equivalent
    return format.toLowerCase() == 'filenames';
}

function isInternalClipTileFormat(format) {
    return format.toLowerCase() == "mp internal content";
}

// #endregion State

// #region Actions

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
    window.addEventListener('paste', onPaste, true);
    window.addEventListener('cut', onCut, true);
    window.addEventListener('copy', onCopy, true);
}

function stopClipboardHandler() {
    window.removeEventListener('paste', onPaste);
    window.removeEventListener('cut', onCut);
    window.removeEventListener('copy', onCopy);
}

function parseHtmlFromHtmlClipboardFragment(cbDataStr) {
    let cbData = {
        sourceUrl: '',
        html: ''
    };
    let sourceUrlToken = 'SourceURL:';
    let source_url_start_idx = cbDataStr.indexOf(sourceUrlToken) + sourceUrlToken.length;
    if (source_url_start_idx >= 0) {
        let source_url_length = substringByLength(cbDataStr, source_url_start_idx).indexOf(envNewLine());
        if (source_url_length >= 0) {
            let parsed_url = substringByLength(cbDataStr, source_url_start_idx, source_url_length);
            if (isValidHttpUrl(parsed_url)) {
                cbData.sourceUrl = parsed_url;
            }
        }
    }

    let htmlStartToken = '<!--StartFragment-->';
    let htmlEndToken = '<!--EndFragment-->';

    let html_start_idx = cbDataStr.indexOf(htmlStartToken) + htmlStartToken.length;
    if (html_start_idx >= 0) {
        let html_length = cbDataStr.indexOf(htmlEndToken);
        cbData.html = substringByLength(cbDataStr, html_start_idx, html_length);
    }

    return cbData;
}

function createHtmlClipboardFragment(htmlStr, range) {
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
    let post_fragment_str = '<!--EndFragment-->';
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

    fragment_parts[8] += htmlStr + post_fragment_str;

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


// #endregion Actions

// #region Event Handlers

function onCut(e) {
    onSetClipboardRequested_ntf();

    if (ContentItemType == 'Text') {
        setTextInRange(sel, '');
    } else {
        // sub-selection ignored for other types
    }
    e.preventDefault();
    return true;
}
function onCopy(e) {
    onSetClipboardRequested_ntf();
    e.preventDefault();
}

function onPaste(e) {
    let sel = getDocSelection();
    if (!sel) {
        log('no selection, cannot paste');
        return;
    }

    if (e.clipboardData.types.includes('text/html')) {
        setHtmlInRange(sel, e.clipboardData.getData('text/html'), 'user', true);
    } else if (e.clipboardData.types.includes('text/plain')) {
        setTextInRange(sel, e.clipboardData.getData('text/plain'), 'user', true);
    }

    e.preventDefault();
    e.stopPropagation();
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
// #endregion Event Handlers