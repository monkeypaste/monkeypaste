

// #region Life Cycle

function initFindReplaceToolbar() {
	globals.HighlightAttrb = registerClassAttributor('highlight', 'highlight', globals.Parchment.Scope.INLINE);
	//globals.ActiveHighlightAttrb = registerClassAttributor('highlightActive', 'highlight-active', globals.Parchment.Scope.INLINE);

	addClickOrKeyClickEventListener(getFindReplaceEditorToolbarButton(), onFindReplaceToolbarButtonClick);

	getIsReplaceInputElement().addEventListener('change', onFindOrReplaceInputChange);

	getFindInputElement().addEventListener('keypress', onFindReplaceFindInputKeyDown);
	getFindInputElement().addEventListener('input', onFindReplaceFindInputTextInput);

	addClickOrKeyClickEventListener(getFindReplaceNextButton(), onFindReplaceNextButtonClick);
	addClickOrKeyClickEventListener(getFindReplacePreviousButton(), onFindReplacePreviousButtonClick);

	addClickOrKeyClickEventListener(getFindReplaceReplaceAllButton(), onFindReplaceReplaceAllButtonClick);
	addClickOrKeyClickEventListener(getFindReplaceReplaceButton(), onFindReplaceReplaceButtonClick);

	getWrapAroundInputElement().addEventListener('change', updateFindReplaceElementStates);
	getInSelectionInputElement().addEventListener('change', onFindReplaceInSelectionInput);

	initFindReplaceIcons();
	enableResize(getFindReplaceContainerElement());
}

function initFindReplaceIcons() {
	getFindReplacePreviousButton().innerHTML = getSvgHtml('arrow-left',null,false);
	getFindReplaceNextButton().innerHTML = getSvgHtml('arrow-right', null, false);
}
function loadFindReplace(searches) {
	globals.Searches = searches;

	if (searches == null) {
		if (isShowingFindReplaceToolbar()) {
			resetFindReplaceToolbar();
			hideFindReplaceToolbar();
		}
		if (globals.CurFindReplaceDocRanges) {
			hideAllScrollbars();
			resetFindReplaceResults();
		}
	} else {
		showAllScrollbars();
		//setFindReplaceInputState(searches);
		populateFindReplaceResults();
		let hl_html = getHighlightHtml();
		let range_offsets =
			globals.CurFindReplaceDocRangeRectIdxLookup.map(x =>
				`${globals.CurFindReplaceDocRangesRects[x[0]].left}|${globals.CurFindReplaceDocRangesRects[x[0]].top}`).join(' ');
		onQuerySearchRangesChanged_ntf(globals.CurFindReplaceDocRanges.length, hl_html, range_offsets);
	}
}

// #endregion Life Cycle

// #region Getters

function getInactiveMatchRangeBgColor() {
	return getElementComputedStyleProp(document.body, '--inactivematchbgcolor');
}

function getActiveMatchRangeBgColor() {
	return getElementComputedStyleProp(document.body, '--activematchbgcolor');
}

function getMatchRangeBgOpacity() {
	return parseFloat(getElementComputedStyleProp(document.body, '--highlightopacity'));
}

function getFindReplaceEditorToolbarButton() {
	return document.getElementById('findReplaceToolbarButton');
}

function getFindInputElement() {
	return document.getElementById('findInput');
}

function getIsReplaceInputElement() {
	return document.getElementById('isReplaceInput');
}

function getIsCaseSensitiveInputElement() {
	return document.getElementById('matchCaseInput');
}

function getIsWholeWordInputElement() {
	return document.getElementById('wholeWordInput');
}
function getWrapAroundInputElement() {
	return document.getElementById('wrapFindInput');
}
function getInSelectionInputElement() {
	return document.getElementById('selOnlyFindInput');
}

function getUseRegExInputElement() {
	return document.getElementById('useRegexInput');
}

function getReplaceInputElement() {
	return document.getElementById('replaceInput');
}
function getReplaceInputLabelElement() {
	return document.getElementById('replaceInputLabel');
}

function getFindReplaceNextButton() {
	return document.getElementById('findReplaceNextButton');
}

function getFindReplacePreviousButton() {
	return document.getElementById('findReplacePreviousButton');
}
function getFindReplaceReplaceButton() {
	return document.getElementById('replaceButton');
}
function getFindReplaceReplaceAllButton() {
	return document.getElementById('replaceAllButton');
}

function getFindReplaceContainerElement() {
	return document.getElementById('findReplaceContainer');
}

function getFindReplaceToolbarHeight() {
	return getFindReplaceContainerElement().getBoundingClientRect().height;
}


function getFindReplaceInputState() {	
	return {
		searchText: getFindInputElement().value,
		replaceText: getReplaceInputElement().value,
		isReplace: getIsReplaceInputElement().checked,
		isCaseSensitive: getIsCaseSensitiveInputElement().checked,
		isWholeWordMatch: getIsWholeWordInputElement().checked,
		useRegEx: getUseRegExInputElement().checked,
		wrapAround: getWrapAroundInputElement().checked,
		inSelection: getInSelectionInputElement().checked
	};
}

function getDefaultFindReplaceInputState() {	
	return {
		searchText: '',
		replaceText: '',
		isReplace: false,
		isCaseSensitive: false,
		isWholeWordMatch: false,
		useRegEx: false,
		wrapAround: true,
	};
}
function getHighlightHtml() {
	let sup_guid = suppressTextChanged();

	function toggleHighlighting(isEnabled) {
		for (var i = 0; i < globals.CurFindReplaceDocRanges.length; i++) {
			let tr = globals.CurFindReplaceDocRanges[i];
			if (isEnabled) {
				let hl_val =
					i == globals.CurFindReplaceDocRangeIdx ? 'active' : 'inactive';
				globals.quill.formatText(tr.index, tr.length, 'highlight', hl_val, 'user');
				// remove inline styles or hl won't work in rowv
				globals.quill.formatText(tr.index, tr.length, 'background-color', false, 'user');
				globals.quill.formatText(tr.index, tr.length, 'color', false, 'user');
			} else {
				globals.quill.formatText(tr.index, tr.length, 'highlight', false, 'user');
				let fmt = globals.quill.getFormat(tr.index, tr.length);
				if (fmt.themecoloroverride) {
					globals.quill.formatText(tr.index, tr.length, 'color', fmt.themecoloroverride, 'user');
				}
			}
			
		}
	}
	toggleHighlighting(true);
	let result = convertHtmlLineBreaks(getRootHtml());
	toggleHighlighting(false);
	unsupressTextChanged(sup_guid);
	return result;
}
// #endregion Getters

// #region Setters

function setFindReplaceInputState(inputState) {
	if (isNullOrUndefined(inputState)) {
		onShowDebugger_ntf('invalid input state');
		return;
	}

	getIsReplaceInputElement().checked = inputState.isReplace;

	getIsCaseSensitiveInputElement().checked = inputState.isCaseMatch;
	getIsWholeWordInputElement().checked = inputState.isWholeWordMatch;
	getUseRegExInputElement().checked = inputState.useRegEx;
	getWrapAroundInputElement().checked = inputState.wrapAround;
	getInSelectionInputElement().checked = inputState.inSelection;

	getFindInputElement().value = inputState.searchText;
	getReplaceInputElement().value = inputState.replaceText;

	toggleFindOrReplace();
}
function setFindReplaceToolbarButtonToggleState(isToggled) {
	let fr_svg_path_elms = Array.from(getFindReplaceEditorToolbarButton().querySelectorAll('path'));
	fr_svg_path_elms.forEach(x => isToggled ? x.classList.add('toggled') : x.classList.remove('toggled'));
}
// #endregion Setters

// #region State

function hasLocalSearchText() {
	return !isNullOrEmpty(getFindInputElement().value);
}

function hasFindMatches() {
	if (globals.CurFindReplaceDocRanges != null &&
		globals.CurFindReplaceDocRanges.length > 0) {
		return true;
	}
	return false;
}

function isAnySearchState() {
	if (isGlobalSearchState() ||
		hasLocalSearchText() ||
		globals.CurFindReplaceDocRanges != null) {
		return true;
	}
	return false; 
}
function isGlobalSearchState() {
	return globals.Searches != null;
}

function isShowingFindReplaceToolbar() {
	return !getFindReplaceContainerElement().classList.contains('hidden');
}

function isShowingFindTab() {
	return getReplaceInputElement().classList.contains('hidden');
}

function isShowingReplaceTab() {
	return !getReplaceInputElement().classList.contains('hidden');
}

function isFindReplaceStateChanged() {
	if (isGlobalSearchState()) {
		return false;
	}
	if (globals.LastFindReplaceInputState == null ||
		globals.LastFindReplaceInputState.searchText === undefined) {
		return true;
	}

	let cur_state = getFindReplaceInputState();
	if (cur_state.searchText != globals.LastFindReplaceInputState.searchText) {
		return true;
	}
	if (cur_state.replaceText != globals.LastFindReplaceInputState.replaceText) {
		return true;
	}
	if (cur_state.isReplace != globals.LastFindReplaceInputState.isReplace) {
		return true;
	}
	if (cur_state.isCaseSensitive != globals.LastFindReplaceInputState.isCaseSensitive) {
		return true;
	}
	if (cur_state.isWholeWordMatch != globals.LastFindReplaceInputState.isWholeWordMatch) {
		return true;
	}
	if (cur_state.useRegEx != globals.LastFindReplaceInputState.useRegEx) {
		return true;
	}
	return false;
}

function isFindReplaceActive() {
	let is_active = Array.from(getFindReplaceEditorToolbarButton().querySelectorAll('path'))[0].classList.contains('toggled');
	return is_active;
}
// #endregion State

// #region Actions

function showFindReplaceToolbar(fromHost = false) {
	activateFindReplace();

	getFindReplaceContainerElement().classList.remove('hidden');
	setFindReplaceToolbarButtonToggleState(true);

	let inputState = globals.LastFindReplaceInputState || getDefaultFindReplaceInputState();
	globals.LastFindReplaceInputState = null;

	if (isNullOrEmpty(inputState.searchText)) {
		let sel = getDocSelection();
		if (sel.length > 0) {
			inputState.searchText = getText(sel);
		}
	}

	setFindReplaceInputState(inputState);
	updateFindReplaceElementStates();

	updateAllElements();

	populateFindReplaceResults();

	if (!fromHost) {
		onFindReplaceVisibleChange_ntf(true);
	}
}

function hideFindReplaceToolbar(fromHost = false) {
	deactivateFindReplace();
	getFindReplaceContainerElement().classList.add('hidden');
	resetFindReplaceResults();
	updateAllElements();

	if (!fromHost) {
		onFindReplaceVisibleChange_ntf(false);
	}
}

function toggleFindOrReplace() {
	Array.from(getFindReplaceContainerElement().getElementsByClassName('is-replace')).forEach(x => {
		if (getIsReplaceInputElement().checked) {
			x.classList.remove('hidden');
		} else {
			x.classList.add('hidden');
		}
	});
}

function updateFindReplaceToolbarSizesAndPositions() {
	if (isShowingFindReplaceToolbar()) {
		let et_bottom = getEditorToolbarElement().getBoundingClientRect().bottom;
		getFindReplaceContainerElement().style.top = et_bottom + 'px';
	}
}

function resetFindReplaceResults() {
	globals.CurFindReplaceDocRanges = null;
	globals.CurFindReplaceDocRangeIdx = -1;

	globals.CurFindReplaceDocRangesRects = null;
	globals.CurFindReplaceDocRangeRectIdxLookup = null;
}

function resetFindReplaceInput() {
	setFindReplaceInputState(getDefaultFindReplaceInputState());
}

function resetFindReplaceToolbar() {
	resetFindReplaceInput();
	resetFindReplaceResults();
}

function adjustQueryRangesForEmptyContent(rangesWithMatchVal) {
	// HACK leading embed's and listitems underset range.index,
	// this scans from given range until provided text is matched
	const maxIdx = getDocLength();
	let adj_ranges = [];
	for (var i = 0; i < rangesWithMatchVal.length; i++) {
		const match_value = rangesWithMatchVal[i].text.toLowerCase();
		// NOTE duplicating range so param remains intact 
		let adj_range = { index: rangesWithMatchVal[i].index, length: rangesWithMatchVal[i].length };
		while (true) {
			if (adj_range.index > maxIdx) {
				onShowDebugger_ntf($`adj query range error, can't find ${rangesWithMatchVal[i].text}`);
				break;
			}
			if (getText(adj_range).toLowerCase() == match_value) {
				break;
			}
			// move forward until offset is correct
			adj_range.index++;
		}
		//let pre_template_count = getTemplateCountBeforeDocIdx(cur_doc_idx);
		//let pre_list_item_count = getListItemCountBeforeDocIdx(cur_doc_idx);
		//adj_range.index += pre_template_count + pre_list_item_count;

		adj_ranges.push(adj_range);
	}
	return adj_ranges;
}

function processSearch(searchObj) {
	if (isNullOrUndefined(searchObj)) {
		onShowDebugger_ntf('missing searchObj');
		return;
	}

	if (isNullOrEmpty(searchObj.searchText)) {
		resetFindReplaceResults();
		updateFindReplaceRangeRects();
		return;
	}
	let sel = getInSelectionInputElement().checked ? getDocSelection() : null;

	let dirty_ranges_with_match_text = queryText(
		trimQuillTrailingLineEndFromText(getText(sel)),
		searchObj.searchText,
		searchObj.isCaseSensitive,
		searchObj.isWholeWordMatch,
		searchObj.useRegEx,
		searchObj.matchType);

	if (sel) {
		// adjust ranges by sel offset
		for (var i = 0; i < dirty_ranges_with_match_text.length; i++) {
			dirty_ranges_with_match_text[i].index += sel.index;
		}
	}

	if (globals.CurFindReplaceDocRanges == null) {
		globals.CurFindReplaceDocRanges = [];
	}
	const clean_ranges = adjustQueryRangesForEmptyContent(dirty_ranges_with_match_text);

	globals.CurFindReplaceDocRanges.push(...clean_ranges);
}

function populateFindReplaceResults() {
	if (!isAnySearchState()) {
		return;
	}
	// CLEAR
	resetFindReplaceResults();

	// FIND

	if (isGlobalSearchState()) {
		for (var i = 0; i < globals.Searches.length; i++) {
			processSearch(globals.Searches[i]);
		}
	} else {
		const input_search_obj = getFindReplaceInputState();
		processSearch(input_search_obj);
	}

	// FILTER

	globals.CurFindReplaceDocRanges = distinct(globals.CurFindReplaceDocRanges);

	// SORT
	globals.CurFindReplaceDocRanges.sort((a, b) => {
		// sort by docIdx then by range length
		let result = a.index - b.index;
		if (result == 0) {
			result = a.length = b.length;
		}
		return result;
	})

	if (globals.CurFindReplaceDocRanges.length == 0) {
		resetFindReplaceResults();
	} else {
		globals.CurFindReplaceDocRangeIdx = 0;
	}

	if (!Array.isArray(globals.CurFindReplaceDocRanges)) {
		// this seems to happen on a query search where content
		// isn't what has the match
		globals.CurFindReplaceDocRanges = [];
	}
	updateFindReplaceRangeRects();

	updateFindReplaceElementStates();
	
	navigateFindReplaceResults(0);
}

function replaceFindResultIdx(replace_idx) {
	let replace_range = globals.CurFindReplaceDocRanges[replace_idx];
	let replace_text = getReplaceInputElement().value;
	replace_text = !replace_text ? '' : replace_text;

	setTextInRange(replace_range, replace_text, true);

	populateFindReplaceResults();

	if (replace_idx < globals.CurFindReplaceDocRanges.length) {
		// when there is remaining replacable result before eod set current to it 
		globals.CurFindReplaceDocRangeIdx = replace_idx;
		drawOverlay();
	}
}

function updateFindReplaceRangeRects() {
	//setDocSelectionRanges(globals.CurFindReplaceDocRanges);

	if (globals.CurFindReplaceDocRanges == null) {
		drawOverlay();
		return;
	}
	globals.CurFindReplaceDocRangesRects = [];
	globals.CurFindReplaceDocRangeRectIdxLookup = [];
	for (var i = 0; i < globals.CurFindReplaceDocRanges.length; i++) {
		let cur_range_rects = getRangeRects(globals.CurFindReplaceDocRanges[i], true, false);

		let rect_lookup = [
			globals.CurFindReplaceDocRangesRects.length,
			globals.CurFindReplaceDocRangesRects.length + cur_range_rects.length - 1
		];
		globals.CurFindReplaceDocRangeRectIdxLookup.push(rect_lookup);
		for (var j = 0; j < cur_range_rects.length; j++) {
			let cur_range_rect = cleanRect(cur_range_rects[j]);
			globals.CurFindReplaceDocRangesRects.push(cur_range_rect);
		}
	}
	drawOverlay();
}

function updateFindReplaceElementStates() {
	const has_matches = hasFindMatches();

	Array.from(getFindReplaceContainerElement().querySelectorAll('input[type=text]'))
		.forEach(x => has_matches ? x.classList.remove('invalid') : x.classList.add('invalid'));
	Array.from(getFindReplaceContainerElement().querySelectorAll('button'))
		.forEach(x => has_matches ? x.classList.remove('disabled') : x.classList.add('disabled'));

	if (!has_matches) {
		return;
	}
	const wrap_nav = getFindReplaceInputState().wrapAround;

	const is_prev_disabled = !wrap_nav && globals.CurFindReplaceDocRangeIdx == 0;
	const is_nav_tail = !wrap_nav && globals.CurFindReplaceDocRanges && globals.CurFindReplaceDocRangeIdx == globals.CurFindReplaceDocRanges.length - 1;

	is_prev_disabled ?
		getFindReplacePreviousButton().classList.add('disabled') :
		getFindReplacePreviousButton().classList.remove('disabled');

	is_nav_tail ?
		getFindReplaceNextButton().classList.add('disabled') :
		getFindReplaceNextButton().classList.remove('disabled');
}

function applyRangeRectStyle(isActive, range_rect) {
	range_rect.fill = isActive && isFindReplaceActive() ?
		getActiveMatchRangeBgColor() : getInactiveMatchRangeBgColor();
	range_rect.fillOpacity = getMatchRangeBgOpacity();
	range_rect.strokeWidth = 0;

	return range_rect;
}

function navigateFindReplaceResults(dir) {
	let needsPopulate = globals.CurFindReplaceDocRanges == null || isFindReplaceStateChanged();
	if (needsPopulate) {
		globals.LastFindReplaceInputState = getFindReplaceInputState();
		populateFindReplaceResults();
		if (globals.CurFindReplaceDocRanges == null) {
			// TODO show validate here
			return;
		}
	}
	globals.CurFindReplaceDocRangeIdx += dir;
	if (globals.CurFindReplaceDocRangeIdx >= globals.CurFindReplaceDocRanges.length) {
		globals.CurFindReplaceDocRangeIdx = 0;
	}

	if (globals.CurFindReplaceDocRangeIdx < 0) {
		globals.CurFindReplaceDocRangeIdx = globals.CurFindReplaceDocRanges.length - 1;
	}

	let cur_doc_range = globals.CurFindReplaceDocRanges[globals.CurFindReplaceDocRangeIdx];
	if (cur_doc_range) {
		//let cur_doc_range_head_rect = getRangeRects({ index: cur_doc_range.index, length: 0 }, true, true, false)[0];
		//scrollToEditorLoc(cur_doc_range_head_rect.left, cur_doc_range_head_rect.top);
		const scroll_opts = {
			start: { behavior: 'auto', block: 'center', inline: 'center' },
			end: { behavior: 'auto', block: 'center', inline: 'center' }
		};
		scrollDocRangeIntoView(cur_doc_range, scroll_opts);
	}
}

function deactivateFindReplace(redraw = true) {
	setFindReplaceToolbarButtonToggleState(false);
	if (redraw) {
		drawOverlay();
	}	
}
function activateFindReplace(redraw = true) {
	setFindReplaceToolbarButtonToggleState(true);
	if (redraw) {
		drawOverlay();
	}
}

// #endregion Actions

// #region Event Handlers

function onFindReplaceFindInputKeyDown(e) {
	if (e.key != 'Enter') {
		return;
	}
	populateFindReplaceResults();
}
function onFindReplaceFindInputTextInput(e) {
	populateFindReplaceResults();
}

function onFindReplaceInSelectionInput(e) {
	if (getInSelectionInputElement().checked) {
		getEditorElement().addEventListener('mouseup', populateFindReplaceResults);
	} else {
		getEditorElement().removeEventListener('mouseup', populateFindReplaceResults);
	}
	populateFindReplaceResults();
}

function onFindReplaceToolbarButtonClick(e) {
	if (isShowingFindReplaceToolbar()) {
		hideFindReplaceToolbar();
	} else {
		showFindReplaceToolbar();
	}
}

function onFindOrReplaceInputChange(e) {
	updateFindReplaceElementStates();
	toggleFindOrReplace();
}
function onWrapAroundInputChange(e) {
	updateFindReplaceElementStates();
}

function onFindReplaceNextButtonClick(e) {
	navigateFindReplaceResults(1);
	updateFindReplaceElementStates();
}

function onFindReplacePreviousButtonClick(e) {
	navigateFindReplaceResults(-1);
	updateFindReplaceElementStates();
}
function onFindReplaceReplaceButtonClick(e) {
	replaceFindResultIdx(globals.CurFindReplaceDocRangeIdx);
}
function onFindReplaceReplaceAllButtonClick(e) {
	let replace_count = globals.CurFindReplaceDocRanges.length;
	while (replace_count > 0) {
		replaceFindResultIdx(0);
		replace_count--;
	}
}

// #endregion Event Handlers