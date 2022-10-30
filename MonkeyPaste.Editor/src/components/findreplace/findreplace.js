// #region Globals

var CurFindReplaceDocRanges = null;
var CurFindReplaceDocRangeIdx = -1;

var CurFindReplaceDocRangeRectIdxLookup = null;

var CurFindReplaceDocRangesRects = null;
var CurFindReplaceDocRangeIdx_rects_sidx = -1;
var CurFindReplaceDocRangeIdx_rects_eidx = -1;

// #endregion Globals

// #region Life Cycle

function initFindReplaceToolbar() {
	const findReplaceToolbarButton = new QuillToolbarButton({
		icon: getSvgHtml('findreplace')
	});

	findReplaceToolbarButton.qlFormatsEl.addEventListener('click', onFindReplaceToolbarButtonClick);
	findReplaceToolbarButton.attach(quill);

	let findButton = getFindTabButtonElement();
	addClickOrKeyClickEventListener(findButton, onTabButtonClick);

	let replaceButton = getReplaceTabButtonElement();
	addClickOrKeyClickEventListener(replaceButton, onTabButtonClick);

	let closeButton = getFindReplaceCloseButtonElement();
	addClickOrKeyClickEventListener(closeButton, onFindReplaceCloseButtonClick);

	let nextButton = getFindReplaceNextButton();
	addClickOrKeyClickEventListener(nextButton, onFindReplaceNextButtonClick);

	let previousButton = getFindReplacePreviousButton();
	addClickOrKeyClickEventListener(previousButton, onFindReplacePreviousButtonClick);
}

// #endregion Life Cycle

// #region Getters

function getReplaceInputElement() {
	return document.getElementById('replaceInput');
}
function getReplaceInputLabelElement() {
	return document.getElementById('replaceInputLabel');
}


function getReplaceTabElement() {
	return document.getElementById('replaceTab');
}
function getReplaceTabButtonElement() {
	return document.getElementById('replaceTabButton');
}

function getFindTabButtonElement() {
	return document.getElementById('findTabButton');
}

function getFindReplaceNextButton() {
	return document.getElementById('findReplaceNextButton');
}

function getFindReplacePreviousButton() {
	return document.getElementById('findReplacePreviousButton');
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
// #endregion Getters

// #region Setters

function setSelectedFindReplaceTab(tabButtonId) {
	let tabId = tabButtonId.replace('Button', '');
	if (tabId == 'findTab') {
		getReplaceInputElement().classList.add('hidden');
		getReplaceInputLabelElement().classList.add('hidden');

		getFindTabButtonElement().classList.add('selected');
		getReplaceTabButtonElement().classList.remove('selected');

		//getFindReplaceNextButton().innerHTML = "Find Next";
	} else if (tabId == 'replaceTab') {
		getReplaceInputElement().classList.remove('hidden');
		getReplaceInputLabelElement().classList.remove('hidden');

		getFindTabButtonElement().classList.remove('selected');
		getReplaceTabButtonElement().classList.add('selected');

		//getFindReplaceNextButton().innerHTML = "Replace Next";
	} else {
		log('unknown tabId: ' + tabId);
		debugger;
	}
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
// #endregion State

// #region Actions

function showFindReplaceToolbar(isReplaceSelected = false, searchText = '', isCaseMatch = false, isWholeWordMatch = false, useRegex = false) {
	getFindReplaceToolbarElement().classList.remove('hidden');
	if (isReplaceSelected) {
		setSelectedFindReplaceTab('replaceTab');
	} else {
		setSelectedFindReplaceTab('findTab');
	}
	updateAllSizeAndPositions();
}

function hideFindReplaceToolbar() {
	resetFindReplaceResults();
	getFindReplaceToolbarElement().classList.add('hidden');
	updateAllSizeAndPositions();
}

function updateFindReplaceToolbarSizesAndPositions() {
	if (!isShowingFindReplaceToolbar()) {
		return;
	}
	let et_bottom = getEditorToolbarElement().getBoundingClientRect().bottom;
	getFindReplaceToolbarElement().style.top = et_bottom + 'px';
}

function resetFindReplaceResults() {
	CurFindReplaceDocRanges = null;
	CurFindReplaceDocRangeIdx = -1;

	CurFindReplaceDocRangesRects = null;
	CurFindReplaceDocRangeRectIdxLookup = null;
	CurFindReplaceDocRangeIdx_rects_sidx = -1;
	CurFindReplaceDocRangeIdx_rects_eidx = -1;
}

function executeFindReplace() {
	resetFindReplaceResults();

	let search_text = document.getElementById('findInput').value;
	let replace_text = isShowingReplaceTab() ? document.getElementById('replaceInput').value : null;

	let is_case_sensitive = document.getElementById('matchCaseInput').checked;
	let is_whole_word = document.getElementById('wholeWordInput').checked;
	let use_regex = document.getElementById('useRegexInput').checked;

	if (isNullOrEmpty(search_text)) {
		return;
	}

	let pt = getText();
	if (!is_case_sensitive) {
		pt = pt.toLowerCase();
		search_text = search_text.toLowerCase();
	}

	let sel = getEditorSelection();
	if (sel && sel.length > 0) {		
		// when text is selected unselect but retain caret idx
		sel.length = 0;
		setEditorSelection(sel.index, 0);
	}
	sel = sel ? sel : { index: 0, length: 0 };


	let cur_idx = pt.indexOf(search_text);
	while (cur_idx >= 0) {
		if (CurFindReplaceDocRanges == null) {
			CurFindReplaceDocRanges = [];
			//CurFindReplaceDocRangeIdx = 0;
		}
		let cur_range = { index: cur_idx, length: search_text.length }
		CurFindReplaceDocRanges.push(cur_range);

		if (cur_idx > sel.index) {
			CurFindReplaceDocRangeIdx = Math.max(0, CurFindReplaceDocRanges.length - 1);
		}

		let cur_pt = substringByLength(pt, cur_idx + search_text.length);
		let rel_idx = cur_pt.indexOf(search_text);
		if (rel_idx < 0) {
			break;
		}
		cur_idx = pt.length - cur_pt.length + rel_idx;
	}

	if (CurFindReplaceDocRanges.length == 0) {
		return;
	}
	if (CurFindReplaceDocRangeIdx < 0) {
		CurFindReplaceDocRangeIdx = 0;
	}


	updateFindReplaceRangeRects();

}

function updateFindReplaceRangeRects() {
	CurFindReplaceDocRangesRects = [];
	CurFindReplaceDocRangeRectIdxLookup = [];
	for (var i = 0; i < CurFindReplaceDocRanges.length; i++) {
		let cur_range_rects = getRangeRects(CurFindReplaceDocRanges[i]);

		let rect_lookup = [
			CurFindReplaceDocRangesRects.length,
			CurFindReplaceDocRangesRects.length + cur_range_rects.length - 1
		];
		CurFindReplaceDocRangeRectIdxLookup.push(rect_lookup);
		for (var j = 0; j < cur_range_rects.length; j++) {
			CurFindReplaceDocRangesRects.push(cur_range_rects[j]);
		}
	}
	drawOverlay();
}

// #endregion Actions

// #region Event Handlers

function onFindReplaceToolbarButtonClick(e) {
	if (isShowingFindReplaceToolbar()) {
		hideFindReplaceToolbar();
	} else {
		showFindReplaceToolbar();
	}
}

function onTabButtonClick(e) {
	let tabId = e.currentTarget.id;

	setSelectedFindReplaceTab(tabId);
}

function onFindReplaceCloseButtonClick(e) {
	hideFindReplaceToolbar();
}

function onFindReplaceNextButtonClick(e) {
	if (CurFindReplaceDocRanges == null) {
		executeFindReplace();
		if (CurFindReplaceDocRanges == null) {
			// TODO show validate here
			return;
		}
	}
	CurFindReplaceDocRangeIdx++;
	if (CurFindReplaceDocRangeIdx >= CurFindReplaceDocRanges.length) {
		CurFindReplaceDocRangeIdx = 0;
	}
	let active_y = CurFindReplaceDocRangesRects[CurFindReplaceDocRangeRectIdxLookup[CurFindReplaceDocRangeIdx][0]].top;
	scrollEditorTop(active_y);

	drawOverlay();
}

function onFindReplacePreviousButtonClick(e) {
	if (CurFindReplaceDocRanges == null) {
		executeFindReplace();
		if (CurFindReplaceDocRanges == null) {
			// TODO show validate here
			return;
		}
	}

	CurFindReplaceDocRangeIdx--;
	if (CurFindReplaceDocRangeIdx < 0) {
		CurFindReplaceDocRangeIdx = CurFindReplaceDocRanges.length - 1;
	}
	let active_y = CurFindReplaceDocRangesRects[CurFindReplaceDocRangeRectIdxLookup[CurFindReplaceDocRangeIdx][0]].top;
	scrollEditorTop(active_y);
	drawOverlay();
}

// #endregion Event Handlers