// #region Globals

const TOOLTIP_HOVER_ATTRB_NAME = "hover-tooltip";

// #endregion Globals

// #region Life Cycle

function initTooltip() {
	const htt_elms = getTooltipHoverElements();
	for (var i = 0; i < htt_elms.length; i++) {
		const htt_elm = htt_elms[i];
		htt_elm.addEventListener('pointerenter', (e) => {
			showTooltipOverlay(e.currentTarget);
		});
		htt_elm.addEventListener('pointerleave', (e) => {
			hideTooltipOverlay();
		});
	}
}

// #endregion Life Cycle

// #region Getters

function getTooltipOverlayElement() {
	return document.getElementById('tooltipOverlay');
}

function getTooltipToolbarElement() {
	return document.getElementById('tooltipToolbar');
}

function getTooltipHoverElements() {
	return Array.from(document.querySelectorAll(`[${TOOLTIP_HOVER_ATTRB_NAME}]`));
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

function showTooltipOverlay(targetElm, tooltipText) {
	let is_html = false;
	if (isNullOrUndefined(tooltipText) &&
		targetElm &&
		typeof targetElm.hasAttribute === 'function' &&
		targetElm.hasAttribute(TOOLTIP_HOVER_ATTRB_NAME)) {
		// only use attrb value if exists and not provided in param
		tooltipText = targetElm.getAttribute(TOOLTIP_HOVER_ATTRB_NAME);
		if (isNullOrEmpty(tooltipText)) {
			tooltipText = targetElm.innerHTML;
			is_html = true;
		}
	}
	if (isNullOrEmpty(tooltipText)) {
		// don't show empty tooltip
		return;
	}
	tooltipText = decodeStringWithShortcut(tooltipText);

	let tt_elm = getTooltipOverlayElement();
	if (is_html) {
		tt_elm.innerHTML = tooltipText;
	} else {
		tt_elm.innerHTML = `<span class="tooltiptext">${tooltipText}</span>`;
	}
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