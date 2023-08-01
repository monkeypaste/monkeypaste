// #region Life Cycle

function initTooltip() {

	const hover_tt_elms = getTooltipHoverElements();
	for (var i = 0; i < hover_tt_elms.length; i++) {
		const htt_elm = hover_tt_elms[i];
		htt_elm.addEventListener('pointerenter', (e) => {
			showTooltipOverlay(e.currentTarget);
		});
		htt_elm.addEventListener('pointerleave', (e) => {
			hideTooltipOverlay();
		});
	}

	const toolbar_tt_elms = getTooltipToolbarlements();
	for (var i = 0; i < toolbar_tt_elms.length; i++) {
		const ttt_elm = toolbar_tt_elms[i];
		ttt_elm.addEventListener('pointerenter', (e) => {
			showTooltipToolbar(e.currentTarget.getAttribute(globals.TOOLTIP_TOOLBAR_ATTRB_NAME));
		});
		ttt_elm.addEventListener('pointerleave', (e) => {
			hideTooltipToolbar();
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
	return Array.from(document.querySelectorAll(`[${globals.TOOLTIP_HOVER_ATTRB_NAME}]`));
}
function getTooltipToolbarlements() {
	return Array.from(document.querySelectorAll(`[${globals.TOOLTIP_TOOLBAR_ATTRB_NAME}]`));
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
		targetElm.hasAttribute(globals.TOOLTIP_HOVER_ATTRB_NAME)) {
		// only use attrb value if exists and not provided in param
		tooltipText = targetElm.getAttribute(globals.TOOLTIP_HOVER_ATTRB_NAME);
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


function showTooltipToolbar(htmlStr, showTimeMs = 0) {
	if (!globals.IsTooltipToolbarEnabled) {
		return;
	}
	let tt_elm = getTooltipToolbarElement();
	tt_elm.innerHTML = htmlStr;
	if (tt_elm && tt_elm.firstChild && tt_elm.firstChild.nodeType === 3 ||
		!tt_elm.firstChild.classList.contains('tooltiptext')) {
		// presume any html passed will be setup for tooltip
		tt_elm.innerHTML = `<span class="tooltiptext">${htmlStr}</span>`;
	}
	tt_elm.classList.remove('hidden');
	updateTooltipToolbarSizesAndPositions();

	if (showTimeMs > 0) {
		delay(showTimeMs)
			.then(() => {
				hideTooltipToolbar();
			});
	}
}

function hideTooltipToolbar() {
	if (!globals.IsTooltipToolbarEnabled) {
		return;
	}
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
		tttb_bottom += getPasteToolbarContainerElement().getBoundingClientRect().height;
	}
	if (isShowingEditTemplateToolbar()) {
		tttb_bottom += getEditTemplateToolbarContainerElement().getBoundingClientRect().height;
	} 

	const tttb_elm = getTooltipToolbarElement();
	tttb_elm.style.bottom = `${tttb_bottom}px`;
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers