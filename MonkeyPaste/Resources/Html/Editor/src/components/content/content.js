var contentAttribute;

var ENCODED_CONTENT_OPEN_TOKEN = "{c{";
var ENCODED_CONTENT_CLOSE_TOKEN = "}c}";
var ENCODED_CONTENT_REGEXP;

function registerContentGuidAttribute() {
    Parchment = Quill.import('parchment');

    let config = {
        scope: Parchment.Scope.ANY,
    };

    contentAttribute = new Parchment.ClassAttributor('copyItemGuid', 'copyItemGuid', config);
    Quill.register(contentAttribute, true);
}


function initContent(itemOps, openTag = "{c{", closeTag = "}c}") {
    //encodeContentOps(itemOps, openTag, closeTag);

    //decodeContent(itemOps);
    setHtml(itemOps);

    //initContentRangeListeners();
}

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
    //while (quill.getLength() <= 1) {
    //    await sleep(100);
    //}
    //let parser = new DOMParser();
    //document.getElementById('editor').addEventListener('mouseover', onOverContent);
    
    Array.from(document.querySelectorAll('[copyItemGuid]')).forEach(cie => {
        cie.addEventListener('mouseover', onOverContent);
    });
    
    return;
}

function onOverContent(e) {
    if (!e.target) {
        return;
    }
    let ciguid = e.target.getAttribute('copyItemGuid');
    let testColor = getRandomColor();
    Array.from(document.querySelectorAll('[copyItemGuid="' + ciguid + '"]')).forEach(cie => {
        cie.style.backgroundColor = testColor;
    });

    //e.target.style.backgroundColor = getRandomColor();
    //let blot = Quill.find(e.target);
    //if (blot && blot.domNode.getAttribute('opGuid') != null) {
    //    log('over: ' + blot.domNode.getAttribute('opGuid'));
    //} else {

    //    log(e.target);
    //}
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

function onMouseEnterContentRange(e) {
    log('entered content range');
    e.target.style.backgroundColor = 'magenta';
}

function onMouseLeaveContentRange(e) {
    log('left content range');
    e.target.style.backgroundColor = 'transparent';
}

function registerContentBlots(Quill) {
    const Parchment = Quill.imports.parchment;

    class ContentBlot extends Parchment.InlineBlot {
        static create(value) {
            let node = super.create(value);
            applyContentItemToDomNode(node, value);
            return node;
        }

        static value(domNode) {
            getContentItemFromDomNode(domNode);
        }
    }
    ContentBlot.blotName = 'content';
    ContentBlot.tagName = 'DIV';//['P','SPAN'];

    Quill.register(ContentBlot);

    //class ContentBlockBlot extends Parchment.EmbedBlot {
    //    static create(value) {
    //        let node = super.create(value);
    //        node.setAttribute('copyitemid', value.id);
    //        node.innerHtml = value.itemData;
    //        return node;
    //    }

    //    static value(domNode) {
    //        return {
    //            id: domNode.getAttribute('copyitemid'),
    //            itemData: domNode.innerHtml
    //        }
    //    }
    //}
    //ContentBlockBlot.blotName = 'contentBlock';
    //ContentBlockBlot.tagName = 'P';

    //Quill.register(ContentBlockBlot);
}

//#region Convert To/From Blot/DomNode

function getContentItemFromDomNode(domNode) {
    if (domNode == null) {
        return null;
    }
    return {
        opGuid: domNode.getAttribute('opGuid'),
        copyItemGuid: domNode.getAttribute('copyItemGuid'),
        opData: domNode.outerHTML
    }
}

function applyContentItemToDomNode(node, value) {
    if (node == null || value == null) {
        return node;
    }

    node.setAttribute('copyItemGuid', value.copyItemGuid);
    node.setAttribute('opGuid', value.opGuid);
    node.innerHTML = value.opData;

    //node.setAttribute('class', 'ql-content-embed-blot');

    node.addEventListener('mouseenter', function (e) {
        console.log('entered content: ' + value.copyItemGuid);
    });

    return node;
}
//#endregion
