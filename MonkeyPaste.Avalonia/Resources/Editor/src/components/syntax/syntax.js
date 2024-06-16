// #region Life Cycle
function initSyntax() {
    setSelectedSyntaxTheme(globals.SelectedSyntaxTheme);
}
// #endregion Life Cycle

// #region Getters

function getAllCodeBlocks() {
    return Array.from(document.querySelectorAll('pre.ql-code-block-container'));
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
// #endregion Setters

// #region State

// #endregion State

// #region Actions

function highlightSyntaxDelayed(delayMs = 500) {
    var sup_guid = suppressTextChanged();
    let pre_elms = getAllCodeBlocks();
    for (var i = 0; i < pre_elms.length; i++) {
        let pre_elm = pre_elms[i];
        let pre_doc_range = getElementDocRange(pre_elm);
        // clear code block
        formatDocRange(pre_doc_range, { 'code-block': false });
    }
    sleep(delayMs)
        .then(() => {
            for (var i = 0; i < pre_elms.length; i++) {
                let pre_elm = pre_elms[i];
                let pre_doc_range = getElementDocRange(pre_elm);
                // reformat code block
                formatDocRange(pre_doc_range, { 'code-block': 'plain' });
            }

            unsupressTextChanged(sup_guid);
        });    
}

function highlightSyntax() {
    // BUG trying to restore previously highlighted html doesn't work,
    // spans get bunched together. i think its from the hljs classes on the elements...
    // but clearing code blocks then restoring code blocks works...

    var sup_guid = suppressTextChanged();
    let pre_elms = getAllCodeBlocks();
    for (var i = 0; i < pre_elms.length; i++) {
        let pre_elm = pre_elms[i];
        let pre_doc_range = getElementDocRange(pre_elm);
        // clear code block
        formatDocRange(pre_doc_range, { 'code-block': false });
        // reformat code block
        formatDocRange(pre_doc_range, { 'code-block': 'plain' });
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
// #endregion Event Handlers