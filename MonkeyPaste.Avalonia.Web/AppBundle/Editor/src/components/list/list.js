
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

function getListItemToolbarElements() {
	return [
		getOrderedListToolbarButton(),
		getDiscListToolbarButton(),
		getCheckableListToolbarButton()
	];
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

function getListItemElementIndentedIdx(li_elm) {
	let li_elm_idx = 0;
	let prev_li_elm = li_elm.previousSibling;
	while (true) {
		if (prev_li_elm == null) {
			return li_elm_idx;
		}
		let is_same_indent =
			Array.from(prev_li_elm.classList).every(x => Array.from(li_elm.classList).includes(x)) &&
			Array.from(li_elm.classList).every(x => Array.from(prev_li_elm.classList).includes(x));
		if (!is_same_indent) {
			return li_elm_idx;
		}
		li_elm_idx++;
		prev_li_elm = prev_li_elm.previousSibling;
	}
}

function getListItemElementCounterValue(li_elm) {
	if (!li_elm) {
		return null;
	}
	return window.getComputedStyle(li_elm.firstChild, '::before').getPropertyValue('content');
}

function getListItemIndentLevel(li_elm) {
	if (li_elm.classList.length == 0) {
		return 0;
	}
	return parseInt(li_elm.classList[0].split('ql-indent-')[1]);
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
		let li_elm_idx = getListItemElementIndentedIdx(li_elm);
		let li_elm_counter_val = getListItemElementCounterValue(li_elm);
		return getOrderedListItemBulletText(li_elm_idx, li_elm_counter_val);
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
	let encoded_li_str = `${globals.ENCODED_LIST_ITEM_OPEN_TOKEN}${getListItemElementBulletText(li_elm)}[${getListItemIndentLevel(li_elm)}]${globals.ENCODED_LIST_ITEM_CLOSE_TOKEN}`;
	return encoded_li_str;
}


function getDecodedListItemText(encoded_text) {
	if (!encoded_text) {
		return encoded_text;
	}
	let decoded_text = encoded_text;
	var result = globals.ENCODED_LIST_ITEM_REGEXP.exec(decoded_text);
	while (result) {
		let encoded_li_text = decoded_text.substr(result.index, result[0].length);
		let li_text = encoded_li_text.replace(globals.ENCODED_LIST_ITEM_OPEN_TOKEN, '').replace(globals.ENCODED_LIST_ITEM_CLOSE_TOKEN, '');
		let indent_lvl = parseInt(li_text.split('[')[1].split(']')[0])
		li_text = '\t'.repeat(indent_lvl) + li_text.split('[')[0];
		decoded_text = decoded_text.replaceAll(encoded_li_text, li_text);
		result = globals.ENCODED_LIST_ITEM_REGEXP.exec(decoded_text);
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

function isAddListItemValid() {
	let sel = getDocSelection();
	if (!sel) {
		return false;
	}
	if (isDocIdxInTable(sel.index) ||
		isDocIdxInTable(sel.index + sel.length)) {
		return false;
	}
	return true;
}

// #endregion State

// #region Actions

function updateAddListItemToolbarButtonIsEnabled() {
	// NOTE since ordered and disc use built in events
	// have to manually disabled because they're not using addClick helper
	const is_valid = isAddListItemValid();
	getListItemToolbarElements().forEach(x => {
		if (is_valid) {
			x.classList.remove('disabled');
			x.removeAttribute('disabled');
		} else {
			x.classList.add('disabled');
			x.setAttribute('disabled',true);
		}
	});
}
// #endregion Actions

// #region Event Handlers

// #endregion Event Handlers