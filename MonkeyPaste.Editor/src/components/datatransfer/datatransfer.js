// #region Globals

const LOCAL_HOST_URL = 'https://localhost';

const URL_DATA_FORMAT = "UniformResourceLocator";

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

    if (dt.types.includes('text/html')) {
        let dt_html_str = dt.getData('text/html');
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

    if (!source_url && dt.types.includes(URL_DATA_FORMAT)) {
        source_url = dt.getData(URL_DATA_FORMAT);
    }
    onDataTransferCompleted_ntf(source_url);
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers