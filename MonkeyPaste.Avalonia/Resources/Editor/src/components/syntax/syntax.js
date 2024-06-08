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
function initSyntax() {
    globals.SyntaxThemes = getSyntaxThemes();
    globals.SyntaxThemeClassAttrb = registerClassAttributor('syntaxTheme', globals.SYNTAX_THEME_PREFIX, globals.Parchment.Scope.ANY);
}



// #region Life Cycle

// #endregion Life Cycle

// #region Getters

function getAllCodeBlocks() {
    return Array.from(document.querySelectorAll('pre.ql-code-block-container'));
}
function getSyntaxThemes() {
    return [
        "a11y-dark",
        "a11y-light",
        "agate",
        "an-old-hope",
        "androidstudio",
        "arduino-light",
        "arta",
        "ascetic",
        "atom-one-dark-reasonable",
        "atom-one-dark",
        "atom-one-light",
        "brown-paper",
        "codepen-embed",
        "color-brewer",
        "dark",
        "default",
        "devibeans",
        "docco",
        "far",
        "felipec",
        "foundation",
        "github-dark-dimmed",
        "github-dark",
        "github",
        "gml",
        "googlecode",
        "gradient-dark",
        "gradient-light",
        "grayscale",
        "hybrid",
        "idea",
        "intellij-light",
        "ir-black",
        "isbl-editor-dark",
        "isbl-editor-light",
        "kimbie-dark",
        "kimbie-light",
        "lightfair",
        "lioshi",
        "magula",
        "mono-blue",
        "monokai-sublime",
        "monokai",
        "night-owl",
        "nnfx-dark",
        "nnfx-light",
        "nord",
        "obsidian",
        "panda-syntax-dark",
        "panda-syntax-light",
        "paraiso-dark",
        "paraiso-light",
        "pojoaque",
        "purebasic",
        "qtcreator-dark",
        "qtcreator-light",
        "rainbow",
        "routeros",
        "school-book",
        "shades-of-purple",
        "srcery",
        "stackoverflow-dark",
        "stackoverflow-light",
        "sunburst",
        "tokyo-night-dark",
        "tokyo-night-light",
        "tomorrow-night-blue",
        "tomorrow-night-bright",
        "vs",
        "vs2015",
        "xcode",
        "xt256",
    ];
}

function getCodeBlockTheme(pre_elm) {
    let theme_type = null;
    let theme_type_vals = Array.from(pre_elm.classList).filter(x => !isNullOrEmpty(x) && x.startsWith(globals.SYNTAX_THEME_PREFIX));
    if (theme_type_vals && theme_type_vals.length > 0) {
        theme_type = theme_type_vals[0].replace(globals.SYNTAX_THEME_PREFIX, '');
    } 
    return theme_type;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function updateSyntaxBlocks(fromTextChanged) {
    let code_blocks = getAllCodeBlocks();
    for (var i = 0; i < code_blocks.length; i++) {
        let pre_elm = code_blocks[i];

        let theme_type = getCodeBlockTheme(pre_elm);
        let theme_type_vals = Array.from(pre_elm.classList).filter(x => !isNullOrEmpty(x) && x.startsWith(globals.SYNTAX_THEME_PREFIX));
        if (!theme_type) {
            globals.SyntaxThemeClassAttrb.add(pre_elm, globals.DefaultSyntaxTheme);
        }
        updateThemePicker(pre_elm, theme_type);
	}
}
function updateThemePicker(preElm, selThemeType) {
    let theme_picker = preElm.querySelector('select.syntax-theme-picker');
    if (isNullOrUndefined(theme_picker)) {
        theme_picker = document.createElement('select');
        for (var i = 0; i < globals.SyntaxThemes.length; i++) {
            let cur_theme_name = globals.SyntaxThemes[i];
            let theme_opt_elm = document.createElement('option');
            theme_opt_elm.classList.add('ql-ui');
            theme_opt_elm.classList.add('syntax-theme-picker');
            theme_opt_elm.setAttribute('value', cur_theme_name);
            theme_opt_elm.innerHTML = cur_theme_name;
            theme_opt_elm.onchange = onThemePickerChanged;
            theme_picker.append(theme_opt_elm);
        }
        preElm.append(theme_picker);
    }
    theme_picker.value = selThemeType;
}
function prevSyntaxTheme(preElm) {
    let idx = globals.SyntaxThemes.indexOf(getCodeBlockTheme(preElm));
    idx--;
    if (idx < 0) {
        idx = globals.SyntaxThemes.length - 1
    }
    changeSyntaxTheme(preElm,idx);
}

function nextSyntaxTheme(preElm) {
    let idx = globals.SyntaxThemes.indexOf(getCodeBlockTheme(preElm));
    idx++;
    if (idx >= globals.SyntaxThemes.length) {
        idx = 0;
    }
    changeSyntaxTheme(preElm,idx);
}

function changeSyntaxTheme(idx) {
    let newThemeName = globals.SyntaxThemes[idx];
    document.querySelector('link.hljs').remove();
    let link = document.createElement("link");
    link.classList.add('hljs');
    let theme = newThemeName;
    link.rel = "stylesheet";
    link.href = `https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/${theme}.min.css`;
    document.querySelector("head").append(link);

    globals.SelectedSyntaxTheme = newThemeName;
    log('syntax theme changed to: ' + newThemeName);
}
// #endregion Actions

// #region Event Handlers

function onThemePickerChanged(e) {
    debugger;
}

// #endregion Event Handlers