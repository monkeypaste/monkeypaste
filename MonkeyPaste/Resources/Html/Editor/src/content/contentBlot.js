function registerContentBlots(Quill) {
    const Parchment = Quill.imports.parchment;

    class ContentSpanBlot extends Parchment.EmbedBlot {
        static create(value) {
            let node = super.create(value);
            node.setAttribute('copyitemid', value.id);
            node.innerHtml = value.itemData;

            node.domNode.addEventListener('mouseenter', function (e) {
                console.log(node.domNode);
            });
            return node;
        }

        static value(domNode) {
            return {
                id: domNode.getAttribute('copyitemid'),
                itemData: domNode.innerHtml
            }
        }
    }
    ContentSpanBlot.blotName = 'contentSpan';
    ContentSpanBlot.tagName = 'SPAN';

    Quill.register(ContentSpanBlot);

    class ContentBlockBlot extends Parchment.EmbedBlot {
        static create(value) {
            let node = super.create(value);
            node.setAttribute('copyitemid', value.id);
            node.innerHtml = value.itemData;
            return node;
        }

        static value(domNode) {
            return {
                id: domNode.getAttribute('copyitemid'),
                itemData: domNode.innerHtml
            }
        }
    }
    ContentBlockBlot.blotName = 'contentBlock';
    ContentBlockBlot.tagName = 'P';

    Quill.register(ContentBlockBlot);
}
