
// #region Life Cycle

function initFontColorToolbarItems() {
	getFontColorToolbarButton().innerHTML = getSvgHtml('fontfg', null, false);
	addClickOrKeyClickEventListener(getFontColorToolbarButton(), onFontColorOrBgToolbarButtonClick);

	getFontBackgroundToolbarButton().innerHTML = getSvgHtml('fontbg', null, false);
	addClickOrKeyClickEventListener(getFontBackgroundToolbarButton(), onFontColorOrBgToolbarButtonClick);

	initFontColorClassAttributes();
}

function initFontColorClassAttributes() {
	globals.FontColorOverrideAttrb = registerClassAttributor('fontColorOverride', 'font-color-override', globals.Parchment.Scope.INLINE);
	globals.FontBgColorOverrideAttrb = registerClassAttributor('fontBgColorOverride', 'font-bg-color-override', globals.Parchment.Scope.INLINE);
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
	return cleanHexColor(css_color,1,true);
}

function getFontHexBgColorAtDocIdx(docIdx) {
	let css_color = getElementComputedStyleProp(getElementAtDocIdx(docIdx, true), 'background-color');
	return cleanHexColor(css_color,null,false);
}

function getSelDocIdxForFontColor() {
	let sel = getDocSelection();
	if (!sel) {
		return 0;
	}
	if (sel.index < getDocLength() - 1) {
		// get forward color (untested on last idx)
		sel.index++;
	}
	return sel.index;
}
// #region Setters

function setFontColorToolbarButtonColor(fg_hex_color) {
	let fg_svg_elm = Array.from(getFontColorToolbarButton().querySelectorAll('path'))[0];
	setElementComputedStyleProp(fg_svg_elm, 'fill', cleanColor(fg_hex_color,null,'rgbaStyle'));
}

function setFontBackgroundToolbarButtonColor(bg_hex_color,fg_hex_color) {
	let bg_svg_elm = getFontBackgroundToolbarButton().firstChild;
	setElementComputedStyleProp(bg_svg_elm, 'background-color', bg_hex_color);// cleanColor(bg_hex_color, null, 'rgbaStyle'));

	// draw bg fg to match fg color
	let bg_svg_inner_outline_elm = Array.from(getFontBackgroundToolbarButton().querySelectorAll('g'))[0];
	setElementComputedStyleProp(bg_svg_inner_outline_elm, 'fill', fg_hex_color);
}
// #endregion Setters

// #region State

function hasUserFontColor(attr) {
	if (!attr) {
		return false;
	}
	if (attr['fontColorOverride'] == 'on' &&
		attr['color'] !== undefined) {
		return true;
	}
	return false;
}
function hasUserBgFontColor(attr) {
	if (!attr) {
		return false;
	}
	if (attr['fontBgColorOverride'] == 'on' &&
		attr['background'] !== undefined) {
		return true;
	}
	return false;
}

// #endregion State

// #region Actions

function showFontColorPaletteMenu(toolbarElm) {
	hideColorPaletteMenu();
	if (!toolbarElm) {
		return;
	}
	const font_sel_idx = getSelDocIdxForFontColor();

	let hexColorStr =
		toolbarElm == getFontColorToolbarButton() ?
			getFontHexColorAtDocIdx(font_sel_idx) :
			getFontHexBgColorAtDocIdx(font_sel_idx);

	showColorPaletteMenu(
		toolbarElm,
		'bottom|left',
		null,
		hexColorStr,
		onFontColorOrBgColorPaletteItemClick);

}

function hideFontColorPaletteMenu() {
	if (globals.ColorPaletteAnchorElement != getFontColorToolbarButton() &&
		globals.ColorPaletteAnchorElement != getFontBackgroundToolbarButton()) {
		return;
	}
	hideColorPaletteMenu();
}

function updateFontColorToolbarItemsToSelection() {
	if (!isSubSelectionEnabled()) {
		return;
	}
	const font_sel_idx = getSelDocIdxForFontColor();
	// NOTE these selectors are unique to the types of svg's in use right now
	// and could just apply to everything within element but looks better being specific...

	let fg_hex = getFontHexColorAtDocIdx(font_sel_idx);
	setFontColorToolbarButtonColor(fg_hex);

	let bg_hex = getFontHexBgColorAtDocIdx(font_sel_idx);
	setFontBackgroundToolbarButtonColor(bg_hex, fg_hex);
}

// #endregion Actions

// #region Event Handlers

function onFontColorOrBgColorPaletteItemClick(chex) {
	log('color palette clicked');
	if (globals.ColorPaletteAnchorElement == null) {
		return;
	}
	let sel = getDocSelection();
	if (sel && sel.length == 0) {
		// color won't change unless there's a range
		sel.length = 1;
	}
	// NOTE for setting color, use actual doc idx but 
	// to get current color (to update bg btn) need the forward idx
	const font_sel_doc_idx = getSelDocIdxForFontColor();
	let bg_chex = null;
	let fg_chex = null;
	let source = 'api';
	if (globals.ColorPaletteAnchorElement == getFontBackgroundToolbarButton()) {
		// remove theme override if preset
		removeThemeAttrFromDocRange(sel, true, source);
		formatDocRange(sel, { background: chex, 'font-bg-color-override': 'on' }, source);
		bg_chex = chex;
		fg_chex = getFontHexColorAtDocIdx(font_sel_doc_idx);
	} else {
		// remove theme override if preset
		removeThemeAttrFromDocRange(sel, false, source);
		formatDocRange(sel, { color: chex, 'font-color-override': 'on' }, source);
		bg_chex = getFontHexBgColorAtDocIdx(font_sel_doc_idx);
		fg_chex = chex;
	}
	setFontColorToolbarButtonColor(fg_chex);
	setFontBackgroundToolbarButtonColor(bg_chex, fg_chex);
}

function onFontColorOrBgToolbarButtonClick(e) {
	if (isShowingColorPaletteMenu()) {
		if (globals.ColorPaletteAnchorElement == e.currentTarget) {
			hideColorPaletteMenu();
			return;
		}
		hideColorPaletteMenu();
	}
	showFontColorPaletteMenu(e.currentTarget);
}


// #endregion Event Handlers