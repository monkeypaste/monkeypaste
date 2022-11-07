// #region Globals

// #endregion Globals

// #region Life Cycle

function initFontColorToolbarItems() {
	getFontColorToolbarButton().innerHTML = getSvgHtml('fontfg');
	addClickOrKeyClickEventListener(getFontColorToolbarButton(), onFontColorOrBgToolbarButtonClick);


	getFontBackgroundToolbarButton().innerHTML = getSvgHtml('fontbg');
	addClickOrKeyClickEventListener(getFontBackgroundToolbarButton(), onFontColorOrBgToolbarButtonClick);
}
// #endregion Life Cycle

// #region Getters

// #endregion Getters

function getFontColorToolbarButton() {
	return document.getElementById('fontColorToolbarButton');
}
function getFontBackgroundToolbarButton() {
	return document.getElementById('fontBackgroundToolbarButton');
}

function getFontHexColorAtDocIdx(docIdx) {
	let css_color = getElementComputedStyleProp(getElementAtDocIdx(docIdx, true), 'color');
	return cleanHexColor(css_color,null,false);
}

function getFontHexBgColorAtDocIdx(docIdx) {
	let css_color = getElementComputedStyleProp(getElementAtDocIdx(docIdx, true), 'background-color');
	return cleanHexColor(css_color, 1);
}

// #region Setters

function setFontColorToolbarButtonColor(fg_hex_color) {
	let fg_svg_elm = Array.from(getFontColorToolbarButton().querySelectorAll('path'))[0];
	setElementComputedStyleProp(fg_svg_elm, 'fill', cleanColorStyle(fg_hex_color));
}

function setFontBackgroundToolbarButtonColor(bg_hex_color) {
	let bg_svg_elm = Array.from(getFontBackgroundToolbarButton().querySelectorAll('g'))[0];
	setElementComputedStyleProp(bg_svg_elm, 'fill', cleanColorStyle(bg_hex_color));
}
// #endregion Setters

// #region State

// #endregion State

// #region Actions

function showFontColorPaletteMenu(toolbarElm) {
	hideColorPaletteMenu();
	if (!toolbarElm) {
		return;
	}

	let hexColorStr =
		toolbarElm == getFontColorToolbarButton() ?
			getFontHexColorAtDocIdx() :
			getFontHexBgColorAtDocIdx();

	showColorPaletteMenu(
		toolbarElm,
		'bottom|left',
		null,
		hexColorStr,
		onFontColorOrBgColorPaletteItemClick);

}

function hideFontColorPaletteMenu() {
	if (ColorPaletteAnchorElement != getFontColorToolbarButton() &&
		ColorPaletteAnchorElement != getFontBackgroundToolbarButton()) {
		return;
	}
	hideColorPaletteMenu();
}

function updateFontColorToolbarItemsToSelection() {
	let sel = getDocSelection();
	if (!sel) {
		return;
	}
	if (sel && sel.length == 0) {
		if (sel.index < getDocLength() - 1) {
			// get forward color (untested on last idx)
			sel.index++;
		}
	}
	// NOTE these selectors are unique to the types of svg's in use right now
	// and could just apply to everything within element but looks better being specific...

	let fg_hex_color = getFontHexColorAtDocIdx(sel.index);
	setFontColorToolbarButtonColor(fg_hex_color);


	let bg_hex_color = getFontHexBgColorAtDocIdx(sel.index);
	setFontBackgroundToolbarButtonColor(bg_hex_color);
}

// #endregion Actions

// #region Event Handlers

function onFontColorOrBgColorPaletteItemClick(chex) {
	/*formatDocRange(getDocSelection(), {})*/
	if (ColorPaletteAnchorElement == null) {
		return;
	}
	let sel = getDocSelection();
	if (sel && sel.length == 0) {
		// color won't change unless there's a range
		sel.length = 1;
	}
	if (ColorPaletteAnchorElement == getFontBackgroundToolbarButton()) {
		formatDocRange(sel, { background: chex });
		setFontBackgroundToolbarButtonColor(chex);
	} else {
		formatDocRange(sel, { color: chex });
		setFontColorToolbarButtonColor(chex);
	}
}

function onFontColorOrBgToolbarButtonClick(e) {
	if (isShowingColorPaletteMenu()) {
		if (ColorPaletteAnchorElement == e.currentTarget) {
			hideColorPaletteMenu();
			return;
		}
		hideColorPaletteMenu();
	}
	showFontColorPaletteMenu(e.currentTarget);
}


// #endregion Event Handlers