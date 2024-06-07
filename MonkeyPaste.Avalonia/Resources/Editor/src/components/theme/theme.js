// #region Life Cycle

function initTheme() {
    // THEME NOTES
    // 1. nothing but original colors and user specified colors are stored in db
    // 2. in load font color matcher swaps out original colors w/ theme adj colors
    //    and stores orig in respective attribute
    // 3. any access to html (getHtml) will show original/user altered colored html
    initThemeAttributes();
}

function initThemeAttributes() {
    globals.ThemeColorOverrideAttrb = registerPlainAttributor('themecoloroverride', 'themecoloroverride', globals.Parchment.Scope.INLINE);
    globals.ThemeBgColorOverrideAttrb = registerPlainAttributor('themebgcoloroverride', 'themebgcoloroverride', globals.Parchment.Scope.INLINE);

}
// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

function setAttrThemeColorVal(attributes, colorVal, isBg) {
    if (!globals.IsThemeEnabled) {
        return attributes;
    }
    // store original color in theme attributes
    let theme_attr = isBg ? 'themebgcoloroverride' : 'themecoloroverride';
    let color_type_attr = isBg ? 'background' : 'color';
    attributes[theme_attr] = attributes[color_type_attr];
    attributes[color_type_attr] = colorVal;
    return attributes;
}

function unsetAttrThemeColorVal(attributes, isBg) {
    if (!globals.IsThemeEnabled) {
        return attributes;
    }
    // store original color in theme attributes
    let theme_attr = isBg ? 'themebgcoloroverride' : 'themecoloroverride';
    let color_type_attr = isBg ? 'background' : 'color';
    if (attributes[theme_attr] === undefined) {
        return attributes;
    }
    attributes[color_type_attr] = attributes[theme_attr];
    delete attributes[theme_attr];
    return attributes;
}

// #endregion Setters

// #region State

function isThemeColorOverriden(attr) {
    if (!attr || attr['themecoloroverride'] === undefined) {
        return false;
    }
    return true;
}
function isThemeBgColorOverriden(attr) {
    if (!attr || attr['themebgcoloroverride'] === undefined) {
        return false;
    }
    return true;
}
function isAnyThemeOverriden(attr) {
    if (isThemeColorOverriden(attr) || isThemeBgColorOverriden(attr)) {
        return true;
    }
    return false;
}
// #endregion State

// #region Actions

function restoreContentColorsFromDelta(delta) {
    if (!globals.IsThemeEnabled) {
        return delta;
    }
    if (!delta || !Array.isArray(delta.ops)) {
        return delta;
    }
    for (var i = 0; i < delta.ops.length; i++) {
        delta.ops[i] = restoreContentColorsFromOp(delta.ops[i]);
    }
    return delta;
}

function restoreContentColorsFromOp(op) {
    if (!globals.IsThemeEnabled) {
        return op;
    }
    if (!op || op.attributes === undefined) {
        return op;
    }

    if (isThemeBgColorOverriden(op.attributes)) {
        op.attributes = unsetAttrThemeColorVal(op.attributes, true);
    } 
    if (isThemeColorOverriden(op.attributes)) {
        op.attributes = unsetAttrThemeColorVal(op.attributes, false);
    } 
    return op;
}
function removeThemeAttrFromDocRange(range, isBg = false, source = 'api') {
    let range_format = getFormatForDocRange(range);

    let theme_prop = isBg ? 'themebgcoloroverride' : 'themecoloroverride';
    range_format[theme_prop] = false;

    formatDocRange(range, range_format, source);
    let test = getFormatForDocRange(range);
    return;
}

function adjustDeltaOpForTheme(op) {
    if (!globals.IsThemeEnabled) {
        return op;
    }
    // NOTE based on theme fg/bg this adjusts non-user defined fg/bg colors
    // to theme, when that happens the original color is stored in the an attr

    if (!op || !isString(op.insert)) {
        return op;
    }
    if (op.attributes === undefined) {
        op.attributes = {};
    }
    if (op.attributes.link) {
        // remove ops
        let link_fg = getElementComputedStyleProp(document.body, '--linkcolor');
        setAttrThemeColorVal(op.attributes, link_fg, false);
        delete op.attributes.background;
        return op;
    }
    if (op.attributes.color === undefined) {
        // op has text but no color info
        // so set based on theme
        let theme_fg = getElementComputedStyleProp(document.body, '--defcontentfgcolor');
        op.attributes = setAttrThemeColorVal(op.attributes, theme_fg, false);
    } else if (!hasUserFontColor(op.attributes) && !isThemeColorOverriden(op.attributes)) {
        try {
            // when fg color is provided from content and is NOT user specified
            let theme_fg = adjustFgToTheme(op.attributes.color);
            op.attributes = setAttrThemeColorVal(op.attributes, theme_fg, false);
        } catch (ex) {
            // error parsing color, use default
            log('fg conv error: ' + ex);
            let theme_fg = getElementComputedStyleProp(document.body, '--defcontentfgcolor');
            op.attributes = setAttrThemeColorVal(op.attributes, theme_fg, false);
        }
    }

    if (!hasUserBgFontColor(op.attributes) &&
        !isThemeBgColorOverriden(op.attributes) &&
        op.attributes.background !== undefined) {
        try {
            // when bg color is provided from content and is NOT user specified
            let theme_bg = adjustBgToTheme(op.attributes.background);
            op.attributes = setAttrThemeColorVal(op.attributes, theme_bg, true);
        } catch (ex) {
            // error parsing bg color, set to transparent
            log('bg conv error: ' + ex);
            op.attributes = setAttrThemeColorVal(op.attributes, 'transparent', true);
        }
    }
    return op;
}

function adjustBgToTheme(bg_color_obj) {
    // NOTE l changes are just experimental atm, bg is uniformly set to transparent
    // (but this function is skipped if bg is user specified in editor)
    let rgba = cleanColor(bg_color_obj);
    if (!rgba) {
        throw new DOMException('THeme bg conv. error cannot convert: ' + bg_color_obj);
    }
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

function adjustFgToTheme(fg_color_obj) {
    let rgba = cleanColor(fg_color_obj);

    if (!rgba) {
        throw new DOMException('THeme fg conv. error cannot convert: ' + fg_color_obj);
    }
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

function applyThemeToDelta(delta) {
    if (!globals.IsThemeEnabled) {
        return delta;
    }
    if (!delta || delta.ops === undefined || delta.ops.length == 0) {
        return delta;
    }
    for (var i = 0; i < delta.ops.length; i++) {
        delta.ops[i] = adjustDeltaOpForTheme(delta.ops[i]);
    }
    return delta;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers