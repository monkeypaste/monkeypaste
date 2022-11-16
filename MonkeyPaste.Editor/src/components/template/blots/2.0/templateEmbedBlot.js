var TemplateEmbedClass = 'template-blot';

var Template_FOCUSED_INSTANCE_Class = 'template-blot-focus';
var Template_FOCUSED_NOT_INSTANCE_Class = 'template-blot-focus-not-instance';

var Template_IN_SEL_RANGE_Class = 'template-blot-selected-overlay';

var Template_BEFORE_INSERT_Class = 'template-blot-before-insert';
var Template_AT_INSERT_Class = 'template-blot-at-insert'; 
var Template_AFTER_INSERT_Class = 'template-blot-after-insert';

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

        if (IsLoaded) {
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

    update(mutations, context) {
        //let last_sel = LastSelRange;
        //let sel = getDocSelection();

        //for (var i = 0; i < mutations.length; i++) {
        //    let outstr = 'mutation: ' + mutations[i].type;
        //    if (mutations[i].type == 'attributes') {
        //        outstr += ' ' + mutations[i].attributeName;
        //    }
        //    if (mutations[i].type == 'characterData') {
        //        let forced_text = this.domNode.getAttribute('templateName');
        //        let added_text = substringByLength(this.domNode.innerText, forced_text.length);
        //        this.domNode.innerText = forced_text;
        //        insertText(sel.index + 1, added_text, 'user');
        //        if (!sel || sel.length > 0) {
        //            // what's happening? how do we handle this?
        //            debugger;
        //            continue;
        //        }

        //        setDocSelection(sel.index + added_text.length + 1, 0, 'api');
        //        return;
        //    }
        //    log(outstr);
        //}
        //super.update(mutations, context);
    }

    //length() {
    //    return 1;
    //}

    static value(node) {
        return getTemplateFromDomNode(node);
    }
}

function registerTemplateBlots() {    
 //   let suppressWarning = false;
 //   let config = {
 //       scope: Parchment.Scope.INLINE,
 //   };

 //   for (var i = 0; i < TemplateEmbedHtmlAttributes.length; i++) {
 //       let attrb_name = TemplateEmbedHtmlAttributes[i];
 //       let attrb = new Parchment.Attributor(attrb_name, attrb_name, config);
 //       Quill.register(attrb, suppressWarning);
	//}

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
    let icon_elm = document.createElement('SVG');
    let span_elm = document.createElement('SPAN');
    let delete_elm = document.createElement('SVG');

    node.replaceChildren(icon_elm, span_elm, delete_elm);

    node.style.backgroundColor = value.templateColor;
    node.style.color = getContrastHexColor(value.templateColor);

    icon_elm.outerHTML = getSvgHtml(getTemplateTypeSvgKey(value.templateType),'template-type-icon svg-icon');
    setSvgElmColor(icon_elm, node.style.color);

    span_elm.innerHTML = value.templateHtmlFormat;
    span_elm.innerText = getTemplateDisplayValue(value);
    span_elm.style.color = node.style.color;

    delete_elm.outerHTML = getSvgHtml('delete','delete-template-button');
    delete_elm.addEventListener('click', onTemplateDeleteButtonClick);

    node.addEventListener('click', onTemplateClick);
    return node;
}

function onTemplateDeleteButtonClick(e) {
    let telm = e.target.parentNode;
    if (telm) {
        telm.parentNode.removeChild(telm);
    }
}

function onTemplateClick(e) {
    if (e.target.classList.contains('delete-template-button')) {
        onTemplateDeleteButtonClick(e);
        return;
    }
    let t = getTemplateFromDomNode(e.currentTarget);
    if (!t) {
        debugger;
	}
    if (!isSubSelectionEnabled()) {
        log("Selection disabled so ignoring click on template " + value.templateInstanceGuid);
        return;
    }

    focusTemplate(t.templateGuid, false, false, true);
    //setTemplateElementFocus(e.currentTarget, true, true);
}
