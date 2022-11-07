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
function isDocIdxAtEmptyListItem(docIdx) {
	let block_elm = getBlockElementAtDocIdx(docIdx);
	if (block_elm.tagName == 'LI') {
		let doc_idx_elm = getElementAtDocIdx(docIdx);
		return doc_idx_elm && doc_idx_elm.tagName == 'BR';
	}
	return false;
}

// #endregion State

// #region Actions

// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers