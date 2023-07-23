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

function updateSelCursor() {
	// change cursor to move when hovering over selected range and
	// not in a drag state
	if (globals.WindowMouseDownLoc ||
		!globals.CurSelRects) {
		getEditorContainerElement().classList.remove('range-selected');
		return;
	}
	let over_sel = globals.CurSelRects.some(x => isPointInRect(x, globals.WindowMouseLoc));
	if (over_sel) {
		getEditorContainerElement().classList.add('range-selected');
	} else {
		getEditorContainerElement().classList.remove('range-selected');
	}
}

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers