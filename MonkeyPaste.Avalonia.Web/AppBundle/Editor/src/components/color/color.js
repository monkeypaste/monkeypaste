
// #region Globals

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

function getContrastHexColor(chex, contrast_bright = '#000000', contrast_dark = '#FFFFFF') {
    return isBright(chex) ? contrast_bright : contrast_dark;
}

function getHexChannelStrPart(val, multiplier = 1) {
    // BUG workaround when val is 0 only 1 '0' digit is returned for hex
    let strPart = parseInt(val * multiplier).toString(16);
    if (strPart == '0') {
        strPart += '0';
    }
    return strPart;
}

function getRandomColor() {
    // BUG random doesn't work first time so just calling it to get random value
    Math.random();

    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

function getRandomPaletteColor() {
    let color_idx = Math.floor(Math.random() * ContentColors.length - 1);
    while (get2dIdx(color_idx, COLOR_PALETTE_COL_COUNT)[1] == COLOR_PALETTE_COL_COUNT - 1) {
        // ignore last column
        color_idx = Math.floor(Math.random() * ContentColors.length - 1);
    }
    return ContentColors[color_idx];
}

function getColorObjType(color) {
    if (typeof color === 'string' || color instanceof String) {
        if (color.startsWith('#')) {
            return 'hex' + (color.length - 1);
        }
        if (color.startsWith('rgb')) {
            if (color.startsWith('rgba')) {
                return 'rgba';
            }
            return 'rgb';
        }
        return 'named';
    }
    if (Array.isArray(color)) {
        if (isInt(color[0])) {
            return 'byte' + (color.length);
        }
        return 'decimal' + (color.length);
    }
    if (color.r !== undefined) {
        if (color.a === undefined) {
            if (isInt(color.r)) {
                return 'obj_rgb_byte';
            }
            return 'obj_rgb_decimal';
        }
        if (isInt(color.r)) {
            return 'obj_rgba_byte';
        }
        return 'obj_rgba_decimal';
    }
    return null;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isHexColorStr(str) {
    return hexToRgba(str) != null;
}
function isBright(hex_or_color_name_or_rgb_or_rgba, brightThreshold = 150) {
    var rgb = parseRgba(hex_or_color_name_or_rgb_or_rgba);
    if (rgb.a < brightThreshold) {
        return true;
    }
    var grayVal = Math.sqrt(
        rgb.r * rgb.r * .299 +
        rgb.g * rgb.g * .587 +
        rgb.b * rgb.b * .114);
    return grayVal > brightThreshold;
}

// #endregion State

// #region Actions

function parseRgba(rgb_Or_rgba_Or_colorName_Or_hex_Str) {
    if (isNullOrUndefined(rgb_Or_rgba_Or_colorName_Or_hex_Str)) {
        return { r: 0, g: 0, b: 0, a: 0 };
    }
    let rgba = null;
    if (typeof rgb_Or_rgba_Or_colorName_Or_hex_Str === 'string' ||
        rgb_Or_rgba_Or_colorName_Or_hex_Str instanceof String) {
        // is string
        if (rgb_Or_rgba_Or_colorName_Or_hex_Str.startsWith('var(')) {
            // css variable 
            // (occurs in plain html conversion)
            // NOTE since no way to get value or know its usage,
            // and this probably a bad idea but substituting it for a random hex color
            rgb_Or_rgba_Or_colorName_Or_hex_Str = getRandomPaletteColor()
        }
        if (rgb_Or_rgba_Or_colorName_Or_hex_Str.startsWith('#')) {
            // is hex color string
            rgba = hexToRgba(rgb_Or_rgba_Or_colorName_Or_hex_Str);
            return rgba;
        }
        let hex = colorNameToHex(rgb_Or_rgba_Or_colorName_Or_hex_Str);
        if (hex) {
            // is color name
            rgba = hexToRgba(hex);
            return rgba;
        }
        // rgb or rgba color
        rgba = rgbaCssStrToRgba(rgb_Or_rgba_Or_colorName_Or_hex_Str);
        if (!rgba) {
            // what is the data?
            debugger;

        }
        return rgba;
    }
    if (rgb_Or_rgba_Or_colorName_Or_hex_Str.r === undefined) {
        // what is the data?
        debugger;
    }
    return rgb_Or_rgba_Or_colorName_Or_hex_Str;
}

function rgbaToRgbaStyle(rgba) {
    if (rgba.a === undefined) {
        rgba.a = 1;
    }
    return 'rgba(' + rgba.r + ',' + rgba.g + ',' + rgba.b + ',' + (rgba.a / 1) + ')';
}

function rgbaCssStrToRgba(rgbaCssStr) {
    if (!rgbaCssStr || rgbaCssStr.length == 0) {
        return null;
    }
    rgbaCssStr = rgbaCssStr.replace('(', '').replace(')', '');
    if (rgbaCssStr.startsWith('rgba')) {
        rgbaCssStr = rgbaCssStr.replace('rgba', '');
    } else if (rgbaCssStr.startsWith('rgb')) {
        rgbaCssStr = rgbaCssStr.replace('rgb', '');
    } else {
        return null;
    }
    let rgbaParts = rgbaCssStr.split(',');
    let rgba = {};
    rgba.r = parseFloat(rgbaParts[0]);
    rgba.g = parseFloat(rgbaParts[1]);
    rgba.b = parseFloat(rgbaParts[2]);
    rgba.a = rgbaParts.length == 3 ? 1 : parseFloat(rgbaParts[3]);
    return rgba;
}

function hexToRgba(hexStr) {
    if (typeof hexStr !== 'string' && !(hexStr instanceof String)) {
        debugger;
    }
    hexStr = hexStr.toLowerCase()
    if (hexStr.indexOf('#') != -1) {
        hexStr = hexStr.replace('#', '');
    }
    let x = hexStr.length == 8 ? 2 : 0;

    let r = parseInt(substringByLength(hexStr, 0, 2), 16);
    let g = parseInt(substringByLength(hexStr, 2, 2), 16);
    let b = parseInt(substringByLength(hexStr, 4, 2), 16);
    let a = hexStr.length == 8 ? parseInt(substringByLength(hexStr,6, 2), 16) / 255 : 1;

    return { r: r, g: g, b: b, a: a };
}

function rgbaToHex(rgba, ignoreAlpha = true) {
    let hex = '#';
    hex += getHexChannelStrPart(rgba.r);
    hex += getHexChannelStrPart(rgba.g);
    hex += getHexChannelStrPart(rgba.b);

    if (!ignoreAlpha && !isNullOrUndefined(rgba.a)) {
        hex += getHexChannelStrPart(rgba.a, 255);
    }
    return hex;
}

function dotnetHexToCssHex(dotnet_hex) {
    // NOTE dotnet alpha [1],[2]
    if (isNullOrEmpty(dotnet_hex)) {
        return dotnet_hex;
    }
    if (!dotnet_hex.startsWith('#')) {
        dotnet_hex = '#' + dotnet_hex;
    }
    if (dotnet_hex.length <= 7) {
        // no transparency
        return dotnet_hex;
    }
    let alpha_str = dotnet_hex[1] + dotnet_hex[2];
    let color_str = substringByLength(dotnet_hex, 3, 6);
    const css_hex = '#' + color_str + alpha_str;
    return css_hex;
}

function cssHexToDotNetHex(css_hex) {
    // NOTE css alpha [7] [8]
    if (isNullOrEmpty(css_hex)) {
        return css_hex;
    }
    if (!css_hex.startsWith('#')) {
        css_hex = '#' + css_hex;
    }
    if (css_hex.length <= 7) {
        // no transparency
        return css_hex;
    }
    let alpha_str = css_hex[7] + css_hex[8];
    let color_str = substringByLength(css_hex, 1, 6);
    const dotnet_hex = '#' + alpha_str + color_str;
    return dotnet_hex;
}

function hexToRgb(hex) {
    let rgba = hexToRgba(hex);
    delete rgba.a;
    return rgba;
}

function cleanHexColor(rgb_Or_rgba_Or_colorName_Or_hex_Str, forcedOpacity, ignoreAlpha) {
    let rgba = cleanColor(rgb_Or_rgba_Or_colorName_Or_hex_Str, forcedOpacity);
    return rgbaToHex(rgba, ignoreAlpha);
}

function cleanColor(rgb_Or_rgba_Or_colorName_Or_hex_Str, forcedOpacity, outputType = 'rgbaObj') {
    let color = parseRgba(rgb_Or_rgba_Or_colorName_Or_hex_Str);
    if (!isNullOrUndefined(forcedOpacity)) {
        color.a = forcedOpacity;
    }
    if (outputType == 'rgbaObj') {
        return color;
    }
    if (outputType == 'rgbaStyle') {
        return rgbaToRgbaStyle(color);
    }
    if (outputType == 'hex') {
        return rgbaToHex()
    }
    return color;
}

function findElementBackgroundColor(elm,fallback) {
    // NOTE iterates over elment ancestors until non-transparent bg is found
    let cur_elm = elm;
    while (cur_elm != null && cur_elm != document) {
        let cur_bg = cleanColor(getElementComputedStyleProp(cur_elm, 'background-color'), null, 'rgbaObj');
        if (cur_bg.a > 0) {
            return cleanColor(cur_bg,null, 'rgbaStyle');
        }
        cur_elm = cur_elm.parentNode;
    }
    return isNullOrUndefined(fallback) ? 'rgba(255,255,255,1)' : fallback;
}

function rgbaToCssColor(rgba) {
    if (!rgba) {
        return 'rgba(0,0,0,0)';
    }
    return 'rgba(' + rgba.r + ',' + rgba.g + ',' + rgba.b + ',' + rgba.a + ')';
}

function colorNameToHex(color) {
    if (typeof CssColorLookup[color.toLowerCase()] != 'undefined')
        return CssColorLookup[color.toLowerCase()];

    return false;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers