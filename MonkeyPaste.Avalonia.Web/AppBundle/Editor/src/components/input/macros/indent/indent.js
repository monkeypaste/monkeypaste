// #region Globals

// #endregion Globals

// #region Life Cycle
function initIndent() {
	getEditorElement().addEventListener('keydown', onEditorKeyPressForIndent, true);
}
// #endregion Life Cycle

// #region Getters

function getIndentPlusOneButtonElement() {
	return document.getElementById('indentPlusOne');
}
function getIndentMinusOneButtonElement() {
	return document.getElementById('indentMinusOne');
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function indentSelection(indentLevel) {
	// NOTE simulating click because undent is weird delta for multiple line selection
	const indent_btn_elm = indentLevel == 1 ?
		getIndentPlusOneButtonElement() :
		getIndentMinusOneButtonElement();
	indent_btn_elm.click();
}

// #endregion Actions

// #region Event Handlers
function onEditorKeyPressForIndent(e) {
	if (isReadOnly()) {
		return;
	}
	if (e.key != 'Tab') {
		return;
	}
	e.preventDefault();

	let lvl = 1;
	if (e.shiftKey == true) {
		lvl = -1;
	}
	indentSelection(lvl);
}
// #endregion Event Handlers