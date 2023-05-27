
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
    let color_idx = Math.floor(Math.random() * globals.ContentColors.length - 1);
    while (get2dIdx(color_idx, globals.COLOR_PALETTE_COL_COUNT)[1] == globals.COLOR_PALETTE_COL_COUNT - 1) {
        // ignore last column
        color_idx = Math.floor(Math.random() * globals.ContentColors.length - 1);
    }
    return globals.ContentColors[color_idx];
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

function isNormalizedRgb(rgb) {
    if (!isRgb(rgb)) {
        return false;
    }
    return rgb.r < 1 && rgb.g < 1 && rgb.b < 1;
}

function isRgb(obj) {
    if (isNullOrUndefined(obj) ||
        isNullOrUndefined(obj.r) ||
        isNullOrUndefined(obj.g) ||
        isNullOrUndefined(obj.b)) {
        return false;
    }
    return true;
}

function isRgba(obj) {
    if (!isRgb(obj) ||
        isNullOrUndefined(obj.a)) {
        return false;
    }
    return true;
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

function isRgbFuzzyBlack(rgb) {
    // tested with https://www.w3schools.com/css/css_colors_hsl.asp
    const threshold = 30;
    const result =
        rgb.r <= threshold &&
        rgb.g <= threshold &&
        rgb.b <= threshold;
    return result
}
function isRgbFuzzyWhite(rgb) {
    // tested with https://www.w3schools.com/css/css_colors_hsl.asp
    const hsl = rgb2hsl(rgb);
    return hsl.l > 98;
}

function isRgbFuzzyBlackOrWhite(rgb) {
    if (isRgbFuzzyBlack(rgb) || isRgbFuzzyWhite(rgb)) {
        return true;
    }
    return false;
}
// #endregion State

// #region Actions

function parseRgba(rgb_Or_rgba_Or_colorName_Or_hex_Str) {
    const fallback_rgba = { r: 0, g: 0, b: 0, a: 0 };
    if (isNullOrUndefined(rgb_Or_rgba_Or_colorName_Or_hex_Str)) {
        return fallback_rgba;
    }
    let rgba = null;
    if (isString(rgb_Or_rgba_Or_colorName_Or_hex_Str)) {
        // is string
        if (isNullOrWhiteSpace(rgb_Or_rgba_Or_colorName_Or_hex_Str)) {
            return fallback_rgba;
        }
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


function normalizeRgba(rgba) {
    if (isNormalizedRgb(rgba)) {
        return rgba;
    }
    if (!isRgb(rgba)) {
        // bad input
        debugger;
        return rgba;
    }
    let nrgba = {
        r: rgba.r / 255,
        g: rgba.g / 255,
        b: rgba.b / 255
    };
    if (isRgba(rgba)) {
        if (rgba.a > 1) {
            // assume alpha 0-255
            nrgba.a = rgba.a / 255;
        } else {
            // already normalized
            nrgba.a = rgba.a;
        }
    }
    return nrgba;
}

function rgb2hsl(rgb) {
    const alpha = isNullOrUndefined(rgb.a) ? null : rgb.a;
    let nrgb = normalizeRgba(rgb);
    const r = rgb.r;
    const g = rgb.g;
    const b = rgb.b;

    // from https://www.30secondsofcode.org/js/s/rgb-to-hsl/

    // in: r,g,b in [0,1],
    // out: The range of the resulting values is H: [0, 360], S: [0, 100], L: [0, 100].
    const l = Math.max(r, g, b);
    const s = l - Math.min(r, g, b);
    const h = s
        ? l === r
            ? (g - b) / s
            : l === g
                ? 2 + (b - r) / s
                : 4 + (r - g) / s
        : 0;
    let result = [
        60 * h < 0 ? 60 * h + 360 : 60 * h,
        100 * (s ? (l <= 0.5 ? s / (2 * l - s) : s / (2 - (2 * l - s))) : 0),
        (100 * (2 * l - s)) / 2,
    ];
    let output = {
        h: result[0],
        s: result[1],
        l: result[2]
    };
    if (alpha) {
        output.a = alpha;
    }
    return output;
}
function hsl2Rgb(hsl) {
    const alpha = isNullOrUndefined(hsl.a) ? null : hsl.a;
    let h = hsl.h;
    let s = hsl.s;
    let l = hsl.l;
    // from https://www.30secondsofcode.org/js/s/hsl-to-rgb/
    // in: H: [0, 360], S: [0, 100], L: [0, 100].
    // out: [0, 255].

    s /= 100;
    l /= 100;
    const k = n => (n + h / 30) % 12;
    const a = s * Math.min(l, 1 - l);
    const f = n =>
        l - a * Math.max(-1, Math.min(k(n) - 3, Math.min(9 - k(n), 1)));
    let result = [255 * f(0), 255 * f(8), 255 * f(4)];
    let output = {
        r: result[0],
        g: result[1],
        b: result[2]
    };
    if (alpha) {
        output.a = alpha;
    }
    return output;
}

function shiftRgbaLightness(rgb, lightDelta) {
    let hsl = rgb2hsl(rgb);
    hsl.l = Math.max(0, Math.min(1, hsl.l + lightDelta));
    let shifted_rgb = hsl2Rgb(hsl);
    return shifted_rgb;
}

function shiftRgbaHue(rgb, hueDelta) {
    let hsl = rgb2hsl(rgb);
    hsl.h = wrapNumber(hsl.h + hueDelta, 0, 360);
    let shifted_rgb = hsl2Rgb(hsl);
    return shifted_rgb;
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
    if (typeof globals.CssColorLookup[color.toLowerCase()] != 'undefined')
        return globals.CssColorLookup[color.toLowerCase()];

    return false;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers