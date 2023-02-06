// #region Globals

// #endregion Globals

// #region Life Cycle

// #endregion Life Cycle

// #region Getters

function getForcedCursorStyleElement() {
	return document.getElementById('cursor-style');
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function createForcedCursor() {
	let fc_elm = getForcedCursorStyleElement();
	if (fc_elm) {
		return fc_elm;
	}

	const cursorStyle = document.createElement('style');
	cursorStyle.id = 'cursor-style';
	document.head.appendChild(cursorStyle);

	return getForcedCursorStyleElement();
}

function forceCursor(cursor) {
	let fc_elm = getForcedCursorStyleElement();
	if (!fc_elm) {
		fc_elm = createForcedCursor();
	}
	fc_elm.innerHTML = `*{cursor: ${cursor}!important;}`;
}

function resetForcedCursor() {
	if (getForcedCursorStyleElement()) {
		getForcedCursorStyleElement().remove();
	}
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers