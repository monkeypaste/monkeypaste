// #region Globals

// #endregion Globals

// #region Life Cycle

function initClipboard() {
    startClipboardHandler();
    initAllMatchers();
}

function initAllMatchers() {
    initLineBreakMatcher();
    initWhitespaceMatcher();
    initPreSwapMatcher();
    //initHeaderConverterMatcherMatcher();

    //if (isPlainHtmlConverter()) {
    //    initFontColorMatcher();
    //    return;
    //}
    initSpecialCharacterMatcher();
    initFontColorMatcher();
    initLinkMatcher();
    initFontFamilyMatcher();
    initFontSizeMatcher();
    initCheckableListMatcher();
    initTemplateMatcher();
}

function initTemplateMatcher() {
    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    let Delta = Quill.imports.delta;

    globals.quill.clipboard.addMatcher('span', function (node, delta) {
        if (node.hasAttribute('templateguid')) {
            delta.ops[0].attributes = delta.ops[0].insert.template;
            //delete delta.ops[0].insert.template;
            //delta.ops[0].insert = '';
        }
        return delta;
    });
}
function initPreSwapMatcher() {
    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    let Delta = Quill.imports.delta;

    globals.quill.clipboard.addMatcher('pre', function (node, delta) {

        if (node.hasAttribute('templateguid')) {
            delta.ops[0].attributes = delta.ops[0].insert.template;
            //delete delta.ops[0].insert.template;
            //delta.ops[0].insert = '';
        }
        return delta;
    });
}

function initSpecialCharacterMatcher() {
    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    let Delta = Quill.imports.delta;

    globals.quill.clipboard.addMatcher(Node.TEXT_NODE, function (node, delta) {
        if (node.parentNode.tagName != 'CODE') {
            return delta;
        }
        // this fixes whitespace issues

        return new Delta().insert(decodeHtmlSpecialEntities(node.data));
    });    
}

function initWhitespaceMatcher() {
    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    let Delta = Quill.imports.delta;

    globals.quill.clipboard.addMatcher(Node.TEXT_NODE, function (node, delta) {
        // this fixes whitespace issues 
        if(node.data.match(/[^\n\S]|\t/)) {
            return new Delta().insert(node.data);
        }
        return delta;
    });    
}

function initHeaderConverterMatcherMatcher() {
    // BUG header blots don't use font sizes and full line blots so converting to big spans
    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    let Delta = Quill.imports.delta;
    let headers = ['h1', 'h2', 'h3', 'h4', 'h5', 'h6'];
    let sizes = ['42px','32px','32px','24px','24px','20px']
    globals.quill.clipboard.addMatcher(Node.ELEMENT_NODE, function (node, delta) {
        // this fixes whitespace issues 
        let h_idx = headers.indexOf(node.tagName.toLowerCase())
        if (h_idx < 0) {
            return delta;
        }
        delta.forEach((op) => {
            if (isNullOrUndefined(op.attributes)) {
                op.attributes = {};
            }
            op.attributes.size = sizes[h_idx];
        });
        return delta;
    });    
}

function initLineBreakMatcher() {
    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    let Delta = Quill.imports.delta;

    globals.quill.clipboard.addMatcher(Node.TEXT_NODE, function (node, delta) {
        if (node.data.includes(`\n`) ||
            node.data.includes(`\r\n`)) {

            let fixed_breaks = node.data.replace(`\r\n`, `\n`);
            //debugger;
            return new Delta().insert(fixed_breaks);
        }
        return delta;
    });    
}

function initCheckableListMatcher() {
    // NOTE! quill renders all li's with data-list attr (bullet|ordered|checked|unchecked)
    // delta-html converter clears ordered and bullet li's attrs and encloses in ol|ul respectively
    // delta-html converter substitutes li's w/ data-list attr (checked|unchecked) w/ data-checked attr (true|false)

    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    let Delta = Quill.imports.delta;

    globals.quill.clipboard.addMatcher('li', function (node, delta) {
        if (node.hasAttribute('data-checked')) {
            let is_checked = parseBool(node.getAttribute('data-checked'));
            if (delta && delta.ops !== undefined && delta.ops.length > 0) {
                for (var i = 0; i < delta.ops.length; i++) {
                    if (delta.ops[i].insert === undefined) {
                        continue;
                    }
                    if (delta.ops[i].attributes === undefined) {
                        delta.ops[i].attributes = {};
                    }
                    delta.ops[i].attributes.list = is_checked ? 'checked' : 'unchecked';

                }
            }
        }
        return delta;
    });
}
function initFontColorMatcher() {
    globals.quill.clipboard.addMatcher(Node.ELEMENT_NODE, function (node, delta) {
        let result = applyThemeToDelta(node,delta);
        return result;
    });
}
function initFontSizeMatcher() {
    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    let Delta = Quill.imports.delta;

    globals.quill.clipboard.addMatcher(Node.ELEMENT_NODE, function (node, delta) {
        let fs_class = Array.from(node.classList).find(x => x.startsWith('ql-size'));
        if (!fs_class) {
            return delta;
        }
        let size_val = fs_class.replace('ql-size-', '');
        if (delta && delta.ops !== undefined && delta.ops.length > 0) {
            for (var i = 0; i < delta.ops.length; i++) {
                if (delta.ops[i].insert === undefined) {
                    continue;
                }
                if (delta.ops[i].attributes === undefined) {
                    delta.ops[i].attributes = {};
                }
                delta.ops[i].attributes.size = size_val;

            }
        }
        return delta;
    });
}


function initFontFamilyMatcher() {
    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    let Delta = Quill.imports.delta;

    globals.quill.clipboard.addMatcher(Node.ELEMENT_NODE, function (node, delta) {
        let ff_class = Array.from(node.classList).find(x => x.startsWith('ql-font-'));
        if (!ff_class) {
            return delta;
        }
        let ff_val = ff_class.replace('ql-font-', '');
        if (delta && delta.ops !== undefined && delta.ops.length > 0) {
            for (var i = 0; i < delta.ops.length; i++) {
                if (delta.ops[i].insert === undefined) {
                    continue;
                }
                if (delta.ops[i].attributes === undefined) {
                    delta.ops[i].attributes = {};
                }
                delta.ops[i].attributes.font = ff_val;

            }
        }
        return delta;
    });
}

function initLinkMatcher() {
    // NOTE! quill renders all li's with data-list attr (bullet|ordered|checked|unchecked)
    // delta-html converter clears ordered and bullet li's attrs and encloses in ol|ul respectively
    // delta-html converter substitutes li's w/ data-list attr (checked|unchecked) w/ data-checked attr (true|false)

    if (Quill === undefined) {
        /// host load error case
        debugger;
    }
    let Delta = Quill.imports.delta;

    globals.quill.clipboard.addMatcher('a', function (node, delta) {
        if (node.hasAttribute('style')) {
            let bg = getElementComputedStyleProp(node, 'background-color');
            if (bg) {
                bg = cleanHexColor(bg, 1, true);
            }
            let fg = getElementComputedStyleProp(node, 'color');
            if (fg) {
                fg = cleanHexColor(fg, 1, true);
            }

            //log('link text: ' + node.innerText + ' bg: ' + bg + ' fg: ' + fg);
            if (delta && delta.ops !== undefined && delta.ops.length > 0) {
                for (var i = 0; i < delta.ops.length; i++) {
                    if (delta.ops[i].insert === undefined) {
                        continue;
                    }
                    if (delta.ops[i].attributes === undefined) {
                        delta.ops[i].attributes = {};
                    }
                    if (bg) {
                        delta.ops[i].attributes.color = bg;
                    }

                    if (fg) {
                        delta.ops[i].attributes.color = fg;
                    }

                }
            }
        }
        let link_type = Array.from(node.classList).find(x => globals.LinkTypes.includes(x));
        if (link_type) {
            //log('link class type: ' + link_type);

            if (delta && delta.ops !== undefined && delta.ops.length > 0) {
                for (var i = 0; i < delta.ops.length; i++) {
                    if (delta.ops[i].insert === undefined) {
                        continue;
                    }
                    if (delta.ops[i].attributes === undefined) {
                        delta.ops[i].attributes = {};
                    }
                    delta.ops[i].attributes.linkType = link_type;
                    if (link_type == 'hexcolor') {
                        delta.ops[i].attributes.background = node.innerText;
                        delta.ops[i].attributes.color = isBright(node.innerText) ? 'black' : 'white';
                    }
                }
            }
            globals.LinkTypeAttrb.add(node, link_type);
        } else {
            //log('no type class for link, classes: ' + node.getAttribute('class'));
        }
        return delta;
    });
}

// #endregion Life Cycle

// #region Getters
function getClipboardEnabledElements() {
    return [
        getEditorElement(),
        ...document.querySelectorAll('textarea')
    ];
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State
function isHtmlFormat(lwc_format) {
    const result =
        lwc_format == 'html format' ||
        lwc_format == 'text/html';
    return result;
}

function isUri(lwc_format) {
    const result =
        lwc_format == 'html format' ||
        lwc_format == 'text/html';
    return result;
}

function isPlainTextFormat(lwc_format) {
    const result =
        lwc_format == 'text' ||
        lwc_format == 'unicode' ||
        lwc_format == 'oemtext' ||
        lwc_format == 'text/plain';

    return result;
}
function isCsvFormat(lwc_format) {
    const result =
        lwc_format == 'csv' ||
        lwc_format == 'text/csv';
    return result;
}

function isImageFormat(lwc_format) {
    const result = 
        lwc_format == 'png' ||
        lwc_format == 'bitmap' ||
        lwc_format == 'deviceindependentbitmap' ||
        lwc_format.startsWith('image/');

    return result;
}

function isFileListFormat(lwc_format) {
    // NOTE files aren't in dataTransfer.items so no mime type equivalent
    const result =
        lwc_format == 'files';
    return result;
}

function isInternalClipTileFormat(lwc_format) {
    const result =
        lwc_format == "mp internal content";
    return result;
}

// #endregion State

// #region Actions

function startClipboardHandler() {
    getClipboardEnabledElements().forEach(x => {
        x.addEventListener('paste', onPaste, true);
        x.addEventListener('cut', onCut, true);
        x.addEventListener('copy', onCopy, true);
    });
}

function stopClipboardHandler() {
    getClipboardEnabledElements().forEach(x => {
        x.removeEventListener('paste', onPaste);
        x.removeEventListener('cut', onCut);
        x.removeEventListener('copy', onCopy);
    });
}


// #endregion Actions

// #region Event Handlers

function onCut(e) {
    onSetClipboardRequested_ntf();
    let sel = getDocSelection();
    if (globals.ContentItemType == 'Text') {
        setTextInRange(sel, '');
    } else {
        // sub-selection ignored for other types
    }
    e.preventDefault();
    return true;
}
function onCopy(e) {
    onSetClipboardRequested_ntf();
    e.preventDefault();
}

function onPaste(e) {
    if (!isRunningOnHost()) {
        let test = e.clipboardData.getData('text/plain');
        let test2 = e.clipboardData.getData('text/html');
        e.handled = false;
        return;
    }
    // NOTE if cut/copy was internal and all supported formats set,
    // the e.clipboardData obj strips everything but files from the transfer 
    // so this makes a get request and gets back current clipboard asynchronously
    e.preventDefault();
    e.stopPropagation();

    var cur_paste_sel = getDocSelection();

    getClipboardDataTransferObjectAsync_get()
        .then((result) => {
            performDataTransferOnContent(result, null, cur_paste_sel,'api','Pasted');
    });
}

function onManualClipboardKeyDown(e) {
    if (isEditorToolbarVisible() || !isContentEditable()) {
        // these events shouldn't be enabled in edit mode
        //debugger;
        return;
    }
    if (e.ctrlKey && e.key === 'z') {
        // undo
        undoManualClipboardAction();
        return;
    }
    if (e.ctrlKey && e.key === 'y') {
        // redo
        redoManualClipboardAction();
        return;
    }
}
// #endregion Event Handlers