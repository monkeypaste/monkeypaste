const TEMPLATE_VALID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890#-_ ";

var ENCODED_TEMPLATE_OPEN_TOKEN = "{{";
var ENCODED_TEMPLATE_CLOSE_TOKEN = "}}";
var ENCODED_TEMPLATE_REGEXP = new RegExp(ENCODED_TEMPLATE_OPEN_TOKEN + ".*?" + ENCODED_TEMPLATE_CLOSE_TOKEN, "");

var isShowingEditTemplateToolbar = false;
var isShowingPasteTemplateToolbar = false;

var wasLastClickOnTemplate = false;

function registerTemplateSpan(Quill) {
    const Parchment = Quill.imports.parchment;

    class TemplateSpanBlot extends Parchment.EmbedBlot {
        static blotName = 'templatespan';
        static tagName = 'DIV';
        static className = 'template_btn';

        static create(value) {
            const node = super.create(value);
            if (value.domNode != null) {
                // creating existing instance
                value = getTemplateFromDomNode(value.domNode);
            }
            //ensure new template has unique instance guid
            value.templateInstanceGuid = generateGuid();

            applyTemplateToDomNode(node, value);

            return node;
        }

        static value(domNode) {
            return getTemplateFromDomNode(domNode);
        }
    }

    Quill.register(TemplateSpanBlot);
}

//#region Init
function initTemplates(templateDefsStr, templateRegExInfoStr) {
    if (templateRegExInfoStr != null) {
        let templateRegExInfo = JSON.parse(templateRegExInfoStr);
        ENCODED_TEMPLATE_OPEN_TOKEN = templateRegExInfo[0];
        ENCODED_TEMPLATE_REGEXP = templateRegExInfo[1];
        ENCODED_TEMPLATE_CLOSE_TOKEN = templateRegExInfo[2];
    }

    if (templateDefsStr != null) {
        let templateDefinitions = JSON.parse(templateDefsStr);
        decodeTemplates(templateDefinitions);
    }

    let templateNameTextArea = document.getElementById('templateNameTextInput');
    let minNameHeight = $('#templateNameTextInput').outerHeight();

    function outputsize() {
        updateEditTemplateToolbarPosition();

        if ($('#templateNameTextInput').height() < minNameHeight) {
            $('#templateNameTextInput').height(minNameHeight);
        }
        console.log(templateNameTextArea.offsetWidth+'x'+templateNameTextArea.offsetHeight);
    }
    //outputsize()

    new ResizeObserver(outputsize).observe(templateNameTextArea);

    initTemplateToolbarButton();
}

function initTemplateToolbarButton() {
    // Toolbar Template Button
    const templateToolbarButton = new QuillToolbarButton({
        icon: '<img id="templateToolbarButton" style="height:20px" src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAALQAAAC0CAYAAAA9zQYyAAAJ9ElEQVR4nO3dTW4bRxoG4LdaZhOYESDfQL6BtfBigMAS5wTWnECdE4Q+QTQnsOYEZk4Q+gRpytBOgOnd7EY5QWjAXoiU+5tFs6UWTVJssqrr731WiSJ3F+IXH6rrV4GskMv0DQoMoNRz222xRnAD4Ob+31UxhlI5/jbN1St82eaRSk/LqAkZdc6AZGC7HU4TGUNhiESG6vXs86Z/jIFumYw674Ckb7sdfikydTL7bZPfZKBbJHn6HkplttvhJUGOzm2mfsKf636NgW6BXOMAX9McSh3ZbovXRCZIkKnj6YdVv5K02Z4YycfOS4ZZE6WeQ9RQ8vT9yl9psz2xkY+dl/ieDKHwwnZbgiMyxv60tzgawgptSBlmlTPMhih1hK/d4eKPGWgDZNQ5Q5GMox5jboNCTy7TN49/RFpxjLllghvs3x5VXQ9WaI3mY8wD2+2IisILfOtmD/9KWnCM2aJalWaF3pFc40Dy7h8Ms0UKL/At7Zf/SFsrJ0y6Qyj0bLeFAPz99vme7Tb4qjb79w/bbaG5WfJfBnoLnMp2lADPbLfBN+VUtso5xuwi9ZwfhQ08zP4xzK5ioDcko84Zw+w+djk2cD/7xzEh57FCP4FT2X5hzVmDYW6quFAns7fL/ouM0l8BdW709SITBnoFTmU3sTrIi0wHm4FegmFuYvMwV0yGmoFewDA30TzMFcnTv0yMGPGjsIZhbmbbMJd/GBcam1J7LAFgmLehTm53yo+MuqKrLRVWaDDMIYl6YoWLjMITbYWeh3nAMIclygrNyhyu6Co0wxy2qALNMLtD8vSTiedGE2iGWb9yxm9Lhv4eogg0w2yIoL9NqE1VZyCCiRWG2TCRCRQu1Mn03xv9ep5+Mvl3EXSg5QqHmKVDhtmwDUJtOsiVYAM93//HcebIBBlohjlewQWaYY5bUIHmifkUTKDLD8AuT8yPXBBrOeajGWMo8MyMyHlfoR/CzANgyPOZQvnYeVl2MxhmKnlbofkBSMt4GWh+ANIq3n0U8gOQ1vEq0LXKzDDTUt50Oeb3mYzZzaB1vBjlKE/NZ5jpac5XaFZmasLpCi1XOGSYqQlnA/2wOJ9hps052eXgThPalnOB5h5A2oVTgWaYaVfOBJphJh2cCDTDTLpYDzTDTDpZXcsxnzQZQoFhJi3sjkOXYe5ZbQMFxVqgJU9/Z5hJNyuBnt9pcmrj3RS21j8KeUEPmdRqoBlmMq21UY75dbhZW++jOLVSoWXU/QUwc3MoUZ3xQMuocwYkA9PvIQIMB1ou0zcQNTT5DqI6Y4GWj52XKJKxqecTLWNkHHp+RnNu4tlE62gPtFzhsDyii+fNUfu0djm4Q5ts01ahH5aBMsxkj74ux9eU95qQdVoCzcVG5Iqdp745pU0u2emjkLOA5JqtA82JE3LRVn1oTpyQqxpX6Pnw3A0nTshFjSp07cgBhpmc1KzL8TW94FgzuWzjYbtyRIPDc+S2jfrQcvnsBLKXG24L0Y6K7MlA8+ph8kORqZPZb2v70HKNg/nB4wwzOawMM/DURyE/Asl9/SrMwJo+NKe1yXkiA9Wb/lz/0dJA388EsqtBziou1Mns7eJPf+hyyDUO8F0NGGZy1/IwA8v60N/SPvvN5C45XxVmYKHLwfFmctqSPvOixxW62BuYbA/R9oqLp8IM1AIto/RXbnAlJ4kM1nUz6hTAJaHkstUfgMuUFbqcQGGYyTHNwgwASq5wiLvujZkGEW2reZgBIMFdmhloDdH2GvSZFyUQ8DwNcscGQ3PrJJxEIWfsGGbA9sWbRBUNYQaABCITHe0h2prIUEeYASABFA+LIXtExtifZroel0BJruthRI2IjNGZnqpX+KLrkQkS4aU+1D6RCfYkUz/hT52PLae+8+7/uI6DWpUUR+r17LP2xwIAVHGu+8FEqxWZiTADtfXQkqd/cT0HGadpeG6Vh3Fopc5NvYQIACDITYYZWNyxwr40mVIOz/V0jmgs83imMPmemXwZRUpkont4bpVHgVbHdyMIh/FIo3J4rqd7eG6VH9dy7E8zCG7aeDkFrgqzoRGNZX4ItHqFL9gruKSUdpfA2PDc6lcuUTaiuGizIRQQkQmS4kgdTz+0/eq1x+lK3v0DCr2W2kIhsNDNqFu/Hnr/9pT9aWpESd9WmIENTvDnfYS0kbKbcaqO70Y2m/HkjpV5fzproS3kK5EJEmS2wwxsuAWrPFBazg23hXx0X5nb/wBcptHFmzLqvAOSvqnGkGcsfwAu0/wm2Tx9D8Xr3aLnYJiBLS+vlzz9HUpx8iVWjoYZ2DLQACB5+olnekTI4TADu5zLsT/tQYTDeTEpN7Ua2Tqly9YVGnh0mT0rdehaWs+8q50CDTDUUfAkzICGQAMMddA8CjOgKdAAQx0kz8IMaDysUb3CF34oBkRk6FuYAY0VulJW6u6Qy049JshV7/aftpuxDe2BrnBG0VOGz80wzVigAYbaO56HGTAcaICh9kYAYQZaOMG//J/E/YlOCyTMQAsVuiKjzhmQDNp6H20ooDADLQYaYKidE1iYgZYDDcz3KH5XOU86tczjobl1Wr8FS72efcae9Lib3CKRMfZvg1zP3nqFrnCq3JLy4MSjts6aa5u1ewofpsqR22pDdFo+ONEGaxW6jmPVLXB8p4kuTtwkOx+rzmy3I2iWTzRqixMVusIREFOKrDxbJXxOVOiKej37jIRH+epVXMQSZsCxQAPzWwTY/dBDZKBOZm9tN6NNTnU56jiruKMAZwE34VyFrvA8vR1EGmbA4Qpd4ZBeQyJD1Zv+y3YzbHG2QldUb/ozRAa22+GFclNrZrsZNjkfaADA/rTPzbdP8HCHtgnOdzkqXPuxBsN8z48Kjeq6OckgMrHdFqcwzI94E2igvvSUoQZQhnlPMob5gTddjjq5fHYC2cttt8MqwQ06t0GvnNuGVxW6Ev1sosiEYV7Oy0AD1cQL4rvvJYI1zbvwNtAAoE5u/xPVbGIka5p34WUfelEUs4mCG+wVpwzzekEEGgg81Bya21gwgQYCvZ2LYW7E6z70D/anWVBT5AxzY0EFunbo+tB2W3bGMG8lqC5Hndd96siXgO4iqApd5+2y0/IqiMx2M3wVbIWu+FWpi4vY9gDqFnygAW/2J/bLiSLaRbBdjjqn9yeKTKDklGHWI4oKXZHL9A0KDJw5yIazf9pFFWjAodOZBDn2b085LKdXdIEGXLhLkR9/pkQZ6IqMOu+ApL0lqCITJMjU8fRDa++MTNSBBlrsV7OL0YroAw0Y7oKITKDUOUcx2sFA18io+wtEzrVVa5EhOtM+d5e0h4FeIFc4xF2nv1PfWpBjr4jigHHXMNAryBUOMUvPG02bC3IkcsGPPnsY6CfINQ7wrZtBJFt6apPIGEoNkBQ5K7J9/we5E3dBSMQUXgAAAABJRU5ErkJggg=="></img>'//`<svg viewBox="0 0 18 18"> <path class="ql-stroke" d="M5,3V9a4.012,4.012,0,0,0,4,4H9a4.012,4.012,0,0,0,4-4V3"></path></svg>`
    })

    templateToolbarButton.onClick = function (e) {
        var templateButton = document.getElementById('templateToolbarButton');
        var tl = getAvailableTemplateDefinitions();
        if (tl.length > 0) {
            showTemplateToolbarContextMenu(templateButton);
        } else {
            createTemplate();
        }
    }
    templateToolbarButton.attach(quill);
}
//#endregion

//#region Convert To/From Blot/DomNode

function getTemplateFromDomNode(domNode) {
    if (domNode == null) {
        return null;
    }
    return {
        templateGuid: domNode.getAttribute('templateGuid'),
        templateInstanceGuid: domNode.getAttribute('templateInstanceGuid'),
        isFocus: domNode.getAttribute('isFocus'),
        templateName: domNode.getAttribute('templateName'),
        templateColor: domNode.getAttribute('templateColor'),
        templateText: domNode.getAttribute('templateText'),
        templateType: domNode.getAttribute('templateType'),
        templateData: domNode.getAttribute('templateData'),
        templateDeltaFormat: domNode.getAttribute('templateDeltaFormat'),
        templateHtmlFormat: domNode.getAttribute('templateHtmlFormat')
    }
}

function applyTemplateToDomNode(node, value) {
    if (node == null || value == null) {
        return node;
    }

    node.setAttribute('templateGuid', value.templateGuid);
    node.setAttribute('templateInstanceGuid', value.templateInstanceGuid);
    node.setAttribute('templateName', value.templateName);
    node.setAttribute('templateType', value.templateType);
    node.setAttribute('templateColor', value.templateColor);
    node.setAttribute('templateData', value.templateData);
    node.setAttribute('templateText', value.templateText);
    node.setAttribute('templateDeltaFormat', value.templateDeltaFormat);
    node.setAttribute('templateHtmlFormat', value.templateHtmlFormat);

    var textColor = isBright(value.templateColor) ? 'black' : 'white';
    node.setAttribute('style', 'background-color: ' + value.templateColor + ';color:' + textColor + ';');

    node.innerHTML = value.templateHtmlFormat;
    changeInnerText(node, node.innerText, value.templateName);

    // TODO instead of rejecting mouse down, template should be draggable

    //disable text selection
    node.setAttribute('unselectable', 'on');
    node.setAttribute('onselectstart', 'return false;');
    node.setAttribute('onmousedown', 'return false;');

    node.contentEditable = 'false';
    node.setAttribute('isFocus', false);
    node.setAttribute("spellcheck", "false");
    node.setAttribute('class', 'template_btn');

    node.addEventListener('click', function (e) {
        focusTemplate(node.getAttribute('templateGuid'));
    });

    return node;
}
//#endregion

//#region Encode/Decode

function getEncodedTemplateGuids(itemData) {
    // this returns all parsed templates to html extension on load BEFORE init
    let etgl = [];

    let tcount = 0;
    while (result = ENCODED_TEMPLATE_REGEXP.exec(itemData)) {
        let encodedTemplateStr = result[0];
        let tguid = parseEncodedTemplateGuid(encodedTemplateStr);

        let isDefined = etgl.some(function (etg) {
            return etg == tguid
        });
        if (!isDefined) {
            etgl.push(tguid);
        }
    }
    return etgl;
}

function decodeTemplates(templateDefs) {
    //templateDefs is all FOUND template defs in db from pre init getEncodedTempalteGuids
    //when def is not found that means user clicked delete template which removes it from db
    //and deletes all current instances in active document so when not provided replace 
    //encoded template w/ empty character

    let qtext = quill.getText();
    let tcount = 0; 
    while (result = ENCODED_TEMPLATE_REGEXP.exec(qtext)) {
        let encodedTemplateStr = result[0];
        let tguid = parseEncodedTemplateGuid(encodedTemplateStr);

        // NOTE embed blots have zero length in text (completely ignored) 
        // BUT take up a signle character in actual content so tcount keeps track of the
        // ignored character as they are added (SHEESH)
        let tsIdx = qtext.indexOf(encodedTemplateStr) + tcount;

        // remove ' ' padding from both ends of template or extra ' ' will be added time it loads
        quill.deleteText(tsIdx-1, encodedTemplateStr.length + 1);

        let t = templateDefs.forEach(function (td) {
            if (td.templateGuid == tguid) {
                return td;
            }
        });

        if (t != null) {
            quill.insertEmbed(tsIdx, 'templatespan', t);
            tcount++;
        } else {
            console.log('template def \'' + tguid + '\' not found so omitting from editor');
        }
        qtext = quill.getText();
    }
}

function parseEncodedTemplateGuid(encodedTemplateStr, sToken = ENCODED_TEMPLATE_OPEN_TOKEN, eToken = ENCODED_TEMPLATE_CLOSE_TOKEN) {
    var tsIdx = encodedTemplateStr.indexOf(sToken);
    var teIdx = encodedTemplateStr.indexOf(eToken);

    if (tsIdx < 0 || teIdx < 0) {
        return null;
    }

    return encodedTemplateStr.substring(tsIdx + sToken.length, teIdx);
}

function getTemplateEmbedStr(t, sToken = ENCODED_TEMPLATE_OPEN_TOKEN, eToken = ENCODED_TEMPLATE_CLOSE_TOKEN) {
    var result = sToken + t.domNode.getAttribute('templateGuid') + eToken;
    return result;
}

function encodeTemplates() {
    // NOTE template text should be cleared from html before calling this
    var html = getHtml();
    var til = getUsedTemplateInstances();
    for (var i = 0; i < til.length; i++) {
        var ti = til[i];
        var tin = ti.domNode;
        var tihtml = tin.outerHTML;
        var ties = getTemplateEmbedStr(ti);
        html = html.replace(tihtml, ties);
    }
    return html;
}

//#endregion

function isTemplateFocused() {
    return getFocusTemplate() != null;
}

function getFocusTemplate() {
    let til = getUsedTemplateInstances();
    let result = til.find(x => x.domNode.getAttribute('isFocus') == "true");
    return result;
}

function getFocusTemplateGuid() {
    let ft = getFocusTemplate();
    if (ft == null) {
        return null;
    }
    return ft.domNode.getAttribute('templateGuid');
}

function getTemplatesFromRange(range) {
    if (range == null || range.index == null) {
        console.log('invalid range: ' + range);
    }
    let tl = [];
    getUsedTemplateInstances().forEach(function (tn) {
        let docIdx = tn.docIdx;
        if (docIdx >= range.index && docIdx <= range.index + range.length) {
            tl.push(tn);
        }
    });
    return tl;
}


//#region Paste 

function getTextWithEmbedTokens() {
    // for pasting NOT encoding for db
    var text = getText().split('');
    var otl = getUsedTemplateInstances();
    var outText = '';
    var offset = 0;
    otl.forEach(function (ot) {
        offset += parseInt(ot.docIdx);
        var embedStr = getTemplateEmbedStr(ot);
        //text.splice(offset, 1);
        for (var i = 0; i < embedStr.length; i++) {
            text.splice(offset + i, 0, embedStr[i]);
        }
        offset += (embedStr.length);
    });
    return text.join('');
}

//#endregion

function getUsedTemplateDefinitions() {
    let tdl = [];
    getUsedTemplateInstances().forEach(function (ti) {
        let isDefined = tdl.find(x => x.domNode.getAttribute('templateGuid') == ti.domNode.getAttribute('templateGuid')) != null;
        if (!isDefined) {
            tdl.push(ti);
        }
    });
    return tdl;
}

function getAvailableTemplateDefinitions() {
    if (availableTemplates == null) {
        return getUsedTemplateDefinitions();
    }

    let utdl = getUsedTemplateDefinitions();
    let allMergedTemplates = [];
    for (var i = 0; i < availableTemplates.length; i++) {
        let atd = availableTemplates[i];
        let isUsed = false;
        //loop through all templates from master collection
        for (var j = 0; j < utdl.length; j++) {
            //check if this editor is using one available in master
            let utd = utdl[j];
            if (utd.domNode.getAttribute('templateGuid') == atd.templateGuid) {
                //if this editor is using a template already known from master use this editor's version
                allMergedTemplates.push(getTemplateFromDomNode(utd.domNode));
                isUsed = true;
                break;
            }
        }
        if (!isUsed) {
            allMergedTemplates.push(atd);
        }
    }

    // now add any new templates in this editor that weren't in master list
    for (var i = 0; i < utdl.length; i++) {
        let utd = utdl[i];
        let wasFound = allMergedTemplates.find(x => x.templateGuid == utd.domNode.getAttribute('templateGuid')) != null;
        if (!wasFound) {
            allMergedTemplates.push(getTemplateFromDomNode(utd.domNode));
        }
    }
    return allMergedTemplates;
}

function getTemplateDocIdx(tguid, tiguid) {
    let docLength = quill.getLength();
    for (var i = 0; i < docLength; i++) {
        let curDelta = quill.getContents(i, 1);
        if (curDelta.ops[0].hasOwnProperty('templateSpan')) {
            let curTemplate = curDelta.ops[0].templateSpan;
            if (curTemplate.templateGuid == tguid && curTemplate.templateInstanceGuid == tiguid) {
                return i;
            }
        }
    }
    return -1;
} 

function getUsedTemplateInstances() {
    var domTemplates = document.querySelectorAll('.template_btn');
    var templates = [];
    for (var i = 0; i < domTemplates.length; i++) {
        var domTemplate = domTemplates[i];
        var templateBlot = Quill.find(domTemplate);
        if (templateBlot != null) {
            templates.push(templateBlot);
        }
    }
    return templates.sort((a, b) => (
        getTemplateDocIdx(a.domNode.getAttribute('templateGuid'), a.domNode.getAttribute('templateInstanceGuid')) > 
        getTemplateDocIdx(b.domNode.getAttribute('templateGuid'), b.domNode.getAttribute('templateInstanceGuid')) ? -1 : 1));
}

function createTemplate(templateObjOrId, idx, len) {
    var templateObj;
    if (templateObjOrId != null && typeof templateObjOrId === 'string') {
        templateObj = getTemplateDefByGuid(templateObjOrId);
    } else {
        templateObj = templateObjOrId;
    }

    var range = quill.getSelection(true);
    if (idx != null && length != null) {
        range = { index: idx, length: len };
    }

    var isNew = templateObj == null;
    var newTemplateObj = templateObj;

    if (isNew) {
        //grab the selection head's html to set formatting of template div
        let selectionInnerHtml = '';
        let shtmlStr = getSelectedHtml(1);
        if (shtmlStr != null && shtmlStr.length > 0) {
            let parser = new DOMParser();
            let shtml = parser.parseFromString(shtmlStr, 'text/html');
            let pc = shtml.getElementsByTagName('p');
            if (pc != null && pc.length > 0) {
                let p = pc[0];
                //clear text from selection
                selectionInnerHtml = p.innerHTML;
            }
        }

        let newTemplateName = '';
        if (range.length == 0) {
            newTemplateName = getLowestAnonTemplateName();
        } else {
            newTemplateName = quill.getText(range.index, range.length).trim();
        }
        if (selectionInnerHtml == '<br>') {
            //this occurs when selection.length == 0
            selectionInnerHtml = newTemplateName;
        }
        let formatInfo = quill.getFormat(range.index, 1);
        newTemplateObj = {
            templateGuid: generateGuid(),
            templateColor: getRandomColor(),
            templateName: newTemplateName,
            templateType: 'dynamic',
            templateData: '',
            templateDeltaFormat: JSON.stringify(formatInfo),
            templateHtmlFormat: selectionInnerHtml
        };
    }


    quill.deleteText(range.index, range.length);

    quill.insertText(range.index, ' ');
    range.index++;
    quill.insertEmbed(range.index, "templatespan", newTemplateObj);
    quill.insertText(range.index + 1, ' ');
    quill.setSelection(range.index + 1, Quill.sources.API);

    let t = getTemplateDefByGuid(newTemplateObj.templateGuid);

    //t.domNode.setAttribute('style', t.templateHtmlFormat);

    //quill.formatText(range.index, JSON.parse(newTemplateObj.templateDeltaFormat));

    focusTemplate(newTemplateObj['templateGuid'], true);

    showEditTemplateToolbar();

    hideTemplateToolbarContextMenu();

    return range.index;
}


function getLowestAnonTemplateName(anonPrefix = 'Template #') {
    var tl = getUsedTemplateDefinitions();
    var maxNum = 0;
    tl.forEach(function (t) {
        if (t.domNode.getAttribute('templateName').startsWith(anonPrefix)) {
            var anonNum = parseInt(t.domNode.getAttribute('templateName').substring(anonPrefix.length));
            maxNum = Math.max(maxNum, anonNum);
        }
    });
    return anonPrefix + (parseInt(maxNum) + 1);
}

//function getNewTemplateId() {
//    var tl = getUsedTemplateDefinitions();
//    var minId = 0;
//    tl.forEach(function (t) {
//        minId = Math.min(minId, t.templateId);
//    });
//    return (parseInt(minId) - 1);
//}

function clearTemplateSelection() {
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        t.style.border = "";
    }
}

function setTemplateType(templateGuid, templateTypeValue) {
    console.log('Template: ' + templateGuid + " selected type: " + templateTypeValue);

    var t = getTemplateDefByGuid(templateGuid);
    t.domNode.setAttribute('templateType', templateTypeValue);
    document.getElementById("editTemplateTypeMenuSelector").value = templateTypeValue;

    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var te = stl[i];
        let ctguid = te.getAttribute('templateGuid');
        if (ctguid == templateGuid) {
            te.setAttribute('templateType', templateTypeValue);
        }
    }

    if (templateTypeValue == 'dynamic') {
        document.getElementById('templateDetailTextInputContainer').style.display = 'none';
    } else {
        document.getElementById('templateDetailTextInputContainer').style.display = 'inline-block';
        document.getElementById('templateDetailTextInput').value = t.domNode.getAttribute('templateData');
        document.getElementById('templateDetailTextInput').addEventListener('input', onTemplateDetailChanged);
    }

    document.getElementById('templateColorBox').style.backgroundColor = t.domNode.getAttribute('templateColor');
    document.getElementById('templateColorBox').addEventListener('click', onTemplateColorBoxContainerClick);

    //document.getElementById('templateNameAndColorContainer').style.display = 'inline-block';
    document.getElementById('templateNameTextInput').value = t.domNode.getAttribute('templateName');
    document.getElementById('templateNameTextInput').addEventListener('input', onTemplateNameChanged);
}


//function focusTemplate(tn) {
//    clearTemplateFocus();
//    hideAllContextMenus();
//    let tguid = tn.getAttribute('templateGuid');
//    let tiguid = parseInt(tn.getAttribute('templateInstanceGuid'));
//    let te = getTemplateElements(tguid, tiguid);
//    te.isFocus = true;
//    te.setAttribute('isFocus', true);
//    focusTemplate(tguid, false, tiguid);
//}

function focusTemplate(tguid, fromDropDown, tiguid) {
    if (tguid == null) {
        return;
    }
    clearTemplateFocus();
    hideAllContextMenus();

    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        if (t.getAttribute('templateGuid') == tguid) {
            if (isPastingTemplate) {
                $('#templateTextArea').placeholder = "Enter text for " + t.innerText;
                if (t.innerText != getTemplateDefByGuid(tguid)['templateName']) {
                    $('#templateTextArea').val(t.innerText);
                } else {
                    $('#templateTextArea').val('');
                }
            }
            t.setAttribute('isFocus', true);
            t.style.border = "2px solid red";
            if (tiguid != null && t.getAttribute('templateInstanceGuid') == tiguid) {
                t.style.border = "2px solid lightseagreen";
            }
        } else {
            t.setAttribute('isFocus', false);
            t.style.border = "";
        }
    }
    if (fromDropDown == null || !fromDropDown) {
        if (isPastingTemplate) {
            //when user clicks a template this will adjust to drop dwon to the clicked element
            var items = document.getElementById('paste-template-custom-select').getElementsByTagName("div");
            for (var i = 0; i < items.length; i++) {
                if (items[i].getAttribute('optionId') != null && items[i].getAttribute('optionId') == getFocusTemplateGuid()) {
                    eventFire(items[i], 'click');
                    break;
                }
            }
        }
        //moved from quill embed constructor maybe causing selection issue on android
        wasLastClickOnTemplate = true;

        hideAllContextMenus();
        //showTemplateContextMenu(getFocusTemplateElement());
        showEditTemplateToolbar();
    }
}

function getTemplateElements(tguid, iguid) {
    var tel = [];
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        let t = stl[i];
        let ctguid = t.getAttribute('templateGuid');
        let ctiguid = t.getAttribute('templateInstanceGuid');
        if (ctguid == tguid) {
            if (iguid == null) {
                tel.push(t);
            } else if (ctiguid == iguid) {
                return t;
            }
        }
    }
    return iguid == null ? tel : null;
}


function clearTemplateFocus() {
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        t.isFocus = false;
    }
}

function onColorPaletteItemClick(chex) {
    let tel = getTemplateElements(getFocusTemplateGuid());

    for (var i = 0; i < tel.length; i++) {
        var te = tel[i];
        te.style.backgroundColor = chex;
        te.setAttribute('templateColor', chex);

        te.style.color = isBright(chex) ? 'black' : 'white';
    }
    document.getElementById('templateColorBox').style.backgroundColor = chex;
}

function deleteAllTemplateClick(tguid) {
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        if (t.getAttribute('templateGuid') == tguid) {
            t.parentNode.removeChild(t);
        }
    }
}

function eventFire(el, etype) {
    if (el.fireEvent) {
        el.fireEvent('on' + etype);
    } else {
        var evObj = document.createEvent('Events');
        evObj.initEvent(etype, true, false);
        el.dispatchEvent(evObj);
    }
}

function templateTextAreaChange() {
    var curText = $('#templateTextArea').val();
    setTemplateText(getFocusTemplateGuid(), curText);
}

function setTemplateName(tguid, name) {
    var tl = getTemplateElements(tguid);
    for (var i = 0; i < tl.length; i++) {
        var te = tl[i];
        if (te.getAttribute('templateGuid') == tguid) {
            te.setAttribute('templateName', name);
            te.innerHTML = name;
        }
    }
}

function setTemplateDetailData(tguid, detailData) {
    var tl = getTemplateElements(tguid);
    for (var i = 0; i < tl.length; i++) {
        var te = tl[i];
        if (te.getAttribute('templateGuid') == tguid) {
            te.setAttribute('templateData', detailData);
        }
    }
}

var origTemplateName = null;

function startSetTemplateName(tguid, tiguid) {
    enterKeyBindings = disableKey('Enter');
    document.addEventListener('keypress', onLogKeyPress);
    document.addEventListener('keydown', onLogKeyDown);

    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        let te = stl[i];
        te.setAttribute('contentEditable', false);
        te.style.cursor = "pointer";
        te.style.caretColor = "transparent";

        let ctId = te.getAttribute('templateGuid');
        let ciId = te.getAttribute('templateInstanceGuid');
        if (ctId != tguid && ciId != tiguid) {
            continue;
        }

        te.setAttribute('contentEditable', true);
        te.style.cursor = "text";
        te.style.caretColor = "black";

        origTemplateName = te.innerText;
        selectText(te);
    }
    hideTemplateContextMenu();
}

function endSetTemplateName(wasCancel) {
    let fte = getFocusTemplate().domNode;
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        t.contentEditable = false;
        t.style.cursor = "pointer";
        //t.style.caretColor = "transparent";
        if (t.getAttribute('templateGuid') == fte.getAttribute('templateGuid') && wasCancel == true) {
            t.setAttribute('templateName', origTemplateName);
            t.innerText = origTemplateName;
        }
    }

    reenableKey('Enter', enterKeyBindings);
    enterKeyBindings = null;

    origTemplateName = null;

    document.removeEventListener('keypress', onLogKeyPress);
    document.removeEventListener('keydown', onLogKeyDown);
}

function onTemplateTypeChanged(e) {
    setTemplateType(getFocusTemplateGuid(), this.value);
}

function onTemplateColorBoxContainerClick(e) {
    showTemplateColorPaletteMenu();
}

function onTemplateNameChanged(e) {
    let newTemplateName = document.getElementById('templateNameTextInput').value;
    setTemplateName(getFocusTemplateGuid(), newTemplateName);
}

function onTemplateDetailChanged(e) {
    let newDetailData = document.getElementById('templateDetailTextInput').value;
    setTemplateDetailData(getFocusTemplateGuid(), newDetailData);
}

function onLogKeyPress(e) {
    let ft = getFocusTemplate();
    setTemplateName(ft.domNode.getAttribute('templateGuid'), ft.domNode.getAttribute('templateInstanceGuid'), ft.domNode.innerText);
}

function onLogKeyDown(e) {
    if (!isRenamingTemplate) {
        return;
    }

    if (e.code == "Escape") {
        endSetTemplateName(true);
        return;
    }
    if (e.code == "Enter") {
        e.preventDefault();
        endSetTemplateName(false);
        return;
    }

    //if(!TEMPLATE_VALID_CHARS.includes(String.fromCharCode(e.keyCode))) {
    //    e.preventDefault();
    //}
}

function disableKey(keyName) {
    var tempBindings = null;

    var keyboard = quill.getModule('keyboard');
    tempBindings = keyboard.bindings[keyName];
    keyboard.bindings[keyName] = null;

    return tempBindings;
}

function reenableKey(keyName, bindings) {
    var keyboard = quill.getModule('keyboard');
    keyboard.bindings[keyName] = bindings;
}

function selectText(elm) {
    if (document.body.createTextRange) {
        const range = document.body.createTextRange();
        range.moveToElementText(elm);
        range.select();
    } else if (window.getSelection) {
        const selection = window.getSelection();
        const range = document.createRange();
        range.selectNodeContents(elm);
        selection.removeAllRanges();
        selection.addRange(range);
    } else {
        console.warn("Could not select text in node: Unsupported browser.");
    }
}

function isRenamingTemplate() {
    return getUsedTemplateInstances().some(function (ti) {
        return ti.domNode.contentEditable;
    })
}

//function setTemplateDocIdx(t, oldIdx, newIdx) {
//    var domTemplates = document.getElementsByClassName("template_btn");
//    for (var i = 0; i < domTemplates.length; i++) {
//        var domTemplate = domTemplates[i];
//        var template = {
//            templateId: domTemplate.getAttribute('templateId'),
//            templateName: domTemplate.getAttribute('templateName'),
//            templateColor: domTemplate.getAttribute('templateColor'),
//            docIdx: domTemplate.getAttribute('docIdx'),
//            templateText: domTemplate.innerText,

//            //templateType: domTemplate.getAttribute('templateType'),
//            //templateData: domTemplate.getAttribute('templateData'),
//            //isFocus: domTemplate.getAttribute('isFocus'),
//            //instanceId: domTemplate.getAttribute('instanceId'),
//            //domNode: domTemplate
//        }
//        if (template.templateId == t.templateId && template.docIdx == oldIdx) {
//            domTemplate.setAttribute('docIdx', newIdx);
//            return;
//        }
//    }
//}

function getTemplateDefByGuid(tguid) {
    return getUsedTemplateDefinitions().find(x=>x.domNode.getAttribute('templateGuid') == tguid);
}

function showTemplateToolbarContextMenu(tb) {
    clickCount = 0;

    var rgtClickContextMenu = document.getElementById('templateToolbarMenu');
    var rgtClickContextMenuList = document.getElementById('templateToolbarMenuList');
    rgtClickContextMenuList.innerHTML = '';

    var atdl = getAvailableTemplateDefinitions();
    for (var i = 0; i < atdl.length; i++) {
        var t = atdl[i];
        var templateItem = '<li onclick="createTemplate(\'' + t.domNode.getAttribute('templateGuid') + '\');">' +
            '<div style="background-color:' + t.domNode.getAttribute('templateColor') + '"></div>' +
            '<span>' + t.domNode.getAttribute('templateName') + '</span></li>';
        rgtClickContextMenuList.innerHTML += templateItem;
    }

    rgtClickContextMenuList.innerHTML += '<li onclick="createTemplate();">' +
        '<span style="color:lightseagreen;font-size:20px">+</span>' +
        '<span>Add Template</span></li>';
    const rect = tb.getBoundingClientRect();
    const x = rect.left + 20;
    const y = rect.bottom;
    rgtClickContextMenu.style.left = `${x}px`;
    rgtClickContextMenu.style.top = `${y}px`;
    rgtClickContextMenu.style.display = 'block';

    isShowingTemplateToolbarMenu = true;
}

function hideTemplateToolbarContextMenu() {
    var rgtClickContextMenu = document.getElementById('templateToolbarMenu');
    rgtClickContextMenu.style.display = 'none';

    isShowingTemplateToolbarMenu = false;
}

//function showTemplateContextMenu(te) {
//    if (te == null) {
//        return;
//    }
//    if (isShowingTemplateContextMenu) {
//        hideTemplateContextMenu();
//    }
//    clickCount = 0;

//    var rgtClickContextMenu = document.getElementById('templateContextMenu');
//    var rgtClickContextMenuList = document.getElementById('templateContextMenuList');
//    rgtClickContextMenuList.innerHTML = '';

//    let tId = te.getAttribute('templateId');
//    let iId = te.getAttribute('instanceId');
//    var context_items = [
//        { icon_class: 'template_rename_icon', label: '   Rename', func: 'renameTemplateClick' },
//        { icon_class: 'template_color_icon', label: '   Change Color', func: 'changeTemplateColorClick' },
//        { icon_class: 'template_delete_icon', label: '   Delete All', func: 'deleteAllTemplateClick' }];

//    context_items.forEach(function (ci) {
//        var context_item = '<li><a href="javascript:void(0);" onclick="' + ci.func + '(' + tId + ',' + iId + ');">' +
//            '<div class="template_context_item ' + ci.icon_class + '"></div> ' +
//            '<span style="font-family:arial;">' + ci.label + '</span></a></li>';
//        rgtClickContextMenuList.innerHTML += context_item;
//    });

//    const rect = te.getBoundingClientRect();
//    const x = rect.left + 20;
//    const y = rect.bottom;
//    rgtClickContextMenu.style.left = `${x}px`;
//    rgtClickContextMenu.style.top = `${y}px`;
//    rgtClickContextMenu.style.display = 'block';

//    isShowingTemplateContextMenu = true;
//}

//function hideTemplateContextMenu() {
//    var rgtClickContextMenu = document.getElementById('templateContextMenu');
//    rgtClickContextMenu.style.display = 'none';

//    isShowingTemplateContextMenu = false;
//}

function showTemplateColorPaletteMenu() {
    if (isShowingTemplateColorPaletteMenu) {
        hideTemplateColorPaletteMenu();
    }
    clickCount = 0;

    let tguid = getFocusTemplateGuid();// te.getAttribute('templateId');
    var palette_item = {
        style_class: 'template_color_palette_item',
        func: 'onColorPaletteItemClick'
    };

    let paletteHtml = '<table>';
    for (var r = 0; r < 10; r++) {
        paletteHtml += '<tr>';
        for (var c = 0; c < 10; c++) {
            let c = getRandomColor().trim();
            let item = '<td><a href="javascript:void(0);" onclick="' + palette_item.func + '(\'' + c + '\');">' +
                '<div class="' + palette_item.style_class + '" style="background-color: ' + c + '" ></div></a></td > ';
            paletteHtml += item;
        }
        paletteHtml += '</tr>';
    }

    var rgtClickColorPaletteMenu = document.getElementById('templateColorPaletteMenu');
    rgtClickColorPaletteMenu.innerHTML = paletteHtml;

    rgtClickColorPaletteMenu.style.display = 'block';
    var paletteMenuRect = rgtClickColorPaletteMenu.getBoundingClientRect();

    const editToolbarRect = document.getElementById('editTemplateToolbar').getBoundingClientRect(); //te.getBoundingClientRect();
    const colorBoxRect = document.getElementById('templateColorBox').getBoundingClientRect();
    const x = colorBoxRect.left;
    const y = editToolbarRect.top - paletteMenuRect.height;
    rgtClickColorPaletteMenu.style.left = `${x}px`;
    rgtClickColorPaletteMenu.style.top = `${y}px`;

    isShowingTemplateColorPaletteMenu = true;
}

function hideTemplateColorPaletteMenu() {
    var rgtClickColorPaletteMenu = document.getElementById('templateColorPaletteMenu');
    rgtClickColorPaletteMenu.style.display = 'none';

    isShowingTemplateColorPaletteMenu = false;
}

function hideAllContextMenus() {
    hideTemplateColorPaletteMenu();
    //hideTemplateContextMenu();
    hideTemplateToolbarContextMenu();
}

function updateEditTemplateToolbarPosition() {
    $("#editTemplateToolbar").css("position", "absolute");
    $("#editTemplateToolbar").css("top", $(window).height() - $("#editTemplateToolbar").outerHeight());
}

function showEditTemplateToolbar() {
    clickCount = 0;
    isShowingEditTemplateToolbar = true;
    var ett = document.getElementById('editTemplateToolbar');
    ett.style.display = 'flex';

    updateEditTemplateToolbarPosition();

    var t = getTemplateDefByGuid(getFocusTemplateGuid());

    setTemplateType(t.domNode.getAttribute('templateGuid'), t.domNode.getAttribute('templateType'));

    document.getElementById('editTemplateTypeMenuSelector').addEventListener('change', onTemplateTypeChanged);
}

function hideEditTemplateToolbar() {
    isShowingEditTemplateToolbar = false;
    var ett = document.getElementById('editTemplateToolbar');
    ett.style.display = 'none';

    document.getElementById('editTemplateTypeMenuSelector').removeEventListener('change', onTemplateTypeChanged);
}

function selectedTemplateTypeChanged() {
    console.log("changed");
}

//#region Paste Toolbar

function showPasteTemplateToolbar() {
    isShowingPasteTemplateToolbar = true;
    var ptt = document.getElementById('pasteTemplateToolbar');
    ptt.style.display = 'inline-block';

    document.getElementById('paste-template-custom-select').innerHTML = '<select id="pasteTemplateToolbarMenuSelector"></select >';

    var templateMenuSelector = document.getElementById('pasteTemplateToolbarMenuSelector');
    templateMenuSelector.innerHTML = '';

    var tl = getUsedTemplateDefinitions();
    for (var i = 0; i < tl.length; i++) {
        var t = tl[i];
        var templateItem = '<option class="templateOption" value="' + t['templateGuid'] + '" onchange="focusTemplate(' + t['templateGuid'] + ');">' +
            t['templateName'] + '</option>';
        templateMenuSelector.innerHTML += templateItem;
    }

    document.getElementById('nextTemplateButton').addEventListener('click', function (e) {
        gotoNextTemplate();
    });
    document.getElementById('nextTemplateButton').addEventListener('keydown', function (e) {
        gotoNextTemplate();
    });
    document.getElementById('previousTemplateButton').addEventListener('click', function (e) {
        gotoPrevTemplate();
    });
    document.getElementById('previousTemplateButton').addEventListener('keydown', function (e) {
        gotoPrevTemplate();
    });
    document.getElementById('clearAllTemplateTextButton').addEventListener('click', function (e) {
        clearAllTemplateText();
    });
    document.getElementById('clearAllTemplateTextButton').addEventListener('keydown', function (e) {
        clearAllTemplateText();
    });
    document.getElementById('pasteTemplateButton').addEventListener('click', function (e) {
        isCompleted = true;
    });
    document.getElementById('pasteTemplateButton').addEventListener('keydown', function (e) {
        isCompleted = true;
    });

    createTemplateSelectorStyling(templateMenuSelector);

    //let tbb = isShowingToolbar ? $(".ql-toolbar").outerHeight() : 0;
    //moveToolbarTop(0);
    let wb = $(window).height();
    movePasteTemplateToolbarTop($(window).height() - $("#pasteTemplateToolbar").outerHeight());
    //moveEditorTop($("#pasteTemplateToolbar").outerHeight() + tbb);

    $('#templateTextArea').focus();
}

function hidePasteTemplateToolbar() {
    isShowingPasteTemplateToolbar = false;
    var ptt = document.getElementById('pasteTemplateToolbar');
    ptt.style.display = 'none';
    if (isShowingEditorToolbar) {
        moveEditorTop($(".ql-toolbar").outerHeight());
    } else {
        moveEditorTop(0);
    }
}

function gotoNextTemplate() {
    var tl = getUsedTemplateDefinitions();
    var curIdx = 0;
    for (var i = 0; i < tl.length; i++) {
        if (tl[i]['templateGuid'] == getFocusTemplateGuid()) {
            curIdx = i;
            break;
        }
    }
    var nextIdx = curIdx + 1;
    if (nextIdx >= tl.length) {
        nextIdx = 0;
    }
    focusTemplate(tl[nextIdx]['templateGuid']);
}

function gotoPrevTemplate() {
    var tl = getUsedTemplateDefinitions();
    var curIdx = 0;
    for (var i = 0; i < tl.length; i++) {
        if (tl[i]['templateGuid'] == getFocusTemplateGuid()) {
            curIdx = i;
            break;
        }
    }
    var prevIdx = curIdx - 1;
    if (prevIdx < 0) {
        prevIdx = tl.length - 1;
    }
    focusTemplate(tl[prevIdx]['templateGuid']);
}

function clearAllTemplateText() {
    var tl = getUsedTemplateDefinitions();
    for (var i = 0; i < tl.length; i++) {
        setTemplateText(tl[i]['templateGuid'], '');
    }
}

function setTemplateText(tguid, text) {
    var stl = document.getElementsByClassName("template_btn");
    for (var i = 0; i < stl.length; i++) {
        var t = stl[i];
        if (t.getAttribute('templateGuid') == tguid) {
            t.innerText = text;
            t.templateText = text;
        }
    }
}

//#endregion

function resetTemplates() {
    var til = getUsedTemplateInstances();
    for (var i = 0; i < til.length; i++) {
        var ti = til[i];

        ti.domNode.setAttribute('templateText', '');
        ti.domNode.setAttribute('isFocus', 'false');
        ti.domNode.innerHTML = ti.templateName;
    }
}

function createTemplateSelectorStyling() {
    var tl = getUsedTemplateDefinitions();
    var x, i, j, selElmnt, a, b, c;
    x = document.getElementsByClassName("paste-template-custom-select");
    for (i = 0; i < x.length; i++) {
        selElmnt = x[i].getElementsByTagName("select")[0];
        if (selElmnt.options.length == 0) {
            continue;
        }
        var st = getTemplateDefByGuid(selElmnt.options[selElmnt.selectedIndex].getAttribute('value'));
        a = document.createElement("DIV");
        a.setAttribute("class", "select-selected");
        a2 = document.createElement("SPAN");
        a2.setAttribute("class", "square-box");
        a2.setAttribute("style", "background-color: " + st['templateColor']);
        a.appendChild(a2);
        a.innerHTML += '<span style="margin: 0 10px 0 50px;">' + selElmnt.options[selElmnt.selectedIndex].innerHTML + '</span>';
        x[i].appendChild(a);
        b = document.createElement("DIV");
        b.setAttribute("class", "select-items select-hide");
        for (j = 0; j < selElmnt.length; j++) {
            var t = getTemplateDefByGuid(selElmnt.options[j].getAttribute('value'));
            c = document.createElement("DIV");
            c.setAttribute('optionId', t['templateGuid']);
            //c.innerHTML = selElmnt.options[j].innerHTML;
            d = document.createElement("SPAN");
            d.setAttribute("class", "square-box");
            d.setAttribute("style", "background-color: " + t['templateColor']);
            c.appendChild(d);
            c.innerHTML += selElmnt.options[j].innerHTML;
            c.addEventListener("click", onTemplateOptionClick);
            b.appendChild(c);
            if (j == selElmnt.selectedIndex) {
                c.classList.add("class", "same-as-selected");
            }
        }
        x[i].appendChild(b);
        a.addEventListener("click", function (e) {
            e.stopPropagation();
            closeAllSelect(this);
            this.nextSibling.classList.toggle("select-hide");
            this.classList.toggle("select-arrow-active");
        });
    }

    function closeAllSelect(elmnt) {
        var x, y, i, arrNo = [];
        x = document.getElementsByClassName("select-items");
        y = document.getElementsByClassName("select-selected");
        for (i = 0; i < y.length; i++) {
            if (elmnt == y[i]) {
                arrNo.push(i)
            } else {
                y[i].classList.remove("select-arrow-active");
            }
        }
        for (i = 0; i < x.length; i++) {
            if (arrNo.indexOf(i)) {
                x[i].classList.add("select-hide");
            }
        }
    }
    document.addEventListener("click", closeAllSelect);
}

function onTemplateOptionClick(e, target) {
    var targetDiv = e == null ? target : e.currentTarget;
    var tVal = targetDiv.getAttribute('optionId');
    var t = getTemplateDefByGuid(tVal);
    var y, i, k, s, h;
    s = targetDiv.parentNode.parentNode.getElementsByTagName("select")[0];
    h = targetDiv.parentNode.previousSibling;
    for (i = 0; i < s.length; i++) {
        var sVal = s.options[i].value;
        if (sVal == tVal) {
            s.selectedIndex = i;
            h.innerHTML = targetDiv.innerHTML.replace(t['templateName'], '<span style="margin: 0 10px 0 50px;">' + t['templateName'] + '</span>');
            y = targetDiv.parentNode.getElementsByClassName("same-as-selected");
            for (k = 0; k < y.length; k++) {
                y[k].removeAttribute("class");
            }
            targetDiv.setAttribute("class", "same-as-selected");
            break;
        }
    }

    focusTemplate(s.options[s.selectedIndex].value, true);
    h.click();
}

function isEmptyOrSpaces(str) {
    return str === null || str.match(/^ *$/) !== null;
}

function generateGuid() {
    var d = new Date().getTime();//Timestamp
    var d2 = (performance && performance.now && (performance.now() * 1000)) || 0;//Time in microseconds since page-load or 0 if unsupported
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16;//random number between 0 and 16
        if (d > 0) {//Use timestamp until depleted
            r = (d + r) % 16 | 0;
            d = Math.floor(d / 16);
        } else {//Use microseconds since page-load if supported
            r = (d2 + r) % 16 | 0;
            d2 = Math.floor(d2 / 16);
        }
        return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
}

// end templates