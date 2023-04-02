// #region Globals
var ColorPaletteAnchorElement = null;
var ColorPaletteAnchorResultCallback = null;

const COLOR_PALETTE_ROW_COUNT = 5;
const COLOR_PALETTE_COL_COUNT = 14;

var IsCustomColorPaletteOpen = false;

// #endregion Globals

// #region Life Cycle

function showColorPaletteMenu(
    anchor_elm,
    anchor_sides,
    offset,
    sel_color,
    color_change_callback) {
    
    let pal_elm = getColorPaletteContainerElement();
    pal_elm.innerHTML = getColorPaletteHtml(sel_color);

    pal_elm.classList.remove('hidden');

    let pal_elm_items = pal_elm.querySelectorAll('a');
    pal_elm_items.forEach((x, idx) => {
        x.addEventListener('click', function (e) {
            if (idx < pal_elm_items.length - 1) {
                let item_color = x.firstChild.style.backgroundColor;
                item_color = cleanHexColor(item_color);
                color_change_callback(item_color);
            } else {
                e.originalColorStr = sel_color;
                onCustomColorPaletteItemClick(e, color_change_callback);
                log('custom color item clicked');
            }
        });
    });

    var paletteMenuRect = cleanRect(pal_elm.getBoundingClientRect());

    let anchor_elm_rect = cleanRect();
    if (anchor_elm) {
        ColorPaletteAnchorElement = anchor_elm;
        anchor_elm_rect = cleanRect(anchor_elm.getBoundingClientRect());
    } else {
        ColorPaletteAnchorElement = {};
    }

    anchor_sides = anchor_sides ? anchor_sides : 'top|left';
    let anchor_loc = parsePointFromSides(anchor_elm_rect, anchor_sides);
    let origin = anchor_loc;

    if (typeof offset === 'string' || offset instanceof String) {
        if (offset == 'above') {
            origin.y -= paletteMenuRect.height;
        }
    } else {
        offset = offset ? offset : { x: 0, y: 0 };
        origin = addPoints(origin, offset);
	}
    
    paletteMenuRect = moveRectLocation(paletteMenuRect, origin);

    let win_rect = getWindowRect();
    if (!isRectContainOtherRect(win_rect, paletteMenuRect)) {
        if (anchor_sides.includes('top')) {

        }
    }
    //offset.y -= paletteMenuRect.height;

    pal_elm.style.left = `${origin.x}px`;
    pal_elm.style.top = `${origin.y}px`;

    window.addEventListener('click', onWindowClickWithColorPaletteOpen);
}

function hideColorPaletteMenu() {
    getColorPaletteContainerElement().classList.add('hidden');
    window.removeEventListener('click', onWindowClickWithColorPaletteOpen);
    if (IsCustomColorPaletteOpen) {
        // preserve anchor element when custom is open, palette gets closed on outside click
        return;
    }
    ColorPaletteAnchorElement = null;
}
// #endregion Life Cycle

// #region Getters

function getColorPaletteContainerElement() {
	return document.getElementById('colorPaletteContainer');
}

function getColorPaletteHtml(sel_color) {
    let rc = COLOR_PALETTE_ROW_COUNT;
    let cc = COLOR_PALETTE_COL_COUNT;
    let idx = 0;
    let paletteHtml = '<table>';
    let wasSelColorFound = false;

    // NOTE need to strip transparency if on sel color or won't compare right
    const palette_sel_color = isNullOrEmpty(sel_color) ? null : cleanHexColor(sel_color, null, true);

    for (var r = 0; r < rc; r++) {
        paletteHtml += '<tr>';
        for (var c = 0; c < cc; c++) {
            let c = ContentColors[idx];
            let is_selected = false;
            let item_class = 'color-palette-item';
            if (sel_color) {
                if (c.toLowerCase() == palette_sel_color.toLowerCase() ||
                    (idx == ContentColors.length - 1 && !wasSelColorFound)) {
                    // when this is the selected palette item and its not
                    // the custom palette item
                    item_class += ' color-palette-item-selected';
                    is_selected = true;
                    wasSelColorFound = true;
                }
            }
            let item_style = '';
            let item_inner_html = '';
            if (idx == ContentColors.length - 1) {
                // custom br pallete item
                item_inner_html = `<span>+</span>`;
                c = sel_color;
                item_class += ' custom-color-palette-item';
                if (is_selected) {
                    item_style = `background-color: ${c};color: ${getContrastHexColor(c)}`;
                } else {
                    item_style = `background-color: white; color: black`;
                }
            } else {
                item_style = `background-color: ${c};`;
            }

            let item = `<td><a href="javascript:void(0);"><div class="${item_class}" style="${item_style}">${item_inner_html}</div></a></td>`;
            paletteHtml += item;
            idx++;
        }
        paletteHtml += '</tr>';
    }
    return paletteHtml;
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isShowingColorPaletteMenu() {
	return !getColorPaletteContainerElement().classList.contains('hidden');
}

function resetColorPaletteState() {
    IsCustomColorPaletteOpen = false;
    ColorPaletteAnchorResultCallback = null;
    ColorPaletteAnchorElement = null;
}

// #endregion State

// #region Actions

function processCustomColorResult(dotnetHexResult) {
    const css_hex = dotnetHexToCssHex(dotnetHexResult);
    ColorPaletteAnchorResultCallback(css_hex);
    resetColorPaletteState();
}


// #endregion Actions

// #region Event Handlers

function onCustomColorPaletteItemClick(e, orgCallbackHandler) {
    // TODO this should be a binding to host to show color chooser and there should be a ext msg that's returned w/ hex color
    if (e.originalColorStr === undefined) {
        debugger;
    }
    IsCustomColorPaletteOpen = true;
    // NOTE storing org callback here so custom selection is handled fluidly w/ original request
    ColorPaletteAnchorResultCallback = orgCallbackHandler;
    const dotnet_hex = cssHexToDotNetHex(e.originalColorStr);
    onShowCustomColorPicker_ntf(dotnet_hex);
}

function onWindowClickWithColorPaletteOpen(e) {
    if (isClassInElementPath(e.currentTarget, 'custom-color-palette-item')) {
        // ignore custom click
        return;
    }
    hideColorPaletteMenu();
}
// #endregion Event Handlers