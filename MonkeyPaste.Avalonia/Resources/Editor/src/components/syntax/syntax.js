// #region Life Cycle
function initSyntax() {
    setSelectedSyntaxTheme(globals.SelectedSyntaxTheme);
    initLanguageSelectors();
    initSyntaxDomWatcher();
    let code_tb_btn_elm = getEditorToolbarElement().querySelector('.ql-code-block');
    if (isNullOrUndefined(code_tb_btn_elm)) {
        return;
    }
    addClickOrKeyClickEventListener(code_tb_btn_elm, onCodeBlockToolbarElementClick);
}

function initLanguageSelectors() {
    getAllCodeBlocks().forEach(x => {
        initCodeBlock(x);
    });
}

function initCodeBlock(preElm) {
    let sel_elm = getCodeBlockLanguageSelectorElement(preElm);
    if (isNullOrUndefined(sel_elm)) {
        return;
    }
    sel_elm.addEventListener('change', onLanguageChanged);
}

function initSyntaxDomWatcher() {
    let observer = new MutationObserver(mutations => {
        for (let mutation of mutations) {
            // examine new nodes, there is something to highlight
            for (let node of mutation.addedNodes) {
                if (!(node instanceof HTMLElement)) {
                    continue;
                }
                if (!node.classList.contains('ql-code-block-container') ||
                    node.tagName != 'PRE') {
                    continue;
                }
                initCodeBlock(node);
            }
        }
    });

    observer.observe(document.body, {
        childList: true,
        subtree: true,
        attributes: true,
    });
}
// #endregion Life Cycle

// #region Getters

function getAllCodeBlocks() {
    return Array.from(document.querySelectorAll('pre.ql-code-block-container'));
}

function getCodeBlockLanguageSelectorElement(codeBlockPreElm) {
    if (isNullOrUndefined(codeBlockPreElm)) {
        return null;
    }
    return codeBlockPreElm.querySelector('select.ql-ui');
}

function getCodeBlockLanguage(codeBlockPreElm) {
    if (isNullOrUndefined(codeBlockPreElm)) {
        return;
    }
    return codeBlockPreElm.querySelector('div').getAttribute('data-language');
}
// #endregion Getters

// #region Setters

function setSelectedSyntaxTheme(themeName) {
    let cur_elm = document.querySelector('link.hljs');
    if (cur_elm) {
        cur_elm.remove();
    }
    
    let link = document.createElement("link");
    link.classList.add('hljs');
    let theme = themeName;
    link.rel = "stylesheet";
    link.href = `${globals.wwwroot}src/components/syntax/hljs-styles/${themeName}.min.css`;
    document.querySelector("head").append(link);

    globals.SelectedSyntaxTheme = themeName;
    log('syntax theme changed to: ' + themeName);
}

function setCodeBlockLanguage(pre_elm, lang, reset, source = 'api') {
    let pre_doc_range = getElementDocRange(pre_elm);
    if (isNullOrUndefined(pre_doc_range)) {
        debugger;
        return;
    }
    if (reset) {
        formatDocRange(pre_doc_range, { 'code-block': false }, source);
    }
    formatDocRange(pre_doc_range, { 'code-block': lang }, source);
}
// #endregion Setters

// #region State

// #endregion State

// #region Actions

function highlightSyntax() {
    // BUG trying to restore previously highlighted html doesn't work,
    // spans get bunched together. i think its from the hljs classes on the elements...
    // but clearing code blocks then restoring code blocks works...

    var sup_guid = suppressTextChanged();
    let pre_elms = getAllCodeBlocks();
    for (var i = 0; i < pre_elms.length; i++) {
        let pre_elm = pre_elms[i];
        let lang = getCodeBlockLanguage(pre_elm);
        setCodeBlockLanguage(pre_elm, lang, true);
    }

    unsupressTextChanged(sup_guid);
}
function forceSyntax() {
    globals.quill.getModule('syntax').highlight(null, true);
}
function prevSyntaxTheme(preElm) {
    let idx = globals.SyntaxThemes.indexOf(globals.SelectedSyntaxTheme);
    idx--;
    if (idx < 0) {
        idx = globals.SyntaxThemes.length - 1
    }
    changeSyntaxTheme(idx);
}

function nextSyntaxTheme(preElm) {
    let idx = globals.SyntaxThemes.indexOf(globals.SelectedSyntaxTheme);
    idx++;
    if (idx >= globals.SyntaxThemes.length) {
        idx = 0;
    }
    changeSyntaxTheme(idx);
}

function changeSyntaxTheme(idx) {
    let newThemeName = globals.SyntaxThemes[idx];
    setSelectedSyntaxTheme(newThemeName);
}

function testHl() {
    const Delta = Quill.import('delta');
    globals.quill.setContents(
        new Delta()
            .insert('const language = "JavaScript";')
            .insert('\n', { 'code-block': 'javascript' })
            .insert('console.log("I love " + language + "!");')
            .insert('\n', { 'code-block': 'javascript' })
    );
}
// #endregion Actions

// #region Event Handlers
function onLanguageChanged(e) {
    let cur_lang = e.currentTarget.value;
    let code_block_elm = e.currentTarget.parentNode;
    let code_block_elm_idx = getAllCodeBlocks().indexOf(code_block_elm);
    log('code block #' + code_block_elm_idx + ' lang changed to: ' + cur_lang);
    setCodeBlockLanguage(code_block_elm, cur_lang, 'user');
}

function onCodeBlockToolbarElementClick(e) {
    initLanguageSelectors();
}
// #endregion Event Handlers