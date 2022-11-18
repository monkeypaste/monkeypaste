// #region Globals

const ENCODED_LIST_ITEM_OPEN_TOKEN = "{li{";
const ENCODED_LIST_ITEM_CLOSE_TOKEN = "}li}";
const ENCODED_LIST_ITEM_REGEXP = new RegExp(ENCODED_LIST_ITEM_OPEN_TOKEN + ".*?" + ENCODED_LIST_ITEM_CLOSE_TOKEN, "");

// #endregion Globals

// #region Life Cycle

function initLists() {
	initCheckableList();
	//initOrderedList();
}

function registerListBlot() {
	
	//class MyListContainer extends ListContainer {
	//	static tagName = ["OL", "UL"];
	//	static defaultTag = "OL";

	//	static create(value) {
	//		return document.createElement(this.getTag(value));
	//	}

	//	static getTag(val) {
	//		// Our "ql-list" values are "bullet" and "ordered"
	//		const map = {
	//			bullet: "UL",
	//			ordered: "OL",
	//		};
	//		return map[val] || this.defaultTag;
	//	}

	//	checkMerge() {
	//		// Only merge if the next list is the same type as this one
	//		return (
	//			super.checkMerge() &&
	//			this.domNode.tagName === this.next.domNode.tagName
	//		);
	//	}
	//}

	//class MyListItem extends ListItem {
	//	static requiredContainer = MyListContainer;

	//	static register() {
	//		Quill.register(MyListContainer, true);
	//	}

	//	optimize(context) {
	//		if (
	//			this.statics.requiredContainer &&
	//			!(this.parent instanceof this.statics.requiredContainer)
	//		) {
	//			// Insert the format value (bullet, ordered) into wrap arguments
	//			this.wrap(
	//				this.statics.requiredContainer.blotName,
	//				MyListItem.formats(this.domNode)
	//			);
	//		}
	//		super.optimize(context);
	//	}

	//	format(name, value) {
	//		// If the list type is different, wrap this list item in a new MyListContainer of that type
	//		if (
	//			name === ListItem.blotName &&
	//			value !== MyListItem.formats(this.domNode)
	//		) {
	//			this.wrap(this.statics.requiredContainer.blotName, value);
	//		}
	//		super.format(name, value);
	//	}
	//}

}

// #endregion Life Cycle

// #region Getters


function getDiscListToolbarButton() {
	return document.getElementById('discListToolbarButton');
}

function getCheckListToolbarButton() {
	return document.getElementById('checkListToolbarButton');
}

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

function getListItemElementAtDocIdx(doc_idx) {
	let cur_elm = getElementAtDocIdx(doc_idx);
	if (!cur_elm) {
		return null;
	}
	while (true) {
		if (!cur_elm) {
			return null;
		}
		if (cur_elm == getEditorElement()) {
			return null;
		}
		if (cur_elm.tagName !== undefined && cur_elm.tagName.toLowerCase() == 'li') {
			return cur_elm;
		}
		cur_elm = cur_elm.parentNode;
	}
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

function getEncodedListItemStr(li_elm) {
	if (!li_elm) {
		return '';
	}
	let encoded_li_str = `${ENCODED_LIST_ITEM_OPEN_TOKEN}${getListItemElementBulletText(li_elm)}${ENCODED_LIST_ITEM_CLOSE_TOKEN}`;
	return encoded_li_str;
}

function getDecodedListItemText(encoded_text) {
	if (!encoded_text) {
		return encoded_text;
	}
	let decoded_text = encoded_text;
	var result = ENCODED_LIST_ITEM_REGEXP.exec(decoded_text);
	while (result) {
		let encoded_li_text = decoded_text.substr(result.index, result[0].length);
		let li_text = encoded_li_text.replace(ENCODED_LIST_ITEM_OPEN_TOKEN, '').replace(ENCODED_LIST_ITEM_CLOSE_TOKEN, '');
		decoded_text = decoded_text.replaceAll(encoded_li_text, li_text);
		result = ENCODED_LIST_ITEM_REGEXP.exec(decoded_text);
	}
	return decoded_text;
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