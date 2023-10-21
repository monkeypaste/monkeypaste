
// #region Life Cycle

function initLists() {
	initCheckableList();
	//initOrderedList();
	addClickOrKeyClickEventListener(getListToolbarContainerElement(), onListToolbarButtonClick);
	registerListBlot();
}

function registerListBlot() {
	// from https://github.com/quilljs/quill/issues/409#issuecomment-1488435233

	const customFontFamilyAttributor = registerStyleAttributor('custom-family-attributor', 'font-family',null, true)
	const customSizeAttributor = registerStyleAttributor('custom-size-attributor', 'font-size', null, true)
	const customColorAttributor = registerStyleAttributor('custom-color-attributor', 'color', null, true)

	const ListItemBlot = Quill.import('formats/list');
	class CustomListItem extends ListItemBlot {
		optimize(context) {
			super.optimize(context);
			if (this.children.length == 0) {
				return;
			}
			const child = this.children.head;
			const attributes = child?.attributes?.attributes;

			if (attributes) {
				for (const key in attributes) {
					const element = attributes[key];
					let name = element.keyName;
					const value = element.value(child.domNode);

					if (name === 'color') {
						super.format('custom-color-attributor', value);
					}
					else if (name === 'font-family') {
						super.format('custom-family-attributor', value);
					}
					else if (name === 'font-size') {
						super.format('custom-size-attributor', value);
					}
				}
			} else {
				super.format('custom-color-attributor', false);
				super.format('custom-family-attributor', false);
				super.format('custom-size-attributor', false);
			}
		}
	}

	Quill.register(customColorAttributor, true);
	Quill.register(customFontFamilyAttributor, true);
	Quill.register(customSizeAttributor, true);
	Quill.register(CustomListItem, true, true);
}

// #endregion Life Cycle

// #region Getters

function getListToolbarContainerElement() {
	return document.getElementById('listToolbarPopupButton');
}
function getListToolbarPopupElements() {
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
function isEditorListToolbarMenuVisible() {
	return getListToolbarContainerElement().classList.contains('expanded');
}
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
function showEditorListMenu() {
	window.addEventListener('mousedown', onEditorListMenuTempWindowClick, true);

	getListToolbarContainerElement().classList.add('expanded');
	let cm = [];
	for (var i = 0; i < globals.ListOptionItems.length; i++) {
		let aomi = globals.ListOptionItems[i];
		aomi.action = function (option, contextMenuIndex, optionIndex) {
			onListToolbarItemClick(optionIndex);
		};
		cm.push(aomi);
	}
	superCm.destroyMenu();

	let align_tb_elm_rect = getListToolbarContainerElement().getBoundingClientRect();
	let x = align_tb_elm_rect.left;
	let y = align_tb_elm_rect.bottom;
	superCm.createMenu(cm, { pageX: x, pageY: y });
}

function hideEditorListMenu() {
	getListToolbarContainerElement().classList.remove('expanded');
	window.removeEventListener('mousedown', onEditorListMenuTempWindowClick, true);
	superCm.destroyMenu();
}
function updateAddListItemToolbarButtonIsEnabled() {
	// NOTE since ordered and disc use built in events
	// have to manually disabled because they're not using addClick helper
	const is_valid = isAddListItemValid();
	getListToolbarPopupElements().forEach(x => {
		if (is_valid) {
			x.classList.remove('disabled');
			x.removeAttribute('disabled');
		} else {
			x.classList.add('disabled');
			x.setAttribute('disabled',true);
		}
	});
}

function updateLists() {
	let li_elms = getAllListItemElements();
	for (var i = 0; i < li_elms.length; i++) {
		let li_elm = li_elms[i];

		let li_id = globals.ListItemIdAttrb.value(li_elm);
		if (isNullOrEmpty(li_id)) {
			// add li id
			li_id = 'elm_' + generateShortGuid();
			globals.ListItemIdAttrb.add(li_elm, li_id);

		}
		let li_style_id = 'style_' + li_id.split('elm_')[1];
		let li_style_node = document.head.querySelector(`style[id="${li_style_id}"]`);
		if (isNullOrUndefined(li_style_node)) {
			// add li style elm
			li_style_node = document.head.appendChild(document.createElement("style"));
			li_style_node.setAttribute('id', li_style_id);

			//li_style_node.innerHTML = `.ql-editor li#${li_id} > ql-ui::before {color: pink !important;}`;
			li_style_node.innerHTML = `.ql-editor li#${li_id} > ql-ui::before {font-size: 32px !important;}`;
		} else {

		}
	}
    
}
// #endregion Actions

// #region Event Handlers
function onListToolbarItemClick(idx) {
	let list_val = null;
	if (idx == globals.ListOrderedOptIdx) {
		// stupid quill ignores 'left' have to use false
		list_val = 'ordered';
	} else if (idx == globals.ListBulletOptIdx) {
		list_val = 'bullet';
	} else if (idx == globals.ListCheckableOptIdx) {
		list_val = 'unchecked';
	} 
	if (list_val == null) {
		log('list click error, unknown idx: ' + idx);
		return;
	}
	globals.quill.focus();
	let sel = getDocSelection();
	let cur_list_elm = getListItemElementAtDocIdx(sel.index);
	if (cur_list_elm && cur_list_elm.getAttribute('data-list') == list_val) {
		// toggle list off
		list_val = null;
	}
	formatSelection('list', list_val, 'user');
}

function onEditorListMenuTempWindowClick(e) {
	if (isChildOfElement(e.target, getListToolbarContainerElement())) {
		return;
	}
	if (isClassInElementPath(e.target, 'context-menu-option')) {
		return;
	}
	hideEditorListMenu();
}

function onListToolbarButtonClick(e) {
	if (isEditorListToolbarMenuVisible()) {
		hideEditorListMenu();
	} else {
		showEditorListMenu();
	}
}
// #endregion Event Handlers