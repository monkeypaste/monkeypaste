var HostDataObj = null;

function convertHostDataToDataTransferObject(hdo) {
    let dtObj = {
        types: {}
    };
    if (!hdo.items) {
        // occurs or dragleave
        return dtObj;
	}
    for (var i = 0; i < hdo.items.length; i++) {
        let hdo_item = hdo.items[i];
        let dtf = hdo_item.format;// convertHostDataFormatToDataTransferFormat(hdo_item.format);
        if (dtf) {
            dtObj.types[dtf] = hdo_item.data;// dtf == 'text/html' ? atob(hdo_item.data) : hdo_item.data;
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

function getDataByType(dt, typeStr) {
    if (!dt) {
        return null;
    }
    if (typeof dt.getData === 'function') {
        return dt.getData(typeStr);
    }
    if (!dt.types) {
        return null;
	}
    return dt.types['text/plain'];
}

function hasPlainText(dt) {
    return getDataByType(dt, 'text/plain') != null;
}

function hasHtml(dt) {
    return getDataByType(dt, 'text/html') != null;
}

function convertDataTransferToPlainText(dt) {
    if (dt == null) {
        return '';
    }
    if (hasPlainText(dt)) {
        let itemData = getDataByType(dt,'text/plain');
        return itemData;
    }
    if (hasHtml(dt)) {
        let itemData = getDataByType(dt, 'text/html');
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

    if (hasHtml(dt)) {
        let itemData = getDataByType(dt, 'text/html');
        itemData = parseForHtmlContentStr(itemData);
        //isHtml = true;
        return itemData;
    }
    if (hasPlainText()) {
        let itemData = getDataByType(dt, 'text/plain');
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