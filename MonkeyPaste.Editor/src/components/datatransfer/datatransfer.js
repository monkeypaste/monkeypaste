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

    let pre_doc_length = getDocLength();

    if (!isNullOrEmpty(dt_html_str)) {
        setHtmlInRange(dest_doc_range, dt_html_str, source, true);
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

    // SELECT TRANSFER

    var dt_range = dest_doc_range;
    dt_range.length += dt_length_diff;
    setDocSelection(dt_range.index, dt_range.length);
    scrollDocRangeIntoView(dt_range);    

    if (!suppressMsg) {
        onDataTransferCompleted_ntf(source_url);
    }    

    if (wasTextChangeSuppressed) {
        SuppressTextChangedNtf = false;
    }
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers