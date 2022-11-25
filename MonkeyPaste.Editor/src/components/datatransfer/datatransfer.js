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

function performDataTransferOnContent(dt, dest_doc_range) {
	if (!dt || !dest_doc_range) {
        log('data transfer error  no data transfer or destination range');
        return;
    }

    let source_url = null;
    let dt_html_str = null;
    if (dt.types.includes('html format')) {
        // prefer system html format to get source_url (on windows)
        dt_html_str = b64_to_utf8(dt.getData('html format'));
    } else if (dt.types.includes('text/html')) {
        let dt_html_str = dt.getData('text/html');
    }

    if (!isNullOrEmpty(dt_html_str)) {
        if (isHtmlClipboardFragment(dt_html_str)) {
            let cb_frag = parseHtmlFromHtmlClipboardFragment(dt_html_str);
            dt_html_str = cb_frag.html;
            source_url = cb_frag.sourceUrl;
        }
        setHtmlInRange(dest_doc_range, dt_html_str, 'user', true);
    } else if (dt.types.includes('text/plain')) {
        let dt_pt_str = dt.getData('text/plain');
        setTextInRange(dest_doc_range, dt_pt_str, 'user', true);
    }

    // TODO (on linux at least) check for moz uri here for source url

    if (!source_url && dt.types.includes(URL_DATA_FORMAT)) {
        let url_base64 = dt.getData(URL_DATA_FORMAT);
        source_url = b64_to_utf8(url_base64);
    }
    onDataTransferCompleted_ntf(source_url);
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers