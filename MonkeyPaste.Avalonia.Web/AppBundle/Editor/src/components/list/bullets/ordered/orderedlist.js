// #region Globals

// #endregion Globals

// #region Life Cycle

function initOrderedList() {
	initOrderListBlot();
	initOrderedListToolbarButton();
}

function initOrderedListToolbarButton() {
	addClickOrKeyClickEventListener(getOrderedListToolbarButton(), onOrderedListToolbarButtonClick);
	getOrderedListToolbarButton().innerHTML = getSvgHtml('orderedlist');
}

function initOrderListBlot() {

	if (Quill === undefined) {
		/// host load error case
		debugger;
	}
	let Parchment = Quill.imports.parchment;
	class OrderedListContainer extends Parchment.ContainerBlot {
		static create(value) {
			const node = super.create(value);
			node.setAttribute('type', '1');
			return node;
		}
	}

	class OrderedListItem extends Parchment.BlockBlot {
		static blotName = 'ordered-list-item';
		static tagName = 'LI';
		static requiredContainer = OrderedListContainer;

		static register() {
			Quill.register(OrderedListContainer);
		}
		constructor(scroll, domNode) {
			super(scroll, domNode);
			//const ui = domNode.ownerDocument.createElement('span');
			//const listEventHandler = e => {
			//	if (!scroll.isEnabled()) return;
			//	const format = this.statics.formats(domNode, scroll);
			//	if (format === 'checked') {
			//		this.format('list', 'unchecked');
			//		e.preventDefault();
			//	} else if (format === 'unchecked') {
			//		this.format('list', 'checked');
			//		e.preventDefault();
			//	}
			//};
			//ui.addEventListener('mousedown', listEventHandler);
			//ui.addEventListener('touchstart', listEventHandler);
			//this.attachUI(ui);
			//e.preventDefault();
		}
	}

	
	OrderedListContainer.blotName = 'ordered-list-container';
	OrderedListContainer.tagName = 'OL';
	OrderedListContainer.allowedChildren = [OrderedListItem];

	Quill.register(OrderedListItem, true);
	Quill.register(OrderedListContainer, true);
}

// #endregion Life Cycle

// #region Getters

function getOrderedListToolbarButton() {
	return document.getElementById('orderedListToolbarButton');
}

function getOrderedListItemBulletText(idx, counter_val) {
	let symbol = '';
	let punc = '. ';
	if (isNullOrEmpty(counter_val)) {
		symbol = (idx + 1).toString();
	} else {
		// test results:

		// indent 0: 'counter(list-0) ". "''
		// indent 1: 'counter(list-1, lower-alpha) ". "'
		// indent 2: 'counter(list-2, lower-roman) ". "'
		// indent 3: goto 0

		let indent = -1;
		if (counter_val.startsWith('counter(list-0)')) {
			indent = 0;
		} else {
			indent = parseInt(counter_val.split('counter(list-')[1].split(',')[0]);
		}

		let format = '';
		if (indent % 3 == 0) {
			// types repeat and roman numerals
			format = 'decimal';
		} else {
			format = counter_val.split(',')[1].trim().split(')')[0];
		}

		symbol = getOrderedListItemSymbol(format, idx);

		punc = counter_val.split('"')[1];

	}

	return symbol + punc;
}

function getOrderedListItemSymbol(listType, idx) {
	if (listType == 'decimal') {
		return (idx + 1).toString();
	}
	if (listType == 'lower-alpha') {
		// BUG this will give wrong result > 26 prolly A instead of aa
		return String.fromCharCode('a'.charCodeAt(0) + idx);
	}
	if (listType == 'lower-roman') {
		return convertIntToRomanNumeral(idx + 1).toLowerCase();
	}
}

// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

// #endregion State

// #region Actions

// #endregion Actions

// #region Event Handlers

function onOrderedListToolbarButtonClick(e) {
	let sel = getDocSelection();
	if (!sel) {
		return;
	}
	if (sel.length == 0) {
		sel.length = 1;
	}
	formatDocRange(sel, 'ordered-list-item');
}

// #endregion Event Handlers