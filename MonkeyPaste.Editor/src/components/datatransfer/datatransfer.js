// #region Globals

const LOCAL_HOST_URL = 'https://localhost';

const URL_DATA_FORMAT = "uniformresourcelocator";

const URI_LIST_FORMAT = 'text/uri-list';

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

function getDataTransferDeltaAndAdjRange(dest_doc_range, dt_delta) {
    const Delta = Quill.imports.delta;

    // PROCESS DEST RANGE MODE (if any)
    dest_doc_range.mode = dest_doc_range.mode === undefined ? 'inline' : dest_doc_range.mode;
    let mode_delta = null;
    if (dest_doc_range.mode == 'block') {
        if (dest_doc_range.index == 0) {
            // pre transfer
            mode_delta = new Delta().insert('\n');
        } else {
            // post transfer
            let eol_idx = getLineEndDocIdx(dest_doc_range.index);
            if (eol_idx < getDocLength() - 1) {
                // ignore new line for last line since it already is a new line
                mode_delta = new Delta().retain(eol_idx).insert('\n');
                dest_doc_range.index = 1;
            }
        }
    } else if (dest_doc_range.mode == 'split') {
        // split transfer
        mode_delta = new Delta().retain(dest_doc_range.index).insert('\n\n');
        dest_doc_range.index = 1;
    }
    if (mode_delta == null) {
        // inline/default transfer
        mode_delta = new Delta().retain(dest_doc_range.index).delete(dest_doc_range.length);
        //dest_doc_range.index = 0;
    }


    let adj_dt_delta = new Delta().retain(dest_doc_range.index).compose(dt_delta);
    let out_delta = mode_delta.compose(adj_dt_delta);

    return {
        delta: out_delta,
        dest_range: dest_doc_range
    };
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

// #endregion State

// #region Actions

function convertHostDataItemsToDataTransfer(dataItemsMsgObj) {
    // input 'MpQuillHostDataItemsMessageFragment'

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
    // output 'MpQuillHostDataItemsMessageFragment'

    let host_dimobj = {
        dataItems: []
    };
    for (var i = 0; i < dt.types.length; i++) {
        let dataStr = dt.getData(dt.types[i]);
        if (typeof dt.getData(dt.types[i]) !== 'string' && !(dt.getData(dt.types[i]) instanceof String)) {
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

    // PREPARE DROPRANGE

    dest_doc_range = prepareDestDocRangeForDataTransfer(dest_doc_range, source);

    // PRE TRANSFER DEFS

    //let transfer_deltas = null;
    let pre_delta = LastTextChangedDelta;
    let pre_doc_length = getDocLength();

    // PERFORM TRANSFER

    //let dt_delta_obj = null;
    if (!isNullOrEmpty(dt_html_str)) {
        //transfer_deltas = [];
        let dt_html_delta = convertHtmlToDelta(dt_html_str);
        dt_html_delta = decodeHtmlEntitiesInDeltaInserts(dt_html_delta);
        //dt_delta_obj = getDataTransferDeltaAndAdjRange(dest_doc_range, dt_html_delta);
        insertDelta(dest_doc_range, dt_html_delta, source);
        //setHtmlInRange(dest_doc_range, dt_html_str, source, true);
    } else if (dt.types.includes('text/plain')) {
        //transfer_deltas = [];
        let dt_pt_str = dt.getData('text/plain');
        //const Delta = Quill.imports.delta;
        //dt_delta_obj = getDataTransferDeltaAndAdjRange(dest_doc_range, new Delta().insert(dt_pt_str));

        setTextInRange(dest_doc_range, dt_pt_str, source, true);
    }
    //if (transfer_deltas &&
    //    JSON.stringify(pre_delta) == JSON.stringify(LastTextChangedDelta)) {
    //    log('warning no data tranfser delta recorded!');
    //    transfer_deltas = null;
    //}
    //if (transfer_deltas) {
    //    transfer_deltas.push(LastTextChangedDelta);
    //}

    //if (dt_delta_obj != null) {
    //    quill.updateContents(dt_delta_obj, source);
    //    dest_doc_range = dt_delta_obj.dest_range;
    //}

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
        //let pre_cut_delta = LastTextChangedDelta;
        setTextInRange(source_doc_range, '', source);

        //if (JSON.stringify(pre_cut_delta) == JSON.stringify(LastTextChangedDelta)) {
        //    log('warning no data tranfser delta recorded during cut');
        //} else {
        //    if (!transfer_deltas) {
        //        transfer_deltas = [];
        //    }
        //    transfer_deltas.push(LastTextChangedDelta);
        //}
    }

    // SELECT TRANSFER

    var dt_range = dest_doc_range;
    dt_range.length += dt_length_diff;
    setDocSelection(dt_range.index, dt_range.length);
    scrollDocRangeIntoView(dt_range);    

    //let result_delta = null;
    //if (transfer_deltas) {
    //    if (transfer_deltas.length > 1) {
    //        if (transfer_deltas.length > 2) {
    //            // should only be max of 2 
    //            debugger;
    //        }
    //        result_delta = mergeDeltas(transfer_deltas[0], transfer_deltas[1]);
    //    } else if (transfer_deltas.length == 1) {
    //        result_delta = transfer_deltas[0];
    //    }
    //}
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



// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers