// #region Globals
var ColorPaletteAnchorElement = null;
var ColorPaletteAnchorResultCallback = null;
// #endregion Globals

// #region Life Cycle

function showColorPaletteMenu(
    anchor_elm,
    anchor_sides,
    offset,
    sel_color,
    color_change_callback) {
    let rc = 5;
    let cc = 14;
    let idx = 0;
    let paletteHtml = '<table>';
    let wasSelColorFound = false;


    for (var r = 0; r < rc; r++) {
        paletteHtml += '<tr>';
        for (var c = 0; c < cc; c++) {
            let c = ContentColors[idx];
            let is_selected = false;
            let item_class = 'color-palette-item';
            if (sel_color) {
                if (c.toLowerCase() == sel_color.toLowerCase() ||
                    (idx == ContentColors.length - 1 && !wasSelColorFound)) {
                    item_class += ' color-palette-item-selected';
                    is_selected = true;
                }
            }
            let item_style = '';
            if (idx == ContentColors.length - 1) {
                c = sel_color;
                item_class += ' fa-solid fa-plus color-palette-item custom-color-palette-item';
                item_style = `background-color: transparent;`;
                if (is_selected) {
                    item_style += `color: ${c};`;
                } else {
                    item_style += `color: black`;
                }
            } else {
                item_style = 'background-color: ' + c + ';';
            }
            let item = `<td><a href="javascript:void(0);"><div class="${item_class}" style="${item_style}" ></div></a></td>`;
            paletteHtml += item;
            idx++;
        }
        paletteHtml += '</tr>';
    }

    let pal_elm = getColorPaletteContainerElement();
    pal_elm.innerHTML = paletteHtml;

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

    offset = offset ? offset : { x: 0, y: 0 };
    origin = addPoints(origin, offset);
    paletteMenuRect = moveRectLocation(paletteMenuRect, origin);

    let win_rect = getWindowRect();
    if (!rectContainsRect(win_rect, paletteMenuRect)) {
        if (anchor_sides.includes('top')) {

        }
    }
    //offset.y -= paletteMenuRect.height;

    pal_elm.style.left = `${origin.x}px`;
    pal_elm.style.top = `${origin.y}px`;

    window.addEventListener('click', onWindowClickWithColorPaletteOpen);
}

function hideColorPaletteMenu() {
    ColorPaletteAnchorElement = null;
    getColorPaletteContainerElement().classList.add('hidden');

    window.removeEventListener('click', onWindowClickWithColorPaletteOpen);
}
// #endregion Life Cycle

// #region Getters

function getColorPaletteContainerElement() {
	return document.getElementById('colorPaletteContainer');
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isShowingColorPaletteMenu() {
	return !getColorPaletteContainerElement().classList.contains('hidden');
}

function resetColorPaletteState() {
    ColorPaletteAnchorResultCallback = null;
    ColorPaletteAnchorElement = null;
}

// #endregion State

// #region Actions

function processCustomColorResult(resultStr) {
    alert('Custom Color result: ' + resultStr);
    if (ColorPaletteAnchorResultCallback == null) {
        debugger;
	}
    ColorPaletteAnchorResultCallback(resultStr);
}


// #endregion Actions

// #region Event Handlers

function onCustomColorPaletteItemClick(e, orgCallbackHandler) {
    // TODO this should be a binding to host to show color chooser and there should be a ext msg that's returned w/ hex color
    if (e.originalColorStr === undefined) {
        debugger;
	}
    alert('yo homey. orig: ' + e.originalColorStr);

    // NOTE storing org callback here so custom selection is handled fluidly w/ original request
    ColorPaletteAnchorResultCallback = orgCallbackHandler;
    onShowCustomColorPicker_ntf(e.originalColorStr);
}

function onWindowClickWithColorPaletteOpen(e) {
    if (isClassInElementPath(e.currentTarget, 'custom-color-palette-item')) {
        // ignore custom click
        return;
    }
    hideColorPaletteMenu();
}
// #endregion Event Handlers