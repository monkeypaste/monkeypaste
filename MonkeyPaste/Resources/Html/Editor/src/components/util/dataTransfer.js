var HostDataObj = null;

function convertHostDataToDataTransferObject(hdo) {
    let dtObj = {
        types: {}
    };
    for (var i = 0; i < hdo.items.length; i++) {
        let hdo_item = hdo.items[i];
        let dtf = convertHostDataFormatToDataTransferFormat(hdo_item.format);
        if (dtf) {
            dtObj.types[dtf] = dtf == 'text/html' ? atob(hdo_item.data) : hdo_item.data;
		}
    }
    return dtObj;
}

function convertHostDataFormatToDataTransferFormat(hdof) {
    if (hdof == 'HTML Format') {
        return 'text/html';
    }
    if (hdof == 'Text') {
        return 'text/plain';
    }
    log('unknown host format (passing through): ' + hdof);
    return null;
}

function convertDataTransferFormatToHostDataFormat(dtf) {
    if (dtf == 'text/html') {
        return 'HTML Format';
    }
    if (dtf == 'text/plain') {
        return 'Text';
    }
    log('unknown data transfer format (passing through): ' + dtf);
    return dtf;
}

function getDataTransferObject(e) {
    if (CefDragData) {
        let hdto = convertHostDataToDataTransferObject(CefDragData);
        return hdto;
    }

    let dtObj = e.detail ? e.detail.original.dataTransfer : e.dataTransfer;
    return dtObj;
}

function isDataTransferValid(dt) {
    if (CopyItemType == 'Text') {
        return hasPlainText(dt) || hasHtml(dt);
    }
    return false;
}

function hasPlainText(dt) {
    if (CefDragData) {
        return dt && dt.types && dt.types['text/plain'] != null;
	}
    return dt && dt.types && dt.types.indexOf('text/plain') > -1;
}

function hasHtml(dt) {
    if (CefDragData) {
        return dt && dt.types && dt.types['text/html'] != null;
    }
    return dt && dt.types && dt.types.indexOf('text/html') > -1;
}

function convertDataTransferToPlainText(dt) {
    if (dt == null) {
        return '';
    }
    if (hasPlainText(dt)) {
        let itemData = dt.getData('text/plain');
        return itemData;
    }
    if (hasHtml(dt)) {
        let itemData = dt.getData('text/html');
        itemData = parseForHtmlContentStr(itemData);
        let item_html_doc = domParser.parseFromString(itemData);
        //isHtml = true;
        return item_html_doc.body.innerText;
    }
    return '';
}

function convertDataTransferToHtml(dt) {
    if (dt == null) {
        return null;
    }
    for (var i = 0; i < dt.types.length; i++) {
        log('available type: ' + dt.types[i]);
    }

    if (dt.types.indexOf('text/html') > -1) {
        let itemData = dt.getData('text/html');
        itemData = parseForHtmlContentStr(itemData);
        //isHtml = true;
        return itemData;
    }
    if (dt.types.indexOf('text/plain') > -1) {
        let itemData = dt.getData('text/plain');
        itemData = '<html><body>' + itemData + '</body></html>';
        return itemData;
    }
}

function parseForHtmlContentStr(htmlStr) {
    if (!htmlStr) {
        return '';
    }

    let preTag = '<!--StartFragment-->';
    let preIdx = htmlStr.indexOf(preTag);
    if (preIdx < 0) {
        let bodyOpenTag = '<body>';
        let bodyOpenIdx = htmlStr.toLowerCase().indexOf(bodyOpenTag);
        let bodyCloseTag = '</body>';
        let bodyCloseIdx = htmlStr.toLowerCase().indexOf(bodyCloseTag);
        if (bodyOpenIdx >= 0 && bodyCloseIdx >= 0) {
            htmlStr = htmlStr.substring(bodyOpenIdx + bodyOpenTag.length, bodyCloseIdx - 1);
        }
        return htmlStr;
    }
    preIdx += preTag.length;

    let postTag = '<!--EndFragment-->';
    let postIdx = htmlStr.indexOf(postTag);
    if (postIdx < 0) {
        postIdx = htmlStr.length - 1;
    }
    let result = htmlStr.substring(preIdx, postIdx);
    return result;
}