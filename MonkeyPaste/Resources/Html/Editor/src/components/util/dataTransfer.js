
function convertDataTransferToPlainText(dt) {
    if (dt == null) {
        return '';
    }
    if (dt.types.indexOf('text/plain') > -1) {
        let itemData = dt.getData('text/plain');
        return itemData;
    }
    if (dt.types.indexOf('text/html') > -1) {
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