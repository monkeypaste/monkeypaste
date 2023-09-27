// #region Life Cycle

function initTheme() {
    initThemeClassAttributes();
}
function initThemeClassAttributes() {
    globals.ThemeColorOverrideAttrb = registerClassAttributor('themeColorOverride', 'theme-color-override', globals.Parchment.Scope.INLINE);
    globals.ThemeBgColorOverrideAttrb = registerClassAttributor('themeBgColorOverride', 'theme-bg-color-override', globals.Parchment.Scope.INLINE);

}
// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isThemeColorOverriden(attr) {
    if (!attr) {
        return false;
    }
    if (attr['themeColorOverride'] == 'on') {
        return true;
    }
    return false;
}
function isThemeBgColorOverriden(attr) {
    if (!attr) {
        return false;
    }
    if (attr['themeBgColorOverride'] == 'on') {
        return true;
    }
    return false;
}
// #endregion State

// #region Actions

function omitThemeColorsFromDelta(delta) {
    if (!delta || !Array.isArray(delta.ops)) {
        return delta;
    }
    for (var i = 0; i < delta.ops.length; i++) {
        delta.ops[i] = omitThemeFromOp(delta.ops[i]);
    }
    return delta;
}

function omitThemeFromOp(op) {
    if (!op || op.attributes === undefined) {
        return op;
    }
    if (isThemeBgColorOverriden(op.attributes)) {
        delete op.attributes['themeBgColorOverride'];
        delete op.attributes['background'];
    }
    if (isThemeColorOverriden(op.attributes)) {
        delete op.attributes['themeColorOverride'];
        delete op.attributes['color'];
    }
    if (Object.keys(op.attributes).length == 0) {
        // remove empty attr
        delete op.attributes;
    }
    return op;
}
function removeThemeAttrFromDocRange(range, isBg = false, source = 'api') {
    let range_format = getFormatForDocRange(range);

    let theme_prop = isBg ? 'themeBgColorOverride' : 'themeColorOverride';
    range_format[theme_prop] = 'off';

    formatDocRange(range, range_format, source);
}

function adjustDeltaOpForTheme(op, node) {
    // NOTE based on theme fg/bg this adjusts non-user defined fg/bg colors
    // to theme, when that happens the op gets a class blot put on it
    // so when html is saved, that color is stripped because if theme
    // changes but content doesn't it will appear that content changed
    // and will resave it, which means EVERY item will resave when it loads
    // and makes everything very slow

    if (!op) {
        return op;
    }

    if (op.attributes === undefined ||
        op.attributes.color === undefined) {
        if (op.insert !== undefined) {
            if (op.attributes === undefined) {
                op.attributes = {};
            }
            op.attributes.color = getElementComputedStyleProp(document.body, '--defcontentfgcolor');
            op.attributes['theme-color-override'] = 'on';
        } else {
            return op;
        }
    }

    if (!isFontBgColorOverriden(op.attributes) &&
        op.attributes.background !== undefined) {
        // when bg color is provided from content and is NOT user specified
        let adj_bg = adjustBgToTheme(op.attributes.background, node);
        if (op.attributes.background != adj_bg) {
            op.attributes.background = adj_bg;
            op.attributes['theme-bg-color-override'] = 'on';
        }
    }

    if (!isFontColorOverriden(op.attributes) &&
        op.attributes.color !== undefined) {
        // when fg color is provided from content and is NOT user specified
        let adj_fg = adjustFgToTheme(op.attributes.color, node);
        if (op.attributes.background != adj_fg) {
            op.attributes.color = adj_fg;
            op.attributes['theme-color-override'] = 'on';
        }
    }
    return op;
}

function adjustBgToTheme(rgb_Or_rgba_Or_colorName_Or_hex_Str, elm) {
    // NOTE l changes are just experimental atm, bg is uniformly set to transparent
    // (but this function is skipped if bg is user specified in editor)
    let rgba = cleanColor(rgb_Or_rgba_Or_colorName_Or_hex_Str);
    let hsl = rgb2hsl(rgba);
    if (globals.EditorTheme == 'light') {
        hsl.l = Math.max(hsl.l, 85);
    } else {
        hsl.l = Math.min(hsl.l, 15);
    }
    let adj_rgb = hsl2Rgb(hsl);
    adj_rgb.a = 0;// Math.min(rgba.a, 0.5);

    let css_rgba = rgbaToCssColor(adj_rgb);
    return css_rgba;
}

function adjustFgToTheme(rgb_Or_rgba_Or_colorName_Or_hex_Str, elm) {
    let rgba = cleanColor(rgb_Or_rgba_Or_colorName_Or_hex_Str);
    let hsl = rgb2hsl(rgba);
    if (globals.EditorTheme == 'dark') {
        hsl.l = Math.max(hsl.l == 0 ? 100 : hsl.l, 75);
    } else {
        hsl.l = Math.min(hsl.l == 100 ? 0 : hsl.l, 25);
    }
    let adj_rgb = hsl2Rgb(hsl);
    adj_rgb.a = 1;

    let css_rgba = rgbaToCssColor(adj_rgb);
    return css_rgba;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers