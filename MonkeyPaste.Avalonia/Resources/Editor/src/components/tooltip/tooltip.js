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
			showTooltipToolbar(e.currentTarget, e.currentTarget.getAttribute(globals.TOOLTIP_TOOLBAR_ATTRB_NAME));
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
	if (isRunningInHost()) {
		let target_loc = getRectCenter(cleanRect(targetElm.getBoundingClientRect()));
		onShowToolTip_ntf(true, target_loc, tooltipText, parseShortcutText(tooltipText));
		return;
	}

	tooltipText = decodeStringWithShortcut(tooltipText);
	let tt_elm = getTooltipOverlayElement();
	if (is_html) {
		tt_elm.innerHTML = tooltipText;
	} else {
		tt_elm.innerHTML = `<span class="tooltiptext">${tooltipText}</span>`;
	}
	tt_elm.classList.remove('hidden');

	positionTooltipOverlayLocation(targetElm, tt_elm);
}

function hideTooltipOverlay() {
	if (isRunningInHost()) {
		onShowToolTip_ntf(false);
		return;
	}
	getTooltipOverlayElement().classList.add('hidden');
}


function showTooltipToolbar(targetElm, htmlStr, showTimeMs = 0) {
	if (isRunningInHost()) {
		onShowToolTip_ntf(true, getRectCenter(cleanRect(targetElm.getBoundingClientRect())), htmlStr);
		return;
	}
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
	if (isRunningInHost()) {
		onShowToolTip_ntf(false);
		return;
	}
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


function positionTooltipOverlayLocation(targetElm, tooltipElm) {
	const editor_rect = cleanRect(getEditorElement().getBoundingClientRect());
	let target_rect = cleanRect(targetElm.getBoundingClientRect());
	let tt_rect = cleanRect(tooltipElm.getBoundingClientRect());

	// start with tooltip bottom center of targets 
	let start_loc = {
		x: target_rect.left + (target_rect.width / 2) - (tt_rect.width / 2),
		y: target_rect.bottom + 10
	}

	tt_rect = moveAbsoluteElement(tooltipElm, start_loc);


	// top,right,bottom, left,
	//let best_rect = tt_rect;
	//const dirs = [{ x: 0, y: -1, dir: 'top' }, { x: 1, y: 0, dir: 'right' },{ x: 0, y: 1, dir: 'bottom' }, { x: -1, y: 0, dir: 'left' }];
	//for (var i = 0; i < dirs.length; i++) {
	//	const dir = dirs[i];
	//	let cur_loc = start_loc;
	//	while (true) {
	//		cur_loc = addPoints(cur_loc, dir);
	//		best_rect = moveAbsoluteElement(tooltipElm, cur_loc);
	//		if (!isRectOverlapOtherRect(best_rect, target_rect)) {
	//			return;
	//		}
	//		if (!isPointInRect(editor_rect, getRectCornerByIdx(best_rect, i))) {
	//			// this dir no good, rect outside editor
	//			break;
	//		}
			
	//	}
	//}
	//tt_rect = best_rect;
	// fallback to center above
	//start_loc.y = target_rect.top - tt_rect.height;
	//moveAbsoluteElement(tooltipElm, start_loc);
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers