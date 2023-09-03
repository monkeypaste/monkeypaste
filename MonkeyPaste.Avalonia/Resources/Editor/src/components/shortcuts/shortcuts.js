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
    }   

}
// #endregion Life Cycle

// #region Getters

function getShortcutHtml(shortcutText) {
    return `<em class="option-keys">${shortcutText}</em>`;
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function parseShortcutText(str) {
    // NOTE this presumes theres only 1 shortcut
    // example: 'This is a tooltip command ##ShortcutTypeName##'
    // returns ShortcutTypeGestureText
    if (!isString(str)) {
        return '';
    }
    let str_parts = str.split(globals.SHORTCUT_STR_TOKEN);
    for (var i = 0; i < str_parts.length; i++) {
        if (i % 2 == 0) {
            continue;
        }

        let str_part = str_parts[i];
        if (globals.ShortcutKeysLookup[str_part] !== undefined) {
            return globals.ShortcutKeysLookup[str_part];
        }
    }
    return '';
}

function decodeStringWithShortcut(str) {
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
            if (globals.ShortcutKeysLookup[str_part] !== undefined) {
                out_str += globals.ShortcutKeysLookup[str_part]
            }
        }
    }
    return out_str;
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers