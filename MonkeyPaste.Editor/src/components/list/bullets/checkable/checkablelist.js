// #region Globals

var CheckableListItemAttributor;
// #endregion Globals

// #region Life Cycle

function initCheckableList() {
	initCheckableListMatcher();

	getEditorElement().addEventListener('mousedown', onEditorClickForCheckableListItem, true);	
}

function initCheckableListMatcher() {
	// NOTE! quill renders all li's with data-list attr (bullet|ordered|checked|unchecked)
	// delta-html converter clears ordered and bullet li's attrs and encloses in ol|ul respectively
	// delta-html converter substitutes li's w/ data-list attr (checked|unchecked) w/ data-checked attr (true|false)

	let Delta = Quill.imports.delta;

	quill.clipboard.addMatcher('LI', function (node, delta) {
		if (node.hasAttribute('data-checked')) {
			let is_checked = parseBool(node.getAttribute('data-checked'));
			if (delta && delta.ops !== undefined && delta.ops.length > 0) {
				for (var i = 0; i < delta.ops.length; i++) {
					if (delta.ops[i].insert === undefined) {
						continue;
					}
					if (delta.ops[i].attributes === undefined) {
						delta.ops[i].attributes = {};
					}
					delta.ops[i].attributes.list = is_checked ? 'checked':'unchecked';

				}
			}
		}
		return delta;
	});
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
		quill.update();
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