// #region Globals

// #endregion Globals

// #region Life Cycle
function initDuplicate() {
	getEditorElement().addEventListener('keydown', onEditorKeyPressForDuplicate, true);
}
// #endregion Life Cycle

// #region Getters

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function duplicateSelection() {
	let sel = getDocSelection();

	if (sel.length == 0) {
		let sel_block_elm = getBlockElementAtDocIdx(sel.index);
		if (!sel_block_elm) {
			return;
		}
		let block_clone = sel_block_elm.cloneNode(true);
		if (sel_block_elm.nextSibling) {
			sel_block_elm.parentNode.insertBefore(block_clone, sel_block_elm.nextSibling);
		} else {
			sel_block_elm.parentNode.appendChild(block_clone);
		}
		return;
	}
	const dup_idx = sel.index + sel.length;
	insertText(dup_idx, getText(sel), 'user');
	setDocSelection(dup_idx, sel.length, 'user');
}

// #endregion Actions

// #region Event Handlers
function onEditorKeyPressForDuplicate(e) {
	if (isReadOnly()) {
		return;
	}
	if (e.ctrlKey != true || e.key != 'd') {
		return;
	}
	e.preventDefault();

	duplicateSelection();
}
// #endregion Event Handlers