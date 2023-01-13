// #region Globals

const LOCAL_HOST_URL = 'https://localhost';

const URL_DATA_FORMAT = "uniformresourcelocator";

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

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
        let di = {
            format: dt.types[i],
            data: dataStr
        };
        host_dimobj.dataItems.push(di);
    }
    return host_dimobj;
}

function performDataTransferOnContent(
    dt,
    dest_doc_range,
    source_doc_range,
    source = 'api',
    suppressMsg = false) {
	if (!dt || !dest_doc_range) {
        log('data transfer error  no data transfer or destination range');
        return;
    }

    let wasTextChangeSuppressed = false;
    if (!SuppressTextChangedNtf && suppressMsg) {
        // NOTE don't unflag textchange ntf if currently set (should be wrapped somewhere) only flag if needed
        SuppressTextChangedNtf = true;
        wasTextChangeSuppressed = true;
    }


    // DECODE HTML & URL FRAGMENT SOURCE (IF AVAILABLE)

    let source_url = null;
    let dt_html_str = null;
    if (dt.types.includes('html format')) {
        // prefer system html format to get source_url (on windows)
        dt_html_str = b64_to_utf8(dt.getData('html format'));
    } else if (dt.types.includes('text/html')) {
        dt_html_str = dt.getData('text/html');
    }

    if (dt_html_str != null && isHtmlClipboardFragment(dt_html_str)) {
        let cb_frag = parseHtmlFromHtmlClipboardFragment(dt_html_str);
        dt_html_str = cb_frag.html;
        source_url = cb_frag.sourceUrl;
    }
    // CHECK FOR INTERNAL URL SOURCE

    if (!source_url && dt.types.includes(URL_DATA_FORMAT)) {
        // TODO (on linux at least) check for moz uri here for source url
        let url_base64 = dt.getData(URL_DATA_FORMAT);
        source_url = b64_to_utf8(url_base64);
    }

    // PERFORM TRANSFER

    let transfer_deltas = null;
    let pre_delta = LastTextChangedDelta;
    let pre_doc_length = getDocLength();

    if (!isNullOrEmpty(dt_html_str)) {
        transfer_deltas = [];
        setHtmlInRange(dest_doc_range, dt_html_str, source, true);
    } else if (dt.types.includes('text/plain')) {
        transfer_deltas = [];
        let dt_pt_str = dt.getData('text/plain');
        setTextInRange(dest_doc_range, dt_pt_str, source, true);
    }
    if (transfer_deltas &&
        JSON.stringify(pre_delta) == JSON.stringify(LastTextChangedDelta)) {
        log('warning no data tranfser delta recorded!');
        transfer_deltas = null;
    }
    if (transfer_deltas) {
        transfer_deltas.push(LastTextChangedDelta);
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
        let pre_cut_delta = LastTextChangedDelta;
        setTextInRange(source_doc_range, '', source);

        if (JSON.stringify(pre_cut_delta) == JSON.stringify(LastTextChangedDelta)) {
            log('warning no data tranfser delta recorded during cut');
        } else {
            if (!transfer_deltas) {
                transfer_deltas = [];
            }
            transfer_deltas.push(LastTextChangedDelta);
        }
    }

    // SELECT TRANSFER

    var dt_range = dest_doc_range;
    dt_range.length += dt_length_diff;
    setDocSelection(dt_range.index, dt_range.length);
    scrollDocRangeIntoView(dt_range);    

    if (!suppressMsg) {
        let result_delta = null;
        if (transfer_deltas) {
            if (transfer_deltas.length > 1) {
                if (transfer_deltas.length > 2) {
                    // should only be max of 2 
                    debugger;
                }
                result_delta = transfer_deltas[0].compose(transfer_deltas[1]);
            } else if (transfer_deltas.length == 1) {
                result_delta = transfer_deltas[0];
            }            
        } 
        onDataTransferCompleted_ntf(
            source_url,
            result_delta,
            convertDataTransferToHostDataItems(dt));
    }    

    if (wasTextChangeSuppressed) {
        SuppressTextChangedNtf = false;
    }
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers