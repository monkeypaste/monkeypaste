
// #region Life Cycle

// #endregion Life Cycle

// #region Getters

function getAllowedDataTransferTypes(contentType) {
    // NOTE this returns lower case format names
    if (contentType == 'Image') {
        return [];
    }
    if (contentType == 'FileList') {
        return ['files'];
    }
    return ['text/plain', 'text/html', 'application/json', 'files', 'text', 'html format']
}

function getDataTransferPlainText(dt) {
    if (!isValidDataTransfer(dt)) {
        return '';
    }
    for (var i = 0; i < dt.types.length; i++) {
        let dt_type = dt.types[i];
        if (isPlainTextFormat(dt_type.toLowerCase())) {
            return dt.getData(dt_type);
        }
    }
    return '';
    
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isInternalDataTransfer(dt) {
    if (!dt || 
        typeof dt.getData !== 'function') {
        return false;
    }
    const dt_url_data = dt.getData(globals.URL_DATA_FORMAT);
    const test = dt.getData('UniformResourceLocator');
    if (!isString(dt_url_data)) {
        return false;
    }
    const is_internal = dt_url_data.toLowerCase().startsWith(globals.INTERNAL_SOURCE_BASE_URI);
    log('drop url: ' + dt_url_data);
    return is_internal;
}

function isValidDataTransfer(dt) {
    if (!dt || !Array.isArray(dt.types)) {
        return false;
    }
    for (var i = 0; i < dt.types.length; i++) {
        let dt_type = dt.types[i];
        if (getAllowedDataTransferTypes(globals.ContentItemType).includes(dt_type.toLowerCase())) {
            return true;
        }
    }
    return false;
}
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
   //     if (isFileListFormat(dti.format.toLowerCase())) {
   //         let file_frag_obj = {
   //             fileItems: []
   //         };
   //         let fpl = dti.data.split(envNewLine());
   //         for (var j = 0; j < fpl.length; j++) {
   //             let fp = decodeURIComponent(fpl[j]);
			//}
   //     }
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
        let dataStr = null;
        if (isFileListFormat(dt.types[i].toLowerCase())) {
            // NOTE cef encodes spaces in file names so fix em
            dataStr = Array.from(dt.files).map(x => x.name).join(envNewLine());
        } else {
            dataStr = dt.getData(dt.types[i]);
            if (!isString(dt.types[i])) {
                dataStr = JSON.stringify(dataStr);
            }
        }
        let di = {
            format: dt.types[i],
            data: dataStr
        };
        host_dimobj.dataItems.push(di);
    }
    return host_dimobj;
}

function prepareDeltaLogForDataTransfer() {
    if (!isReadOnly() && hasTextChangedDelta()) {
        // when editing log current editor state as a transaction before drop
        let edit_dt_msg = getLastTextChangedDataTransferMessage();
        onDataTransferCompleted_ntf(
            edit_dt_msg.changeDeltaJsonStr,
            edit_dt_msg.sourceDataItemsJsonStr,
            edit_dt_msg.transferLabel);
    }
    clearLastDelta();
}



function performDataTransferOnContent(
    dt,
    source_doc_range,
    dest_doc_range,
    source = 'api',
    transferLabel = '') {
    // called on paste, drop and append
	if (!dt || !dest_doc_range) {
        log('data transfer error  no data transfer or destination range');
        return;
    }

    let sup_guid = suppressTextChanged();

    // REFRESH DELTA LOG

    prepareDeltaLogForDataTransfer();

    // COLLECT URI SOURCES  (IF AVAILABLE)    
    let source_urls = [];
    if (dt.types.includes(globals.URI_LIST_FORMAT)) {
        let uri_data = dt.getData(globals.URI_LIST_FORMAT);
        if (typeof uri_data === 'string' || uri_data instanceof String) {
            source_urls = splitByNewLine(uri_data);
        } else {
            // what is uri_data type?
            debugger;
        }
    } 

    // CHECK FOR URL FRAGMENT SOURCE (IF AVAILABLE)

    let dt_html_data = getDataTransferHtmlFragment(dt);
    if (dt_html_data && !isNullOrWhiteSpace(dt_html_data.sourceUrl)) {
        source_urls.push(dt_html_data.sourceUrl);
    }

    // CHECK FOR INTERNAL URL SOURCE (internally deprecated but needed on linux (maybe mac too))

    if (dt.types.includes(globals.URL_DATA_FORMAT)) {
        // TODO (on linux at least) check for moz uri here for source url
        let url_base64 = dt.getData(globals.URL_DATA_FORMAT);
        let nx_source_url = b64_to_utf8(url_base64);
        if (!isNullOrEmpty(nx_source_url)) {
            source_urls.push(nx_source_url);
        }        
    }

    // PERFORM TRANSFER

    let dt_range = transferContent(dt, source_doc_range, dest_doc_range, source);

    // REPORT TRANSFER

    if (source_urls.length > 0) {
        dt.setData(globals.URI_LIST_FORMAT, source_urls.join(envNewLine()));
    }
    let host_dt_obj = convertDataTransferToHostDataItems(dt);
    onDataTransferCompleted_ntf(
        toBase64FromJsonObj(globals.LastTextChangedDelta),
        toBase64FromJsonObj(host_dt_obj),
        transferLabel);

    // SCROLL TO DEST

    updateQuill();
    scrollDocRangeIntoView(dt_range);
    drawOverlay();

    // RESET STATE

    // clear delta tracker to mark end of transaction
    clearLastDelta()

    unsupressTextChanged(sup_guid);
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