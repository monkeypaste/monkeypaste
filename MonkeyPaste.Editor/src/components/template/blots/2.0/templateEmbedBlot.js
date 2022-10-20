var TemplateEmbedClass = 'ql-template-embed-blot';

var Template_FOCUSED_INSTANCE_Class = 'ql-template-embed-blot-focus';
var Template_FOCUSED_NOT_INSTANCE_Class = 'ql-template-embed-blot-focus-not-instance';

var Template_IN_SEL_RANGE_Class = 'ql-template-embed-blot-selected-overlay';

var Template_BEFORE_INSERT_Class = 'ql-template-embed-blot-before-insert';
var Template_AT_INSERT_Class = 'ql-template-embed-blot-at-insert'; 
var Template_AFTER_INSERT_Class = 'ql-template-embed-blot-after-insert';

var TemplateEmbedHtmlAttributes = [
    'background-color',
    'wasVisited',
    'docIdx'
    //'color'
];

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

    update(mutations,context) {
        //super.update(mutations, context);
        //const attributeChanged = mutations.some(
        //    (mutation) =>
        //        mutation.target === this.domNode && mutation.type === 'attributes',
        //);
        //if (attributeChanged) {
        //    this.attributes.build();
        //}
    }
    //length() {
    //    return 1;
    //}

    static value(node) {
        return getTemplateFromDomNode(node);
    }
}

function registerTemplateBlots() {    
    let suppressWarning = false;
    let config = {
        scope: Parchment.Scope.INLINE,
    };

    for (var i = 0; i < TemplateEmbedHtmlAttributes.length; i++) {
        let attrb_name = TemplateEmbedHtmlAttributes[i];
        let attrb = new Parchment.Attributor(attrb_name, attrb_name, config);
        Quill.register(attrb, suppressWarning);
	}

    Quill.register(TemplateEmbedBlot, true);
}

function getTemplateFromDomNode(domNode) {
    if (domNode == null) {
        return null;
    }
 //   if (!domNode.hasAttribute('wasVisited')) {
 //       domNode.setAttribute('wasVisited', false);
	//}
    return {
        //domNode: domNode,
        templateGuid: domNode.getAttribute('templateGuid'),
        templateInstanceGuid: domNode.getAttribute('templateInstanceGuid'),
        isFocus: domNode.classList.contains(Template_FOCUSED_INSTANCE_Class) || domNode.classList.contains(Template_FOCUSED_NOT_INSTANCE_Class),
        templateName: domNode.getAttribute('templateName'),
        templateColor: domNode.getAttribute('templateColor'),
        templateText: domNode.getAttribute('templateText'),
        templateType: domNode.getAttribute('templateType'),
        templateData: domNode.getAttribute('templateData'),
        templateDeltaFormat: domNode.getAttribute('templateDeltaFormat'),
        templateHtmlFormat: domNode.getAttribute('templateHtmlFormat'),
        wasVisited: parseBool(domNode.getAttribute('wasVisited'))
    }
}

function applyTemplateToDomNode(node, value) {
    if (node == null || value == null) {
        return node;
    }

    let ttype = value.templateType.toLowerCase();
    if (ttype == 'datetime' && isNullOrWhiteSpace(value.templateData)) {
        value.templateData = 'MM/dd/yyy HH:mm:ss';
    }
    if (ttype == 'static' && value.templateData == null) {
        value.templateData = '';
    }

    if (!value.wasVisited) {
        value.wasVisited = false;
    }
    if (!value.templateText) {
        value.templateText = '';
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
    node.setAttribute('wasVisited', value.wasVisited);

    node.setAttribute("spellcheck", "false");
    node.classList.add(TemplateEmbedClass);
    node.setAttribute('draggable', false);
    node.setAttribute('contenteditable', false);

    if (value.isFocus) {
        if (node.classList.contains(Template_FOCUSED_INSTANCE_Class)) {
            // is this ok time to remove this?
            //debugger;
		}
        node.classList.add(Template_FOCUSED_NOT_INSTANCE_Class);
        node.classList.remove(Template_FOCUSED_INSTANCE_Class);
    } else {
        if (node.classList.contains(Template_FOCUSED_INSTANCE_Class) ||
            node.classList.contains(Template_FOCUSED_NOT_INSTANCE_Class)) {
            // is this ok time to remove this?
            //debugger;
        }
        node.classList.remove(Template_FOCUSED_NOT_INSTANCE_Class);
        node.classList.remove(Template_FOCUSED_INSTANCE_Class);
	}

    node.style.backgroundColor = value.templateColor;
    node.style.color = isBright(value.templateColor) ? 'black' : 'white';

    node.innerHTML = value.templateHtmlFormat;
    node.innerText = getTemplateDisplayValue(value);

    node.addEventListener('click', onTemplateClick);
    return node;
}

function onTemplateClick(e) {
    let t = getTemplateFromDomNode(e.currentTarget);
    if (!t) {
        debugger;
	}
    if (!IsSubSelectionEnabled) {
        log("Selection disabled so ignoring click on template " + value.templateInstanceGuid);
        return;
    }

    focusTemplate(t.templateGuid, false, false, true);
    //setTemplateElementFocus(e.currentTarget, true, true);
}

// unused drag drop stuff

function getEditorPosFromTemplateMouse(e) {
    return getEditorMousePos(e);
    let curMousePos = { x: e.pageX, y: e.pageY };
    curMousePos.x -= e.currentTarget.offsetLeft;
    curMousePos.y -= e.currentTarget.offsetTop;
    return curMousePos;
}
function onTemplatePointerDown(e) {
    node.addEventListener('pointermove', onTemplatePointerMove);
    node.setPointerCapture(e.pointerId);
    //templateDocIdxCache
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