

// #region Life Cycle

function initFindReplaceToolbar() {
	addClickOrKeyClickEventListener(getFindReplaceEditorToolbarButton(), onFindReplaceToolbarButtonClick);

	getIsReplaceInputElement().addEventListener('change', onFindOrReplaceRadioChange);

	getFindInputElement().addEventListener('keypress', onFindReplaceFindInputKeyDown);
	getFindInputElement().addEventListener('input', onFindReplaceFindInputTextInput);

	addClickOrKeyClickEventListener(getFindReplaceNextButton(), onFindReplaceNextButtonClick);
	addClickOrKeyClickEventListener(getFindReplacePreviousButton(), onFindReplacePreviousButtonClick);

	addClickOrKeyClickEventListener(getFindReplaceReplaceAllButton(), onFindReplaceReplaceAllButtonClick);
	addClickOrKeyClickEventListener(getFindReplaceReplaceButton(), onFindReplaceReplaceButtonClick);

	initFindReplaceIcons();
	enableResize(getFindReplaceToolbarElement());
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
		onQuerySearchRangesChanged_ntf(globals.CurFindReplaceDocRanges.length);
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

function getFindReplaceToolbarElement() {
	return document.getElementById('findReplaceToolbar');
}

function getFindReplaceToolbarHeight() {
	return getFindReplaceToolbarElement().getBoundingClientRect().height;
}


function getFindReplaceInputState() {	
	return {
		searchText: getFindInputElement().value,
		replaceText: getReplaceInputElement().value,
		isReplace: getIsReplaceInputElement().checked,
		isCaseSensitive: getIsCaseSensitiveInputElement().checked,
		isWholeWordMatch: getIsWholeWordInputElement().checked,
		useRegEx: getUseRegExInputElement().checked
	};
}

function getDefaultFindReplaceInputState() {	
	return {
		searchText: '',
		replaceText: '',
		isReplace: false,
		isCaseSensitive: false,
		isWholeWordMatch: false,
		useRegEx: false
	};
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
	return !getFindReplaceToolbarElement().classList.contains('hidden');
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

	getFindReplaceToolbarElement().classList.remove('hidden');
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

	updateAllElements();

	if (!fromHost) {
		onFindReplaceVisibleChange_ntf(true);
	}
}

function hideFindReplaceToolbar(fromHost = false) {
	deactivateFindReplace();
	getFindReplaceToolbarElement().classList.add('hidden');
	resetFindReplaceResults();
	updateAllElements();

	if (!fromHost) {
		onFindReplaceVisibleChange_ntf(false);
	}
}

function toggleFindOrReplace() {
	Array.from(getFindReplaceToolbarElement().getElementsByClassName('is-replace')).forEach(x => {
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
		getFindReplaceToolbarElement().style.top = et_bottom + 'px';
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

	let dirty_ranges_with_match_text = queryText(
		trimQuillTrailingLineEndFromText(getText()),
		searchObj.searchText,
		searchObj.isCaseSensitive,
		searchObj.isWholeWordMatch,
		searchObj.useRegEx,
		searchObj.matchType);

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
	let cur_dom_range = convertDocRangeToDomRange(cur_doc_range);
	if (cur_dom_range && cur_dom_range.startContainer) {
		if (cur_dom_range.startContainer.nodeType === 3) {
			cur_dom_range.startContainer.parentNode.scrollIntoView();
		} else {
			cur_dom_range.startContainer.scrollIntoView();
		}
	}
	//let cur_doc_range_scroll_offset = getDocRangeScrollOffset(cur_doc_range);
	//scrollEditorTop(cur_doc_range_scroll_offset.top);

	//let active_y = globals.CurFindReplaceDocRangesRects[globals.CurFindReplaceDocRangeRectIdxLookup[globals.CurFindReplaceDocRangeIdx][0]].top;
	//scrollEditorTop(active_y);

	//drawOverlay();
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

function onFindReplaceToolbarButtonClick(e) {
	if (isShowingFindReplaceToolbar()) {
		hideFindReplaceToolbar();
	} else {
		showFindReplaceToolbar();
	}
}

function onFindOrReplaceRadioChange(e) {
	toggleFindOrReplace();
}

function onFindReplaceNextButtonClick(e) {
	navigateFindReplaceResults(1);
}

function onFindReplacePreviousButtonClick(e) {
	navigateFindReplaceResults(-1);
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