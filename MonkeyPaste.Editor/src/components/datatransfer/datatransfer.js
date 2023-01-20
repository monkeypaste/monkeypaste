// #region Globals

const LOCAL_HOST_URL = 'https://localhost';
const URL_DATA_FORMAT = "uniformresourcelocator";
const URI_LIST_FORMAT = 'text/uri-list';
const HTML_FORMAT = 'text/html'
const HTML_FRAGMENT_FORMAT = 'html format'
const TEXT_FORMAT = 'text/plain'

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function convertHostDataItemsToDataTransfer(dataItemsMsgObj) {
    // input 'MpQuillHostDataItemsMessage'

    let dt = new DataTransfer();
    if (!dataItemsMsgObj || !dataItemsMsgObj.dataItems || dataItemsMsgObj.dataItems.length == 0) {
        return dt;
    }
    for (var i = 0; i < dataItemsMsgObj.dataItems.length; i++) {
        let dti = dataItemsMsgObj.dataItems[i];
        dt.setData(dti.format, dti.data);
    }
    return dt;
}

function convertDataTransferToHostDataItems(dt) {
    // output 'MpQuillHostDataItemsMessage'

    let host_dimobj = {
        dataItems: [],
        effectAllowed: dt.effectAllowed
    };
    for (var i = 0; i < dt.types.length; i++) {
        let dataStr = dt.getData(dt.types[i]);
        if (!isString(dt.types[i])) {
            dataStr = JSON.stringify(dataStr);
        }
        let hostFormat = dt.types[i];
        if (hostFormat.toLowerCase() == 'files') {
            //debugger;
            hostFormat = 'FileNames';
        }
        let di = {
            format: hostFormat,
            data: dataStr
        };
        host_dimobj.dataItems.push(di);
    }
    return host_dimobj;
}

function prepareDeltaLogForDataTransfer() {
    if (!isReadOnly() && LastTextChangedDelta != null) {
        // when editing log current editor state as a transaction before drop
        let dti_msg = {
            dataItems: [
                {
                    format: URI_LIST_FORMAT,
                    data: JSON.stringify([`${LOCAL_HOST_URL}/?type=UserDevice&id=-1`])
                }
            ]
        };
        let edit_dt_msg = {
            changeDeltaJsonStr: JSON.stringify(LastTextChangedDelta),
            sourceDataItemsJsonStr: dti_msg,
            transferLabel: 'Edit'
        };
        onDataTransferCompleted_ntf(
            edit_dt_msg.changeDeltaJsonStr,
            edit_dt_msg.sourceDataItemsJsonStr,
            edit_dt_msg.transferLabel);
    }
    clearLastDelta();
}

function prepareDestDocRangeForDataTransfer(dest_doc_range, drop_insert_source) {
    dest_doc_range.mode = dest_doc_range.mode === undefined ? 'inline' : dest_doc_range.mode;
    switch (dest_doc_range.mode) {
        case 'split':
            insertText(dest_doc_range.index, '\n', drop_insert_source);
            insertText(dest_doc_range.index, '\n', drop_insert_source);
            dest_doc_range.index += 1;
            break;
        case 'pre':
            dest_doc_range.index = 0;
            insertText(dest_doc_range.index, '\n', drop_insert_source);
            break;
        case 'post':
            dest_doc_range.index = getLineEndDocIdx(dest_doc_range.index);
            if (dest_doc_range.index < getDocLength() - 1) {
                // ignore new line for last line since it already is a new line
                insertText(dest_doc_range.index, '\n', drop_insert_source);
                dest_doc_range.index += 1;
            }
            break;
        case 'inline':
        default:
            break;
    }
    return dest_doc_range;
}

function performDataTransferOnContent(
    dt,
    dest_doc_range,
    source_doc_range,
    source = 'api',
    transferLabel = '') {
    // called on paste, drop and append
	if (!dt || !dest_doc_range) {
        log('data transfer error  no data transfer or destination range');
        return;
    }

    let wasTextChangeSuppressed = SuppressTextChangedNtf;
    if (!SuppressTextChangedNtf) {
        // NOTE don't unflag textchange ntf if currently set (should be wrapped somewhere) only flag if needed
        SuppressTextChangedNtf = true;
    }

    // REFRESH DELTA LOG

    prepareDeltaLogForDataTransfer();

    // COLLECT URI SOURCES  (IF AVAILABLE)    
    let source_urls = [];
    if (dt.types.includes(URI_LIST_FORMAT)) {
        let uri_data = dt.getData(URI_LIST_FORMAT);
        if (typeof uri_data === 'string' || uri_data instanceof String) {
            source_urls = splitByNewLine(uri_data);
        } else {
            // what is uri_data type?
            debugger;
        }
    } 

    // DECODE HTML & URL FRAGMENT SOURCE (IF AVAILABLE)    

    let dt_html_str = null;
    if (dt.types.includes('html format')) {
        // prefer system html format to get sourceurl (on windows)
        dt_html_str = b64_to_utf8(dt.getData('html format'));
    } else if (dt.types.includes('text/html')) {
        dt_html_str = dt.getData('text/html');
    }

    if (dt_html_str != null && isHtmlClipboardFragment(dt_html_str)) {
        let cb_frag = parseHtmlFromHtmlClipboardFragment(dt_html_str);
        dt_html_str = cb_frag.html;
        if (!isNullOrEmpty(cb_frag.sourceUrl)) {
            source_urls.push(cb_frag.sourceUrl);
        }
    }

    // CHECK FOR INTERNAL URL SOURCE (internally deprecated but needed on linux (maybe mac too))

    if (dt.types.includes(URL_DATA_FORMAT)) {
        // TODO (on linux at least) check for moz uri here for source url
        let url_base64 = dt.getData(URL_DATA_FORMAT);
        let nx_source_url = b64_to_utf8(url_base64);
        if (!isNullOrEmpty(nx_source_url)) {
            source_urls.push(nx_source_url);
        }        
    }

    // PREPARE DROP RANGE

    dest_doc_range = prepareDestDocRangeForDataTransfer(dest_doc_range, source);

    // PRE TRANSFER DEFS

    let pre_doc_length = getDocLength();

    // PERFORM TRANSFER

    if (!isNullOrEmpty(dt_html_str)) {
        let dt_html_delta = convertHtmlToDelta(dt_html_str);
        dt_html_delta = decodeHtmlEntitiesInDeltaInserts(dt_html_delta);
        insertDelta(dest_doc_range, dt_html_delta, source);
    } else if (dt.types.includes('text/plain')) {
        let dt_pt_str = dt.getData('text/plain');
        setTextInRange(dest_doc_range, dt_pt_str, source, true);
    }

    let dt_length_diff = getDocLength() - pre_doc_length;

    // REMOVE SOURCE

    if (source_doc_range) {
        if (dest_doc_range.index < source_doc_range.index) {
            // when drop is before drag sel adjust drag range from added drop length
            source_doc_range.index += dt_length_diff;
        } else {
            // adjust doc diff for removed source for removed drag length
            dest_doc_range.index -= source_doc_range.length;
        }
        setTextInRange(source_doc_range, '', source);
    }

    // SELECT DEST

    var dt_range = dest_doc_range;
    dt_range.length += dt_length_diff;
    setDocSelection(dt_range.index, dt_range.length);
    scrollDocRangeIntoView(dt_range);    

    if (source_urls.length > 0) {
        dt.setData(URI_LIST_FORMAT, source_urls.join('\r\n'));
    }
    let host_dt_obj = convertDataTransferToHostDataItems(dt);
    onDataTransferCompleted_ntf(
        LastTextChangedDelta,
        host_dt_obj,
        transferLabel);

    // clear delta tracker to mark end of transaction
    clearLastDelta()

    if (wasTextChangeSuppressed) {
        SuppressTextChangedNtf = false;
    }
}


function logDataTransfer(dt, title) {
    log('------------------------------------------------');
    if (title) {
        log(title);
    }
    if (isNullOrUndefined(dt)) {
        log('null dataTransfer');
        return;
    }
    for (var i = 0; i < dt.types.length; i++) {
        log(`Format: '${dt.types[i]}'`);
        log(dt.getData(dt.types[i]));
    }
}

function logHostDataObject(hdo, title) {
    log('------------------------------------------------');
    if (title) {
        log(title);
    }
    if (isNullOrUndefined(hdo) || isNullOrUndefined(hdo.dataItems)) {
        log('no host data object to log');
        return;
    }
    for (var i = 0; i < hdo.dataItems.length; i++) {
        log(`Format: '${hdo.dataItems[i].format}'`);
        log(hdo.dataItems[i].data);
    }
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers