//var templatePadAttribute;
//const Parchment = Quill.imports.parchment;
//let Inline = Quill.import('blots/inline');
var TemplatePadClass = 'ql-template-pad-blot';
const PRE_PAD_OBJ = { isPre: true };
const POST_PAD_OBJ = { isPre: false };

class TemplatePadBlot extends Parchment.EmbedBlot {
    static blotName = 'templatePad';
    static tagName = 'FRAGMENT';
    static className = TemplatePadClass;

    static create(value) {
        const node = super.create(value);
  //      if (value) {
  //          node.classList.add('templatePad_pre');
  //          node.classList.remove('templatePad_post');
  //      } else {
  //          node.classList.add('templatePad_post');
  //          node.classList.remove('templatePad_pre');
		//}
        node.setAttribute('isPre', value.isPre);
        //node.setAttribute('contenteditable', true);
        
       // node.innerHTML = '<span contenteditable="false"> </span>';
        node.innerText = ' ';
  //      if (!value.isPre) {

  //          let inner_span = document.createElement('span');
  //          node.parentNode.appendChild(inner_span);
		//}
        return node;
    }
    static formats(node) {
        return node.getAttribute('isPre');
    }
    format(name, value) {
        super.format(name, value);
    }
    length() {
        return 1;
    }
    static value(node) {
        //return domNode.classList.contains('templatePad_pre');
        return {
            isPre: node.getAttribute('isPre')
        };
    }
}

function registerTemplatePadAttribute() {
    const Parchment = Quill.imports.parchment;
    let suppressWarning = false;
    let config = {
        scope: Parchment.Scope.INLINE,
    };

    let templatePadAttribute_pre = new Parchment.Attributor('templatePad_pre', 'templatePad_pre', config);
    Quill.register(templatePadAttribute_pre, suppressWarning);

    let templatePadAttribute_post = new Parchment.Attributor('templatePad_post', 'templatePad_post', config);
    Quill.register(templatePadAttribute_post, suppressWarning);

    let domNode_attr = new Parchment.Attributor('domNode', 'domNode', config);
    Quill.register(domNode_attr, suppressWarning);
    //Quill.register(TemplatePadBlot, true);
}
