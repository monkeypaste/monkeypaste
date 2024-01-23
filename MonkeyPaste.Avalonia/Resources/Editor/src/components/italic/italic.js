// #region Globals

// #endregion Globals

// #region Life Cycle

function initItalic() {
	addClickOrKeyClickEventListener(getItalicToolbarElement(), onItalicToolbarButtonClick);

}

// #endregion Life Cycle

// #region Getters
function getItalicToolbarElement() {
	return document.getElementById('italicToolbarButton');
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isSelHeadItalic() {
	let sel = getDocSelection();
	if (!sel) {
		return false;
	}
	let sel_format = getFormatForDocRange(sel);
	if (!sel_format ||
		typeof sel_format.italic !== 'boolean' ||
		typeof sel_format.italic == false) {
		return false;
	}
	return true;
}

// #endregion State

// #region Actions

function setItalicToolbarButtonToggleState(isToggled) {
	if (isToggled) {
		getItalicToolbarElement().classList.add('toggled');
	} else {
		getItalicToolbarElement().classList.remove('toggled');
	}
}
function toggleSelectionItalicState() {
	let sel = getDocSelection();
	if (!sel) {
		return;
	}
	let is_toggled = !isSelHeadItalic();

	formatDocRange(sel, { italic: is_toggled }, 'user');
	setItalicToolbarButtonToggleState(is_toggled);
}
function updateItalicToolbarButtonToSelection() {
	if (!isSubSelectionEnabled()) {
		return;
	}
	setItalicToolbarButtonToggleState(isSelHeadItalic());
}
// #endregion Actions

// #region Event Handlers

function onItalicToolbarButtonClick(e) {
	toggleSelectionItalicState();
}
// #endregion Event Handlers