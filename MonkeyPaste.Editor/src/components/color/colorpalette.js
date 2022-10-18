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

function showColorPaletteMenu(anchor_x,anchor_y, sel_color, color_click_handler_name) {
    var palette_item = {
        style_class: 'template-color-palette-item',
        func_name: color_click_handler_name
    };

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
            let item_class = palette_item.style_class;
            if (sel_color) {
                if (c.toLowerCase() == sel_color.toLowerCase() ||
                    (idx == ContentColors.length - 1 && !wasSelColorFound)) {
                    item_class += ' template-color-palette-item-selected';
                    is_selected = true;
                }
            }
            let item_style = '';
            let item_func_name = palette_item.func_name;
            if (idx == ContentColors.length - 1) {
                item_func_name = 'showCustomColorPicker';
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
            let item = '<td><a href="javascript:void(0);" onclick="' + palette_item.func_name + '(\'' + c + '\'); event.stopPropagation();">' +
                '<div class="' + item_class + '" style="background-color: ' + c + '" ></div></a></td > ';
            paletteHtml += item;

            idx++;
        }
        paletteHtml += '</tr>';
    }

    let pal_elm = getColorPaletteContainerElement();
    pal_elm.innerHTML = paletteHtml;

    pal_elm.classList.remove('hidden');
    var paletteMenuRect = pal_elm.getBoundingClientRect();
    anchor_y -= paletteMenuRect.height;

    pal_elm.style.left = `${anchor_x}px`;
    pal_elm.style.top = `${anchor_y}px`;
}

function hideColorPaletteMenu() {
	getColorPaletteContainerElement().classList.add('hidden');
}

function showCustomColorPicker(...args) {
    // TODO this should be a binding to host to show color chooser and there should be a ext msg that's returned w/ hex color
    args = args ? args : [''];
    alert('yo homey ' + JSON.stringify(args));
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers