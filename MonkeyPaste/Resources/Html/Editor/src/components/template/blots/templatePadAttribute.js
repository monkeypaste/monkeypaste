//var templatePadAttribute;


function registerTemplatePadAttribute() {
    const Parchment = Quill.imports.parchment;

    Parchment = Quill.import('parchment');
    let suppressWarning = false;
    let config = {
        scope: Parchment.Scope.INLINE,
    };

    templatePadAttribute = new Parchment.Attributor('templatePad', 'templatePad', config);
    Quill.register(templatePadAttribute, suppressWarning);
}
