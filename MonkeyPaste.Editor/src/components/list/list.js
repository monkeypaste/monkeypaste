// #region Globals

// #endregion Globals

// #region Life Cycle

function initLists() {
	initCheckableList();
}

// #endregion Life Cycle

// #region Getters

function getListItemCountBeforeDocIdx(docIdx) {
	let list_item_count = 0;
	for (var i = 0; i <= docIdx; i++) {
		i = getLineEndDocIdx(i);
		if (i > docIdx) {
			break;
		}
		if (isDocIdxInListItem(i)) {
			list_item_count++;
		}

	}
	return list_item_count;
}

function getAllListItemElements() {
	return Array.from(getEditorElement().querySelectorAll('li'));
}

function getAllListItemBulletDocIdxs() {
	return getAllListItemElements().map(x => getElementDocIdx(x));
}

function getListItemElementBulletText(li_elm) {
	if (!li_elm || li_elm.tagName === undefined || li_elm.tagName.toLowerCase() != 'li') {
		debugger;
	}
	let item_type = li_elm.getAttribute('data-list').toLowerCase();
	if (item_type == 'bullet') {
		return String.fromCharCode(parseInt(2022, 16)); // •
	}
	if (item_type == 'ordered') {
		let li_elm_idx = Array.from(li_elm.parentNode.children).indexOf(li_elm);
		return (li_elm_idx + 1) + '.';
	}
	if (item_type == 'checked') {
		return String.fromCharCode(parseInt(2611, 16)); // ☑
	}
	if (item_type == 'unchecked') {
		return String.fromCharCode(parseInt(2610, 16));; // ☐
	}

}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isDocIdxInListItem(docIdx) {
	let doc_idx_elm = getElementAtDocIdx(docIdx);
	while (doc_idx_elm != null) {
		if (doc_idx_elm && doc_idx_elm.tagName == 'LI') {
			return true;
		}
		doc_idx_elm = doc_idx_elm.parentNode;
	}
	return false;
}
function isDocIdxAtListItemStart(docIdx) {
	//let block_elm = getBlockElementAtDocIdx(docIdx);
	//if (block_elm.tagName == 'LI') {
	//	let doc_idx_elm = getElementAtDocIdx(docIdx);
	//	return doc_idx_elm && doc_idx_elm.tagName == 'BR';
	//}
	//return false;
	return getAllListItemBulletDocIdxs().includes(docIdx - 1);
}

// #endregion State

// #region Actions

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers