// #region Life Cycle
function initShortcuts(shortcutBase64MsgStr) {
    // input ''
    globals.ShortcutKeysLookup = {};
    let shortcut_items = null;

    if (!isNullOrUndefined(shortcutBase64MsgStr)) {
        const shortcut_msg = toJsonObjFromBase64Str(shortcutBase64MsgStr);
        if (shortcut_msg && Array.isArray(shortcut_msg.shortcuts)) {
            shortcut_items = shortcut_msg.shortcuts;
        }
    }
    for (var i = 0; i < globals.SHORTCUT_TYPES.length; i++) {
        let st = globals.SHORTCUT_TYPES[i];

        //let keys = '';
        //if (shortcut_items && Array.isArray(shortcut_items)) {
        //    let shortcut_item = shortcut_items.find(x => x.shortcutType == st);
        //    if (shortcut_item) {
        //        keys = `<em class="option-keys">${shortcut_item.keys}</em>`;
        //    }
        //}
        //globals.ShortcutKeysLookup[st] = keys;
        if (shortcut_items && Array.isArray(shortcut_items)) {
            let shortcut_item = shortcut_items.find(x => x.shortcutType == st);
            if (shortcut_item) {
                globals.ShortcutKeysLookup[st] = shortcut_item.keys;
            }
        }
        log('Init Shortcut Type: \'' + st + '\' keys: \'' + globals.ShortcutKeysLookup[st] + '\'');
    }   

}
// #endregion Life Cycle

// #region Getters

function getShortcutHtml(shortcutText) {
    return `<em class="option-keys">${shortcutText}</em>`;
}

function getShortcutEncStr(shortcutIdx) {
    return `${globals.SHORTCUT_STR_TOKEN}${globals.SHORTCUT_TYPES[shortcutIdx]}${globals.SHORTCUT_STR_TOKEN}`
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function parseShortcutTooltipParts(str) {
    // NOTE this presumes theres only 1 shortcut and its at the END
    // example: 'This is a tooltip command ##ShortcutTypeName##'
    // returns [0] pre shortcut text, [1] keystr
    let result = ['', ''];
    if (!isString(str)) {
        return result;
    }
    let str_parts = str.split(globals.SHORTCUT_STR_TOKEN);
    result[0] = str_parts[0];
    for (var i = 0; i < str_parts.length; i++) {
        if (i % 2 == 0) {
            continue;
        }

        let str_part = str_parts[i];
        if (globals.ShortcutKeysLookup[str_part] !== undefined) {
            result[1] = globals.ShortcutKeysLookup[str_part];
            break;
        }
    }
    return result;
}

function decodeStringWithShortcut(str,isHtml) {
    // example: 'This is a tooltip command ##ShortcutTypeName##'
    if (!isString(str)) {
        return str;
    }
    let out_str = '';
    let str_parts = str.split(globals.SHORTCUT_STR_TOKEN);
    for (var i = 0; i < str_parts.length; i++) {
        let str_part = str_parts[i];
        if (i % 2 == 0) {
            out_str += str_part;            
        } else {
            const key_str = globals.ShortcutKeysLookup[str_part];
            if (key_str !== undefined) {
                out_str += isHtml ? getShortcutHtml(key_str) : key_str;
            }
        }
    }
    return out_str;
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers