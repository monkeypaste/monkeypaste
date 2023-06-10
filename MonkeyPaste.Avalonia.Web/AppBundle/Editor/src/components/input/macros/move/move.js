// #region Globals

// #endregion Globals

// #region Life Cycle
function initMove() {
	getEditorElement().addEventListener('keydown', onEditorKeyPressForMove, true);
}
// #endregion Life Cycle

// #region Getters
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function moveSelection(dir) {
	let sel = getDocSelection();
	let sel_block_elms = [];
	let sel_block_parent_elm = null;
	for (var i = sel.index; i <= sel.index + sel.length; i++) {
		let cur_block_elm = getBlockElementAtDocIdx(i);
		if (!cur_block_elm) {
			debugger;
			continue;
		}
		if (sel_block_elms.includes(cur_block_elm)) {
			continue;
		}
		sel_block_elms.push(cur_block_elm);
		if (cur_block_elm.parentNode && cur_block_elm.parentNode != sel_block_parent_elm) {
			if (sel_block_parent_elm) {
				// how to handle multiple block parents?
				debugger;
			}
			sel_block_parent_elm = cur_block_elm.parentNode;
		}
	}
	let shift_block_elm = null;
	let shift_start_idx =
		dir == 1 ?
			sel.index + sel.length + 1 :
			sel.index - 1;
	let shift_idx = shift_start_idx;
	while (true) {
		if (shift_idx < 0 || shift_idx > getDocLength()) {
			break;
		}
		let cur_block = getBlockElementAtDocIdx(shift_idx);
		if (cur_block && !sel_block_elms.includes(cur_block)) {
			shift_block_elm = cur_block;
			break;
		}
		shift_idx += dir;
	}
	if (!shift_block_elm) {
		// already at beginning or end
		return;
	}
	if (dir == 1) {

	}
	let ref_elm =
		dir == 1 ?
			sel_block_elms[0] :
			sel_block_elms[sel_block_elms.length - 1].nextSibling;
	if (!ref_elm) {
		return;
	}
	shift_block_elm.remove();
	sel_block_parent_elm.insertBefore(shift_block_elm, ref_elm);
}

// #endregion Actions

// #region Event Handlers
function onEditorKeyPressForMove(e) {
	if (isReadOnly()) {
		return;
	}
	if (e.altKey != true) {
		return;
	}
	let dir =
		e.key == 'ArrowUp' ?
			-1 :
			e.key == 'ArrowDown' ?
				1 :
				null;

	if (!dir) {
		return;
	}
	e.preventDefault();

	moveSelection(dir);
}
// #endregion Event Handlers