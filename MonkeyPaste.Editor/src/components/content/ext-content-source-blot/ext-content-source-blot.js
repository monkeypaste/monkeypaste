// #region Globals


// #endregion Globals

// #region Life Cycle
function initExtContentSourceBlot() {
    if (Quill === undefined) {
        /// host load error case
        debugger;
    }

    let Parchment = Quill.imports.parchment;
    class ExtSourceInlineBlot extends Parchment.InlineBlot {
        static blotName = 'ext-source-inline-blot';
        static tagName = InlineTags;
        static className = 'ext-source-inline';

        static create(value) {
            const node = super.create(value);

            applyExtSourceToDomNode(node, value);
            return node;
        }

        static formats(node) {
            return getExtSourceFromDomNode(node);
        }

        format(name, value) {
            super.format(name, value);
        }
        //optimize(context) {
        //    return;
        //}

        static value(node) {
            return getExtSourceFromDomNode(node);
        }
    }

    Quill.register(ExtSourceInlineBlot, true);

    class ExtSourceTextBlot extends Parchment.TextBlot {
        static blotName = 'ext-source-text-blot';
        static tagName = '#text';
        static className = 'ext-source-text';

        static create(value) {
            const node = super.create(value);
            const span = document.createElement('span');
            span.appendChild(node);
            applyExtSourceToDomNode(span, value);
            return span;
        }

        static formats(node) {
            return getExtSourceFromDomNode(node);
        }

        format(name, value) {
            super.format(name, value);
        }
        //optimize(context) {
        //    return;
        //}

        static value(node) {
            if (node.nodeType === 3) {
                return getExtSourceFromDomNode(node.parentNode);
            }
            return getExtSourceFromDomNode(node);
        }
    }

    Quill.register(ExtSourceTextBlot, true);
}
// #endregion Life Cycle

// #region Getters

function getExtSourceFromDomNode(node) {
    if (node == null) {
        return null;
    }
    return {
        copyItemSourceGuid: node.getAttribute('copyItemSourceGuid')
        //copyItemSourceBgColor: getElementComputedStyleProp(node,'background-color')
    };
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function applyExtSourceToDomNode(node,value) {
    if (node == null || value == null) {
        return node;
    }
    node.setAttribute('copyItemSourceGuid', value.copyItemSourceGuid);
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers