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
    if (!isRunningOnHost()) {
        return;
    }
    // NOTE if cut/copy was internal and all supported formats set,
    // the e.clipboardData obj strips everything but files from the transfer 
    // so this makes a get request and gets back current clipboard asynchronously
    e.preventDefault();
    e.stopPropagation();

    var cur_paste_sel = getDocSelection();

    getClipboardDataTransferObjectAsync_get()
        .then((result) => {
            performDataTransferOnContent(result, cur_paste_sel);
    });
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