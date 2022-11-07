// #region Globals

// #endregion Globals

// #region Life Cycle

function initCheckableList() {
	getEditorElement().addEventListener('click', onEditorClickForCheckableListItem);
}

// #endregion Life Cycle

// #region Getters

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

function onEditorMutationCheckableWatcher(e) {
	let mutations = e;


	for (let mutation of mutations) {
		let li_adds = mutation.addedNodes.filter(x => x.tagName == 'LI');
		let li_removes = mutation.addedNodes.filter(x => x.tagName == 'LI');
		if (mutation.target !== undefined &&
			mutation.target.nodeType === 2 &&
			mutation.target.tagName == 'LI') {
			debugger;
		}
	}
}
function onEditorClickForCheckableListItem(e) {

}

// #endregion Event Handlers