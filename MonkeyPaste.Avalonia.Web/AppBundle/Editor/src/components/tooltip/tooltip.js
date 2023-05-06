// #region Globals

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters
function getTooltipOverlayElement() {
	return document.getElementById('tooltipOverlay');
}

function getTooltipToolbarElement() {
	return document.getElementById('tooltipToolbar');
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isShowingTooltipToolbar() {
	return !getTooltipToolbarElement().classList.contains('hidden');
}

// #endregion State

// #region Actions

function showTooltipOverlay(targetElm, innerHtmlStr) {
	let tt_elm = getTooltipOverlayElement();
	tt_elm.innerHTML = `<span class="tooltiptext">${innerHtmlStr}</span>`;
	let target_rect = targetElm.getBoundingClientRect();
	tt_elm.classList.remove('hidden');
	let tt_rect = tt_elm.getBoundingClientRect();
	let tt_x = Math.max(0,target_rect.left + (target_rect.width / 2) - (tt_rect.width / 2));
	setElementComputedStyleProp(tt_elm, 'margin-left', `${tt_x}px`);
	let tt_y = Math.max(0,target_rect.top - tt_rect.height - 5);
	setElementComputedStyleProp(tt_elm, 'margin-top', `${tt_y}px`);
}

function hideTooltipOverlay() {
	getTooltipOverlayElement().classList.add('hidden');
}


function showTooltipToolbar(innerHtmlStr) {
	let tt_elm = getTooltipToolbarElement();
	tt_elm.innerHTML = `<span class="tooltiptext">${innerHtmlStr}</span>`;
	tt_elm.classList.remove('hidden');
	updateTooltipToolbarSizesAndPositions();
}

function hideTooltipToolbar() {
	getTooltipToolbarElement().style.bottom = '0px';
	delay(getToolbarTransitionMs())
		.then(() => {
			getTooltipToolbarElement().classList.add('hidden');
		});
}
function updateTooltipToolbarSizesAndPositions() {
	if (!isShowingTooltipToolbar()) {
		return;
	}
	let tttb_bottom = 0;
	if (isShowingPasteToolbar()) {
		//ett.classList.remove('bottom-align');
		tttb_bottom += getPasteToolbarContainerElement().getBoundingClientRect().height;
	}
	if (isShowingEditTemplateToolbar()) {
		//ett.classList.remove('bottom-align');
		tttb_bottom += getEditTemplateToolbarContainerElement().getBoundingClientRect().height;
	} 

	const tttb_elm = getTooltipToolbarElement();
	tttb_elm.style.bottom = `${tttb_bottom}px`;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers