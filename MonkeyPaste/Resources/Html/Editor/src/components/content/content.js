var ContentInlineGuid;
var ContentSourceInlineGuid;

var ContentBlockGuid;
var ContentSourceBlockGuid;

var ENCODED_CONTENT_OPEN_TOKEN = "{c{";
var ENCODED_CONTENT_CLOSE_TOKEN = "}c}";
var ENCODED_CONTENT_REGEXP;


var InlineTags = ['span', 'a', 'em', 'strong', 'u', 's', 'sub', 'sup', 'img'];
var BlockTags = ['p', 'ol', 'ul', 'li', 'div', 'table', 'colgroup', 'col', 'tbody', 'tr', 'td', 'iframe']


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

    ContentInlineGuid = new Parchment.Attributor('copyItemInlineGuid', 'copyItemInlineGuid', inlineConfig);
    Quill.register(ContentInlineGuid, suppressWarning);

    ContentSourceInlineGuid = new Parchment.Attributor('copyItemSourceInlineGuid', 'copyItemSourceInlineGuid', inlineConfig);
    Quill.register(ContentSourceInlineGuid, suppressWarning);

    let blockConfig = {
        scope: Parchment.Scope.BLOCK,
    };

    ContentBlockGuid = new Parchment.Attributor('copyItemBlockGuid', 'copyItemBlockGuid', blockConfig);
    Quill.register(ContentBlockGuid, suppressWarning);

    ContentSourceBlockGuid = new Parchment.Attributor('copyItemSourceBlockGuid', 'copyItemSourceBlockGuid', blockConfig);
    Quill.register(ContentSourceBlockGuid, suppressWarning);
}

function registerContentBlots() {
    const Parchment = Quill.imports.parchment;

    class ContentInlineBlot extends Parchment.InlineBlot {
        static create(value) {
            let node = super.create(value);
            applyContentItemToDomNode(node, value);
            return node;
        }

        static value(domNode) {
            getContentItemFromDomNode(domNode);
        }

        static formats(node) {
            //return getContentItemFromDomNode(node);
            return node.attributes;
        }

        format(name, value) {
            super.format(name, value);
        }
    }
    ContentInlineBlot.blotName = 'contentInline';
    ContentInlineBlot.tagName = InlineTags;

    Quill.register(ContentInlineBlot);

    class ContentBlockBlot extends Parchment.BlockBlot {
        static create(value) {
            let node = super.create(value);
            applyContentItemToDomNode(node, value);
            return node;
        }

        static value(domNode) {
            getContentItemFromDomNode(domNode);
        }

        //static formats(node) {
        //    return getContentItemFromDomNode(node);
        //}

        //format(name, value) {
        //    super.format(name, value);
        //}
    }
    ContentBlockBlot.blotName = 'contentBlock';
    ContentBlockBlot.tagName = BlockTags;

    //Quill.register(ContentBlockBlot);
}

function getContentItemFromDomNode(domNode) {
    if (domNode == null) {
        return null;
    }
    //if (typeof domNode != HTMLElement) {
    //    return { copyItemGuid: generateGuid(), copyItemSourceGuid: null };
    //}
    let ci = null;

    while (!getContentNodeProperty(domNode,'copyItemGuid')) {
        if (!domNode || domNode.id == 'editor' || domNode.tagName == 'body') {
            return null;
        }
        domNode = domNode.parentNode;
    }
    ci = {
        copyItemGuid: getContentNodeProperty(domNode, 'copyItemGuid'),
        copyItemSourceGuid: getContentNodeProperty(domNode, 'copyItemSourceGuid')
    }
    
    return ci;
}

function applyContentItemToDomNode(node, value) {
    if (node == null || value == null) {
        return node;
    }
    setContentNodeProperty(node, 'copyItemGuid', value.copyItemGuid);
    setContentNodeProperty(node, 'copyItemSourceGuid', value.copyItemSourceGuid);    
    return node;
}

function isContentItemNode(node) {
    return getContentNodeProperty(node, 'copyItemGuid') != null;
}

function setContentNodeProperty(node, property, value) {
    if (isBlockElement(node)) {
        if (property == 'copyItemGuid') {
            node.setAttribute('copyItemBlockGuid', value);
        } else {
            node.setAttribute('copyItemSourceBlockGuid', value);
        }
    } else if (isInlineElement(node)) {
        if (property == 'copyItemGuid') {
            node.setAttribute('copyItemInlineGuid', value);
        } else {
            node.setAttribute('copyItemSourceInlineGuid', value);
        }
    }
    log('setContentNodeProperty error, unknown property: ' + property + ' on node ' + node.outerHTML + ' with value: '+value);
}

function getContentNodeProperty(node, property) {
    if (isBlockElement(node)) {
        if (property == 'copyItemGuid') {
            return node.getAttribute('copyItemBlockGuid');
        } else {
            return node.getAttribute('copyItemBlockSourceGuid');
        }
    } else if (isInlineElement(node)) {
        if (property == 'copyItemGuid') {
            return node.getAttribute('copyItemInlineGuid');
        } else {
            return node.getAttribute('copyItemSourceInlineGuid');
        }
    }
    //log('getContentNodeProperty error, unknown property: ' + property + ' on node' + node.outerHTML);
    return null;
}

function retargetContentItemDomNode(node, newContentGuid) {
    if (node == null) {
        return node;
    }

    let contentItem = getContentItemFromDomNode(node);
    if (!contentItem) {
        //when html content is dropped from external source
        contentItem = {};
    }
    newContentGuid = newContentGuid == null ? generateGuid() : newContentGuid;
    if (contentItem.copyItemSourceGuid) {
        //when source is already set ignore if it changes again because new one is not original
    } else {
        setContentNodeProperty(node,'copyItemSourceGuid', contentItem.copyItemGuid);
    }
    setContentNodeProperty(node, 'copyItemGuid', newContentGuid);
    return node;
}

function formatContentChange(delta, oldDelta, source) {
    let srange = quill.getSelection();
    if (srange.length > 0) {
        // NOTE when selection is being formatted 
        // the element at idx will be PREVIOUS element so tick it
        srange.index++;
    }
    let domNode = getElementAtIdx(srange.index); 
    let contentBlot = getContentItemFromDomNode(domNode);
    while (contentBlot == null) {
        //new content doesn't have a content guid so find first previous
        srange.index--;
        if (srange.index < 0) {
            debugger;
        }
        domNode = getElementAtIdx(srange.index);
        contentBlot = getContentItemFromDomNode(domNode);
    }
    if (PasteNode) {
        if (isContentItemNode(PasteNode) || isTemplateNode(PasteNode)) {
            let contentGuidAtIdx = getContentGuidByIdx(srange.index);
            domNode = retargetContentItemDomNode(PasteNode, contentGuidAtIdx);
        } else {
            domNode = PasteNode;
        }        
    }
    if (contentBlot && contentBlot.copyItemGuid) {
        if (isBlockElement(domNode)) {
            ContentBlockGuid.add(domNode, contentBlot.copyItemGuid);
            ContentSourceBlockGuid.add(domNode, contentBlot.copyItemSourceGuid);

            let inlineChildNodes = domNode.querySelectorAll(InlineTags.join(','));
            inlineChildNodes.forEach(icn => {
                ContentInlineGuid.add(icn, contentBlot.copyItemGuid);
                ContentSourceInlineGuid.add(icn, contentBlot.copyItemSourceGuid);
            });
        } else {
            ContentInlineGuid.add(domNode, contentBlot.copyItemGuid);
            ContentSourceInlineGuid.add(domNode, contentBlot.copyItemSourceGuid);
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
    Array.from(document.querySelectorAll('[copyItemInlineGuid],[copyItemBlockGuid]')).forEach(cie => {
        cie.addEventListener('mouseover', onOverContent);
    });
}

function onOverContent(e) {
    if (!e.target) {
        return;
    }
    let ci = getContentItemFromDomNode(e.target);
    let testColor = getRandomColor();
    Array.from(document.querySelectorAll('[copyItemInlineGuid="' + ci.copyItemGuid + '"],[copyItemBlockGuid="' + ci.copyItemGuid + '"]')).forEach(cie => {
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

function getContentGuidByIdx(docIdx) {
    if (docIdx < 0 || docIdx >= quill.getLength()) {
        return ''
    };

    let leafElementNode = getElementAtIdx(docIdx);
    return getContentNodeProperty(leafElementNode, 'copyItemGuid');
}

function getContentSourceGuidByIdx(docIdx) {
    if (docIdx < 0 || docIdx >= quill.getLength()) {
        return ''
    };
    let leafElementNode = getElementAtIdx(docIdx);
    return getContentNodeProperty(leafElementNode, 'copyItemSourceGuid');
}

function getContentItemByIdx(docIdx) {
    return {
        copyItemGuid: getContentGuidByIdx(docIdx),
        copyItemSourceGuid: getContentSourceGuidByIdx(docIdx)
    };
}

function getContentWidth() {
    var bounds = quill.getBounds(0, quill.getLength());
    return bounds.width;
}

function getContentHeight() {
    var bounds = quill.getBounds(0, quill.getLength());
    return bounds.height;
}

function isBlockElement(elm) {
    if (elm == null || !elm instanceof HTMLElement) {
        return false;
    }
    let tn = elm.tagName.toLowerCase();
    return BlockTags.includes(tn);
}

function isInlineElement(elm) {
    if (elm == null || !elm instanceof HTMLElement) {
        return false;
    }
    let tn = elm.tagName.toLowerCase();
    return InlineTags.includes(tn);
}

function isDocIdxLineStart(docIdx) {
    if (docIdx == 0) {
        return true;
    }
    if (docIdx >= quill.getLength()) {
        return false;
    }
    let idxLine = quill.getLine(docIdx);
    let prevIdxLine = quill.getLine(docIdx - 1);
    return idxLine[0] != prevIdxLine[0];
}

function isDocIdxLineEnd(docIdx) {
    if (docIdx == quill.getLength()) {
        return true;
    }
    if (docIdx < 0) {
        return false;
    }
    let idxLine = quill.getLine(docIdx);
    let nextIdxLine = quill.getLine(docIdx + 1);
    return idxLine[0] != nextIdxLine[0];
}

