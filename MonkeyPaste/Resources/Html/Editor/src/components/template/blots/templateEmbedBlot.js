var TemplateEmbedClass = 'ql-template-embed-blot';

var Template_FOCUSED_INSTANCE_Class = 'ql-template-embed-blot-focus';
var Template_FOCUSED_NOT_INSTANCE_Class = 'ql-template-embed-blot-focus-not-instance';

var Template_IN_SEL_RANGE_Class = 'ql-template-embed-blot-selected-overlay';

var Template_BEFORE_INSERT_Class = 'ql-template-embed-blot-before-insert';
var Template_AT_INSERT_Class = 'ql-template-embed-blot-at-insert'; 
var Template_AFTER_INSERT_Class = 'ql-template-embed-blot-after-insert';

const Parchment = Quill.imports.parchment;

class TemplateEmbedBlot extends Parchment.EmbedBlot {
    static blotName = 'template';
    static tagName = 'SPAN';
    static className = TemplateEmbedClass;

    static create(value) {
        const node = super.create(value);

        if (value.domNode != null) {
            // creating existing instance
            value = getTemplateFromDomNode(value.domNode);
        }

        if (!IsMovingTemplate) {
            //ensure new template has unique instance guid
            value.templateInstanceGuid = generateGuid();
        }

        applyTemplateToDomNode(node, value);
        return node;
    }

    static formats(node) {
        return getTemplateFromDomNode(node);
    }
    format(name, value) {
        super.format(name, value);
    }


    //length() {
    //    return 1;
    //}

    static value(domNode) {
        return getTemplateFromDomNode(domNode);
    }
}
function registerTemplateBlots() {
    
    Quill.register(TemplateEmbedBlot, true);
}

function getTemplateFromDomNode(domNode) {
    if (domNode == null) {
        return null;
    }
    return {
        //domNode: domNode,
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

    setTemplateBgColor(null, value.templateInstanceGuid, value.templateColor, false);

    node.innerHTML = value.templateHtmlFormat;
    changeInnerText(node, node.innerText, value.templateName);

    // TODO instead of rejecting mouse down, template should be draggable

    //disable text selection
    //node.setAttribute('unselectable', 'on');
    //node.setAttribute('onselectstart', 'return false;');
    //node.setAttribute('onmousedown', 'return false;');

    node.setAttribute('isFocus', false);
    node.setAttribute("spellcheck", "false");
    node.classList.add(TemplateEmbedClass);
    node.setAttribute('draggable', false);
    node.setAttribute('contenteditable', false);

    var templateDocIdxCache;
    function onTemplatePointerDown(e) {
        node.addEventListener('pointermove', onTemplatePointerMove);
        node.setPointerCapture(e.pointerId);
        templateDocIdxCache
    }

    function onTemplatePointerUp(e) {
        templateDocIdxCache = null;

        node.removeEventListener('pointermove', onTemplatePointerMove);
        node.releasePointerCapture(e.pointerId);
    }

    function onTemplatePointerMove(e) {
        let curMousePos = getEditorPosFromTemplateMouse(e);
        if (!IsMovingTemplate && dist(MouseDownOnTemplatePos, curMousePos) < MIN_TEMPLATE_DRAG_DIST) {
            return;
        }
        if (templateDocIdxCache == null) {
            templateDocIdxCache = getTemplateElementsWithDocIdx();
        }

        let docIdx = getDocIdxFromPoint(curMousePos, templateDocIdxCache);
        log('docIdx: ' + docIdx);
        if (docIdx < 0) {
            return;
        }
        moveTemplate(value.templateInstanceGuid, docIdx, false);

        //if (!quill.hasFocus()) {
        //    quill.focus();
        //}
        //setEditorSelection(docIdx, 0);
    }

    function getEditorPosFromTemplateMouse(e) {
        return getEditorMousePos(e);
        let curMousePos = { x: e.pageX, y: e.pageY };
        curMousePos.x -= e.currentTarget.offsetLeft;
        curMousePos.y -= e.currentTarget.offsetTop;
        return curMousePos;
    }

    //node.addEventListener('pointerdown', onTemplatePointerDown);
    //node.addEventListener('pointerup', onTemplatePointerUp);

    node.addEventListener('click', function (e) {
        if (!IsSubSelectionEnabled) {
            log("Selection disabled so ignoring click on template " + value.templateInstanceGuid);
            return;
		}
        focusTemplate(value.templateGuid, value.templateInstanceGuid, false);
    });
    node.addEventListener('pointerdown', function (e) {
        let ti_doc_idx = getTemplateDocIdx(value.templateInstanceGuid);
        setEditorSelection(ti_doc_idx, 1, 'api');
    });

    return node;
}

