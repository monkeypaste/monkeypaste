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