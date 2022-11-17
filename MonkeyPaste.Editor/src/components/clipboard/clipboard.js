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
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State


function isHtmlFormat(format) {
    return format.toLowerCase() == 'html format' || format == 'text/html';
}

function isUri(format) {
    return format.toLowerCase() == 'html format' || format == 'text/html';
}

function isPlainTextFormat(format) {
    return format.toLowerCase() == 'text' ||
        format.toLowerCase() == 'unicode' ||
        format.toLowerCase() == 'oemtext' ||
            format == 'text/plain';
}
function isCsvFormat(format) {
    return format.toLowerCase() == 'csv' || format == 'text/csv';
}

function isImageFormat(format) {
    return
        format.toLowerCase() == 'png' ||
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
    performDataTransferOnContent(e.clipboardData,getDocSelection());

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