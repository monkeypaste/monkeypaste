// #region Globals

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

function getQuillTooltipElement() {
	let ttelms = Array.from(document.getElementsByClassName('ql-tooltip'));
	if (ttelms.length == 0) {
		// where'd it go?
		debugger;
	}
	if (ttelms.length > 1) {
		// what's the other one?
		debugger;
	}
	return ttelms[0];

}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function positionTooltipToDocRange(docRange) {
	if (!docRange) {
		return;
	}
	let doc_range_rect = getCharacterRect(docRange.index, false);
	getQuillTooltipElement().style.left = doc_range_rect.left + 'px';
	getQuillTooltipElement().style.top = doc_range_rect.bottom + 'px';
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers