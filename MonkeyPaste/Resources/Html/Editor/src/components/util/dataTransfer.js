var HostDataObj = null;


function getDataTransferObject(e) {
    if (CefDragData) {
        let hdto = convertHostDataToDataTransferObject(CefDragData);
        return hdto;
    }
    if (e.detail !== undefined) {
        // drag drop override (unused)
        if (e.detail.original !== undefined) {
            return e.detail.original.dataTransfer;
		}
    }
    if (e.dataTransfer !== undefined) {
        // drag drop
        return e.dataTransfer;
    }
    if (e.clipboardData !== undefined) {
        return e.clipboardData;
	}
    log('unknown data transfer object');
    return null;
}


function setDataTransferObjectForSelection(e, actionType) {
    let dt = getDataTransferObject(e);
    if (actionType == 'drag') {
        if (e.target.classList !== undefined &&
            e.target.classList.contains(TemplateEmbedClass)) {
            // BUGGY this should probably dealt w/ differently, when single template is selected
            let t = getTemplateFromDomNode(e.target);
            dt.setData('text/html', e.target.outerHTML);
            dt.setData('text/plain', '{t{' + t.templateGuid + ',' + t.templateInstanceGuid + '}t}');
        } else {
            dt.setData('text/html', getSelectedHtml());
            dt.setData('text/plain', getSelectedText());
            dt.setData('application/json/quill-delta', getSelectedDeltaJson());
        }
        e.dataTransfer = dt;

    } else if (actionType == 'cut' || actionType == 'copy') {
        dt.setData('text/html', getSelectedHtml());
        dt.setData('text/plain', getSelectedText());
        dt.setData('application/json/quill-delta', getSelectedDeltaJson());
        e.clipboardData = dt;
    } else {
        log('cannot set dataTransfer object, unknown actionType: ' + actionType);
    }
    return e;
}

function getDataTransferObjectForRange(range) {
    let dt = new DataTransfer();
    dt.setData('text/html', getHtml(range));
    dt.setData('text/plain', getText(range));
    dt.setData('application/json/quill-delta', getDeltaJson(range));
}

function isDataTransferValid(dt) {
    if (CopyItemType == 'Text') {
        return hasPlainText(dt) || hasHtml(dt) || hasQuillDeltaJson(dt);
    }
    return false;
}

function hasQuillDeltaJson(dt) {
    return getDataByType(dt, 'application/json/quill-delta');
}
function hasPlainText(dt) {
    return getDataByType(dt, 'text/plain') != null;
}

function hasHtml(dt) {
    return getDataByType(dt, 'text/html') != null;
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



function getDataTransferPlainText(dt) {
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

function getDataTransferHtml(dt) {
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

function getDataTransferDeltaJson(dt) {
    if (hasQuillDeltaJson(dt)) {
        return getDataByType(dt, 'application/json/quill-delta');
    }
    return null;
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

// host (system format) converters

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