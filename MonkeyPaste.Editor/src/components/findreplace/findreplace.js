// #region Globals

var CurFindReplaceDocRanges = null;
var CurFindReplaceDocRangeIdx = -1;

var CurFindReplaceDocRangeRectIdxLookup = null;

var CurFindReplaceDocRangesRects = null;

const DefaultFindReplaceInputState = {
	searchText: '',
	replaceText: '',
	isReplace: false,
	isCaseSensitive: false,
	isWholeWordMatch: false,
	useRegEx: false
};

var LastFindReplaceInputState = null;

// #endregion Globals

// #region Life Cycle

function initFindReplaceToolbar() {
	addClickOrKeyClickEventListener(getFindReplaceEditorToolbarButton(), onFindReplaceToolbarButtonClick);
	getFindReplaceEditorToolbarButton().innerHTML = getSvgHtml('findreplace')

	getIsFindInputElement().addEventListener('change', onFindOrReplaceRadioChange);
	getIsReplaceInputElement().addEventListener('change', onFindOrReplaceRadioChange);

	getFindInputElement().addEventListener('keypress', onFindReplaceFindInputKeyDown);

	addClickOrKeyClickEventListener(getFindReplaceCloseButtonElement(), onFindReplaceCloseButtonClick);

	addClickOrKeyClickEventListener(getFindReplaceNextButton(), onFindReplaceNextButtonClick);
	addClickOrKeyClickEventListener(getFindReplacePreviousButton(), onFindReplacePreviousButtonClick);

	addClickOrKeyClickEventListener(getFindReplaceReplaceAllButton(), onFindReplaceReplaceAllButtonClick);
	addClickOrKeyClickEventListener(getFindReplaceReplaceButton(), onFindReplaceReplaceButtonClick);

	enableResize(getFindReplaceToolbarElement());
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

function getIsFindInputElement() {
	return document.getElementById('isFindInput');
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

function getFindReplaceCloseButtonElement() {
	return document.getElementById('closeFindReplaceToolbarButton');
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

// #endregion Getters

// #region Setters

function setFindReplaceInputState(inputState) {
	if (!inputState) {
		debugger;
		return;
	}

	getIsReplaceInputElement().checked = inputState.isReplace;
	getIsFindInputElement().checked = !inputState.isReplace;

	getIsCaseSensitiveInputElement().checked = inputState.isCaseMatch;
	getIsWholeWordInputElement().checked = inputState.isWholeWordMatch;
	getUseRegExInputElement().checked = inputState.useRegEx;

	getFindInputElement().value = inputState.searchText;
	getReplaceInputElement().value = inputState.replaceText;

	toggleFindOrReplace();
}

// #endregion Setters

// #region State

function isShowingFindReplaceToolbar() {
	return !getFindReplaceToolbarElement().classList.contains('hidden');
}

function isShowingFindTab() {
	return getReplaceInputElement().classList.contains('hidden');
}

function isShowingReplaceTab() {
	return !getReplaceInputElement().classList.contains('hidden');
}

function isFindReplaceActive() {
	if (!isShowingFindReplaceToolbar()) {
		return false;
	}
	if (quill.hasFocus()) {
		return false;
	}
	// do other toolbar focuses matter here?
	return true;
}

function isFindReplaceStateChanged() {
	if (LastFindReplaceInputState == null || LastFindReplaceInputState.searchText === undefined) {
		return true;
	}

	let cur_state = getFindReplaceInputState();
	if (cur_state.searchText != LastFindReplaceInputState.searchText) {
		return true;
	}
	if (cur_state.replaceText != LastFindReplaceInputState.replaceText) {
		return true;
	}
	if (cur_state.isReplace != LastFindReplaceInputState.isReplace) {
		return true;
	}
	if (cur_state.isCaseSensitive != LastFindReplaceInputState.isCaseSensitive) {
		return true;
	}
	if (cur_state.isWholeWordMatch != LastFindReplaceInputState.isWholeWordMatch) {
		return true;
	}
	if (cur_state.useRegEx != LastFindReplaceInputState.useRegEx) {
		return true;
	}
	return false;
}

// #endregion State

// #region Actions

function showFindReplaceToolbar(fromHost = false) {
	getFindReplaceToolbarElement().classList.remove('hidden');

	let inputState = LastFindReplaceInputState ? LastFindReplaceInputState : DefaultFindReplaceInputState;
	LastFindReplaceInputState = null;

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
	getFindReplaceToolbarElement().classList.add('hidden');
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
	CurFindReplaceSearchText = null;

	CurFindReplaceDocRanges = null;
	CurFindReplaceDocRangeIdx = -1;

	CurFindReplaceDocRangesRects = null;
	CurFindReplaceDocRangeRectIdxLookup = null;
}

function resetFindReplaceInput() {
	setFindReplaceInputState(DefaultFindReplaceInputState);
}

function resetFindReplaceToolbar() {
	resetFindReplaceInput();
	resetFindReplaceResults();
}

function populateFindReplaceResults() {
	resetFindReplaceResults();

	let search_text = getFindInputElement().value;
	let is_case_sensitive = getIsCaseSensitiveInputElement().checked;
	let is_whole_word = getIsWholeWordInputElement().checked;
	let use_regex = getUseRegExInputElement().checked;	

	let sel = getDocSelection();
	if (sel && sel.length > 0) {		
		// when text is selected unselect but retain caret idx
		sel.length = 0;
		setDocSelection(sel.index, 0);
	}
	sel = sel ? sel : { index: 0, length: 0 };

	if (isNullOrEmpty(search_text)) {
		resetFindReplaceResults();
		updateFindReplaceRangeRects();
		return;
	} 
	let dirty_ranges = queryText(
		getText(),
		search_text,
		is_case_sensitive,
		is_whole_word,
		use_regex);

	CurFindReplaceDocRanges = adjustQueryRangesForEmptyContent(dirty_ranges);

	if (CurFindReplaceDocRanges.length == 0) {
		resetFindReplaceResults();
	} else {
		CurFindReplaceDocRangeIdx = 0;
	}

	if (!Array.isArray(CurFindReplaceDocRanges)) {
		// this seems to happen on a query search where content
		// isn't what has the match
		CurFindReplaceDocRanges = [];
	}
	updateFindReplaceRangeRects();
	
	navigateFindReplaceResults(0);
}

function replaceFindResultIdx(replace_idx) {
	let replace_range = CurFindReplaceDocRanges[replace_idx];
	let replace_text = getReplaceInputElement().value;
	replace_text = !replace_text ? '' : replace_text;

	setTextInRange(replace_range, replace_text, true);

	populateFindReplaceResults();

	if (replace_idx < CurFindReplaceDocRanges.length) {
		// when there is remaining replacable result before eod set current to it 
		CurFindReplaceDocRangeIdx = replace_idx;
		drawOverlay();
	}
}

function updateFindReplaceRangeRects() {
	//setDocSelectionRanges(CurFindReplaceDocRanges);

	if (CurFindReplaceDocRanges == null) {
		drawOverlay();
		return;
	}
	CurFindReplaceDocRangesRects = [];
	CurFindReplaceDocRangeRectIdxLookup = [];
	for (var i = 0; i < CurFindReplaceDocRanges.length; i++) {
		let cur_range_rects = getRangeRects(CurFindReplaceDocRanges[i], true, false);

		let rect_lookup = [
			CurFindReplaceDocRangesRects.length,
			CurFindReplaceDocRangesRects.length + cur_range_rects.length - 1
		];
		CurFindReplaceDocRangeRectIdxLookup.push(rect_lookup);
		for (var j = 0; j < cur_range_rects.length; j++) {
			let cur_range_rect = cleanRect(cur_range_rects[j]);
			CurFindReplaceDocRangesRects.push(cur_range_rect);
		}
	}
	drawOverlay();
}

function applyRangeRectStyle(isActive, range_rect) {	
	range_rect.fill = isActive ?
		getActiveMatchRangeBgColor() : getInactiveMatchRangeBgColor();
	range_rect.fillOpacity = getMatchRangeBgOpacity();
	range_rect.strokeWidth = 0;

	return range_rect;
}

function navigateFindReplaceResults(dir) {
	let needsPopulate = CurFindReplaceDocRanges == null || isFindReplaceStateChanged();
	if (needsPopulate) {
		LastFindReplaceInputState = getFindReplaceInputState();
		populateFindReplaceResults();
		if (CurFindReplaceDocRanges == null) {
			// TODO show validate here
			return;
		}
	}
	CurFindReplaceDocRangeIdx += dir;
	if (CurFindReplaceDocRangeIdx >= CurFindReplaceDocRanges.length) {
		CurFindReplaceDocRangeIdx = 0;
	}

	if (CurFindReplaceDocRangeIdx < 0) {
		CurFindReplaceDocRangeIdx = CurFindReplaceDocRanges.length - 1;
	}

	let cur_doc_range = CurFindReplaceDocRanges[CurFindReplaceDocRangeIdx];
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

	//let active_y = CurFindReplaceDocRangesRects[CurFindReplaceDocRangeRectIdxLookup[CurFindReplaceDocRangeIdx][0]].top;
	//scrollEditorTop(active_y);

	//drawOverlay();
}

// #endregion Actions

// #region Event Handlers

function onFindReplaceFindInputKeyDown(e) {
	if (e.key != 'Enter') {
		return;
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

function onFindOrReplaceRadioChange(e) {
	toggleFindOrReplace();
}

function onFindReplaceCloseButtonClick(e) {
	hideFindReplaceToolbar();
}

function onFindReplaceNextButtonClick(e) {
	navigateFindReplaceResults(1);
}

function onFindReplacePreviousButtonClick(e) {
	navigateFindReplaceResults(-1);
}
function onFindReplaceReplaceButtonClick(e) {
	replaceFindResultIdx(CurFindReplaceDocRangeIdx);
}
function onFindReplaceReplaceAllButtonClick(e) {
	let replace_count = CurFindReplaceDocRanges.length;
	while (replace_count > 0) {
		replaceFindResultIdx(0);
		replace_count--;
	}
}

// #endregion Event Handlers