// #region Globals

// #endregion Globals

// #region Life Cycle

function initWrap() {
	addClickOrKeyClickEventListener(getWrapToolbarElement(), onWrapToolbarButtonClick);
}

// #endregion Life Cycle

// #region Getters
function getWrapToolbarElement() {
	return document.getElementById('wrapToolbarButton');
}
// #endregion Getters

// #region Setters

function setWrap(isWrapEnabled) {
	if (isWrapEnabled) {
		enableWrap();
	} else {
		disableWrap();
	}
}

// #endregion Setters

// #region State

function isWrapEnabled() {
	if (getEditorContainerElement().classList.contains('unwrap')) {
		return false;
	}
	return true;
}

// #endregion State

// #region Actions

function enableWrap(fromHost = false) {
	if (isWrapEnabled()) {
		return;
	}
	getEditorContainerElement().classList.remove('unwrap');
	updateAllElements();
	if (!fromHost) {
		onWrapEnabledChanged_ntf(true);
	}
}
function disableWrap(fromHost = false) {
	if (!isWrapEnabled()) {
		return;
	}
	getEditorContainerElement().classList.add('unwrap');
	updateAllElements();
	if (!fromHost) {
		onWrapEnabledChanged_ntf(false);
	}
}

function setWrapToolbarButtonToggleState(isToggled) {
	if (isToggled) {
		getWrapToolbarElement().classList.add('toggled');
	} else {
		getWrapToolbarElement().classList.remove('toggled');
	}
}

function toggleWrap() {
	if (isWrapEnabled()) {
		disableWrap();
		setWrapToolbarButtonToggleState(false);
	} else {
		enableWrap();
		setWrapToolbarButtonToggleState(true);
	}
}

function updateWrapToolbarButtonToSelection() {
	setWrapToolbarButtonToggleState(isWrapEnabled());
}
// #endregion Actions

// #region Event Handlers

function onWrapToolbarButtonClick(e) {
	toggleWrap();
}
// #endregion Event Handlers