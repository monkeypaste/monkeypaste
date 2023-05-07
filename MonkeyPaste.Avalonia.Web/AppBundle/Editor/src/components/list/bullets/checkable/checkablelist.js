// #region Globals

var CheckableListItemAttributor;
// #endregion Globals

// #region Life Cycle

function initCheckableList() {
	getEditorElement().addEventListener('mousedown', onEditorClickForCheckableListItem, true);	
}

// #endregion Life Cycle

// #region Getters

function getCheckableListToolbarButton() {
	return document.getElementById('checkListToolbarButton');
}
function getUncheckedListItemElements() {
	return Array.from(document.querySelectorAll('[data-list="unchecked"]'));
}

function getCheckedListItemElements() {
	return Array.from(document.querySelectorAll('[data-list="checked"]'));
}


// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

function updateCheckableItemElements() {

}

// #endregion Actions

// #region Event Handlers

function onEditorClickForCheckableListItem(e) {
	let li_elm = null;
	if (e.target.tagName !== undefined &&
		e.target.tagName == 'LI') {
		li_elm = e.target;
	}
	if (!li_elm) {
		if (parseBool(e.target.getAttribute('contenteditable')) == false &&
			e.target.tagName == 'SPAN' &&
			e.target.parentNode.tagName == 'LI' &&
			(e.target.parentNode.getAttribute('data-list') == 'checked' ||
				e.target.parentNode.getAttribute('data-list') == 'unchecked')) {
			li_elm = e.target.parentNode;
		}
	}
	if (li_elm) {
		if (e.button != 0) {
			// only toggle check w/ left mouse button
			e.preventDefault();
			e.stopPropagation();
			return false;
		}
		// when check is toggled quill doesn't emit text change until selection change
		// so force update
		updateQuill();
	}
}

function onCheckableListToolbarButtonClick(e) {
	let sel = getDocSelection();
	if (!sel) {
		return;
	}
	//if (sel.length == 0) {
	//	sel.length = 1;
	//}

	//formatDocRange(sel, 'ordered-list-item');
	CheckableListItemAttributor
}
// #endregion Event Handlers