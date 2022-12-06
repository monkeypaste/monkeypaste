var TemplateEmbedClass = 'template-blot';;

var Template_AT_INSERT_Class = 'template-blot-at-insert'; 

var TemplateEmbedHtmlAttributes = [
    'background-color',
    'wasVisited',
    'docIdx'
    //'color'

    //'templateGuid',
    //'templateInstanceGuid',
    //'isFocus',
    //'templateName',
    //'templateColor',
    //'templateText',
    //'templateType',
    //'templateData',
    //'templateDeltaFormat',
    //'templateHtmlFormat',
    //'wasVisited'
];

function initTemplateBlot() {
    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    if (UseQuill2) {
        initTemplateEmbedBlot_quill2();
    } else {
        initTemplateEmbedBlot_quill1();
    }
    
}

function initTemplateEmbedBlot_quill2() {
    let Parchment = Quill.imports.parchment;
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

            applyTemplateToDomNode(node, value);
            return node;
        }

        static formats(node) {
            return getTemplateFromDomNode(node);
        }

        format(name, value) {
            super.format(name, value);
        }

        update(mutations, context) {
        }

        static value(node) {
            return getTemplateFromDomNode(node);
        }
    }

    Quill.register(TemplateEmbedBlot, true);
}

function initTemplateEmbedBlot_quill1() {
    let Parchment = Quill.imports.parchment;
    if (Parchment == null || Parchment === undefined) {
        debugger;
    }

    class TemplateEmbedBlot extends Parchment.Embed {
        static blotName = 'template';
        static tagName = 'SPAN';
        static className = TemplateEmbedClass;

        static create(value) {
            const node = super.create(value);

            if (value.domNode != null) {
                // creating existing instance
                value = getTemplateFromDomNode(value.domNode);
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

        update(mutations, context) {
        }

        static value(node) {
            return getTemplateFromDomNode(node);
        }
    }

    Parchment.register(TemplateEmbedBlot);
}


function getTemplateFromDomNode(domNode) {
    if (domNode == null) {
        return null;
    }
    return {
        templateGuid: domNode.getAttribute('templateGuid'),
        isFocus: domNode.classList.contains('focused'),
        templateName: domNode.getAttribute('templateName'),
        templateColor: domNode.getAttribute('templateColor'),
        templateText: domNode.getAttribute('templateText'),
        templateType: domNode.getAttribute('templateType'),
        templateData: domNode.getAttribute('templateData'),
        templateDeltaFormat: domNode.getAttribute('templateDeltaFormat'),
        templateHtmlFormat: domNode.getAttribute('templateHtmlFormat'),
        wasVisited: parseBool(domNode.getAttribute('wasVisited')),
    }
}

function applyTemplateToDomNode(node, value) {
    // PRE-GAME
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

    // MODEL
    node.setAttribute('templateGuid', value.templateGuid);
    node.setAttribute('templateName', value.templateName);
    node.setAttribute('templateType', value.templateType);
    node.setAttribute('templateColor', value.templateColor);
    node.setAttribute('templateData', value.templateData);
    node.setAttribute('templateText', value.templateText);
    node.setAttribute('templateDeltaFormat', value.templateDeltaFormat);
    node.setAttribute('templateHtmlFormat', value.templateHtmlFormat);

    // STATE
    node.setAttribute('wasVisited', value.wasVisited);

    // DOM
    node.setAttribute("spellcheck", "false");
    node.setAttribute('draggable', false);
    node.setAttribute('contenteditable', false);

    // STYLE
    node.classList.add(TemplateEmbedClass);
    node.style.backgroundColor = value.templateColor;

    node.replaceChildren();

    // ICON
    let icon_elm = document.createElement('SVG');
    icon_elm = createSvgElement(getTemplateTypeSvgKey(value.templateType), 'template-type-icon svg-icon contrast-bg');
    node.appendChild(icon_elm);

    // LABEL
    let span_elm = document.createElement('SPAN');
    span_elm.classList.add('template-label');
    //span_elm.classList.add('flicker');
    span_elm.innerHTML = value.templateHtmlFormat;
    span_elm.innerText = getTemplateDisplayValue(value);
    span_elm.style.color = getContrastHexColor(value.templateColor);
    node.appendChild(span_elm);

    // DELETE BUTTON
    let delete_elm = document.createElement('SVG');
    delete_elm = createSvgElement('delete','delete-template-button contrast-bg');
    node.appendChild(delete_elm);

    // EVENTS

    node.addEventListener('click', onTemplateClick, true);
    delete_elm.addEventListener('click', onTemplateDeleteButtonClick);

    return node;
}

function onTemplateDeleteButtonClick(e) {
    let telm = e.target.parentNode;
    if (telm) {
        removeTemplateElement(telm);
    }
}

function onTemplateClick(e) {
    log('template clicked');


    if (!isSubSelectionEnabled()) {
        log("Selection disabled so ignoring click on template " + value.templateGuid);
        return;
    }
    if (e.target.classList.contains('delete-template-button')) {
        onTemplateDeleteButtonClick(e);
        return;
    }
    let t = getTemplateFromDomNode(e.currentTarget);

    focusTemplate(t.templateGuid);
}
