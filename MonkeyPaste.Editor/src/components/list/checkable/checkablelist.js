// #region Globals

// #endregion Globals

// #region Life Cycle

function initCheckableList() {
	getEditorElement().addEventListener('click', onEditorClickForCheckableListItem);

	let suppressWarning = false;
    let config = {
        scope: Parchment.Scope.ANY,
    };

	let attrb_name = "data-list";
	let attrb = new Parchment.Attributor(attrb_name, "check", config);
	Quill.register(attrb, suppressWarning);
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

function fixDelta2HtmlCheckables(htmlStr) {
	htmlStr = htmlStr.replaceAll('[data-checked="false"]', '[data-list="unchecked"]');
	htmlStr = htmlStr.replaceAll('[data-checked="true"]', '[data-list="checked"]');
	return htmlStr;
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