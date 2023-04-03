// #region Globals

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters
function getOverlayTooltipElement() {
	return document.getElementById('tooltipOverlay');
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function showOverlayTooltip(targetElm, innerHtmlStr) {
	let tt_elm = getOverlayTooltipElement();
	tt_elm.innerHTML = `<span class="tooltiptext">${innerHtmlStr}</span>`;
	let target_rect = targetElm.getBoundingClientRect();
	tt_elm.classList.remove('hidden');
	let tt_rect = tt_elm.getBoundingClientRect();
	let tt_x = Math.max(0,target_rect.left + (target_rect.width / 2) - (tt_rect.width / 2));
	setElementComputedStyleProp(tt_elm, 'margin-left', `${tt_x}px`);
	let tt_y = Math.max(0,target_rect.top - tt_rect.height - 5);
	setElementComputedStyleProp(tt_elm, 'margin-top', `${tt_y}px`);
}

function hideOverlayTooltip() {
	getOverlayTooltipElement().classList.add('hidden');
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers