var ContentInlineGuid;
var ContentInlineSourceGuid;
var FromUserInline;
var Draggable;

var ENCODED_CONTENT_OPEN_TOKEN = "{c{";
var ENCODED_CONTENT_CLOSE_TOKEN = "}c}";
var ENCODED_CONTENT_REGEXP;

//#region Content Blot Lifecycle

function initContent(itemHtml, openTag = "{c{", closeTag = "}c}") {
    //encodeContentOps(itemOps, openTag, closeTag);

    //decodeContent(itemOps);
    //registerContentGuidAttribute();
    //registerContentBlots();
    setHtml(itemHtml);
    //


    //initContentRangeListeners();
}

function registerContentGuidAttribute() {
    Parchment = Quill.import('parchment');

    let suppressWarning = false;
    let inlineConfig = {
        scope: Parchment.Scope.INLINE,
    };

    ContentGuid = new Parchment.Attributor('copyItemGuid', 'copyItemGuid', inlineConfig);
    Quill.register(ContentGuid, suppressWarning);

    ContentSourceGuid = new Parchment.Attributor('copyItemSourceGuid', 'copyItemSourceGuid', inlineConfig);
    Quill.register(ContentSourceGuid, suppressWarning);

    FromUser = new Parchment.Attributor('fromUser', 'fromUser', inlineConfig);
    Quill.register(FromUser, suppressWarning);
}

function getContentItemFromDomNode(domNode) {
    if (domNode == null) {
        return null;
    }
    let ci = {
        copyItemGuid: domNode.getAttribute('copyItemGuid'),
        copyItemSourceGuid: domNode.getAttribute('copyItemSourceGuid'),
    }
    if (domNode.getAttribute('fromUser')) {
        ci.fromUser = domNode.getAttribute('fromUser')
    }
    return ci;
}

function applyContentItemToDomNode(node, value) {
    if (node == null || value == null) {
        return node;
    }
    if (isBlockElement(node)) {

    }
    node.setAttribute('copyItemGuid', value.copyItemGuid);
    node.setAttribute('copyItemSourceGuid', value.copyItemSourceGuid);
    if (value.fromUser) {
        node.setAttribute('fromUser', '');
    }

    return node;
}

function retargetContentItemDomNode(node, newContentGuid) {
    if (node == null) {
        return node;
    }
    newContentGuid = newContentGuid == null ? generateGuid() : newContentGuid;
    if (node.getAttribute('copyItemSourceGuid')) {
        //when source is already set ignore if it changes again because new one is not original
    } else {
        node.setAttribute('copyItemSourceGuid', node.getAttribute('copyItemGuid'));
    }
    node.setAttribute('copyItemGuid', newContentGuid);
    return node;
}

function formatPasteNodeDelta(delta, oldDelta, source) {
    // NOTE called in text-change when PasteNode != null
    let idx = 0;
    let retargetedNode = retargetContentItemDomNode(PasteNode);
    let contentBlot = getContentItemFromDomNode(retargetedNode);
    for (var i = 0; i < delta.ops.length; i++) {
        let op = delta.ops[i];

        if (op.retain) {
            idx += op.retain;
        }
        if (op.insert) {
            let insertRange = { index: idx, length: op.insert.length };
            if (contentBlot.copyItemGuid) {
                IgnoreNextTextChange = true;
                quill.formatText(insertRange, 'copyItemGuid', contentBlot.copyItemGuid);
                IgnoreNextTextChange = true;
                quill.formatText(insertRange, 'copyItemSourceGuid', contentBlot.copyItemSourceGuid);
            } else {
                IgnoreNextTextChange = true;
                quill.formatText(insertRange, 'fromUser', '');
            }
            idx += op.insert.length;
        }
    }
    PasteNode = null;
}
//#endregion


function encodeContentOps(itemOps, openTag, closeTag) {
    ENCODED_CONTENT_OPEN_TOKEN = openTag;
    ENCODED_CONTENT_CLOSE_TOKEN = closeTag;

    ENCODED_CONTENT_REGEXP = new RegExp(ENCODED_CONTENT_OPEN_TOKEN + ".*?" + ENCODED_CONTENT_CLOSE_TOKEN, "");

    let encodedContent = '';
    for (var i = 0; i < itemOps.length; i++) {
        let ciop = itemOps[i];
        encodedContent += getContentEmbedStr(ciop);
    }
    quill.setText(encodedContent);
}

function decodeContent(itemOps) {
    let qtext = quill.getText();
    while (result = ENCODED_CONTENT_REGEXP.exec(qtext)) {
        let encodedContentOpStr = result[0];
        let curOpGuid = parseEncodedContentOpGuid(encodedContentOpStr);

        let tsIdx = qtext.indexOf(encodedContentOpStr);

        quill.deleteText(tsIdx, encodedContentOpStr.length);

        let curOp = itemOps.filter(x => x.opGuid == curOpGuid)[0];

        if (curOp != null) {
            quill.insertEmbed(tsIdx, 'content', curOp);
        } 
        qtext = quill.getText();
    }
}

function parseEncodedContentOpGuid(encodedContentOpStr, sToken = ENCODED_CONTENT_OPEN_TOKEN, eToken = ENCODED_CONTENT_CLOSE_TOKEN) {
    var tsIdx = encodedContentOpStr.indexOf(sToken);
    var teIdx = encodedContentOpStr.indexOf(eToken);

    if (tsIdx < 0 || teIdx < 0) {
        return null;
    }

    return encodedContentOpStr.substring(tsIdx + sToken.length, teIdx);
}

function getContentEmbedStr(ciop, sToken = ENCODED_CONTENT_OPEN_TOKEN, eToken = ENCODED_CONTENT_CLOSE_TOKEN) {
    var result = sToken + ciop.opGuid + eToken;
    return result;
}

function initContentRangeListeners() {    
    Array.from(document.querySelectorAll('[copyItemGuid]')).forEach(cie => {
        cie.addEventListener('mouseover', onOverContent);
    });
}

function onOverContent(e) {
    if (!e.target) {
        return;
    }
    let ciguid = e.target.getAttribute('copyItemGuid');
    let testColor = getRandomColor();
    Array.from(document.querySelectorAll('[copyItemGuid="' + ciguid + '"]')).forEach(cie => {
        IgnoreNextTextChange = true;
        cie.style.backgroundColor = testColor;
    });
}

function onOverInput(e) {
    if (!e.target) {
        return;
    }
    IgnoreNextTextChange = true;
    e.target.style.backgroundColor = 'orange';
}

function getContentDocRanges(ciguid) {
    let docLength = quill.getLength();

    let curRange = null;

    let allRanges = [];

    for (var i = 0; i < docLength; i++) {
        let curDelta = quill.getContents(i, 1);

        if (curDelta.ops.length > 0 &&
            curDelta.ops[0].hasOwnProperty('insert') &&
            curDelta.ops[0].insert.hasOwnProperty('copyItemGuid')) {
            //if this idx is the beginning of a piece of any content..

            if (curDelta.ops[0].insert.copyItemGuid == ciguid) {
                //if this is the beginning of the match content
                curRange = {
                    index: i,
                    length: 1
                };
            } else if (curRange != null) {
                //this is the end of a match content range
                curRange.length = i - curRange.index;
                //clone and push match range
                allRanges.push(JSON.parse(JSON.stringify(curRange)));
                curRange = null;
            }
        }
    }
    if (curRange != null) {
        //this is the end of a match content range
        curRange.length = docLength - curRange.index;
        //clone and push match range
        allRanges.push(JSON.parse(JSON.stringify(curRange)));
        curRange = null;
    }
    return allRanges;
}