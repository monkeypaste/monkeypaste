// #region Globals

// #endregion Globals

// #region Life Cycle

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
// #endregion State

// #region Actions

function showColorPaletteMenu(anchor_x,anchor_y, sel_color, color_change_callback) {
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
            let item_class = 'template-color-palette-item';
            if (sel_color) {
                if (c.toLowerCase() == sel_color.toLowerCase() ||
                    (idx == ContentColors.length - 1 && !wasSelColorFound)) {
                    item_class += ' template-color-palette-item-selected';
                    is_selected = true;
                }
            }
            let item_style = '';
            if (idx == ContentColors.length - 1) {
                c = sel_color;
                item_class += ' fa-solid fa-plus';
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
                onCustomColorPaletteItemClick(e, color_change_callback);
                log('custom color item clicked');
            }
        });
    });

    var paletteMenuRect = pal_elm.getBoundingClientRect();
    anchor_y -= paletteMenuRect.height;

    pal_elm.style.left = `${anchor_x}px`;
    pal_elm.style.top = `${anchor_y}px`;
}

function hideColorPaletteMenu() {
	getColorPaletteContainerElement().classList.add('hidden');
}

function onCustomColorPaletteItemClick(e, orgCallbackHandler) {
    // TODO this should be a binding to host to show color chooser and there should be a ext msg that's returned w/ hex color
    alert('yo homey');
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers