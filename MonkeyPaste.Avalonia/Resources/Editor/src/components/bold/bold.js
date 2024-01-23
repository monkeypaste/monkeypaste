// #region Globals

// #endregion Globals

// #region Life Cycle

function initBold() {
	addClickOrKeyClickEventListener(getBoldToolbarElement(), onBoldToolbarButtonClick);

}

// #endregion Life Cycle

// #region Getters
function getBoldToolbarElement() {
	return document.getElementById('boldToolbarButton');
}
// #endregion Getters

// #region Setters

// #endregion Setters

// #region State

function isSelHeadBold() {
	let sel = getDocSelection();
	if (!sel) {
		return false;
	}
	let sel_format = getFormatForDocRange(sel);
	if (!sel_format ||
		typeof sel_format.bold !== 'boolean' ||
		typeof sel_format.bold == false) {
		return false;
	}
	return true;
}

// #endregion State

// #region Actions

function setBoldToolbarButtonToggleState(isToggled) {
	if (isToggled) {
		getBoldToolbarElement().classList.add('toggled');
	} else {
		getBoldToolbarElement().classList.remove('toggled');
	}
}

function toggleSelectionBoldState() {
	let sel = getDocSelection();
	if (!sel) {
		return;
	}
	let is_toggled = !isSelHeadBold();
	formatDocRange(sel, { bold: is_toggled },'user');

	setBoldToolbarButtonToggleState(is_toggled);
}

function updateBoldToolbarButtonToSelection() {
	if (!isSubSelectionEnabled()) {
		return;
	}
	setBoldToolbarButtonToggleState(isSelHeadBold());
}
// #endregion Actions

// #region Event Handlers

function onBoldToolbarButtonClick(e) {
	toggleSelectionBoldState();
}
// #endregion Event Handlers