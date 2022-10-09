function getRandomColor() {
    var letters = '0123456789ABCDEF';
    var color = '#';
    for (var i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
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

function isBright(hex_or_color_name_or_rgb_or_rgba, brightThreshold = 150) {
    var rgb = parseRgba(hex_or_color_name_or_rgb_or_rgba);
    var grayVal = Math.sqrt(
        rgb.r * rgb.r * .299 +
        rgb.g * rgb.g * .587 +
        rgb.b * rgb.b * .114);
    return grayVal > brightThreshold;
}

function parseRgba(rgb_Or_rgba_Or_colorName_Or_hex_Str) {
    let rgba = null;
    if (typeof rgb_Or_rgba_Or_colorName_Or_hex_Str === 'string' || rgb_Or_rgba_Or_colorName_Or_hex_Str instanceof String) {
        // is string
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
    if (rgbaCssStr.startsWith('rgb')) {
        rgbaCssStr = rgbaCssStr.replace('rgb', '');
    } else if (rgbaCssStr.startsWith('rgba')) {
        rgbaCssStr = rgbaCssStr.replace('rgba', '');
    } else {
        return null;
    }
    let rgbaParts = rgbaCssStr.split(',');
    let rgba = {};
    for (var i = 0; i < 4; i++) {
        if (rgbaParts.length - 1 < i) {
            // for rgb set a to 1
            rgba.a = 1;
        } else {
            if (i == 0) {
                rgba.r = parseFloat(rgbaParts[i]);
            } else if (i == 1) {
                rgba.g = parseFloat(rgbaParts[i]);
            } if (i == 2) {
                rgba.b = parseFloat(rgbaParts[i]);
            }
        }
    }
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

    let a = x > 0 ? parseInt(hexStr.substring(0, 2), 16) : 1;
    let r = parseInt(substringByLength(hexStr, x, 2), 16);
    let g = parseInt(substringByLength(hexStr, x + 2, 2), 16);
    let b = parseInt(substringByLength(hexStr, x + 4, 2), 16);

    return { r: r, g: g, b: b, a: a };
}


function hexToRgb(hex) {
    let rgba = hexToRgba(hex);
    delete rgba.a;
    return rgba;

    //var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    //return result ? {
    //    R: parseInt(result[1], 16),
    //    G: parseInt(result[2], 16),
    //    B: parseInt(result[3], 16)
    //} : null;
}


function cleanColor(rgb_Or_rgba_Or_colorName_Str, forceAlpha) {
    if (!rgb_Or_rgba_Or_colorName_Str) {
        return { r: 0, g: 0, b: 0, a: 0 };
    }
    let color = parseRgba(rgb_Or_rgba_Or_colorName_Str);
    if (forceAlpha) {
        color.a = forceAlpha;
    }
    return color;
}
function cleanColorStyle(rgb_Or_rgba_Or_colorName_Str, forceAlpha) {
    let color = cleanColor(rgb_Or_rgba_Or_colorName_Str, forceAlpha);
    return rgbaToRgbaStyle(color);
}

function rgbaToCssColor(rgba) {
    if (!rgba) {
        return 'rgba(0,0,0,0)';
    }
    return 'rgba(' + rgba.r + ',' + rgba.g + ',' + rgba.b + ',' + rgba.a + ')';
}

function colorNameToHex(color) {
    var colors = {
        "aliceblue": "#f0f8ff", "antiquewhite": "#faebd7", "aqua": "#00ffff", "aquamarine": "#7fffd4", "azure": "#f0ffff",
        "beige": "#f5f5dc", "bisque": "#ffe4c4", "black": "#000000", "blanchedalmond": "#ffebcd", "blue": "#0000ff", "blueviolet": "#8a2be2", "brown": "#a52a2a", "burlywood": "#deb887",
        "cadetblue": "#5f9ea0", "chartreuse": "#7fff00", "chocolate": "#d2691e", "coral": "#ff7f50", "cornflowerblue": "#6495ed", "cornsilk": "#fff8dc", "crimson": "#dc143c", "cyan": "#00ffff",
        "darkblue": "#00008b", "darkcyan": "#008b8b", "darkgoldenrod": "#b8860b", "darkgray": "#a9a9a9", "darkgreen": "#006400", "darkkhaki": "#bdb76b", "darkmagenta": "#8b008b", "darkolivegreen": "#556b2f",
        "darkorange": "#ff8c00", "darkorchid": "#9932cc", "darkred": "#8b0000", "darksalmon": "#e9967a", "darkseagreen": "#8fbc8f", "darkslateblue": "#483d8b", "darkslategray": "#2f4f4f", "darkturquoise": "#00ced1",
        "darkviolet": "#9400d3", "deeppink": "#ff1493", "deepskyblue": "#00bfff", "dimgray": "#696969", "dodgerblue": "#1e90ff",
        "firebrick": "#b22222", "floralwhite": "#fffaf0", "forestgreen": "#228b22", "fuchsia": "#ff00ff",
        "gainsboro": "#dcdcdc", "ghostwhite": "#f8f8ff", "gold": "#ffd700", "goldenrod": "#daa520", "gray": "#808080", "green": "#008000", "greenyellow": "#adff2f",
        "honeydew": "#f0fff0", "hotpink": "#ff69b4",
        "indianred ": "#cd5c5c", "indigo": "#4b0082", "ivory": "#fffff0", "khaki": "#f0e68c",
        "lavender": "#e6e6fa", "lavenderblush": "#fff0f5", "lawngreen": "#7cfc00", "lemonchiffon": "#fffacd", "lightblue": "#add8e6", "lightcoral": "#f08080", "lightcyan": "#e0ffff", "lightgoldenrodyellow": "#fafad2",
        "lightgrey": "#d3d3d3", "lightgreen": "#90ee90", "lightpink": "#ffb6c1", "lightsalmon": "#ffa07a", "lightseagreen": "#20b2aa", "lightskyblue": "#87cefa", "lightslategray": "#778899", "lightsteelblue": "#b0c4de",
        "lightyellow": "#ffffe0", "lime": "#00ff00", "limegreen": "#32cd32", "linen": "#faf0e6",
        "magenta": "#ff00ff", "maroon": "#800000", "mediumaquamarine": "#66cdaa", "mediumblue": "#0000cd", "mediumorchid": "#ba55d3", "mediumpurple": "#9370d8", "mediumseagreen": "#3cb371", "mediumslateblue": "#7b68ee",
        "mediumspringgreen": "#00fa9a", "mediumturquoise": "#48d1cc", "mediumvioletred": "#c71585", "midnightblue": "#191970", "mintcream": "#f5fffa", "mistyrose": "#ffe4e1", "moccasin": "#ffe4b5",
        "navajowhite": "#ffdead", "navy": "#000080",
        "oldlace": "#fdf5e6", "olive": "#808000", "olivedrab": "#6b8e23", "orange": "#ffa500", "orangered": "#ff4500", "orchid": "#da70d6",
        "palegoldenrod": "#eee8aa", "palegreen": "#98fb98", "paleturquoise": "#afeeee", "palevioletred": "#d87093", "papayawhip": "#ffefd5", "peachpuff": "#ffdab9", "peru": "#cd853f", "pink": "#ffc0cb", "plum": "#dda0dd", "powderblue": "#b0e0e6", "purple": "#800080",
        "rebeccapurple": "#663399", "red": "#ff0000", "rosybrown": "#bc8f8f", "royalblue": "#4169e1",
        "saddlebrown": "#8b4513", "salmon": "#fa8072", "sandybrown": "#f4a460", "seagreen": "#2e8b57", "seashell": "#fff5ee", "sienna": "#a0522d", "silver": "#c0c0c0", "skyblue": "#87ceeb", "slateblue": "#6a5acd", "slategray": "#708090", "snow": "#fffafa", "springgreen": "#00ff7f", "steelblue": "#4682b4",
        "tan": "#d2b48c", "teal": "#008080", "thistle": "#d8bfd8", "tomato": "#ff6347", "turquoise": "#40e0d0",
        "violet": "#ee82ee",
        "wheat": "#f5deb3", "white": "#ffffff", "whitesmoke": "#f5f5f5",
        "yellow": "#ffff00", "yellowgreen": "#9acd32", "transparent": "#00000000"
    };

    if (typeof colors[color.toLowerCase()] != 'undefined')
        return colors[color.toLowerCase()];

    return false;
}