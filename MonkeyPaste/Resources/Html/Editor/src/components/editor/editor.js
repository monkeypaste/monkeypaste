var DefaultEditorWidth = 1200;

var IgnoreNextTextChange = false;
var IgnoreNextSelectionChange = false;

var isPastingTemplate = false;

function initEditor(reqMsg) { 
    loadQuill(reqMsg);

    if (reqMsg.envName == 'web') {
        //for testing in browser
        document.getElementsByClassName('ql-toolbar')[0].classList.add('env-web');
    } else {
        document.getElementsByClassName('ql-toolbar')[0].classList.add('env-wpf');
        if (reqMsg.isReadOnlyEnabled) {
            enableReadOnly();
        } else {
            showEditorToolbar();
            disableReadOnly();
        }
    }
}

function loadQuill(reqMsg) {
    Quill.register("modules/htmlEditButton", htmlEditButton);
    Quill.register({ 'modules/better-table': quillBetterTable }, true);

    //registerContentBlots();
    registerContentGuidAttribute();
    registerTemplateSpan();

    // Append the CSS stylesheet to the page
    var node = document.createElement('style');
    node.innerHTML = registerFontStyles(reqMsg.envName);
    document.body.appendChild(node);

    quill = new Quill(
        '#editor', {
            //debug: true,
        placeholder: '',
        theme: 'snow',
        modules: {
            table: false,
            toolbar: registerToolbar(reqMsg.envName),
            htmlEditButton: {
                syntax: true,
            },
            'better-table': {
                operationMenu: {
                    items: {
                        unmergeCells: {
                            text: 'Unmerge cells'
                        }
                    },
                    color: {
                        colors: ['green', 'red', 'yellow', 'blue', 'white'],
                        text: 'Background Colors:'
                    }
                }
            },
            keyboard: {
                bindings: quillBetterTable.keyboardBindings
            }
        }
    });

    quill.root.setAttribute("spellcheck", "false");

    initTableToolbarButton();

    window.addEventListener('click', (e) => {
        if (e.path.find(x => x.classList && x.classList.contains('edit-template-toolbar')) != null ||
            e.path.find(x => x.classList && x.classList.contains('paste-template-toolbar')) != null ||
            e.path.find(x => x.classList && x.classList.contains('context-menu-option')) != null || 
            e.path.find(x => x.classList && x.classList.contains('ql-toolbar')) != null) {
            //ignore clicks within template toolbars
            return;
        }
        if (e.path.find(x => x.classList && x.classList.contains('ql-template-embed-blot')) == null) {
            hideAllTemplateContextMenus();
            hideEditTemplateToolbar();
            hidePasteTemplateToolbar();
            clearTemplateFocus();
        }
    });

    quill.on('selection-change', function (range, oldRange, source) {
        //LastSelectedHtml = SelectedHtml;
        //SelectedHtml = getSelectedHtml();
        if (IgnoreNextSelectionChange) {
            IgnoreNextSelectionChange = false;
            return;
        }

        if (range) {
            refreshFontSizePicker();
            refreshFontFamilyPicker();

            if (range.length == 0) {
                var text = quill.getText(range.index, 1);
                log('User cursor is at '+ range.index + ' idx before "'+text+'"');

            } else {
                var text = quill.getText(range.index, range.length);
                log('User cursor is at ' + range.index + ' with length '+ range.length + ' and selected text "'+text+'"');
            }

            refreshTemplatesAfterSelectionChange();
            updateTemplatesAfterSelectionChanged(range, oldRange, source)

        } else {
            log('Cursor not in the editor');
        }
        if (!range && oldRange) {
            //blur occured
            //quill.setSelection(oldRange);
        }   
    });

    quill.on('text-change', function (delta, oldDelta, source) {
        updateAllSizeAndPositions();
        if (!IsLoaded) {
            return;
        }
        if (IgnoreNextTextChange) {
            IgnoreNextTextChange = false;
            return;
        }
        let srange = quill.getSelection();
        if (!srange) {
            return;
        }

        formatContentChange(delta, oldDelta, source);

        updateTemplatesAfterTextChanged(delta, oldDelta, source);
    });

    initContent(reqMsg.itemEncodedHtmlData);

    initTemplates(reqMsg.usedTextTemplates, reqMsg.isPasteRequest);

    initDragDrop();

    initClipboard();

    refreshFontSizePicker();
    refreshFontFamilyPicker();
}

function registerToolbar(envName) {
    let sizes = registerFontSizes();
    let fonts = registerFontFamilys(envName);

    var toolbar = {
        container: [
            //[{ 'size': ['small', false, 'large', 'huge'] }],  // custom dropdown
            [{ 'size': sizes }],               // font sizes
            [{ 'font': fonts.whitelist }],
            ['bold', 'italic', 'underline', 'strike'],        // toggled buttons
            ['blockquote', 'code-block'],

            // [{ 'header': 1 }, { 'header': 2 }],               // custom button values
            [{ 'list': 'ordered' }, { 'list': 'bullet' }, {'list': 'check'}],
            [{ 'script': 'sub' }, { 'script': 'super' }],      // superscript/subscript
            [{ 'indent': '-1' }, { 'indent': '+1' }],          // outdent/indent
            [{ 'direction': 'rtl' }],                         // text direction

            // [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
            ['link', 'image', 'video', 'formula'],
            [{ 'color': [] }, { 'background': [] }],          // dropdown with defaults from theme
            [{ 'align': [] }],
            // ['clean'],                                         // remove formatting button
            // ['templatebutton'],
            [{ 'Table-Input': registerTables() }]
        ],
        handlers: {
            'Table-Input': () => { return; }
        }
    };

    return toolbar;
}

function setText(text) {
    quill.setText(text + '\n');
}

function setHtml(html) {
    quill.root.innerHTML = html;
}

function setContents(jsonStr) {
    quill.setContents(JSON.parse(jsonStr));
}

function getText() {
    //var text = quill.getText(0, quill.getLength() - 1);
    //return text;
    var text = quill.root.innerText;
    return text;
}

function getSelectedText() {
    var selection = quill.getSelection();
    return quill.getText(selection.index, selection.length);
}

function getHtml() {
    //document.getElementsByClassName
    //var val = document.getElementsByClassName("ql-editor")[0].innerHTML;
    clearTemplateFocus();
    var val = quill.root.innerHTML;
    return unescape(val);
}

function getEncodedHtml() {
    resetTemplates();
    var result = encodeTemplates();
    return result;
}

function getSelectedHtml(maxLength) {
    maxLength = maxLength == null ? Number.MAX_SAFE_INTEGER : maxLength;

    var selection = quill.getSelection();
    if (selection == null) {
        return '';
    }
    if (!selection.hasOwnProperty('length') || selection.length == 0) {
        selection.length = 1;
    }
    if (selection.length > maxLength) {
        selection.length = maxLength;
    }
    var selectedContent = quill.getContents(selection.index, selection.length);
    var tempContainer = document.createElement('div')
    var tempQuill = new Quill(tempContainer);

    tempQuill.setContents(selectedContent);
    let result = tempContainer.querySelector('.ql-editor').innerHTML;
    tempContainer.remove();
    return result;
}

function getSelectedHtml2() {
    var selection = window.getSelection();
    if (selection.rangeCount > 0) {
        var range = selection.getRangeAt(0);
        var docFrag = range.cloneContents();

        let docFragStr = domSerializer.serializeToString(docFrag);

        const xmlnAttribute = ' xmlns="http://www.w3.org/1999/xhtml"';
        const regEx = new RegExp(xmlnAttribute, 'g');
        docFragStr = docFragStr.replace(regEx, '');
        return docFragStr;
    }
    return '';
}

function createLink() {
    var range = quill.getSelection(true);
    if (range) {
        var text = quill.getText(range.index, range.length);
        quill.deleteText(range.index, range.length);
        var ts = '<a class="square_btn" href="https://www.google.com">' + text + '</a>';
        quill.clipboard.dangerouslyPasteHTML(range.index, ts);

        log('text:\n' + getText());
        console.table('\nhtml:\n' + getHtml());
    }
}


function getEditorIndexFromPoint(p) {
    let closestIdx = -1;
    let closestDist = Number.MAX_SAFE_INTEGER;
    if (!p) {
        return closestIdx;
    }

    let editorRect = document.getElementById('editor').getBoundingClientRect();
    let erect = { x: 0, y: 0, w: editorRect.width, h: editorRect.height };

    let ex = p.x - editorRect.left; //x position within the element.
    let ey = p.y - editorRect.top;  //y position within the element.
    let ep = { x: ex, y: ey };
    //log('editor pos: ' + ep.x + ' '+ep.y);
    if (!isPointInRect(erect, ep)) {
        return closestIdx;
    }

    for (var i = 0; i < quill.getLength(); i++) {
        let irect = quill.getBounds(i, 1);
        let ix = irect.left;
        let iy = irect.top + (irect.height / 2);
        let ip = { x: ix, y: iy };
        let idist = distSqr(ip, ep);
        if (idist < closestDist) {
            closestDist = idist;
            closestIdx = i;
        }
    }

    return closestIdx;
}

function getEditorIndexFromPoint2(p) {
    if (!p) {
        return -1;
    }

    let editorRect = document.getElementById('editor').getBoundingClientRect();
    let erect = { x: 0, y: 0, w: editorRect.width, h: editorRect.height };

    let ex = p.x - editorRect.left; //x position within the element.
    let ey = p.y - editorRect.top;  //y position within the element.
    let ep = { x: ex, y: ey };
    //log('editor pos: ' + ep.x + ' '+ep.y);
    if (!isPointInRect(erect, ep)) {
        return -1;
    }

    let closestLineIdx = -1;
    let closestLineDist = Number.MAX_SAFE_INTEGER;
    let docLines = quill.getLines(0, quill.getLength());

    for (var i = 0; i < docLines.length; i++) {
        let l = docLines[i];
        let lrect = quill.getBounds(quill.getIndex(l));
        let lineY = lrect.top + (lrect.height / 2);
        let curYDist = Math.abs(lineY - ey);
        if (curYDist < closestLineDist) {
            closestLineIdx = i;
            closestLineDist = curYDist;
        }
    }
    if (closestLineIdx < 0) {
        return -1;
    }

    log('closest line idx: ' + closestLineIdx);

    let lineMinDocIdx = quill.getIndex(docLines[closestLineIdx]);
    let nextLineMinDocIdx = quill.getLength();
    if (closestLineIdx < docLines.length - 1) {
        nextLineMinDocIdx = quill.getIndex(docLines[closestLineIdx + 1]);
    }

    let closestIdx = -1;
    let closestDist = Number.MAX_SAFE_INTEGER;
    for (var i = lineMinDocIdx; i < nextLineMinDocIdx; i++) {
        let irect = quill.getBounds(i, 1);
        let ix = irect.left;
        let idist = Math.abs(ix - ex);
        if (idist < closestDist) {
            closestDist = idist;
            closestIdx = i;
        }
    }

    return closestIdx;
}

function getElementAtIdx(docIdx) {
    let leafNode = quill.getLeaf(docIdx)[0].domNode;
    let leafElementNode = leafNode.nodeType == 3 ? leafNode.parentElement : leafNode;
    return leafElementNode;
}

function IsReadOnly() {
    var isEditable = parseBool($('.ql-editor').attr('contenteditable'));
    return !isEditable;
}

function enableReadOnly() {
    deleteJsComAdapter();

    $('.ql-editor').attr('contenteditable', false);
    $('.ql-editor').css('caret-color', 'transparent');

    hideEditorToolbar();

    scrollToHome();
    hideScrollbars();

    updateAllSizeAndPositions();

    //return 'MpQuillResponseMessage'  updated master collection of templates
    let qrmObj = {
        itemEncodedHtmlData: getEncodedHtml(),
        userDeletedTemplateGuids: userDeletedTemplateGuids,
        updatedAllAvailableTextTemplates: getAvailableTemplateDefinitions()
    };
    let qrmJsonStr = JSON.stringify(qrmObj);

    log('enableReadOnly() response msg:');
    log(qrmJsonStr);

    return btoa(qrmJsonStr);
}

function disableReadOnly(disableReadOnlyReqStr) {
    bindJsComAdapter();

    let disableReadOnlyMsg = null;

    if (disableReadOnlyReqStr == null) {
        disableReadOnlyMsg = {
            allAvailableTextTemplates: [],
            editorHeight: window.visualViewport.height
        };
    } else {
        let disableReadOnlyReqStr_decoded = atob(disableReadOnlyReqStr);
        disableReadOnlyMsg = JSON.parse(disableReadOnlyReqStr_decoded);
    }

    availableTemplates = disableReadOnlyMsg.allAvailableTextTemplates;
    //document.body.style.height = disableReadOnlyMsg.editorHeight;

    showEditorToolbar();
    showScrollbars();

    updateAllSizeAndPositions();

    $('.ql-editor').attr('contenteditable', true);
    $('.ql-editor').css('caret-color', 'black');
    //$('.ql-editor').css('min-width', getEditorToolbarWidth());
    //$('.ql-editor').css('min-height', disableReadOnlyMsg.editorHeight);
    //document.getElementById('editor').style.minHeight = disableReadOnlyMsg.editorHeight - getEditorToolbarHeight() + 'px';
    //$('.ql-editor').css('width', DefaultEditorWidth);
    //document.body.style.minHeight = disableReadOnlyMsg.editorHeight;

    updateAllSizeAndPositions();

    let droMsgObj = { editorWidth: DefaultEditorWidth };
    let droMsgJsonStr = JSON.stringify(droMsgObj);


    log('disableReadOnly() response msg:');
    log(droMsgJsonStr);

    return btoa(droMsgJsonStr);
}

function isShowingEditorToolbar() {
    $(".ql-toolbar").css("display") != 'none';
}

function hideEditorToolbar() {
    if (EnvName == 'web') {
        document.getElementsByClassName('ql-toolbar')[0].classList.remove('ql-toolbar-env-web');
    } else {
        document.getElementsByClassName('ql-toolbar')[0].classList.remove('ql-toolbar-env-wpf');
    }
    document.getElementsByClassName('ql-toolbar')[0].classList.add('hidden');
    updateAllSizeAndPositions();
}

function showEditorToolbar() {
    document.getElementsByClassName('ql-toolbar')[0].classList.remove('hidden');
    if (EnvName == 'web') {
        document.getElementsByClassName('ql-toolbar')[0].classList.add('ql-toolbar-env-web');
    } else {
        document.getElementsByClassName('ql-toolbar')[0].classList.add('ql-toolbar-env-wpf');
    }
    updateAllSizeAndPositions();
}

function getEditorWidth() {
    var editorRect = document.getElementById('editor').getBoundingClientRect();
    //var editorHeight = parseInt($('.ql-editor').wi());
    return editorRect.width;
}

function getEditorHeight() {
    var editorRect = document.getElementById('editor').getBoundingClientRect();
    //var editorHeight = parseInt($('.ql-editor').outerHeight());
    return editorRect.height;
}

function getEditorToolbarWidth() {
    if (IsReadOnly()) {
        return 0;
    }
    return document.getElementsByClassName('ql-toolbar')[0].getBoundingClientRect().width;
}

function getEditorToolbarHeight() {
    if (IsReadOnly()) {
        return 0;
    }
    var toolbarHeight = parseInt($('.ql-toolbar').outerHeight());
    return toolbarHeight;
}

function scrollToHome() {
    document.getElementById('editor').scrollTop = 0;
}
