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

const PLACEHOLDER_DATAOBJECT_TEXT = '3acaaed7-862d-47f5-8614-3259d40fce4d';


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


function isHtmlFormat(lwc_format) {
    const result =
        lwc_format == 'html format' ||
        lwc_format == 'text/html';
    return result;
}

function isUri(lwc_format) {
    const result =
        lwc_format == 'html format' ||
        lwc_format == 'text/html';
    return result;
}

function isPlainTextFormat(lwc_format) {
    const result =
        lwc_format == 'text' ||
        lwc_format == 'unicode' ||
        lwc_format == 'oemtext' ||
        lwc_format == 'text/plain';

    return result;
}
function isCsvFormat(lwc_format) {
    const result =
        lwc_format == 'csv' ||
        lwc_format == 'text/csv';
    return result;
}

function isImageFormat(lwc_format) {
    const result = 
        lwc_format == 'png' ||
        lwc_format == 'bitmap' ||
        lwc_format == 'deviceindependentbitmap' ||
        lwc_format.startsWith('image/');

    return result;
}

function isFileListFormat(lwc_format) {
    // NOTE files aren't in dataTransfer.items so no mime type equivalent
    const result =
        lwc_format == 'filenames';
    return result;
}

function isInternalClipTileFormat(lwc_format) {
    const result =
        lwc_format == "mp internal content";
    return result;
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
    let sel = getDocSelection();
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
            performDataTransferOnContent(result, cur_paste_sel,null,'api','Pasted');
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